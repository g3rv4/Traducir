import * as React from "react";
import { NonUndefinedReactNode } from "../NonUndefinedReactNode";
import ISOStringSuggestion, { StringSuggestionState, suggestionStateToString } from "../../Models/SOStringSuggestion";
 
interface ISuggestionsHistoryProps {
    suggestions?: ISOStringSuggestion[];
}

export default class Filters extends React.Component<ISuggestionsHistoryProps> {

    public componentWillReceiveProps(nextProps: ISuggestionsHistoryProps, context: any): void {
        this.setState({ suggestion: nextProps });
    }

    public render(): NonUndefinedReactNode {
        if (!this.props.suggestions) {
            return null;
        }
        return this.props.suggestions.map(sug => <div key={sug.id} className="mt-5">
            <div>
                <span className="font-weight-bold">Original String:</span> <pre className="d-inline">{sug.originalString}</pre>
            </div>
            <div>
                <span className="font-weight-bold">Suggestion:</span> <pre className="d-inline">{sug.suggestion}</pre>
            </div>
            <div>
                <span className="font-weight-bold">State:</span> <span className={`badge ${this.getBadgeClassFromState(sug.state)}`}>{suggestionStateToString(sug.state)}</span>
            </div>
            <table className="table">
                <thead>
                    <tr>
                        <th>Event</th>
                        <th>User</th>
                        <th>Comment</th>
                        <th>Date</th>
                    </tr>
                </thead>
                <tbody>
                    {sug.histories.map(sugHistory => <tr key={sugHistory.id}>
                        <td>
                            {suggestionHistoryTypeToString(sugHistory.historyType)}
                        </td>
                        <td>
                            <a
                                href={`https://${this.props.config.siteDomain}/users/${sugHistory.userId}`}
                                target="_blank"
                                title={`at ${sug.creationDate} UTC`}
                            >
                                {sugHistory.userName}
                            </a>
                        </td>
                        <td>
                            {sugHistory.comment}
                        </td>
                        <td>
                            {sugHistory.creationDate}
                        </td>
                    </tr>)}
                </tbody>
            </table>
        </div>);
    }
}
