import * as React from "react";
import Stats from "../../Models/Stats";
import { Link } from 'react-router-dom';

export interface StatsWithLinksProps {
    stats: Stats;
}

export default class StatsWithLinks extends React.Component<StatsWithLinksProps, {}> {
    render() {
        return <div className="row text-center">
        <div className="col d-none d-lg-block">
            <div className="btn-group" role="group" aria-label="Basic example">
                <button type="button" className="btn btn-outline-secondary" disabled>{this.props.stats.totalStrings} total strings</button>
                <Link to='/filters?translationStatus=2' className="btn btn-outline-danger">{this.props.stats.withoutTranslation} strings without translation</Link>
                <Link to='/filters?suggestionsStatus=3' className="btn btn-outline-primary">{this.props.stats.waitingApproval} suggestions awaiting approval</Link>
                <Link to='/filters?suggestionsStatus=4' className="btn btn-outline-success">{this.props.stats.waitingReview} approved suggestions awaiting review</Link>
            </div>
        </div>
        <div className="col d-sm-block d-lg-none">
            <div className="btn-group-vertical" role="group" aria-label="Basic example">
                <button type="button" className="btn btn-outline-secondary" disabled>{this.props.stats.totalStrings} total strings</button>
                <Link to='/filters?translationStatus=2' className="btn btn-outline-danger">{this.props.stats.withoutTranslation} strings without translation</Link>
                <Link to='/filters?suggestionsStatus=3' className="btn btn-outline-primary">{this.props.stats.waitingApproval} suggestions awaiting approval</Link>
                <Link to='/filters?suggestionsStatus=4' className="btn btn-outline-success">{this.props.stats.waitingReview} approved suggestions awaiting review</Link>
            </div>
        </div>
        </div>
    }
}