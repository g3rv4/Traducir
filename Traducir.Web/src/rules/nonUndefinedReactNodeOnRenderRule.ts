import * as Lint from "tslint";
import * as ts from "typescript";

export class Rule extends Lint.Rules.AbstractRule {
    public static FailureString: string = `In Component and PureComponent, render() return type should be "NonUndefinedReactNode".`;

    public apply(sourceFile: ts.SourceFile): Lint.RuleFailure[] {
        return this.applyWithWalker(new NonUndefinedReactNodeOnRenderWalker(sourceFile, this.getOptions()));
    }
}

// tslint:disable-next-line:max-classes-per-file
class NonUndefinedReactNodeOnRenderWalker extends Lint.RuleWalker {
    public visitMethodDeclaration(node: ts.MethodDeclaration): void {
        if (
            node.name.getText() === "render" &&
            node.type &&
            node.type.getText() !== "NonUndefinedReactNode" &&
            node.parent &&
            node.parent.kind === ts.SyntaxKind.ClassDeclaration &&
            node.parent.heritageClauses &&
            node.parent.heritageClauses.some(h =>
                h.token === ts.SyntaxKind.ExtendsKeyword &&
                h.types.some(t => {
                    const baseClass = t.expression.getText();
                    return baseClass === "React.Component" || baseClass === "React.PureComponent";
                })
            )
        ) {
            this.addFailureAtNode(
                node.type,
                Rule.FailureString,
                Lint.Replacement.replaceNode(node.type, "NonUndefinedReactNode")
            );
        }

        super.visitMethodDeclaration(node);
    }
}
