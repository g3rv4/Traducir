import axios, { AxiosError } from "axios";
import * as React from "react";
import history from "../../history";
import Config from "../../Models/Config";
import ISOStringSuggestion, { StringSuggestionState } from "../../Models/SOStringSuggestion";
import IUserInfo from "../../Models/UserInfo";
import { UserType } from "../../Models/UserType";

export interface ISuggestionsTableProps {
    suggestions: ISOStringSuggestion[];
    config: Config;
    user?: IUserInfo;
    refreshString: (stringIdToUpdate: number) => void;
    showErrorMessage: (messageOrCode: string | number) => void;
}

interface ISuggestionsTableState {
    isButtonDisabled?: boolean;
}

enum ReviewAction {
    Accept = 1,
    Reject = 2
}

export default class SuggestionsTable extends React.Component<ISuggestionsTableProps, ISuggestionsTableState> {
    constructor(props: ISuggestionsTableProps) {
        super(props);
        this.state = {
            isButtonDisabled: false
        };
    }
    public render(): JSX.Element | null {
        if (!this.props.suggestions || !this.props.suggestions.length) {
            return null;
        }

        return <table className="table mt-2">
            <thead>
                <tr>
                    <th>Suggestion</th>
                    <th>Approved By</th>
                    <th>Created by</th>
                    <th></th>
                    <th></th>
                </tr>
            </thead>
            <tbody>
                {this.props.suggestions.map(sug =>
                    <tr key={sug.id} className={sug.state === StringSuggestionState.ApprovedByTrustedUser ? "table-success" : ""}>
                        <td><pre>{sug.suggestion}</pre></td>
                        <td>{sug.lastStateUpdatedByName &&
                            <a href={`https://${this.props.config.siteDomain}/users/${sug.lastStateUpdatedById}`}
                                target="_blank">{sug.lastStateUpdatedByName}</a>
                        }</td>
                        <td><a href={`https://${this.props.config.siteDomain}/users/${sug.createdById}`}
                            target="_blank"
                            title={"at " + sug.creationDate + " UTC"}>{sug.createdByName}</a></td>
                        <td>{this.renderDeleteButton(sug)}</td>
                        <td>{this.renderSuggestionActions(sug)}</td>
                    </tr>)}
            </tbody>
        </table>;
    }

    public renderSuggestionActions(sug: ISOStringSuggestion): JSX.Element | null {
        if (!this.props.user || !this.props.user.canReview) {
            return null;
        }

        if (sug.state === StringSuggestionState.ApprovedByTrustedUser &&
            this.props.user.userType === UserType.TrustedUser) {
            // a trusted user can't act on a suggestion approved by a trusted user
            return null;
        }

        return <div className="btn-group" role="group">
            <button type="button" className="btn btn-sm btn-success" onClick={e => this.processReview(sug, ReviewAction.Accept)} disabled={this.state.isButtonDisabled}>
                <i className="fas fa-thumbs-up"></i>
            </button>
            <button type="button" className="btn btn-sm btn-danger" onClick={e => this.processReview(sug, ReviewAction.Reject)} disabled={this.state.isButtonDisabled}>
                <i className="fas fa-thumbs-down"></i>
            </button>
        </div>;
    }

    public processReview(sug: ISOStringSuggestion, action: ReviewAction) {
        this.setState({
            isButtonDisabled: true
        });
        axios.put("/app/api/review", {
            Approve: action === ReviewAction.Accept,
            SuggestionId: sug.id
        }).then(r => {
            this.props.refreshString(sug.stringId);
            history.push("/filters");
        }).catch(e => {
            if (e.response.status === 400) {
                this.props.showErrorMessage("Error reviewing the suggestion. Do you have enough rights?");
            } else {
                this.props.showErrorMessage(e.response.status);
            }
            this.setState({
                isButtonDisabled: false
            });
        });
    }

    public renderDeleteButton(sug: ISOStringSuggestion): JSX.Element | null {
        if (!this.props.user || sug.createdById !== this.props.user.id) {
            return null;
        }
        return <button type="button" className="btn btn-sm btn-danger" onClick={e => this.processDeleteReview(sug)} disabled={this.state.isButtonDisabled}>
            DELETE
        </button>;
    }

    public processDeleteReview(sug: ISOStringSuggestion) {
        this.setState({
            isButtonDisabled: true
        });
        axios.delete("/app/api/suggestions/" + sug.id
        ).then(r => {
            this.props.refreshString(sug.stringId);
            this.setState({
                isButtonDisabled: false
            });
        }).catch(e => {
            if (e.response.status === 400) {
                this.props.showErrorMessage("Error deleting the suggestion. Do you have enough rights?");
            } else {
                this.props.showErrorMessage(e.response.status);
            }
            this.setState({
                isButtonDisabled: false
            });
        });
    }
}
