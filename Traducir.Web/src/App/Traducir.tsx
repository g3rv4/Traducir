import * as React from "react";
import Filters from "./Components/Filters"
import SOString from "../Models/SOString";

export interface TraducirState
{
}

export default class Traducir extends React.Component<{}, TraducirState> {

    resultsReceived = (strings: SOString[]) => {
        console.log(strings);
    }

    render() {
        return <div><Filters onResultsFetched={this.resultsReceived} /></div>
    }
}