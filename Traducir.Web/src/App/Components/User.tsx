import axios from "axios";
import { autobind } from "core-decorators";
import React = require("react");
import IConfig from "../../Models/Config";
import IUser from "../../Models/User";
import IUserInfo from "../../Models/UserInfo";
import { UserType, userTypeToString } from "../../Models/UserType";

interface IUserProps {
    user: IUser;
    currentUser?: IUserInfo;
    config: IConfig;
    refreshUsers: () => void;
    showErrorMessage: (messageOrCode: string | number) => void;
}

export default class User extends React.Component<IUserProps> {
    public render() {
        return <tr>
            <td>
                <a href={`https://${this.props.config.siteDomain}/users/${this.props.user.id}`} target="_blank">
                    {this.props.user.displayName} {this.props.user.isModerator ? "♦" : ""}
                </a>
            </td>
            <td>{userTypeToString(this.props.user.userType)}</td>
            <td>{this.props.currentUser && this.props.currentUser.canManageUsers &&
                <div className="btn-group" role="group">
                    {this.props.user.userType === UserType.User &&
                        <button type="button" className="btn btn-sm btn-warning" onClick={this.makeTrustedUser}>
                            Make trusted user
                    </button>
                    }
                    {this.props.user.userType === UserType.Banned &&
                        <button type="button" className="btn btn-sm btn-warning" onClick={this.makeRegularUser}>
                            Lift Ban
                    </button>
                    }
                    {this.props.user.userType === UserType.TrustedUser &&
                        <button type="button" className="btn btn-sm btn-danger" onClick={this.makeRegularUser}>
                            Make regular user
                    </button>
                    }
                    {this.props.user.userType !== UserType.Banned && this.props.user.userType !== UserType.TrustedUser && this.props.user.userType !== UserType.Reviewer && !this.props.user.isModerator &&
                        <button type="button" className="btn btn-sm btn-danger" onClick={this.banUser}>
                            Ban User
                    </button>
                    }
                </div>
            }</td>
        </tr>;
    }

    @autobind()
    private makeTrustedUser() {
        this.updateUserType(UserType.TrustedUser);
    }

    @autobind()
    private makeRegularUser() {
        this.updateUserType(UserType.User);
    }

    @autobind()
    private banUser() {
        this.updateUserType(UserType.Banned);
    }

    private async updateUserType(newType: UserType) {
        try {
            await axios.put("/app/api/users/change-type", {
                UserId: this.props.user.id,
                UserType: newType
            });
            this.props.refreshUsers();
        } catch (e) {
            if (e.response.status === 400) {
                this.props.showErrorMessage("Error updating user type");
            } else {
                this.props.showErrorMessage(e.response.status);
            }
        }
    }
}
