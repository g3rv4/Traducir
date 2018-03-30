import * as React from "react";
import axios, { AxiosError } from 'axios';
import SOString from "../../Models/SOString"
import UserInfo from "../../Models/UserInfo"
import Config from "../../Models/Config"
import SOStringSuggestion, { StringSuggestionState } from "../../Models/SOStringSuggestion"

export interface SuggestionsProps {
    user: UserInfo;
    str: SOString;
    config: Config;
    goBackToResults: (stringIdToUpdate?: number) => void;
}

interface SuggestionsState {
    aboutToReviewId?: number;
    actionToPerform?: ReviewAction;
}

enum ReviewAction {
    Accept = 1,
    Reject = 2
}

export default class Suggestions extends React.Component<SuggestionsProps, SuggestionsState> {
    constructor(props: SuggestionsProps) {
        super(props);

        this.state = {
            aboutToReviewId: null,
            actionToPerform: null
        }
    }
    processReview(sug: SOStringSuggestion) {
        if (!this.state.aboutToReviewId || !this.state.actionToPerform) {
            return;
        }

        const _that = this;
        axios.put('/app/api/review', {
            SuggestionId: this.state.aboutToReviewId,
            Approve: this.state.actionToPerform == ReviewAction.Accept
        }).then(r => _that.props.goBackToResults(sug.stringId))
    }
    renderSuggestionActions(sug: SOStringSuggestion): JSX.Element {
        if (!this.props.user || !this.props.user.canReview) {
            return null;
        }

        if (!this.state.actionToPerform) {
            return <div className="btn-group" role="group">
                <button type="button" className="btn btn-sm btn-success" onClick={e => this.setState({
                    actionToPerform: ReviewAction.Accept,
                    aboutToReviewId: sug.id
                })}>Approve</button>
                <button type="button" className="btn btn-sm btn-danger" onClick={e => this.setState({
                    actionToPerform: ReviewAction.Reject,
                    aboutToReviewId: sug.id
                })}>Reject</button>
            </div>;
        }

        if (this.state.aboutToReviewId != sug.id) {
            return null;
        }

        return <div className="text-center">
            <div>
                {this.state.actionToPerform == ReviewAction.Accept ? 'Approve' : 'Reject'} this suggestion?
            </div>
            <div className="btn-group" role="group">
                <button type="button" className="btn btn-sm btn-primary" onClick={e => this.processReview(sug)}>Yes</button>
                <button type="button" className="btn btn-sm btn-secondary" onClick={e => this.setState({
                    actionToPerform: null,
                    aboutToReviewId: null
                })}>No</button>
            </div>
        </div>
    }
    renderSuggestions() {
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
                {this.props.str.suggestions.map(sug =>
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
    render() {
        return <>
            <div className="m-2 text-center">
                <h2>Suggestions</h2>
            </div>
            <div>
                <span className="font-weight-bold">Original String:</span> <pre className="d-inline">{this.props.str.originalString}</pre>
            </div>
            {this.props.str.variant ? <div>
                <span className="font-weight-bold">Variant:</span> {this.props.str.variant.replace('VARIANT: ', '')}
            </div> : null}
            <div>
                <span className="font-weight-bold">Current Translation:</span> {this.props.str.translation ?
                    <pre className="d-inline">{this.props.str.translation}</pre> :
                    <i>Missing translation</i>}
            </div>
            {this.props.str.suggestions && this.props.str.suggestions.length > 0 ?
                this.renderSuggestions() :
                null}
            <div className="float-right mt-1">
                <button type="button" className="btn btn-primary"
                    onClick={e=>this.props.goBackToResults(null)}>Go back</button>
            </div>
        </>
    }
}