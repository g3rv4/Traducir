import * as React from "react";
import * as _ from 'lodash';
import axios from 'axios';
import { Location } from 'history';
import history from '../../history'
import { stringify, parse } from 'query-string';
import { Redirect, Link } from 'react-router-dom';

import SOString from "./../../Models/SOString";

interface FiltersState {
    sourceRegex?: string;
    translationRegex?: string;
    key?: string;
    translationStatus?: TranslationStatus;
    suggestionsStatus?: SuggestionsStatus;
    pushStatus?: PushStatus;
}

export interface FiltersProps {
    onResultsFetched: (strings: SOString[]) => void;
    onLoading: () => void;
    goBackToResults: (stringIdToUpdate?: number) => void;
    showErrorMessage: (message?: string, code?: number) => void;
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

export default class Filters extends React.Component<FiltersProps, FiltersState> {
    constructor(props: FiltersProps) {
        super(props);
        this.state = this.getStateFromLocation(this.props.location);
        if (!this.hasFilter()) {
            history.replace('/');
            return;
        }
    }

    hasFilter = () => {
        return this.state.sourceRegex ||
            this.state.translationRegex ||
            this.state.key ||
            this.state.translationStatus ||
            this.state.suggestionsStatus ||
            this.state.pushStatus;
    }

    componentDidMount() {
        if (this.hasFilter()) {
            this.submitForm();
        }
    }

    componentWillReceiveProps(nextProps: FiltersProps, context: any) {
        if (this.props.location.search == nextProps.location.search ||
            this.props.location.pathname == '/filters') {
            return;
        }

        this.setState(this.getStateFromLocation(nextProps.location), this.submitForm);
    }

    getStateFromLocation(location: Location) {
        this.props.onLoading();
        const parts: FiltersState = parse(location.search);
        return {
            sourceRegex: parts.sourceRegex || "",
            translationRegex: parts.translationRegex || "",
            key: parts.key || "",
            translationStatus: parts.translationStatus || TranslationStatus.AnyStatus,
            suggestionsStatus: parts.suggestionsStatus || SuggestionsStatus.AnyStatus,
            pushStatus: parts.pushStatus || PushStatus.AnyStatus,
        }
    }

    handleField = (updatedState: FiltersState) => {
        this.setState(updatedState, () => {
            if (!this.hasFilter()) {
                history.replace('/');
                return;
            }
            this.submitForm();

            const newPath = '/filters?' + this.currentPath();
            if (location.pathname.startsWith('/filters')) {
                history.replace(newPath);
            } else {
                history.push(newPath);
            }
        });
    }

    reset = () => {
        this.setState({
            sourceRegex: "",
            translationRegex: "",
            key: "",
            translationStatus: TranslationStatus.AnyStatus,
            suggestionsStatus: SuggestionsStatus.AnyStatus,
            pushStatus: PushStatus.AnyStatus
        }, () => {
            this.props.onResultsFetched([]);
        })
    }

    submitForm = _.debounce(() => {
        this.props.onLoading();
        const _that = this;
        axios.post<SOString[]>('/app/api/strings/query', this.state)
            .then(function (response) {
                _that.props.onResultsFetched(response.data);
            })
            .catch(function (error) {
                _that.props.showErrorMessage(null, error.response.status);
            });
    }, 1000);

    currentPath = () => stringify(_.pickBy(this.state, e => e));

    render() {
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
                            onChange={e => this.handleField({ translationStatus: parseInt(e.target.value) })}
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
                            onChange={e => this.handleField({ suggestionsStatus: parseInt(e.target.value) })}
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
                        <label htmlFor="suggestionsStatus">Strings with pending push</label>
                        <select className="form-control" id="pushStatus"
                            value={this.state.pushStatus}
                            onChange={e => this.handleField({ pushStatus: parseInt(e.target.value) })}
                        >
                            <option value={PushStatus.AnyStatus}>Any string</option>
                            <option value={PushStatus.NeedsPush}>Needs push</option>
                            <option value={PushStatus.DoesNotNeedPush}>Is updated</option>
                        </select>
                    </div>
                </div>
            </div>
            <div className="row text-center mb-5">
                <div className="col">
                    <Link to='/' className="btn btn-secondary" onClick={this.reset}>Reset</Link>
                </div>
            </div>
            {location.pathname == '/filters' && location.search == '' && this.hasFilter() ?
                <Redirect
                    to={{
                        pathname: '/filters',
                        search: this.currentPath()
                    }} /> : null}
        </>
    }
}