import * as React from "react";
import Filters from "./Components/Filters"
import Results from "./Components/Results"
import SOString from "../Models/SOString";

export interface TraducirState
{
    strings: SOString[];
}

export default class Traducir extends React.Component<{}, TraducirState> {
    constructor(props: any){
        super(props);

        this.state = {
            strings: []
        };
    }
    resultsReceived = (strings: SOString[]) => {
        console.log(strings);
        this.setState({
            strings
        })
    }

    render() {
        return <>
            <Filters onResultsFetched={this.resultsReceived} />
            <Results results={this.state.strings} />
        </>
    }
}