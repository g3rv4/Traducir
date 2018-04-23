import axios, { AxiosError } from "axios";
import * as React from "react";
import history from "../../history";
import IConfig from "../../Models/Config";
import IUser from "../../Models/User";
import IUserInfo from "../../Models/UserInfo";
import { UserType, userTypeToString } from "../../Models/UserType";

export interface IUsersProps {
    showErrorMessage: (messageOrCode: string | number) => void;
    currentUser?: IUserInfo;
    config: IConfig;
}

interface IUsersState {
    users: IUser[];
}

export default class Users extends React.Component<IUsersProps, IUsersState> {
    constructor(props: IUsersProps) {
        super(props);

        this.state = {
            users: []
        };
    }

    public componentDidMount() {
        this.refreshUsers();
    }

    public async refreshUsers() {
        try {
            const r = await axios.get<IUser[]>("/app/api/users");
            this.setState({
                users: r.data
            });
        } catch (e) {
            if (e.response.status === 401) {
                history.push("/");
            } else {
                this.props.showErrorMessage(e.response.status);
            }
        }
    }

    public async updateUserType(user: IUser, newType: UserType) {
        try {
            await axios.put("/app/api/users/change-type", {
                UserId: user.id,
                UserType: newType
            });
            this.refreshUsers();
        } catch (e) {
            if (e.response.status === 400) {
                this.props.showErrorMessage("Error updating user type");
            } else {
                this.props.showErrorMessage(e.response.status);
            }
        }
    }

    public render() {
        return <>
            <div className="m-2 text-center">
                <h2>Users</h2>
            </div>
            <table className="table">
                <thead className="thead-light">
                    <tr>
                        <th>User</th>
                        <th>Role</th>
                        <th>&nbsp;</th>
                    </tr>
                </thead>
                <tbody>
                    {this.state.users.map(u =>
                        <tr key={u.id}>
                            <td>
                                <a href={`https://${this.props.config.siteDomain}/users/${u.id}`} target="_blank">
                                    {u.displayName} {u.isModerator ? "♦" : ""}
                                </a>
                            </td>
                            <td>{userTypeToString(u.userType)}</td>
                            <td>{this.props.currentUser && this.props.currentUser.canManageUsers &&
                                <div className="btn-group" role="group">
                                    {u.userType === UserType.User &&
                                        <button type="button" className="btn btn-sm btn-warning" onClick={e => this.updateUserType(u, UserType.TrustedUser)}>
                                            Make trusted user
                                        </button>
                                    }
                                    {u.userType === UserType.Banned &&
                                        <button type="button" className="btn btn-sm btn-warning" onClick={e => this.updateUserType(u, UserType.User)}>
                                            Lift Ban
                                        </button>
                                    }
                                    {u.userType === UserType.TrustedUser &&
                                        <button type="button" className="btn btn-sm btn-danger" onClick={e => this.updateUserType(u, UserType.User)}>
                                            Make regular user
                                        </button>
                                    }
                                    {u.userType !== UserType.Banned && u.userType !== UserType.TrustedUser && u.userType !== UserType.Reviewer && !u.isModerator &&
                                        <button type="button" className="btn btn-sm btn-danger" onClick={e => this.updateUserType(u, UserType.Banned)}>
                                            Ban User
                                        </button>
                                    }
                                </div>
                            }</td>
                        </tr>)}
                </tbody>
            </table>
        </>;
    }
}
