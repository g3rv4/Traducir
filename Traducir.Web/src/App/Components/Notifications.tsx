import axios from "axios";
import { autobind } from "core-decorators";
import * as React from "react";
import history from "../../history";
import IConfig from "../../Models/Config";
import { INotificationSettings, NotificationInterval } from "../../Models/INotificationSettings";
import IUserInfo from "../../Models/UserInfo";
import nameofFactory from "../../nameofFactory";
import urlBase64ToUint8Array from "../../urlBase64ToUint8Array";
import { NonUndefinedReactNode } from "../NonUndefinedReactNode";

const nameof = nameofFactory<INotificationSettings>();

export interface INotificationsProps {
    currentUser?: IUserInfo;
    config: IConfig;
    showErrorMessage: (messageOrCode: string | number) => void;
}

interface INotificationsState {
    notifications?: INotificationSettings;
    supportsPush: boolean;
}

class Comp : React.Component {
    function render(){
        return <li onclick={}></li>
    }

    function handler(){
        this.props.toggleState(this.props.nombre)
    }
}

export default class Notifications extends React.Component<INotificationsProps, INotificationsState> {
    constructor(props: INotificationsProps) {
        super(props);

        let supportsPush = true;
        if (!("serviceWorker" in navigator)) {
            supportsPush = false;
        }

        if (!("PushManager" in window)) {
            supportsPush = false;
        }

        this.state = { supportsPush };
    }

    public componentDidMount(): void {
        this.refreshNotifications();

        this.registerServiceWorker()
            .catch(() => this.props.showErrorMessage("Unable to register service worker"));
    }

    public render(): NonUndefinedReactNode {
        if (!this.state.notifications) {
            return null;
        }
        if (!this.state.supportsPush) {
            return <div>Your browser doesn't support push notifications</div>;
        }
        const n = this.state.notifications;
        return <>
            <div className="m-2 text-center">
                <h2>Notification settings</h2>
            </div>
            <div className="row mt-4">
                <div className="col text-center">
                    <h3>System notifications</h3>
                </div>
                <div className="col text-center">
                    <h3>Your suggestion notifications</h3>
                </div>
            </div>
            <div className="row">
                <div className="col">
                    <ul className="list-group">
                        <li className={this.getClass(n.notifyUrgentStrings)} data-name={nameof("notifyUrgentStrings")} onClick={this.toggleState}>Urgent Strings</li>
                        <li className={this.getClass(n.notifySuggestionsAwaitingApproval)} data-name={nameof("notifySuggestionsAwaitingApproval")} onClick={this.toggleState}>Suggestions awaiting approval</li>
                        <li className={this.getClass(n.notifySuggestionsAwaitingReview)} data-name={nameof("notifySuggestionsAwaitingReview")} onClick={this.toggleState}>Suggestions awaiting review</li>
                        <li className={this.getClass(n.notifyStringsPushedToTransifex)} data-name={nameof("notifyStringsPushedToTransifex")} onClick={this.toggleState}>Strings pushed to Transifex</li>
                    </ul>
                </div>
                <div className="col">
                    <ul className="list-group">
                        <li className={this.getClass(n.notifySuggestionsApproved)} data-name={nameof("notifySuggestionsApproved")} onClick={this.toggleState}>Suggestion approved</li>
                        <li className={this.getClass(n.notifySuggestionsRejected)} data-name={nameof("notifySuggestionsRejected")} onClick={this.toggleState}>Suggestion rejected</li>
                        <li className={this.getClass(n.notifySuggestionsReviewed)} data-name={nameof("notifySuggestionsReviewed")} onClick={this.toggleState}>Suggestion reviewed</li>
                        <li className={this.getClass(n.notifySuggestionsOverriden)} data-name={nameof("notifySuggestionsOverriden")} onClick={this.toggleState}>Suggestion overriden</li>
                    </ul>
                </div>
            </div>
            <div className="col">
                <div className="row">
                    Notify me about the same type of event every
                    <input type="number" value={this.state.notifications.notificationsIntervalValue} onChange={this.updateNotificationIntervalValue} />
                    <select value={this.state.notifications.notificationsInterval} onChange={this.updateNotificationInterval}>
                        <option value={NotificationInterval.Days}>days</option>
                        <option value={NotificationInterval.Hours}>hours</option>
                        <option value={NotificationInterval.Minutes}>minutes</option>
                    </select>
                </div>
            </div>
            <div className="text-center mt-4">
                <button className="btn btn-primary mr-2" onClick={this.saveAndAddBrowser}>Save and add current browser</button>
                <button className="btn btn-danger">Stop receiving notifications everywhere</button>
            </div>
        </>;
    }

    public getClass(value: boolean): string {
        let cname = "list-group-item";
        if (value) {
            cname += " active";
        }
        return cname;
    }

    public registerServiceWorker(): Promise<ServiceWorkerRegistration> {
        return navigator.serviceWorker.register("/lib/service-worker.js");
    }

    public async askPermission(): Promise<NotificationPermission> {
        return new Promise<NotificationPermission>((resolve, reject) => {
            const permissionResult = Notification.requestPermission(result => {
                resolve(result);
            });

            if (permissionResult) {
                permissionResult.then(resolve, reject);
            }
        });
    }

    @autobind
    public async subscribeUserToPush(): Promise<PushSubscription> {
        try {
            const registration = await this.registerServiceWorker();

            const subscribeOptions = {
                applicationServerKey: urlBase64ToUint8Array(this.props.config.vapidPublic),
                userVisibleOnly: true
            };

            return await registration.pushManager.subscribe(subscribeOptions);
        } catch (e) {
            this.props.showErrorMessage("Error asking for permission");
            throw e;
        }
    }

    @autobind
    public async saveAndAddBrowser(): Promise<void> {
        if (!this.state.notifications) {
            return;
        }
        try {
            const subscription = await this.subscribeUserToPush();
            await axios.put("/app/api/me/notification-settings",
                {
                    notifications: this.state.notifications,
                    subscription
                });
        } catch (e) {
            if (e.response.status === 401) {
                history.push("/");
            } else {
                this.props.showErrorMessage(e.response.status);
            }
        }
    }

    @autobind
    public toggleState(e: any): void {
        if (!this.state.notifications) {
            return;
        }
        const name = e.target.dataset.name;
        const notifications = this.state.notifications as any;
        notifications[name] = !notifications[name];

        this.setState({
            notifications
        });
    }

    @autobind
    public updateNotificationIntervalValue(e: React.FormEvent<HTMLInputElement>): void {
        const notifications = this.state.notifications;
        if (!notifications) {
            return;
        }
        notifications.notificationsIntervalValue = parseInt(e.currentTarget.value, 10);
        this.setState({
            notifications
        });
    }

    @autobind
    public updateNotificationInterval(e: React.FormEvent<HTMLSelectElement>): void {
        const notifications = this.state.notifications;
        if (!notifications) {
            return;
        }
        notifications.notificationsInterval = parseInt(e.currentTarget.value, 10);
        this.setState({
            notifications
        });
    }

    @autobind
    public async refreshNotifications(): Promise<void> {
        try {
            const r = await axios.get<INotificationSettings>("/app/api/me/notification-settings");

            this.setState({
                notifications: r.data
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
