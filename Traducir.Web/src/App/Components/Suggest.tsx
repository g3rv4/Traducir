import * as React from "react";
import SOString from "../../Models/SOString"

export interface SuggestProps {
    str: SOString;
}

interface SuggestState {

}

export default class Suggest extends React.Component<SuggestProps, SuggestState> {
    render() {
        return <div>yes!</div>
    }
}