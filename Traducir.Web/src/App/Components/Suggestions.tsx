import * as React from "react";
import { Link } from 'react-router-dom';
import axios, { AxiosError } from 'axios';
import history from '../../history';
import SOString from "../../Models/SOString"
import UserInfo from "../../Models/UserInfo"
import Config from "../../Models/Config"
import SuggestionsTable from "./SuggestionsTable"
import SuggestionNew from "./SuggestionNew"

export interface SuggestionsProps {
    user: UserInfo;
    str: SOString;
    config: Config;
    refreshString: (stringIdToUpdate: number) => void;
    showErrorMessage: (message?: string, code?: number) => void;
}

export interface SuggestionsState {
    rawString: boolean;
}

export default class Suggestions extends React.Component<SuggestionsProps, SuggestionsState> {
    constructor(props: SuggestionsProps) {
        super(props);

        this.state = {
            rawString: false
        };
    }
    updateUrgency = (isUrgent: boolean) => {
        axios.put('/app/api/manage-urgency', {
            StringId: this.props.str.id,
            IsUrgent: isUrgent
        }).then(r => {
            this.props.refreshString(this.props.str.id);
            history.push('/filters');
        })
            .catch(e => {
                if (e.response.status == 400) {
                    this.props.showErrorMessage("Failed updating the urgency... maybe a race condition?");
                } else {
                    this.props.showErrorMessage(null, e.response.status);
                }
            });
    }
    renderUrgency = () => {
        if(!this.props.user || !this.props.user.canSuggest){
            return <span>{this.props.str.isUrgent ? 'Yes' : 'No'}</span>
        }
        return this.props.str.isUrgent
            ? <span>Yes <a href="#" className="btn btn-sm btn-warning" onClick={e => this.updateUrgency(false)}>Mark as non urgent</a></span>
            : <span>No <a href="#" className="btn btn-sm btn-danger" onClick={e => this.updateUrgency(true)}>Mark as urgent</a></span>
    }
    onCheckboxChange = () => {
        this.setState({ rawString: !this.state.rawString });
    }

    render() {
        return <>
            <div>
                <span className="font-weight-bold">Key: </span>
                {this.props.config
                    ? <a href={`https://www.transifex.com/${this.props.config.transifexPath}/$?q=key%3A${this.props.str.key}`} target="_blank">{this.props.str.key}</a>
                    : <pre className="d-inline">{this.props.str.key}</pre>}
            </div>
            <div>
                <span className="font-weight-bold">This string needs a new translation ASAP: </span> {this.renderUrgency()}
            </div>
            {this.props.user.canReview && <div>
                <span className="font-weight-bold">Raw string?: </span> <input type="checkbox" checked={this.state.rawString} onChange={this.onCheckboxChange} />
            </div>}
            <div>
                <span className="font-weight-bold">Original String:</span> <pre className="d-inline">{this.props.str.originalString}</pre>
            </div>
            {this.props.str.variant && <div>
                <span className="font-weight-bold">Variant:</span> {this.props.str.variant.replace('VARIANT: ', '')}
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
        </>
    }
}