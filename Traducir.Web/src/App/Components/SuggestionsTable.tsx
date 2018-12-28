import * as React from "react";
import IConfig from "../../Models/Config";
import ISOStringSuggestion, { StringSuggestionState } from "../../Models/SOStringSuggestion";
import IUserInfo from "../../Models/UserInfo";
import { NonUndefinedReactNode } from "../NonUndefinedReactNode";
import Suggestion from "./Suggestion";

export interface ISuggestionsTableProps {
    suggestions: ISOStringSuggestion[];
    config: IConfig;
    user?: IUserInfo;
    refreshString: (stringIdToUpdate: number) => void;
    showErrorMessage: (messageOrCode: string | number) => void;
    stringToReplace: string;
}

export default class SuggestionsTable extends React.Component<ISuggestionsTableProps> {
    public render(): NonUndefinedReactNode {
        if (!this.props.suggestions || !this.props.suggestions.length) {
            return null;
        }

        return <table className="table mt-2">
            <thead>
                <tr>
                    <th>Suggestion</th>
                    <th>Approved By</th>
                    <th>Created by</th>
                    <th />
                    <th />
                </tr>
            </thead>
            <tbody>
                {this.props.suggestions.map(sug => <Suggestion
                    key={sug.id}
                    sug={sug}
                    config={this.props.config}
                    user={this.props.user}
                    refreshString={this.props.refreshString}
                    showErrorMessage={this.props.showErrorMessage}
                    stringToReplace={this.props.stringToReplace}
                />)}
            </tbody>
        </table>;
    }
}
