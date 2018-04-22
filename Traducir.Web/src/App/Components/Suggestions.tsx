import axios, { AxiosError } from "axios";
import * as React from "react";
import { Link } from "react-router-dom";
import history from "../../history";
import Config from "../../Models/Config";
import ISOString from "../../Models/SOString";
import IUserInfo from "../../Models/UserInfo";
import SuggestionNew from "./SuggestionNew";
import SuggestionsTable from "./SuggestionsTable";

export interface ISuggestionsProps {
    user?: IUserInfo;
    str: ISOString;
    config: Config;
    refreshString: (stringIdToUpdate: number) => void;
    showErrorMessage: (messageOrCode: string | number) => void;
}

export interface ISuggestionsState {
    rawString: boolean;
}

export default class Suggestions extends React.Component<ISuggestionsProps, ISuggestionsState> {
    constructor(props: ISuggestionsProps) {
        super(props);

        this.state = {
            rawString: false
        };

        this.onCheckboxChange = this.onCheckboxChange.bind(this);
    }
    public updateUrgency(isUrgent: boolean) {
        axios.put("/app/api/manage-urgency", {
            IsUrgent: isUrgent,
            StringId: this.props.str.id
        }).then(r => {
            if (this.props.str) {
                this.props.refreshString(this.props.str.id);
            }
            history.push("/filters");
        }).catch(e => {
            if (e.response.status === 400) {
                this.props.showErrorMessage("Failed updating the urgency... maybe a race condition?");
            } else {
                this.props.showErrorMessage(e.response.status);
            }
        });
    }
    public renderUrgency() {
        if (!this.props.user || !this.props.user.canSuggest) {
            return <span>{this.props.str.isUrgent ? "Yes" : "No"}</span>;
        }
        return this.props.str.isUrgent
            ? <span>Yes <a href="#" className="btn btn-sm btn-warning" onClick={e => this.updateUrgency(false)}>Mark as non urgent</a></span>
            : <span>No <a href="#" className="btn btn-sm btn-danger" onClick={e => this.updateUrgency(true)}>Mark as urgent</a></span>;
    }
    public onCheckboxChange() {
        this.setState({ rawString: !this.state.rawString });
    }

    public render() {
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
            {this.props.str.variant && <div>
                <span className="font-weight-bold">Variant:</span> {this.props.str.variant.replace("VARIANT: ", "")}
            </div>}
            <div>
                <span className="font-weight-bold">Current Translation:</span> {this.props.str.translation ?
                    <pre className="d-inline">{this.props.str.translation}</pre> :
                    <i>Missing translation</i>}
            </div>
            <SuggestionsTable
                user={this.props.user}
                config={this.props.config}
                suggestions={this.props.str.suggestions}
                refreshString={this.props.refreshString}
                showErrorMessage={this.props.showErrorMessage}
            />

            <SuggestionNew
                user={this.props.user}
                stringId={this.props.str.id}
                rawString={this.state.rawString}
                refreshString={this.props.refreshString}
                showErrorMessage={this.props.showErrorMessage}
            />
        </>;
    }
}
