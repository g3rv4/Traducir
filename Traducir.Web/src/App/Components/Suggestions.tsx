import * as React from "react";
import SOString from "../../Models/SOString"
import UserInfo from "../../Models/UserInfo"
import Config from "../../Models/Config"
import SOStringSuggestion, { StringSuggestionState } from "../../Models/SOStringSuggestion"

export interface SuggestionsProps {
    user: UserInfo;
    str: SOString;
    config: Config;
    goBackToResults: () => void;
}

interface SuggestionsState {

}

export default class Suggestions extends React.Component<SuggestionsProps, SuggestionsState> {
    renderSuggestionActions(sug: SOStringSuggestion) {
        if (!this.props.user || !this.props.user.canReview) {
            return <></>;
        }
    }
    renderSuggestions() {
        console.log(this.props.str.suggestions);
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
                <span className="font-weight-bold">Variant:</span> {this.props.str.variant}
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
                    onClick={this.props.goBackToResults}>Go back</button>
            </div>
        </>
    }
}