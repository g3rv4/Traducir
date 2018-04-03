import * as React from "react";
import * as _ from 'lodash';
import axios from 'axios';

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
    goBackToResults: (stringIdToUpdate?: number) => void;
    showErrorMessage: (message?: string, code?: number) => void;
}

enum SuggestionsStatus {
    AnyStatus = 0,
    DoesNotHaveSuggestionsNeedingApproval = 1,
    HasSuggestionsNeedingApproval = 2,
    HasSuggestionsNeedingApprovalApprovedByTrustedUser = 3
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

        this.state = {
            sourceRegex: "",
            translationRegex: "",
            translationStatus: TranslationStatus.AnyStatus,
            suggestionsStatus: SuggestionsStatus.AnyStatus
        };
    }

    handleField = (updatedState: FiltersState) => {
        this.setState(updatedState, () => this.submitForm());
    }

    reset = () => {
        this.setState({
            sourceRegex: "",
            translationRegex: "",
            translationStatus: TranslationStatus.AnyStatus,
            suggestionsStatus: SuggestionsStatus.AnyStatus
        }, () => {
            this.props.goBackToResults();
            this.props.onResultsFetched([]);
        })
    }

    submitForm = _.debounce(() => {
        const _that = this;
        axios.post<SOString[]>('/app/api/strings/query', this.state)
            .then(function (response) {
                _that.props.onResultsFetched(response.data);
            })
            .catch(function (error) {
                _that.props.showErrorMessage(null, error.response.status);
            });
    }, 1000);

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
                            <option value={SuggestionsStatus.HasSuggestionsNeedingApproval}>Strings with pending suggestions</option>
                            <option value={SuggestionsStatus.HasSuggestionsNeedingApprovalApprovedByTrustedUser}>Strings with pending suggestions approved by a trusted user</option>
                            <option value={SuggestionsStatus.DoesNotHaveSuggestionsNeedingApproval}>Strings without pending suggestions</option>
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
                    <button className="btn btn-secondary" onClick={this.reset}>Reset</button>
                </div>
            </div>
        </>
    }
}