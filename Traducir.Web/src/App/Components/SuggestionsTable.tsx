import * as React from "react";
import axios, { AxiosError } from 'axios';
import history from '../../history';
import SOStringSuggestion, { StringSuggestionState } from "../../Models/SOStringSuggestion"
import Config from "../../Models/Config"
import UserInfo, { UserType } from "../../Models/UserInfo"

export interface SuggestionsTableProps {
    suggestions: SOStringSuggestion[];
    config: Config;
    user: UserInfo;
    goBackToResults: (stringIdToUpdate?: number) => void;
    showErrorMessage: (message?: string, code?: number) => void;
}

interface SuggestionsTableState {
    isButtonDisabled?: boolean;
}

enum ReviewAction {
    Accept = 1,
    Reject = 2
}

export default class SuggestionsTable extends React.Component<SuggestionsTableProps, SuggestionsTableState> {
    constructor(props: SuggestionsTableProps) {
        super(props);
        this.state = {
            isButtonDisabled: false
        };
    }
    render(): JSX.Element {
        if (!this.props.suggestions || !this.props.suggestions.length || !this.props.config) {
            return null;
        }

        return <table className="table mt-2">
            <thead>
                <tr>
                    <th>Suggestion</th>
                    <th>Approved By</th>
                    <th>Created by</th>
                    <th></th>
                </tr>
            </thead>
            <tbody>
                {this.props.suggestions.map(sug =>
                    <tr key={sug.id} className={sug.state == StringSuggestionState.ApprovedByTrustedUser ? 'table-success' : ''}>
                        <td><pre>{sug.suggestion}</pre></td>
                        <td>{sug.lastStateUpdatedByName ?
                            <a href={`https://${this.props.config.siteDomain}/users/${sug.createdById}`}
                                target="_blank">{sug.lastStateUpdatedByName}</a>
                            : null}</td>
                        <td><a href={`https://${this.props.config.siteDomain}/users/${sug.createdById}`}
                            target="_blank"
                            title={'at ' + sug.creationDate + ' UTC'}>{sug.createdByName}</a></td>
                        <td>{this.renderSuggestionActions(sug)}</td>
                    </tr>)}
            </tbody>
        </table>
    }

    renderSuggestionActions(sug: SOStringSuggestion): JSX.Element {
        if (!this.props.user || !this.props.user.canReview) {
            return null;
        }

        if (sug.state == StringSuggestionState.ApprovedByTrustedUser &&
            this.props.user.userType == UserType.TrustedUser) {
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

    processReview(sug: SOStringSuggestion, action: ReviewAction) {
        this.setState({
            isButtonDisabled: true
        });
        const _that = this;
        axios.put('/app/api/review', {
            SuggestionId: sug.id,
            Approve: action == ReviewAction.Accept
        }).then(r => {
            _that.props.goBackToResults(sug.stringId);
            history.push('/filters');
        })
            .catch(e => {
                if (e.response.status == 400) {
                    _that.props.showErrorMessage("Error reviewing the suggestion. Do you have enough rights?");
                } else {
                    _that.props.showErrorMessage(null, e.response.status);
                }
                this.setState({
                    isButtonDisabled: false
                });
            });
    }
}