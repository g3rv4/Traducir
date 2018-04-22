import axios from "axios";
import { Location } from "history";
import * as _ from "lodash";
import { parse, stringify } from "query-string";
import * as React from "react";
import { Link, Redirect } from "react-router-dom";
import history from "../../history";

import ISOString from "./../../Models/SOString";

interface IFiltersState {
    sourceRegex?: string;
    translationRegex?: string;
    key?: string;
    translationStatus?: TranslationStatus;
    suggestionsStatus?: SuggestionsStatus;
    pushStatus?: PushStatus;
    urgencyStatus?: UrgencyStatus;
    hasError?: boolean;
}

export interface IFiltersProps {
    onResultsFetched: (strings: ISOString[]) => void;
    onLoading: () => void;
    showErrorMessage: (messageOrCode: string | number) => void;
    location: Location;
}

enum SuggestionsStatus {
    AnyStatus = 0,
    DoesNotHaveSuggestions = 1,
    HasSuggestionsNeedingReview = 2,
    HasSuggestionsNeedingApproval = 3,
    HasSuggestionsNeedingReviewApprovedByTrustedUser = 4
}

enum TranslationStatus {
    AnyStatus = 0,
    WithTranslation = 1,
    WithoutTranslation = 2
}

enum PushStatus {
    AnyStatus = 0,
    NeedsPush = 1,
    DoesNotNeedPush = 2
}

enum UrgencyStatus {
    AnyStatus = 0,
    IsUrgent = 1,
    IsNotUrgent = 2
}

export default class Filters extends React.Component<IFiltersProps, IFiltersState> {
    public submitForm = _.debounce(() => {
        this.props.onLoading();
        axios.post<ISOString[]>("/app/api/strings/query", this.state)
            .then(response => {
                this.setState({ hasError: false });
                this.props.onResultsFetched(response.data);
            })
            .catch(error => {
                if (error.response.status === 400) {
                    this.setState({ hasError: true });
                    this.props.onResultsFetched([]);
                } else {
                    this.props.showErrorMessage(error.response.status);
                }
            });
    }, 1000);

    constructor(props: IFiltersProps) {
        super(props);
        this.state = this.getStateFromLocation(this.props.location);
        if (!this.hasFilter() && !props.location.pathname.startsWith("/string")) {
            history.replace("/");
            return;
        }
    }

    public hasFilter() {
        return this.state.sourceRegex ||
            this.state.translationRegex ||
            this.state.key ||
            this.state.translationStatus ||
            this.state.suggestionsStatus ||
            this.state.pushStatus ||
            this.state.urgencyStatus;
    }

    public componentDidMount() {
        if (this.hasFilter()) {
            this.submitForm();
        }
    }

    public componentWillReceiveProps(nextProps: IFiltersProps, context: any) {
        if (nextProps.location.pathname === "/filters" &&
            !nextProps.location.search &&
            !this.hasFilter()) {
            history.replace("/");
            return;
        }
        if (this.props.location.search === nextProps.location.search ||
            this.props.location.pathname === "/filters") {
            return;
        }

        this.setState(this.getStateFromLocation(nextProps.location), () => {
            if (!this.hasFilter() && !nextProps.location.pathname.startsWith("/string")) {
                history.replace("/");
                return;
            }
            this.submitForm();
        });
    }

    public getStateFromLocation(location: Location) {
        this.props.onLoading();
        const parts: IFiltersState = parse(location.search);
        return {
            key: parts.key || "",
            pushStatus: parts.pushStatus || PushStatus.AnyStatus,
            sourceRegex: parts.sourceRegex || "",
            suggestionsStatus: parts.suggestionsStatus || SuggestionsStatus.AnyStatus,
            translationRegex: parts.translationRegex || "",
            translationStatus: parts.translationStatus || TranslationStatus.AnyStatus,
            urgencyStatus: parts.urgencyStatus || UrgencyStatus.AnyStatus
        };
    }

    public handleField(updatedState: IFiltersState) {
        this.setState({ ...updatedState, hasError: false }, () => {
            if (!this.hasFilter()) {
                history.replace("/");
                return;
            }
            this.submitForm();

            const newPath = "/filters?" + this.currentPath();
            if (location.pathname.startsWith("/filters")) {
                history.replace(newPath);
            } else {
                history.push(newPath);
            }
        });
    }

