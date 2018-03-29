import * as React from "react";
import Filters from "./Components/Filters"

export interface TraducirState
{
}

export default class Traducir extends React.Component<{}, TraducirState> {
    render() {
        return <div><Filters /></div>
    }
}