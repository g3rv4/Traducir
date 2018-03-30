import * as React from "react";
import SOString from "../../Models/SOString"
import UserInfo from "../../Models/UserInfo"

export interface SuggestionsProps {
    user: UserInfo;
    str: SOString;
    goBackToResults: () => void;
}

interface SuggestionsState {

}

export default class Suggestions extends React.Component<SuggestionsProps, SuggestionsState> {
    render() {
        return <>
            <div className="m-2 text-center">
                <h2>Suggestions</h2>
            </div>
            <div>
                <span className="font-weight-bold">Original String:</span> <pre className="d-inline">{this.props.str.originalString}</pre>
            </div>
            {this.props.str.variant ? <div>
                <span className="font-weight-bold">Variant:</span> {this.props.str.variant}
            </div> : null}
            <div>
                <span className="font-weight-bold">Current Translation:</span> {this.props.str.translation ? 
                <pre className="d-inline">{this.props.str.translation}</pre> :
                <i>Missing translation</i>}
            </div>
            <div className="float-right mt-1">
            <button type="button" className="btn btn-primary"
                onClick={this.props.goBackToResults}>Go back</button>
                </div>
        </>
    }
}