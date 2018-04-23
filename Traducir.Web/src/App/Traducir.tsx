import axios, { AxiosError } from "axios";
import * as _ from "lodash";
import * as React from "react";
import { Link, Route, RouteComponentProps, Switch, withRouter } from "react-router-dom";
import {
    Collapse, DropdownItem, DropdownMenu, DropdownToggle, Modal, ModalBody, ModalFooter,
    ModalHeader, Nav, Navbar, NavbarBrand,
    NavbarToggler, NavItem, NavLink, UncontrolledDropdown
} from "reactstrap";
import history from "../history";
import IConfig from "../Models/Config";
import ISOString from "../Models/SOString";
import IStats from "../Models/Stats";
import IUserInfo, { userTypeToString } from "../Models/UserInfo";
import Filters from "./Components/Filters";
import Results from "./Components/Results";
import StatsWithLinks from "./Components/StatsWithLinks";
import Suggestions from "./Components/Suggestions";
import Users from "./Components/Users";

export interface ITraducirState {
    user?: IUserInfo;
    strings: ISOString[];
    currentString?: ISOString;
    config?: IConfig;
    isOpen: boolean;
    isLoading: boolean;
    stats?: IStats;
}

class Traducir extends React.Component<RouteComponentProps<{}>, ITraducirState> {
    constructor(props: RouteComponentProps<{}>) {
        super(props);

        this.state = {
            isLoading: false,
            isOpen: false,
            strings: []
        };

        this.loadSuggestions = this.loadSuggestions.bind(this);
        this.resultsReceived = this.resultsReceived.bind(this);
        this.refreshString = this.refreshString.bind(this);
    }

    public async componentDidMount() {
        axios.post<IUserInfo>("/app/api/me")
            .then(response => this.setState({ user: response.data }))
            .catch(error => this.setState({ user: undefined }));
        axios.get<IConfig>("/app/api/config")
            .then(response => this.setState({ config: response.data }))
            .catch(error => this.showErrorMessage(error.response.status));
        axios.get<IStats>("/app/api/strings/stats")
            .then(response => this.setState({ stats: response.data }))
            .catch(error => this.showErrorMessage(error.response.status));

        const stringMatch = location.pathname.match(/^\/string\/([0-9]+)$/);
        if (stringMatch) {
            try {
                const r = await axios.get<ISOString>(`/app/api/strings/${stringMatch[1]}`);
                this.setState({
                    currentString: r.data
                });
            } catch (error) {
                this.showErrorMessage(error.response.status);
            }
        }
    }

    public renderLogInLogOut() {
        const returnUrl = encodeURIComponent(location.pathname + location.search);
        if (!this.state || !this.state.user) {
            return <NavItem>
                <NavLink href={`/app/login?returnUrl=${returnUrl}`}>Log in!</NavLink>
            </NavItem>;
        } else if (this.state.user) {
            return <NavItem>
                <NavLink href={`/app/logout?returnUrl=${returnUrl}`}>Log out</NavLink>
            </NavItem>;
        }
    }

    public loadSuggestions(str: ISOString) {
        this.setState({
            currentString: str
        });
    }

    public async refreshString(stringIdToUpdate: number) {
        const idx = _.findIndex(this.state.strings, s => s.id === stringIdToUpdate);
        const r = await axios.get<ISOString>(`/app/api/strings/${stringIdToUpdate}`);
        r.data.touched = true;

        const newStrings = this.state.strings.slice();
        newStrings[idx] = r.data;

        this.setState({
            currentString: r.data,
            strings: newStrings
        });

        try {
            const response = await axios.get<IStats>("/app/api/strings/stats");
            this.setState({ stats: response.data });
        } catch (error) {
            this.showErrorMessage(error.response.status);
        }
    }

    public resultsReceived(strings: ISOString[]) {
        this.setState({
            isLoading: false,
            strings
        });
    }

    public showErrorMessage(messageOrCode: string | number) {
        if (typeof (messageOrCode) === "string") {
            alert(messageOrCode);
        } else {
            if (messageOrCode === 401) {
                alert("Your session has expired... you will be redirected to the log in page");
                window.location.href = `/app/login?returnUrl=${encodeURIComponent(location.pathname + location.search)}`;
            } else {
                alert(`Unknown error. Code: ${messageOrCode}`);
            }
        }
    }

    public toggle() {
        this.setState({
            isOpen: !this.state.isOpen
        });
    }

    public isOpen() {
        return this.props.location.pathname.startsWith("/string/");
    }

    public onToggle() {
        history.push("/filters");
    }

    public render() {
        return <>
            <Navbar color="dark" dark expand="lg" className="fixed-top">
                <div className="container">
                    <Link to="/" className="navbar-brand d-none d-lg-block">{this.state.config && this.state.config.friendlyName} Translations 🦄{this.state.user && ` ${this.state.user.name} (${userTypeToString(this.state.user.userType)})`}</Link>
                    <Link to="/" className="navbar-brand d-lg-none">{this.state.config && this.state.config.friendlyName} Translations 🦄</Link>
                    <NavbarToggler onClick={e => this.toggle()} />
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
                    <Route
                        path="/users"
                        exact
                        render={p =>
                            this.state.config ?
                                <Users
                                    showErrorMessage={this.showErrorMessage}
                                    currentUser={this.state.user}
                                    config={this.state.config}
                                /> :
                                null
                        }
                    />
                    <Route
                        render={p => <>
                            <Filters
                                onResultsFetched={this.resultsReceived}
                                onLoading={() => this.setState({ isLoading: true })}
                                showErrorMessage={this.showErrorMessage}
                                location={p.location}
                            />
                            <Switch>
                                <Route
                                    path="/"
                                    exact
                                    render={q =>
                                        this.state.stats ?
                                            <StatsWithLinks stats={this.state.stats} /> :
                                            null
                                    }
                                />
                                {this.state.strings.length === 0 &&
                                    <Route
                                        path="/string/"
                                        render={q =>
                                            this.state.stats ?
                                                <StatsWithLinks stats={this.state.stats} /> :
                                                null
                                        }
                                    />}
                                <Route
                                    render={q =>
                                        <Results
                                            results={this.state.strings}
                                            loadSuggestions={this.loadSuggestions}
                                            isLoading={this.state.isLoading}
                                        />
                                    }
                                />
                            </Switch>
                        </>}
                    />
                </Switch>
                <Modal isOpen={this.isOpen()} toggle={this.onToggle} className="w-95 mw-100">
                    <ModalHeader toggle={this.onToggle}>Suggestions</ModalHeader>
                    <ModalBody>
                        {this.state.currentString && this.state.config &&
                            <Suggestions
                                config={this.state.config}
                                user={this.state.user}
                                str={this.state.currentString}
                                refreshString={this.refreshString}
                                showErrorMessage={this.showErrorMessage}
                            />}
                    </ModalBody>
                </Modal>
            </div>
        </>;
    }
}

export default withRouter(Traducir);
