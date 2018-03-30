import * as React from "react";
import SOString from "../../Models/SOString"
import UserInfo from "../../Models/UserInfo"
import Config from "../../Models/Config"
import SuggestionsTable from "./SuggestionsTable"

export interface SuggestionsProps {
    user: UserInfo;
    str: SOString;
    config: Config;
    goBackToResults: (stringIdToUpdate?: number) => void;
}

export default class Suggestions extends React.Component<SuggestionsProps, {}> {
    constructor(props: SuggestionsProps) {
        super(props);
    }
    render() {
        return <>
            <div className="m-2 text-center">
                <h2>Suggestions</h2>
            </div>
            <div>
                <span className="font-weight-bold">Key:</span> <pre className="d-inline">{this.props.str.key}</pre>
            </div>
            <div>
                <span className="font-weight-bold">Original String:</span> <pre className="d-inline">{this.props.str.originalString}</pre>
            </div>
            {this.props.str.variant ? <div>
                <span className="font-weight-bold">Variant:</span> {this.props.str.variant.replace('VARIANT: ', '')}
            </div> : null}
            <div>
                <span className="font-weight-bold">Current Translation:</span> {this.props.str.translation ?
                    <pre className="d-inline">{this.props.str.translation}</pre> :
                    <i>Missing translation</i>}
            </div>
            <SuggestionsTable 
                user={this.props.user}
                config={this.props.config}
                suggestions={this.props.str.suggestions}
                goBackToResults={this.props.goBackToResults} />
            <div className="float-right mt-1">
                <button type="button" className="btn btn-primary"
                    onClick={e=>this.props.goBackToResults(null)}>Go back</button>
            </div>
        </>
    }
}