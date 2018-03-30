import * as React from "react";
import axios, { AxiosError } from 'axios';
import * as _ from 'lodash';
import Filters from "./Components/Filters"
import Results from "./Components/Results"
import Suggestions from "./Components/Suggestions"
import SOString from "../Models/SOString"
import UserInfo, { UserType, userTypeToString } from "../Models/UserInfo"
import Config from "../Models/Config"

export interface TraducirState {
    user: UserInfo;
    strings: SOString[];
    action: StringActions;
    currentString: SOString;
    config: Config;
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
            currentString: null,
            config: null
        };
    }

    componentDidMount() {
        const _that = this;
        axios.post<UserInfo>('/app/api/me', this.state)
            .then(response => _that.setState({ user: response.data }))
            .catch(error => _that.setState({ user: null }));
        axios.get<Config>('/app/api/config')
            .then(response => _that.setState({ config: response.data }))
    }

    renderUser() {
        if (!this.state || this.state.user === null) {
            return <li className="nav-item">
                <a href="/app/login" className="nav-link">Anonymous - Log in!</a>
            </li>
        } else if (this.state.user) {
            return <>
                <li className="nav-item"><div className="navbar-text">
                    {this.state.user.name} ({userTypeToString(this.state.user.userType)}) -
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

    goBackToResults = (stringIdToUpdate?: number) => {
        if (stringIdToUpdate) {
            const idx = _.findIndex(this.state.strings, s => s.id == stringIdToUpdate);
            const _that = this;
            axios.get<SOString>(`app/api/strings/${stringIdToUpdate}`)
                .then(r => {
                    let newState = {
                        action: StringActions.None,
                        strings: this.state.strings
                    }
                    newState.strings[idx] = r.data;

                    this.setState(newState);
                })
        } else {
            this.setState({ action: StringActions.None });
        }
    }

    renderBody() {
        if (this.state.action == StringActions.None) {
            return <Results
                user={this.state.user}
                results={this.state.strings}
                loadSuggestions={this.loadSuggestions} />
        } else if (this.state.action == StringActions.Suggestions) {
            return <Suggestions
                config={this.state.config}
                user={this.state.user}
                str={this.state.currentString}
                goBackToResults={this.goBackToResults} />
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
                    <a className="navbar-brand" href="#">{this.state.config && this.state.config.friendlyName} Translations</a>
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