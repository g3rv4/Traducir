import axios, { AxiosError } from "axios";
import * as React from "react";
import { Link } from "react-router-dom";
import history from "../../history";
import { StringSuggestionState } from "../../Models/SOStringSuggestion";
import IUserInfo from "../../Models/UserInfo";
import { UserType } from "../../Models/UserType";

export interface ISuggestionNewProps {
    user?: IUserInfo;
    stringId: number;
    rawString: boolean;
    refreshString: (stringIdToUpdate: number) => void;
    showErrorMessage: (messageOrCode: string | number) => void;
}

interface ISuggestionNewState {
    suggestion: string;
}

enum SuggestionCreationResult {
    CreationOk = 1,
    InvalidStringId = 2,
    SuggestionEqualsOriginal = 3,
    EmptySuggestion = 4,
    SuggestionAlreadyThere = 5,
    TooFewVariables = 6,
    TooManyVariables = 7,
    DatabaseError = 8
}

export default class SuggestionNew extends React.Component<ISuggestionNewProps, ISuggestionNewState> {
    constructor(props: ISuggestionNewProps) {
        super(props);

        this.state = {
            suggestion: ""
        };
    }

    public postSuggestion(approve: boolean) {
        axios.put("/app/api/suggestions", {
            Approve: approve,
            RawString: this.props.rawString,
            StringId: this.props.stringId,
            Suggestion: this.state.suggestion
        }).then(r => {
            this.props.refreshString(this.props.stringId);
            history.push("/filters");
        }).catch(e => {
            if (e.response.status === 400) {
                switch (e.response.data) {
                    case SuggestionCreationResult.DatabaseError:
                        this.props.showErrorMessage("A database error has ocurred, please try again.");
                        break;
                    case SuggestionCreationResult.EmptySuggestion:
                        this.props.showErrorMessage("You send an empty suggestion, please try to send a suggestion next time");
                        break;
                    case SuggestionCreationResult.InvalidStringId:
                        this.props.showErrorMessage("We couldn't find the id you send, did you need to refresh your page?");
                        break;
                    case SuggestionCreationResult.TooFewVariables:
                        this.props.showErrorMessage("Failed sending the suggestion. You are missing some variables");
                        break;
                    case SuggestionCreationResult.TooManyVariables:
                        this.props.showErrorMessage("Failed sending the suggestion. You have included unrecognized variables");
                        break;
                    case SuggestionCreationResult.SuggestionAlreadyThere:
                        this.props.showErrorMessage("The suggestion you are sending is already suggested. Maybe you need to refresh?");
                        break;
                    case SuggestionCreationResult.SuggestionEqualsOriginal:
                        this.props.showErrorMessage("The suggestion you are sending is the same as the actual translation");
                        break;
                    default:
                        this.props.showErrorMessage("The server encountered an error, but we don't know what happened");
                        break;
                }
            } else {
                this.props.showErrorMessage(e.response.status);
            }
        });
    }

    public render(): JSX.Element | null {
        if (!this.props.user || !this.props.user.canSuggest) {
            return null;
        }
        return <form>
            <div className="row">
                <div className="col">
                    <div className="form-group">
                        <label htmlFor="suggestion" className="font-weight-bold">New Suggestion</label>
                        <textarea className="form-control" id="suggestion"
                            value={this.state.suggestion}
                            onChange={e => this.setState({ suggestion: e.target.value })} />
                    </div>
                </div>
            </div>
            <div>
                <div className="mt-1">
                    <div className="btn-group" role="group">
                        <button type="button" className="btn btn-primary float-left"
                            onClick={e => this.postSuggestion(false)}>Send new suggestion</button>
                        {this.props.user.userType >= UserType.Reviewer &&
                            <button type="button" className="btn btn-warning float-left"
                                onClick={e => this.postSuggestion(true)}>Send final translation</button>}
                    </div>
                </div>
            </div>
        </form>;
    }
}
