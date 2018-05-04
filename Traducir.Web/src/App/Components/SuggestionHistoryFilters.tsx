import * as React from "react";
import { Link } from "react-router-dom";
import { StringSuggestionState } from "../../Models/SOStringSuggestion";
import { NonUndefinedReactNode } from "../NonUndefinedReactNode";

export interface ISuggestionHistoryFiltersProps {
    userid: number;
}

export default class Filters extends React.Component<ISuggestionHistoryFiltersProps> {
    public render(): NonUndefinedReactNode {
        return <div className="row text-center mt-3">
            <div className="col d-none d-lg-block">
                <div className="btn-group" role="group">
                    <Link to={`/suggestions/${this.props.userid}`} className="btn btn-outline-info nav-link mx-1">All</Link>
                    <Link to={`/suggestions/${this.props.userid}?state=${StringSuggestionState.Created}`} className="btn btn-outline-info nav-link mx-1">Created</Link>
                    <Link to={`/suggestions/${this.props.userid}?state=${StringSuggestionState.ApprovedByTrustedUser}`} className="btn btn-outline-info nav-link mx-1">Approved by a trusted user</Link>
                    <Link to={`/suggestions/${this.props.userid}?state=${StringSuggestionState.ApprovedByReviewer}`} className="btn btn-outline-info nav-link mx-1">Approved by a reviewer</Link>
                    <Link to={`/suggestions/${this.props.userid}?state=${StringSuggestionState.Rejected}`} className="btn btn-outline-info nav-link mx-1">Rejected</Link>
                    <Link to={`/suggestions/${this.props.userid}?state=${StringSuggestionState.DeletedByOwner}`} className="btn btn-outline-info nav-link mx-1">Deleted by owner</Link>
                    <Link to={`/suggestions/${this.props.userid}?state=${StringSuggestionState.DismissedByOtherString}`} className="btn btn-outline-info nav-link mx-1">Dismissed by other string</Link>
                </div>
            </div>
        </div>;
    }
}
