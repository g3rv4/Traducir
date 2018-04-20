import * as React from "react";
import axios, { AxiosError } from 'axios';
import User from "../../Models/User";
import Config from "../../Models/Config";
import UserInfo from "../../Models/UserInfo";
import history from '../../history';
import { userTypeToString, UserType } from "../../Models/UserType";

export interface UsersProps {
    showErrorMessage: (messageOrCode: string | number) => void;
    currentUser?: UserInfo;
    config: Config;
}

interface UsersState {
    users: User[];
}

export default class Users extends React.Component<UsersProps, UsersState> {
    constructor(props: UsersProps) {
        super(props);

        this.state = {
            users: []
        };
    }
    componentDidMount() {
        this.refreshUsers();
    }
    refreshUsers() {
        const _that = this;
        axios.get<User[]>('/app/api/users').then(r => {
            _that.setState({
                users: r.data
            });
        }).catch(e => {
            if (e.response.status == 401) {
                history.push('/');
            } else {
                _that.props.showErrorMessage(e.response.status);
            }
        });
    }
    updateUserType(user: User, newType: UserType) {
        const _that = this;
        axios.put('/app/api/users/change-type', {
            UserId: user.id,
            UserType: newType
        }).then(r => {
            _that.refreshUsers();
        }).catch(e => {
            if (e.response.status == 400) {
                _that.props.showErrorMessage("Error updating user type");
            } else {
                _that.props.showErrorMessage(e.response.status);
            }
        });
    }
    render() {
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
                                    {u.displayName} {u.isModerator ? '♦' : ''}
                                </a>
                            </td>
                            <td>{userTypeToString(u.userType)}</td>
                            <td>{this.props.currentUser && this.props.currentUser.canManageUsers &&
                                <div className="btn-group" role="group">
                                    {u.userType == UserType.User &&
                                        <button type="button" className="btn btn-sm btn-warning" onClick={e => this.updateUserType(u, UserType.TrustedUser)}>
                                            Make trusted user
                                        </button>
                                    }
                                    {u.userType == UserType.Banned &&
                                        <button type="button" className="btn btn-sm btn-warning" onClick={e => this.updateUserType(u, UserType.User)}>
                                            Lift Ban
                                        </button>
                                    }
                                    {u.userType == UserType.TrustedUser &&
                                        <button type="button" className="btn btn-sm btn-danger" onClick={e => this.updateUserType(u, UserType.User)}>
                                            Make regular user
                                        </button>
                                    }
                                    {u.userType != UserType.Banned && u.userType != UserType.TrustedUser && u.userType != UserType.Reviewer && !u.isModerator &&
                                        <button type="button" className="btn btn-sm btn-danger" onClick={e => this.updateUserType(u, UserType.Banned)}>
                                            Ban User
                                        </button>
                                    }
                                </div>
                            }</td>
                        </tr>)}
                </tbody>
            </table>
        </>
    }
}