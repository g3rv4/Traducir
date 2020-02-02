declare var notificationSettings: any;

import { ajaxPost, defaultAjaxOnErrorResponse } from "../shared/ajax";
import { urlBase64ToUint8Array } from "../shared/utils";

export default function run() {
    initializeNotifications();
}

function initializeNotifications() {
    const supportsPush = ("serviceWorker" in navigator) && ("PushManager" in window);
    if (!supportsPush) {
        const container = document.getElementById("main-container");
        if (!container) {
            throw Error("Could not get the container DOM element");
        }

        container.outerHTML = "<div class='sorry-no-push'>Your browser doesn't support push notifications</div>";
        return;
    }

    document.querySelectorAll("[data-notification-name]").forEach(element => {
        element.addEventListener("click", e => {
            handleNotificationChange(e.target as Element);
        });
    });

    function handleNotificationChange(element: Element) {
        const notificationName = element.getAttribute("data-notification-name");
        if (!notificationName) {
            throw Error("Could not get the notification name");
        }

        notificationSettings[notificationName] = !notificationSettings[notificationName];

        if (notificationSettings[notificationName]) {
            element.classList.add("active");
        } else {
            element.classList.remove("active");
        }
    }

    const intervalValueSelector = document.getElementById("notifications-interval-value") as HTMLInputElement;
    intervalValueSelector.addEventListener("change", () => {
        notificationSettings.notificationsIntervalValue = intervalValueSelector.value;
    });

    const intervalSelector = document.getElementById("notifications-interval") as HTMLInputElement;
    intervalSelector.addEventListener("change", () => {
        notificationSettings.notificationsInterval = intervalSelector.value;
    });

    const saveButton = document.getElementById("save-and-add-browser");
    if (!saveButton) {
        throw Error("Could not find the save button");
    }
    saveButton.addEventListener("click", saveAndAddBrowser);

    const stopNotificationsButton = document.getElementById("stop-receiving-notifications");
    if (!stopNotificationsButton) {
        throw Error("Could not find the stop notifications button");
    }
    stopNotificationsButton.addEventListener("click", wipeNotifications);

    async function saveAndAddBrowser(): Promise<void> {
        const subscription = await subscribeUserToPush();
        ajaxPost(
            "/users/me/notifications/update",
            { notifications: notificationSettings, subscription },
            undefined,
            response => {
                if (response.status === 401) {
                    history.pushState(null, "", "/");
                } else {
                    defaultAjaxOnErrorResponse(response);
                }
            }
        );
    }

    async function subscribeUserToPush(): Promise<PushSubscription> {
        try {
            await navigator.serviceWorker.register("/service-worker.js");
            const registration = await navigator.serviceWorker.ready;

            const subscribeOptions = {
                applicationServerKey: urlBase64ToUint8Array(notificationSettings.vapidPublic),
                userVisibleOnly: true
            };

            return registration.pushManager.subscribe(subscribeOptions);
        } catch (e) {
            alert("Error asking for permission: " + e.message);
            throw e;
        }
    }

    function wipeNotifications() {
        ajaxPost("/delete-notifications", {}, () => location.reload(), response => {
            if (response.status === 401) {
                history.pushState(null, "", "/");
            } else {
                defaultAjaxOnErrorResponse(response);
            }
        });
    }
}
