import axios, { AxiosError } from "axios";
import { autobind } from "core-decorators";
import * as _ from "lodash";
import React = require("react");
import history from "../../history";
import ISOString from "../../Models/SOString";
import { StringSuggestionState } from "../../Models/SOStringSuggestion";
import IUserInfo from "../../Models/UserInfo";
import { UserType } from "../../Models/UserType";
import { NonUndefinedReactNode } from "../NonUndefinedReactNode";

interface IResultProps {
    str: ISOString;
    currentUser?: IUserInfo;
    loadSuggestions: (str: ISOString) => void;
    updateIgnore(str: ISOString, ignored: boolean): Promise<void>;
}

export default class Result extends React.Component<IResultProps> {
    public render(): NonUndefinedReactNode {
        return <tr
            onClick={this.goToString}
            className={this.props.str.isUrgent ? "table-danger" : this.props.str.touched ? "table-success" : ""}
        >
            <td>{this.props.str.originalString}</td>
            <td>{this.renderTranslationColumn()}</td>
            <td>{this.renderSuggestions()}</td>
        </tr>;
    }

    private renderTranslationColumn(): React.ReactNode {
        if (this.props.str.translation) {
            return this.props.str.translation;
        }

        if (!this.props.currentUser) {
            return;
        }

        if (this.props.currentUser.userType < UserType.TrustedUser) {
            return;
        }

        if (this.props.str.isIgnored) {
            return <button type="button" className="btn btn-warning btn-sm" onClick={this.unIgnore}>Stop ignoring</button>;
        } else {
            return <button type="button" className="btn btn-warning btn-sm" onClick={this.ignore}>Ignore!</button>;
        }
    }

    @autobind()
    private ignore(e: React.MouseEvent<HTMLElement>): Promise<void> {
        e.stopPropagation();
        return this.props.updateIgnore(this.props.str, true);
    }

    @autobind()
    private unIgnore(e: React.MouseEvent<HTMLElement>): Promise<void> {
        e.stopPropagation();
        return this.props.updateIgnore(this.props.str, false);
    }

    private renderSuggestions(): React.ReactNode {
        if (!this.props.str.suggestions || !this.props.str.suggestions.length) {
            return;
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
    private goToString(): void {
        this.props.loadSuggestions(this.props.str);
        history.push(`/string/${this.props.str.id}`);
    }
}
