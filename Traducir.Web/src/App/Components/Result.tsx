import { autobind } from "core-decorators";
import * as _ from "lodash";
import React = require("react");
import history from "../../history";
import ISOString from "../../Models/SOString";
import { StringSuggestionState } from "../../Models/SOStringSuggestion";

interface IResultProps {
    str: ISOString;
    loadSuggestions: (str: ISOString) => void;
}

export default class Result extends React.Component<IResultProps> {
    public render() {
        return <tr
            onClick={this.goToString}
            className={this.props.str.isUrgent ? "table-danger" : this.props.str.touched ? "table-success" : ""}
        >
            <td>{this.props.str.originalString}</td>
            <td>{this.props.str.translation}</td>
            <td>{this.renderSuggestions()}</td>
        </tr>;
    }

    private renderSuggestions(): React.ReactFragment | null {
        if (!this.props.str.suggestions || !this.props.str.suggestions.length) {
            return null;
        }

        const approved = _.filter(this.props.str.suggestions, s => s.state === StringSuggestionState.ApprovedByTrustedUser).length;
        const pending = _.filter(this.props.str.suggestions, s => s.state === StringSuggestionState.Created).length;

        return <>
            {approved > 0 && <span className="text-success">{approved}</span>}
            {approved > 0 && pending > 0 && <span> - </span>}
            {pending > 0 && <span className="text-danger">{pending}</span>}
        </>;
    }

    @autobind()
    private goToString() {
        this.props.loadSuggestions(this.props.str);
        history.push(`/string/${this.props.str.id}`);
    }
}
