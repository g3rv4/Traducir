import * as React from "react";
import { Link } from "react-router-dom";
import ISOString from "../../Models/SOString";
import { StringSuggestionState } from "../../Models/SOStringSuggestion";
import IUserInfo from "../../Models/UserInfo";
import { NonUndefinedReactNode } from "../NonUndefinedReactNode";
import Result from "./Result";

export interface IResultsProps {
    results: ISOString[];
    currentUser?: IUserInfo;
    isLoading: boolean;
    loadSuggestions: (str: ISOString) => void;
    updateIgnore(str: ISOString, ignored: boolean): Promise<void>;
}

export default class Results extends React.Component<IResultsProps> {
    constructor(props: IResultsProps) {
        super(props);
    }

    public render(): NonUndefinedReactNode {
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

    public renderRows(strings: ISOString[]): React.ReactNode {
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
            {strings.map(str => <Result
                key={str.id}
                str={str}
                loadSuggestions={this.props.loadSuggestions}
                currentUser={this.props.currentUser}
                updateIgnore={this.props.updateIgnore}
            />)}
        </>;
    }
}
