import axios, { AxiosError } from "axios";
import { autobind } from "core-decorators";
import React = require("react");
import history from "../../history";
import IConfig from "../../Models/Config";
import ISOStringSuggestion, { StringSuggestionState } from "../../Models/SOStringSuggestion";
import IUserInfo from "../../Models/UserInfo";
import { UserType } from "../../Models/UserType";

interface ISuggestionProps {
    sug: ISOStringSuggestion;
    config: IConfig;
    user?: IUserInfo;
    refreshString: (stringIdToUpdate: number) => void;
    showErrorMessage: (messageOrCode: string | number) => void;
}

interface ISuggestionState {
    isButtonDisabled: boolean;
}

enum ReviewAction {
    Accept = 1,
    Reject = 2
}

export default class Suggestion extends React.Component<ISuggestionProps, ISuggestionState> {
    constructor(props: ISuggestionProps) {
        super(props);
        this.state = {
            isButtonDisabled: false
        };
    }
    public render() {
        return <tr className={this.props.sug.state === StringSuggestionState.ApprovedByTrustedUser ? "table-success" : ""}>
            <td><pre>{this.props.sug.suggestion}</pre></td>
            <td>{this.props.sug.lastStateUpdatedByName &&
                <a
                    href={`https://${this.props.config.siteDomain}/users/${this.props.sug.lastStateUpdatedById}`}
                    target="_blank"
                >
                    {this.props.sug.lastStateUpdatedByName}
                </a>
            }</td>
            <td>
                <a
                    href={`https://${this.props.config.siteDomain}/users/${this.props.sug.createdById}`}
                    target="_blank"
                    title={`at ${this.props.sug.creationDate} UTC`}
                >
                    {this.props.sug.createdByName}
                </a>
            </td>
            <td>{this.renderDeleteButton()}</td>
            <td>{this.renderSuggestionActions()}</td>
        </tr>;
    }

    public renderDeleteButton(): JSX.Element | null {
        if (!this.props.user || this.props.sug.createdById !== this.props.user.id) {
            return null;
        }
        return <button type="button" className="btn btn-sm btn-danger" onClick={this.deleteReview} disabled={this.state.isButtonDisabled}>
            DELETE
        </button>;
    }

    public renderSuggestionActions(): JSX.Element | null {
        if (!this.props.user || !this.props.user.canReview) {
            return null;
        }

        if (this.props.sug.state === StringSuggestionState.ApprovedByTrustedUser &&
            this.props.user.userType === UserType.TrustedUser) {
            // a trusted user can't act on a suggestion approved by a trusted user
            return null;
        }

        return <div className="btn-group" role="group">
            <button type="button" className="btn btn-sm btn-success" onClick={this.acceptSuggestion} disabled={this.state.isButtonDisabled}>
                <i className="fas fa-thumbs-up" />
            </button>
            <button type="button" className="btn btn-sm btn-danger" onClick={this.rejectSuggestion} disabled={this.state.isButtonDisabled}>
                <i className="fas fa-thumbs-down" />
            </button>
        </div>;
    }

    @autobind()
    public async deleteReview() {
        this.setState({
            isButtonDisabled: true
        });
        try {
            await axios.delete(`/app/api/suggestions/${this.props.sug.id}`);
            this.props.refreshString(this.props.sug.stringId);
            this.setState({
                isButtonDisabled: false
            });
        } catch (e) {
            if (e.response.status === 400) {
                this.props.showErrorMessage("Error deleting the suggestion. Do you have enough rights?");
            } else {
                this.props.showErrorMessage(e.response.status);
            }
            this.setState({
                isButtonDisabled: false
            });
        }
    }

    @autobind()
    public acceptSuggestion() {
        this.processReview(ReviewAction.Accept);
    }

    @autobind()
    public rejectSuggestion() {
        this.processReview(ReviewAction.Reject);
    }

    private async processReview(action: ReviewAction) {
        this.setState({
            isButtonDisabled: true
        });
        try {
            await axios.put("/app/api/review", {
                Approve: action === ReviewAction.Accept,
                SuggestionId: this.props.sug.id
            });
            this.props.refreshString(this.props.sug.stringId);
            history.push("/filters");
        } catch (e) {
            if (e.response.status === 400) {
                this.props.showErrorMessage("Error reviewing the suggestion. Do you have enough rights?");
            } else {
                this.props.showErrorMessage(e.response.status);
            }
            this.setState({
                isButtonDisabled: false
            });
        }
    }
}
