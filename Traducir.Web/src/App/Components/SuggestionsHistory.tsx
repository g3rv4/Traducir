import axios from "axios";
import { Location } from "history";
import * as React from "react";
import history from "../../history";
import IConfig from "../../Models/Config";
import ISOStringSuggestion, { StringSuggestionState, suggestionStateToString } from "../../Models/SOStringSuggestion";
import { suggestionHistoryTypeToString } from "../../Models/SOStringSuggestionHistory";
import IUserInfo from "../../Models/UserInfo";
import { NonUndefinedReactNode } from "../NonUndefinedReactNode";

export interface ISuggestionsHistoryProps {
    showErrorMessage: (messageOrCode: string | number) => void;
    currentUser?: IUserInfo;
    config: IConfig;
    location: Location;
}

interface ISuggestionsHistoryState {
    suggestions?: ISOStringSuggestion[];
}

export default class SuggestionsHistory extends React.Component<ISuggestionsHistoryProps, ISuggestionsHistoryState> {
    constructor(props: ISuggestionsHistoryProps) {
        super(props);

        this.state = {
            suggestions: undefined
        };
    }

    public componentDidMount(): void {
        this.userChanged(this.props.location);
    }

    public componentWillReceiveProps(nextProps: ISuggestionsHistoryProps, context: any): void {
        if (nextProps.location.pathname !== this.props.location.pathname) {
            this.userChanged(nextProps.location);
        }
    }

    public render(): NonUndefinedReactNode {
        if (!this.props.currentUser || !this.state.suggestions) {
            return null;
        }
        return this.state.suggestions.map(sug => <div key={sug.id} className="mt-5">
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

    public getBadgeClassFromState(state: StringSuggestionState): string | undefined {
        switch (state) {
            case StringSuggestionState.Created:
                return "badge-secondary";
            case StringSuggestionState.ApprovedByReviewer:
            case StringSuggestionState.ApprovedByTrustedUser:
                return "badge-success";
            case StringSuggestionState.Rejected:
                return "badge-danger";
            case StringSuggestionState.DeletedByOwner:
                return "badge-dark";
        }
    }

    public async userChanged(location: Location): Promise<void> {
        const userId = location.pathname.split("/").pop();
        try {
            const r = await axios.get<ISOStringSuggestion[]>(`/app/api/suggestions-by-user/${userId}`);
            this.setState({
                suggestions: r.data
            });
        } catch (e) {
            if (e.response.status === 401) {
                history.push("/");
            } else {
                this.props.showErrorMessage(e.response.status);
            }
        }
    }
}
