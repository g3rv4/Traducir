import axios, { AxiosError } from "axios";
import { autobind } from "core-decorators";
import * as React from "react";
import { Link } from "react-router-dom";
import history from "../../history";
import IConfig from "../../Models/Config";
import ISOString from "../../Models/SOString";
import IUserInfo from "../../Models/UserInfo";
import { NonUndefinedReactNode } from "../NonUndefinedReactNode";
import SuggestionNew from "./SuggestionNew";
import SuggestionsTable from "./SuggestionsTable";

export interface ISuggestionsProps {
    user?: IUserInfo;
    str: ISOString;
    config: IConfig;
    refreshString: (stringIdToUpdate: number) => void;
    showErrorMessage: (messageOrCode: string | number) => void;
}

export interface ISuggestionsState {
    rawString: boolean;
    suggested: string;
}

export default class Suggestions extends React.Component<ISuggestionsProps, ISuggestionsState> {
    constructor(props: ISuggestionsProps) {
        super(props);

        this.state = {
            rawString: false,
            suggested: ""
        };
    }

    public render(): NonUndefinedReactNode {
        return <>
            <div>
                <span className="font-weight-bold">Key: </span>
                <a href={`https://www.transifex.com/${this.props.config.transifexPath}/$?q=key%3A${this.props.str.key}`} target="_blank">{this.props.str.key}</a>
            </div>
            <div>
                <span className="font-weight-bold">This string needs a new translation ASAP: </span> {this.renderUrgency()}
            </div>
            {this.props.user && this.props.user.canReview && <div>
                <span className="font-weight-bold">Raw string?: </span> <input type="checkbox" checked={this.state.rawString} onChange={this.onCheckboxChange} />
            </div>}
            <div>
                <span className="font-weight-bold">Original String:</span> <pre className="d-inline">{this.props.str.originalString}</pre>
            </div>
            {this.props.user && <div>
                <button type="button" className="btn btn-sm btn-primary" onClick={this.copyOriginalString}>
                    Copy as suggestion
                </button>
            </div>}
            {this.props.str.variant && <div>
                <span className="font-weight-bold">Variant:</span> {this.props.str.variant.replace("VARIANT: ", "")}
            </div>}
            <div>
                <span className="font-weight-bold">Current Translation:</span> {this.props.str.translation ?
                    <pre className="d-inline">{this.props.str.translation}</pre> :
                    <i>Missing translation</i>}
            </div>
            {this.props.user && this.props.str.translation && < div >
                <button type="button" className="btn btn-sm btn-primary" onClick={this.copyTranslatedString}>
                    Copy as suggestion
                </button>
            </div>}
            <SuggestionsTable
                user={this.props.user}
                config={this.props.config}
                suggestions={this.props.str.suggestions}
                refreshString={this.props.refreshString}
                showErrorMessage={this.props.showErrorMessage}
                stringToReplace={this.state.suggested}
            />

            <SuggestionNew
                user={this.props.user}
                stringId={this.props.str.id}
                rawString={this.state.rawString}
                refreshString={this.props.refreshString}
                showErrorMessage={this.props.showErrorMessage}
                suggestion={this.state.suggested}
                hasNewSuggestion={this.hasNewSuggestion}
            />
        </>;
    }

    @autobind
    public hasNewSuggestion(NewSuggestion: string): void {
        this.setState({suggested: NewSuggestion});
    }

    public renderUrgency(): React.ReactNode {
        if (!this.props.user || !this.props.user.canSuggest) {
            return <span>{this.props.str.isUrgent ? "Yes" : "No"}</span>;
        }
        return this.props.str.isUrgent
            ? <span>Yes <a href="#" className="btn btn-sm btn-warning" onClick={this.setNonUrgent}>Mark as non urgent</a></span>
            : <span>No <a href="#" className="btn btn-sm btn-danger" onClick={this.setUrgent}>Mark as urgent</a></span>;
    }

    @autobind()
    public onCheckboxChange(): void {
        this.setState({ rawString: !this.state.rawString });
    }

    @autobind()
    public copyOriginalString(): void {
        this.setState({ suggested: this.props.str.originalString });
    }

    @autobind()
    public copyTranslatedString(): void {
        this.setState({ suggested: this.props.str.translation });
    }

    @autobind()
    public setUrgent(): void {
        this.updateUrgency(true);
    }

    @autobind()
    public setNonUrgent(): void {
        this.updateUrgency(false);
    }

    private async updateUrgency(isUrgent: boolean): Promise<void> {
        try {
            await axios.put("/app/api/manage-urgency", {
                IsUrgent: isUrgent,
                StringId: this.props.str.id
            });
            if (this.props.str) {
                this.props.refreshString(this.props.str.id);
            }
            history.push("/filters");
        } catch (e) {
            if (e.response.status === 400) {
                this.props.showErrorMessage("Failed updating the urgency... maybe a race condition?");
            } else {
                this.props.showErrorMessage(e.response.status);
            }
        }
    }
}
