import * as React from "react";
import * as _ from 'lodash';
import SOString from "../../Models/SOString"
import { StringSuggestionState } from "../../Models/SOStringSuggestion"

export interface ResultsProps {
    results: SOString[];
}

interface ResultsState {

}

export default class Results extends React.Component<ResultsProps, ResultsState> {
    constructor(props: ResultsProps) {
        super(props);
    }
    renderSuggestions(str: SOString): React.ReactFragment {
        if (str.suggestions == null || str.suggestions.length == 0) {
            return <></>
        }

        const approved = _.filter(str.suggestions, s => s.state == StringSuggestionState.ApprovedByTrustedUser).length;
        const pending = _.filter(str.suggestions, s => s.state == StringSuggestionState.Created).length;

        return <>
            {approved > 0 ? <span className="text-success">{approved}</span> : null}
            {approved > 0 && pending > 0 ? <span> - </span> : null}
            {pending > 0 ? <span className="text-danger">{pending}</span> : null}
        </>
    }
    renderActions(str: SOString): React.ReactFragment {
        const approved = _.filter(str.suggestions, s => s.state == StringSuggestionState.ApprovedByTrustedUser).length;
        const pending = _.filter(str.suggestions, s => s.state == StringSuggestionState.Created).length;

        return <div className="btn-group" role="group" aria-label="Basic example">
            <button type="button" className="btn btn-sm btn-primary">Suggest</button>
            {approved + pending > 0 ? <button type="button" className="btn btn-sm btn-warning">Review</button> : null}
        </div>
    }
    renderRows(strings: SOString[]): React.ReactFragment {
        if (strings.length == 0) {
            return <tr>
                <td colSpan={4} className="text-center">No results (sad trombone)</td>
            </tr>
        }
        return <>
            {strings.map(str => <tr key={str.id}>
                <td>{str.originalString}</td>
                <td>{str.translation}</td>
                <td>{this.renderSuggestions(str)}</td>
                <td>{this.renderActions(str)}</td>
            </tr>)}
        </>
    }
    render() {
        return <>
            <div className="m-2 text-center">
                <h2>Results</h2>
            </div>
            <table className="table table-hover">
                <thead className="thead-light">
                    <tr>
                        <th>String</th>
                        <th>Translation</th>
                        <th>Suggestions</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    {this.renderRows(this.props.results)}
                </tbody>
            </table>
        </>
    }
}