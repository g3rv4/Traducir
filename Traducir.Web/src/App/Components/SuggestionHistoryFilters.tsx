import * as React from "react";
import { Link } from "react-router-dom";
import { NonUndefinedReactNode } from "../NonUndefinedReactNode";

export interface ISuggestionHistoryFiltersProps {
    userid: number;
}

export default class Filters extends React.Component<ISuggestionHistoryFiltersProps> {
    public render(): NonUndefinedReactNode {
        return <div className="row text-center mt-3">
            <div className="col d-none d-lg-block">
                <div className="btn-group" role="group" aria-label="Basic example">
                    <Link to={"/suggestions/" + this.props.userid} className="btn btn-info">All</Link>
                    <Link to={"/suggestions/" + this.props.userid + "?stateId=1"} className="btn btn-info">Created</Link>
                    <Link to={"/suggestions/" + this.props.userid + "?stateId=2"} className="btn btn-info">ApprovedByTrustedUser</Link>
                    <Link to={"/suggestions/" + this.props.userid + "?stateId=3"} className="btn btn-info">ApprovedByReviewer</Link>
                    <Link to={"/suggestions/" + this.props.userid + "?stateId=4"} className="btn btn-info">Rejected</Link>
                    <Link to={"/suggestions/" + this.props.userid + "?stateId=5"} className="btn btn-info">DeletedByOwner</Link>
                    <Link to={"/suggestions/" + this.props.userid + "?stateId=6"} className="btn btn-info">DismissedByOtherString</Link>
                </div>
            </div>
        </div>;
    }
}
