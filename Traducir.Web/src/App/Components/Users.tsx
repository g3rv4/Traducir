import axios from "axios";
import * as React from "react";
import { Link } from "react-router-dom";
import history from "../../history";
import IConfig from "../../Models/Config";
import IUser from "../../Models/User";
import IUserInfo from "../../Models/UserInfo";
import User from "./User";

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
                    {this.state.users.map(u => <User
                        key={u.id}
                        user={u}
                        config={this.props.config}
                        refreshUsers={this.refreshUsers}
                        showErrorMessage={this.props.showErrorMessage}
                    />)}
                </tbody>
            </table>
        </>;
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
}
