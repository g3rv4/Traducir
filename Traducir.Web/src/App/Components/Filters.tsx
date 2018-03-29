import * as React from "react";

interface FiltersState {
    sourceRegex?: string;
    translationRegex?: string;
    withoutTranslation?: boolean;
    SuggestionsStatus: SuggestionsStatus;
}

enum SuggestionsStatus {
    AnyStatus = 0,
    DoesNotHaveSuggestionsNeedingApproval = 1,
    HasSuggestionsNeedingApproval = 2,
    HasSuggestionsNeedingApprovalApprovedByTrustedUser = 3
}

export default class Filters extends React.Component<{}, FiltersState> {
    render() {
        return <form>
            <div className="m-2 text-center">
                <h2>Filters</h2>
            </div>
            <div className="row">
                <div className="col">
                    <div className="form-group">
                        <label htmlFor="formGroupExampleInput">Source Regex</label>
                        <input type="text" className="form-control" id="formGroupExampleInput" placeholder="^question" />
                    </div>
                </div>
                <div className="col">
                    <div className="form-group">
                        <label htmlFor="formGroupExampleInput2">Translation Regex</label>
                        <input type="text" className="form-control" id="formGroupExampleInput2" placeholder="(?i)pregunta$" />
                    </div>
                </div>
            </div>
            <div className="row">
                <div className="col">
                    <div className="form-group">
                        <label htmlFor="exampleFormControlSelect1">Strings without translation</label>
                        <select className="form-control" id="exampleFormControlSelect1">
                            <option value="">Any string</option>
                            <option value="1">Only strings without translation</option>
                            <option value="0">Only strings with translation</option>
                        </select>
                    </div>
                </div>
                <div className="col">
                    <div className="form-group">
                        <label htmlFor="exampleFormControlSelect1">Strings with pending suggestions</label>
                        <select className="form-control" id="exampleFormControlSelect1">
                            <option value={SuggestionsStatus.AnyStatus}>Any string</option>
                            <option value={SuggestionsStatus.HasSuggestionsNeedingApproval}>Strings with pending suggestions</option>
                            <option value={SuggestionsStatus.HasSuggestionsNeedingApprovalApprovedByTrustedUser}>Strings with pending suggestions approved by a trusted user</option>
                            <option value={SuggestionsStatus.DoesNotHaveSuggestionsNeedingApproval}>Strings without pending suggestions</option>
                        </select>
                    </div>
                </div>
            </div>
        </form>
    }
}