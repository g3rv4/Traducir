import * as React from "react";
import axios, { AxiosError } from 'axios';
import * as _ from 'lodash';
import { Navbar, NavbarBrand, NavbarToggler, Collapse, Nav, NavItem } from 'reactstrap';
import { Route } from 'react-router-dom'
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
    isOpen: boolean;
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
            config: null,
            isOpen: false
        };
    }

    componentDidMount() {
        const _that = this;
        axios.post<UserInfo>('/app/api/me', this.state)
            .then(response => _that.setState({ user: response.data }))
            .catch(error => _that.setState({ user: null }));
        axios.get<Config>('/app/api/config')
            .then(response => _that.setState({ config: response.data }))
            .catch(error => this.showErrorMessage(null, error.response.status ));
    }

    renderUser() {
        if (!this.state || this.state.user === null) {
            return <NavItem>
                <a href="/app/login" className="nav-link">Anonymous - Log in!</a>
            </NavItem>
        } else if (this.state.user) {
            return <>
                <NavItem className="navbar-text">
                    {this.state.user.name} ({userTypeToString(this.state.user.userType)}) - <a href="/app/logout">Log out</a>
                </NavItem>
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
                goBackToResults={this.goBackToResults}
                showErrorMessage={this.showErrorMessage} />
        }
    }

    resultsReceived = (strings: SOString[]) => {
        this.setState({
            strings
        })
    }

    showErrorMessage = (message?: string, code?: number) => {
        if (message) {
            alert(message);
        } else {
            if (code == 401) {
                alert('Your session has expired... you will be redirected to the log in page');
                window.location.href = '/app/login';
            } else {
                alert('Unknown error. Code: ' + code);
            }
        }
    }
    toggle = () => {
        this.setState({
            isOpen: !this.state.isOpen
        });
    }

    render() {
        return <>
            <Navbar color="dark" dark expand="lg" className="fixed-top">
                <div className="container">
                    <div className="navbar-brand">{this.state.config && this.state.config.friendlyName} Translations</div>
                    <NavbarToggler onClick={this.toggle} className="mr-5" />
                    <Collapse isOpen={this.state.isOpen} navbar>
                        <Nav className="ml-auto" navbar>
                            {this.renderUser()}
                        </Nav>
                    </Collapse>
                </div>
            </Navbar>
            <div className="container">
                <Filters
                    onResultsFetched={this.resultsReceived}
                    goBackToResults={this.goBackToResults}
                    showErrorMessage={this.showErrorMessage}
                />
                <Route path='/filter' render={p =>
                    <Results
                        user={this.state.user}
                        results={this.state.strings}
                        loadSuggestions={this.loadSuggestions} />
                } />
            </div>
        </>
    }
}