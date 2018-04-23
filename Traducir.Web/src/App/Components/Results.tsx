import * as _ from "lodash";
import * as React from "react";
import { Link } from "react-router-dom";
import history from "../../history";
import ISOString from "../../Models/SOString";
import { StringSuggestionState } from "../../Models/SOStringSuggestion";

export interface IResultsProps {
    results: ISOString[];
    isLoading: boolean;
    loadSuggestions: (str: ISOString) => void;
}

export default class Results extends React.Component<IResultsProps> {
    constructor(props: IResultsProps) {
        super(props);
    }

    public renderSuggestions(str: ISOString): React.ReactFragment | null {
        if (!str.suggestions || !str.suggestions.length) {
            return null;
        }

        const approved = _.filter(str.suggestions, s => s.state === StringSuggestionState.ApprovedByTrustedUser).length;
        const pending = _.filter(str.suggestions, s => s.state === StringSuggestionState.Created).length;

        return <>
            {approved > 0 && <span className="text-success">{approved}</span>}
            {approved > 0 && pending > 0 && <span> - </span>}
            {pending > 0 && <span className="text-danger">{pending}</span>}
        </>;
    }

    public goToString(str: ISOString) {
        this.props.loadSuggestions(str);
        history.push(`/string/${str.id}`);
    }

    public renderRows(strings: ISOString[]): React.ReactFragment {
        if (this.props.isLoading) {
            return <tr>
                <td colSpan={3} className="text-center">Loading...</td>
            </tr>;
        }
        if (strings.length === 0) {
            return <tr>
                <td colSpan={3} className="text-center">No results (sad trombone)</td>
            </tr>;
        }
        return <>
            {strings.map(str => <tr
                key={str.id}
                onClick={e => this.goToString(str)}
                className={str.isUrgent ? "table-danger" : str.touched ? "table-success" : ""}
            >
                <td>{str.originalString}</td>
                <td>{str.translation}</td>
                <td>{this.renderSuggestions(str)}</td>
            </tr>)}
        </>;
    }

    public render() {
        return <>
            <div className="m-2 text-center">
                <h2>Results {this.props.results &&
                    this.props.results.length > 0 &&
                    !this.props.isLoading &&
                    `(${this.props.results.length})`}</h2>
            </div>
            <table className="table">
                <thead className="thead-light">
                    <tr>
                        <th>String</th>
                        <th>Translation</th>
                        <th>Suggestions</th>
                    </tr>
                </thead>
                <tbody>
                    {this.renderRows(this.props.results)}
                </tbody>
            </table>
        </>;
    }
}
