import * as React from "react";
import axios, { AxiosError } from 'axios';
import Filters from "./Components/Filters"
import Results from "./Components/Results"
import Suggestions from "./Components/Suggestions"
import SOString from "../Models/SOString"
import UserInfo, { UserType } from "../Models/UserInfo"

export interface TraducirState {
    user: UserInfo;
    strings: SOString[];
    action: StringActions;
    currentString: SOString;
}

export enum StringActions {
    None = 0,
    Suggestions = 1
}

export default class Traducir extends React.Component<{}, TraducirState> {
    constructor(props: any) {
        super(props);

        this.state = {
            user: undefined,
            strings: [],
            action: StringActions.None,
            currentString: null
        };
    }

    componentDidMount() {
        const _that = this;
        axios.post<UserInfo>('/app/api/me', this.state)
            .then(function (response) {
                _that.setState({
                    user: response.data
                });
            })
            .catch(function (error) {
                _that.setState({
                    user: null
                });
            });
    }

    renderUser() {
        if (this.state.user === null) {
            return <li className="nav-item">
                <a href="/app/login" className="nav-link">Anonymous - Log in!</a>
            </li>
        } else if (this.state.user) {
            let userType: string;

            switch (this.state.user.userType) {
                case UserType.Banned: {
                    userType = 'Banned';
                    break;
                }
                case UserType.User: {
                    userType = "User";
                    break;
                }
                case UserType.TrustedUser: {
                    userType = "Trusted User";
                    break;
                }
                case UserType.Reviewer: {
                    userType = "Reviewer";
                    break;
                }
            }

            return <>
                <li className="nav-item"><div className="navbar-text">
                    {this.state.user.name} ({userType}) -
                </div></li>
                <li>
                    <a href="/app/logout" className="nav-link">Log out</a>
                </li>
            </>
        }
    }

    loadSuggestions = (str: SOString) => {
        this.setState({
            action: StringActions.Suggestions,
            currentString: str
        });
    }

    renderBody() {
        if (this.state.action == StringActions.None) {
            return <Results
                user={this.state.user}
                results={this.state.strings}
                loadSuggestions={this.loadSuggestions} />
        } else if (this.state.action == StringActions.Suggestions) {
            return <Suggestions
                user={this.state.user}
                str={this.state.currentString}
                goBackToResults={()=>this.setState({action: StringActions.None})} />
        }
    }

    resultsReceived = (strings: SOString[]) => {
        this.setState({
            strings
        })
    }

    render() {
        return <>
            <nav className="navbar navbar-expand-lg navbar-dark bg-dark fixed-top">
                <div className="container">
                    <a className="navbar-brand" href="#">SOes Translations</a>
                    <div className="collapse navbar-collapse" id="navbarResponsive">
                        <ul className="navbar-nav ml-auto">
                            {this.renderUser()}
                        </ul>
                    </div>
                </div>
            </nav>
            <div className="container">
                <Filters onResultsFetched={this.resultsReceived} />
                {this.renderBody()}
            </div>
        </>
    }
}