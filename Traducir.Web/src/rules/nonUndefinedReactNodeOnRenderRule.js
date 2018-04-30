"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Lint = require("tslint");
const ts = require("typescript");
class Rule extends Lint.Rules.AbstractRule {
    apply(sourceFile) {
        return this.applyWithWalker(new NonUndefinedReactNodeOnRenderWalker(sourceFile, this.getOptions()));
    }
}
Rule.FailureString = `In Component and PureComponent, render() return type should be "NonUndefinedReactNode".`;
exports.Rule = Rule;
// tslint:disable-next-line:max-classes-per-file
class NonUndefinedReactNodeOnRenderWalker extends Lint.RuleWalker {
    visitMethodDeclaration(node) {
        if (node.name.getText() === "render" &&
            node.type &&
            node.type.getText() !== "NonUndefinedReactNode" &&
            node.parent &&
            node.parent.kind === ts.SyntaxKind.ClassDeclaration &&
            node.parent.heritageClauses &&
            node.parent.heritageClauses.some(h => h.token === ts.SyntaxKind.ExtendsKeyword &&
                h.types.some(t => {
                    const baseClass = t.expression.getText();
                    return baseClass === "React.Component" || baseClass === "React.PureComponent";
                }))) {
            this.addFailureAtNode(node.type, Rule.FailureString, Lint.Replacement.replaceNode(node.type, "NonUndefinedReactNode"));
        }
        super.visitMethodDeclaration(node);
    }
}