    public reset() {
        this.setState({
            key: "",
            pushStatus: PushStatus.AnyStatus,
            sourceRegex: "",
            suggestionsStatus: SuggestionsStatus.AnyStatus,
            translationRegex: "",
            translationStatus: TranslationStatus.AnyStatus,
            urgencyStatus: UrgencyStatus.AnyStatus
        }, () => {
            this.props.onResultsFetched([]);
        });
    }

    public currentPath() {
        return stringify(_.pickBy(this.state, e => e));
    }

    public render() {
        return <>
            <div className="m-2 text-center">
                <h2>Filters</h2>
            </div>
            <div className="row">
                <div className="col">
                    <div className="form-group">
                        <label htmlFor="sourceRegex">Source Regex</label>
                        <input type="text" className="form-control" id="sourceRegex" placeholder="^question"
                            value={this.state.sourceRegex}
                            onChange={e => this.handleField({ sourceRegex: e.target.value })} />
                    </div>
                </div>
                <div className="col">
                    <div className="form-group">
                        <label htmlFor="translationRegex">Translation Regex</label>
                        <input type="text" className="form-control" id="translationRegex" placeholder="(?i)pregunta$"
                            value={this.state.translationRegex}
                            onChange={e => this.handleField({ translationRegex: e.target.value })} />
                    </div>
                </div>
            </div>
            <div className="row">
                <div className="col">
                    <div className="form-group">
                        <label htmlFor="withoutTranslation">Strings without translation</label>
                        <select className="form-control" id="withoutTranslation"
                            value={this.state.translationStatus}
                            onChange={e => this.handleField({ translationStatus: parseInt(e.target.value, 10) })}
                        >
                            <option value={TranslationStatus.AnyStatus}>Any string</option>
                            <option value={TranslationStatus.WithoutTranslation}>Only strings without translation</option>
                            <option value={TranslationStatus.WithTranslation}>Only strings with translation</option>
                        </select>
                    </div>
                </div>
                <div className="col">
                    <div className="form-group">
                        <label htmlFor="suggestionsStatus">Strings with pending suggestions</label>
                        <select className="form-control" id="suggestionsStatus"
                            value={this.state.suggestionsStatus}
                            onChange={e => this.handleField({ suggestionsStatus: parseInt(e.target.value, 10) })}
                        >
                            <option value={SuggestionsStatus.AnyStatus}>Any string</option>
                            <option value={SuggestionsStatus.DoesNotHaveSuggestions}>Strings without suggestions</option>
                            <option value={SuggestionsStatus.HasSuggestionsNeedingApproval}>Strings with suggestions awaiting approval</option>
                            <option value={SuggestionsStatus.HasSuggestionsNeedingReview}>Strings with suggestions awaiting review</option>
                            <option value={SuggestionsStatus.HasSuggestionsNeedingReviewApprovedByTrustedUser}>Strings with approved suggestions awaiting review</option>
                        </select>
                    </div>
                </div>
            </div>
            <div className="row">
                <div className="col">
                    <div className="form-group">
                        <label htmlFor="key">Key</label>
                        <input type="text" className="form-control" id="key"
                            value={this.state.key}
                            onChange={e => this.handleField({ key: e.target.value })} />
                    </div>
                </div>
                <div className="col">
                    <div className="form-group">
                        <label htmlFor="suggestionsStatus">Strings with urgency status</label>
                        <select className="form-control" id="urgencyStatus"
                            value={this.state.urgencyStatus}
                            onChange={e => this.handleField({ urgencyStatus: parseInt(e.target.value, 10) })}
                        >
                            <option value={UrgencyStatus.AnyStatus}>Any string</option>
                            <option value={UrgencyStatus.IsUrgent}>Is urgent</option>
                            <option value={UrgencyStatus.IsNotUrgent}>Is not urgent</option>
                        </select>
                    </div>
                </div>
            </div>
            {this.state.hasError &&
                <div className="row">
                    <div className="col">
                        <div className="alert alert-danger" role="alert">
                            Error when performing the filter... are the regular expressions ok?
                    </div>
                    </div>
                </div>
            }
            <div className="row text-center mb-5">
                <div className="col">
                    <Link to="/" className="btn btn-secondary" onClick={e => this.reset()}>Reset</Link>
                </div>
            </div>
            {location.pathname === "/filters" && location.search === "" && this.hasFilter() &&
                <Redirect
                    to={{
                        pathname: "/filters",
                        search: this.currentPath()
                    }} />}
        </>;
    }
}
