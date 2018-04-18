import * as React from "react";
import axios, { AxiosError } from 'axios';
import * as _ from 'lodash';
import {
    Navbar, NavbarBrand, NavbarToggler, Collapse, Nav, NavItem, NavLink,
    UncontrolledDropdown, DropdownToggle, DropdownMenu, DropdownItem,
    Modal, ModalHeader, ModalBody, ModalFooter
} from 'reactstrap';
import { Route, Switch, withRouter, RouteComponentProps, Link } from 'react-router-dom';
import history from '../history';
import Filters from "./Components/Filters";
import Results from "./Components/Results";
import Suggestions from "./Components/Suggestions";
import StatsWithLinks from "./Components/StatsWithLinks";
import Users from "./Components/Users";
import SOString from "../Models/SOString";
import UserInfo, { userTypeToString } from "../Models/UserInfo";
import Config from "../Models/Config";
import Stats from "../Models/Stats";

export interface TraducirState {
    user: UserInfo;
    strings: SOString[];
    currentString: SOString;
    config: Config;
    isOpen: boolean;
    isLoading: boolean;
    stats: Stats;
}

class Traducir extends React.Component<RouteComponentProps<{}>, TraducirState> {
    constructor(props: RouteComponentProps<{}>) {
        super(props);

        this.state = {
            user: undefined,
            strings: [],
            currentString: null,
            config: null,
            stats: null,
            isOpen: false,
            isLoading: false
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

    renderLogInLogOut() {
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

    refreshString = (stringIdToUpdate: number) => {
        const idx = _.findIndex(this.state.strings, s => s.id == stringIdToUpdate);
        const _that = this;
        axios.get<SOString>(`/app/api/strings/${stringIdToUpdate}`)
            .then(r => {
                r.data.touched = true;

                const newStrings = this.state.strings.slice();
                newStrings[idx] = r.data;

                this.setState({
                    strings: newStrings,
                    currentString: r.data
                });
                axios.get<Stats>('/app/api/strings/stats')
                    .then(response => _that.setState({ stats: response.data }))
                    .catch(error => this.showErrorMessage(null, error.response.status));
            })
    }

    resultsReceived = (strings: SOString[]) => {
        this.setState({
            strings,
            isLoading: false
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

    isOpen = () => {
        return this.props.location.pathname.startsWith('/string/')
    }

    onToggle = () => {
        history.push('/filters');
    }

    render() {
        return <>
            <Navbar color="dark" dark expand="lg" className="fixed-top">
                <div className="container">
                    <Link to='/' className="navbar-brand d-none d-lg-block">{this.state.config && this.state.config.friendlyName} Translations 🦄{this.state.user && ` ${this.state.user.name} (${userTypeToString(this.state.user.userType)})`}</Link>
                    <Link to='/' className='navbar-brand d-lg-none'>{this.state.config && this.state.config.friendlyName} Translations 🦄</Link> 
                    <NavbarToggler onClick={this.toggle} />
                    <Collapse isOpen={this.state.isOpen} navbar>
                        <Nav className="ml-auto" navbar>
                            <NavItem>
                                <NavLink href="https://github.com/g3rv4/Traducir" target="_blank">Source Code</NavLink>
                            </NavItem>
                            <UncontrolledDropdown nav inNavbar>
                                <DropdownToggle nav caret>
                                    Database
                                </DropdownToggle>
                                <DropdownMenu right>
                                    <DropdownItem>
                                        <a href="https://db.traducir.win" className="dropdown-item" target="_blank">Access the Database</a>
                                    </DropdownItem>
                                    <DropdownItem>
                                        <a href="https://github.com/g3rv4/Traducir/blob/master/docs/USING_REDASH.md" className="dropdown-item" target="_blank">Instructions</a>
                                    </DropdownItem>
                                </DropdownMenu>
                            </UncontrolledDropdown>
                            {this.state.user &&
                                <NavItem>
                                    <Link to="/users" className="nav-link">Users</Link>
                                </NavItem>
                            }
                            {this.renderLogInLogOut()}
                        </Nav>
                    </Collapse>
                </div>
            </Navbar>
            <div className="container">
                <Switch>
                    <Route path='/users' render={p=>
                        <Users
                            showErrorMessage={this.showErrorMessage}
                            currentUser={this.state.user}
                        />
                    } />
                    <Route render={p=>
                        <Filters
                            onResultsFetched={this.resultsReceived}
                            onLoading={() => this.setState({ isLoading: true })}
                            showErrorMessage={this.showErrorMessage}
                            location={p.location}
                        />} />
                        <Switch>
                            <Route path='/' exact render={p =>
                                this.state.stats &&
                                <StatsWithLinks stats={this.state.stats} />} />
                            {this.state.strings.length == 0 && <Route path='/string/' render={p =>
                                this.state.stats &&
                                <StatsWithLinks stats={this.state.stats} />} />}
                            <Route render={p =>
                                <Results
                                    user={this.state.user}
                                    results={this.state.strings}
                                    loadSuggestions={this.loadSuggestions}
                                    isLoading={this.state.isLoading} />
                            } />
                        </Switch>
                    }
                </Switch>
                <Modal isOpen={this.isOpen()} toggle={this.onToggle} className="w-95 mw-100">
                    <ModalHeader toggle={this.onToggle}>Suggestions</ModalHeader>
                    <ModalBody>
                        {this.state.currentString && <Suggestions
                            config={this.state.config}
                            user={this.state.user}
                            str={this.state.currentString}
                            refreshString={this.refreshString}
                            showErrorMessage={this.showErrorMessage}
                        />}
                    </ModalBody>
                </Modal>
            </div>
        </>
    }
}

export default withRouter(Traducir);