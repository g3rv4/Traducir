import axios, { AxiosError } from "axios";
import { autobind } from "core-decorators";
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
import IUserInfo from "../Models/UserInfo";
import { userTypeToString } from "../Models/UserType";
import Filters from "./Components/Filters";
import Results from "./Components/Results";
import StatsWithLinks from "./Components/StatsWithLinks";
import Suggestions from "./Components/Suggestions";
import SuggestionsHistory from "./Components/SuggestionsHistory";
import Users from "./Components/Users";
import { NonUndefinedReactNode } from "./NonUndefinedReactNode";

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
    }

    public render(): NonUndefinedReactNode {
        return <>
            <Navbar color="dark" dark expand="lg" className="fixed-top">
                <div className="container">
                    <Link to="/" className="navbar-brand d-none d-lg-block">{this.state.config && this.state.config.friendlyName} Translations 🦄{this.state.user && ` ${this.state.user.name} (${userTypeToString(this.state.user.userType)})`}</Link>
                    <Link to="/" className="navbar-brand d-lg-none">{this.state.config && this.state.config.friendlyName} Translations 🦄</Link>
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
                                <>
                                    <NavItem>
                                        <Link to="/users" className="nav-link">Users</Link>
                                    </NavItem>
                                    <NavItem>
                                        <Link to={`/suggestions/${this.state.user.id}`} className="nav-link">My Suggestions</Link>
                                    </NavItem>
                                </>
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
                        render={this.renderUsers}
                    />
                    <Route
                        path="/suggestions/:userId"
                        render={this.renderSuggestionsHistory}
                    />
                    <Route
                        render={this.renderHome}
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

    public async componentDidMount(): Promise<void> {
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

    public renderLogInLogOut(): React.ReactNode {
        const returnUrl = encodeURIComponent(location.pathname + location.search);
        return !this.state.user ?
            <NavItem>
                <NavLink href={`/app/login?returnUrl=${returnUrl}`}>Log in!</NavLink>
            </NavItem> :
            <NavItem>
                <NavLink href={`/app/logout?returnUrl=${returnUrl}`}>Log out</NavLink>
            </NavItem>;
    }

    @autobind()
    public renderUsers(): NonUndefinedReactNode {
        return this.state.config ?
            <Users
                showErrorMessage={this.showErrorMessage}
                currentUser={this.state.user}
                config={this.state.config}
            /> :
            null;
    }

    @autobind()
    public renderSuggestionsHistory(p: RouteComponentProps<any>): NonUndefinedReactNode {
        return this.state.config ?
            <SuggestionsHistory
                showErrorMessage={this.showErrorMessage}
                currentUser={this.state.user}
                location={p.location}
                config={this.state.config}
            /> :
            null;
    }

    @autobind()
    public renderHome(p: RouteComponentProps<any>): NonUndefinedReactNode {
        return <>
            <Filters
                onResultsFetched={this.resultsReceived}
                onLoading={this.handleLoading}
                showErrorMessage={this.showErrorMessage}
                location={p.location}
            />
            <Switch>
                <Route
                    path="/"
                    exact
                    render={this.renderStats}
                />
                {this.state.strings.length === 0 &&
                    <Route
                        path="/string/"
                        render={this.renderStats}
                    />}
                <Route
                    render={this.renderResults}
                />
            </Switch>
        </>;
    }

    @autobind()
    public renderStats(): NonUndefinedReactNode {
        return this.state.stats ?
            <StatsWithLinks stats={this.state.stats} /> :
            null;
    }

    @autobind()
    public renderResults(): NonUndefinedReactNode {
        return <Results
            results={this.state.strings}
            loadSuggestions={this.loadSuggestions}
            isLoading={this.state.isLoading}
            currentUser={this.state.user}
            updateIgnore={this.updateIgnore}
        />;
    }

    @autobind()
    public handleLoading(): void {
        this.setState({
            isLoading: true
        });
    }

    @autobind()
    public loadSuggestions(str: ISOString): void {
        this.setState({
            currentString: str
        });
    }

    @autobind()
    public async refreshString(stringIdToUpdate: number): Promise<void> {
        const r = await axios.get<ISOString>(`/app/api/strings/${stringIdToUpdate}`);
        await this.updateStrings([r.data]);
    }

    @autobind()
    public async refreshStringsByKey(keyToUpdate: string): Promise<void> {
        const r = await axios.get<ISOString[]>(`/app/api/strings/by-key/${keyToUpdate}`);
        await this.updateStrings(r.data);
    }

    @autobind()
    public resultsReceived(strings: ISOString[]): void {
        this.setState({
            isLoading: false,
            strings
        });
    }

    public showErrorMessage(messageOrCode: string | number): void {
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

    @autobind()
    public toggle(): void {
        this.setState({
            isOpen: !this.state.isOpen
        });
    }

    public isOpen(): boolean {
        return this.props.location.pathname.startsWith("/string/");
    }

    public onToggle(): void {
        history.push("/filters");
    }

    private async updateStrings(strs: ISOString[]): Promise<void> {
        const newStrings = this.state.strings.slice();

        for (const str of strs) {
            const idx = _.findIndex(newStrings, s => s.id === str.id);
            if (idx === -1) {
                continue;
            }

            str.touched = true;
            newStrings[idx] = str;
        }

        this.setState({
            currentString: strs.length === 1 ? strs[0] : undefined,
            strings: newStrings
        });

        try {
            const response = await axios.get<IStats>("/app/api/strings/stats");
            this.setState({ stats: response.data });
        } catch (error) {
            this.showErrorMessage(error.response.status);
        }
    }

    @autobind
    private async updateIgnore(str: ISOString, ignored: boolean): Promise<void> {
        try {
            await axios.put("/app/api/manage-ignore", {
                Ignored: ignored,
                StringId: str.id
            });
            this.refreshStringsByKey(str.familyKey);
        } catch (e) {
            if (e.response.status === 400) {
                this.showErrorMessage("Failed ignoring the string");
            } else {
                this.showErrorMessage(e.response.status);
            }
        }
    }
}

export default withRouter(Traducir);
