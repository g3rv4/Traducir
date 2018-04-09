import * as React from "react";
import axios, { AxiosError } from 'axios';
import * as _ from 'lodash';
import { Navbar, NavbarBrand, NavbarToggler, Collapse, Nav, NavItem, NavLink, UncontrolledDropdown, DropdownToggle, DropdownMenu, DropdownItem } from 'reactstrap';
import { Route, Switch } from 'react-router-dom'
import Filters from "./Components/Filters"
import Results from "./Components/Results"
import Suggestions from "./Components/Suggestions"
import StatsWithLinks from "./Components/StatsWithLinks"
import SOString from "../Models/SOString"
import UserInfo, { UserType, userTypeToString } from "../Models/UserInfo"
import Config from "../Models/Config"
import Stats from "../Models/Stats"

export interface TraducirState {
    user: UserInfo;
    strings: SOString[];
    currentString: SOString;
    config: Config;
    isOpen: boolean;
    stats: Stats;
}

export default class Traducir extends React.Component<{}, TraducirState> {
    constructor(props: any) {
        super(props);

        this.state = {
            user: undefined,
            strings: [],
            currentString: null,
            config: null,
            stats: null,
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
            .catch(error => this.showErrorMessage(null, error.response.status));
        axios.get<Stats>('/app/api/strings/stats')
            .then(response => _that.setState({ stats: response.data }))
            .catch(error => this.showErrorMessage(null, error.response.status));

        const stringMatch = location.pathname.match(/^\/string\/([0-9]+)$/)
        if (stringMatch) {
            axios.get<SOString>(`/app/api/strings/${stringMatch[1]}`)
                .then(r => {
                    this.setState({
                        currentString: r.data
                    });
                })
                .catch(error => this.showErrorMessage(null, error.response.status));
        }
    }

    renderUser() {
        const returnUrl = encodeURIComponent(location.pathname + location.search);
        if (!this.state || this.state.user === null) {
            return <NavItem>
                <NavLink href={`/app/login?returnUrl=${returnUrl}`}>Log in!</NavLink>
            </NavItem>
        } else if (this.state.user) {
            return <>
                <NavItem>
                    <NavLink href={`/app/logout?returnUrl=${returnUrl}`}>Log out</NavLink>
                </NavItem>
            </>
        }
    }

    loadSuggestions = (str: SOString) => {
        this.setState({
            currentString: str
        });
    }

    goBackToResults = (stringIdToUpdate?: number) => {
        if (stringIdToUpdate) {
            const idx = _.findIndex(this.state.strings, s => s.id == stringIdToUpdate);
            const _that = this;
            axios.get<SOString>(`/app/api/strings/${stringIdToUpdate}`)
                .then(r => {
                    let newState = {
                        strings: this.state.strings
                    }
                    newState.strings[idx] = r.data;

                    this.setState(newState);
                    axios.get<Stats>('/app/api/strings/stats')
                        .then(response => _that.setState({ stats: response.data }))
                        .catch(error => this.showErrorMessage(null, error.response.status));
                })
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
                window.location.href = `/app/login?returnUrl=${encodeURIComponent(location.pathname + location.search)}`;
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
                    <div className="navbar-brand">{this.state.config && this.state.config.friendlyName} Translations{this.state.user && ` | ${this.state.user.name} (${userTypeToString(this.state.user.userType)})`}</div>
                    <NavbarToggler onClick={this.toggle} className="mr-5" />
                    <Collapse isOpen={this.state.isOpen} navbar>
                        <Nav className="ml-auto" navbar>
                            <UncontrolledDropdown nav inNavbar>
                                <DropdownToggle nav caret>
                                    Database
                                </DropdownToggle>
                                <DropdownMenu right>
                                    <DropdownItem>
                                        <a href="https://db.traducir.win" className="dropdown-item" target="_blank">Access to the Database</a>
                                    </DropdownItem>
                                    <DropdownItem>
                                        <a href="https://github.com/g3rv4/Traducir/blob/master/docs/USING_REDASH.md" className="dropdown-item" target="_blank">Instructions</a>
                                    </DropdownItem>
                                </DropdownMenu>
                            </UncontrolledDropdown>
                            {this.renderUser()}
                        </Nav>
                    </Collapse>
                </div>
            </Navbar>
            <div className="container">
                <Route render={p =>
                    <Filters
                        onResultsFetched={this.resultsReceived}
                        goBackToResults={this.goBackToResults}
                        showErrorMessage={this.showErrorMessage}
                        location={p.location}
                    />} />
                <Switch>
                    <Route path='/filters' render={p =>
                        <Results
                            user={this.state.user}
                            results={this.state.strings}
                            loadSuggestions={this.loadSuggestions} />
                    } />
                    <Route path='/string' render={p =>
                        this.state.currentString ? <Suggestions
                            config={this.state.config}
                            user={this.state.user}
                            str={this.state.currentString}
                            goBackToResults={this.goBackToResults}
                            showErrorMessage={this.showErrorMessage} />
                            : null
                    } />
                    <Route render={p =>
                        this.state.stats ?
                            <StatsWithLinks stats={this.state.stats} />
                            : null
                    } />
                </Switch>
            </div>
        </>
    }
}