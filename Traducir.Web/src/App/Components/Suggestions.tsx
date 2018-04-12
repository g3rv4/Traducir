import * as React from "react";
import { Link } from 'react-router-dom';
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
    goBackToResults: (stringIdToUpdate?: number) => void;
    showErrorMessage: (message?: string, code?: number) => void;
}

export default class Suggestions extends React.Component<SuggestionsProps, {}> {
    constructor(props: SuggestionsProps) {
        super(props);
    }
    render() {
        return <>
            <div>
                <span className="font-weight-bold">Key:</span> <pre className="d-inline">{this.props.str.key}</pre>
            </div>
            {this.props.config ? <div>
                <span className="font-weight-bold">Transifex:</span> <a href={`https://www.transifex.com/${this.props.config.transifexPath}/$?q=key%3A${this.props.str.key}`} target="_blank">View it on Transifex</a>
            </div>
                : null}
            <div>
                <span className="font-weight-bold">Original String:</span> <pre className="d-inline">{this.props.str.originalString}</pre>
            </div>
            {this.props.str.variant ? <div>
                <span className="font-weight-bold">Variant:</span> {this.props.str.variant.replace('VARIANT: ', '')}
            </div>
                : null}
            <div>
                <span className="font-weight-bold">Current Translation:</span> {this.props.str.translation ?
                    <pre className="d-inline">{this.props.str.translation}</pre> :
                    <i>Missing translation</i>}
            </div>
            <SuggestionsTable
                user={this.props.user}
                config={this.props.config}
                suggestions={this.props.str.suggestions}
                goBackToResults={this.props.goBackToResults}
                showErrorMessage={this.props.showErrorMessage}
            />

            <SuggestionNew
                user={this.props.user}
                stringId={this.props.str.id}
                goBackToResults={this.props.goBackToResults}
                showErrorMessage={this.props.showErrorMessage}
            />
        </>
    }
}