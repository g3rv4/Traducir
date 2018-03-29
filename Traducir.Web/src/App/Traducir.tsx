import * as React from "react";
import Filters from "./Components/Filters"
import Results from "./Components/Results"
import Suggest from "./Components/Suggest"
import SOString from "../Models/SOString"

export interface TraducirState {
    strings: SOString[];
    action: StringActions;
    currentString: SOString;
}

export enum StringActions {
    None = 0,
    Suggest = 1,
    Review = 2
}

export default class Traducir extends React.Component<{}, TraducirState> {
    constructor(props: any) {
        super(props);

        this.state = {
            strings: [],
            action: StringActions.None,
            currentString: null
        };
    }

    makeSuggestion = (str: SOString) => {
        this.setState({
            action: StringActions.Suggest,
            currentString: str
        });
    }

    renderBody() {
        if (this.state.action == StringActions.None) {
            return <Results results={this.state.strings} makeSuggestion={this.makeSuggestion} />
        } else if (this.state.action == StringActions.Suggest) {
            return <Suggest str={this.state.currentString} />
        }
    }

    resultsReceived = (strings: SOString[]) => {
        this.setState({
            strings
        })
    }

    render() {
        return <>
            <Filters onResultsFetched={this.resultsReceived} />
            {this.renderBody()}
        </>
    }
}