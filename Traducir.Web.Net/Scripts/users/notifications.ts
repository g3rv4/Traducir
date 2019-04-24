declare var notificationSettings: any;

import { ajaxPost, defaultAjaxOnErrorResponse } from "../shared/ajax";
import { urlBase64ToUint8Array } from "../shared/utils";

initializeNotifications();

function initializeNotifications() {
    const supportsPush = ("serviceWorker" in navigator) && ("PushManager" in window);
    if (!supportsPush) {
        document.getElementById("main-container").outerHTML = "<div class='sorry-no-push'>Your browser doesn't support push notifications</div>";
        return;
    }

    document.querySelectorAll("[data-notification-name]").forEach(element => {
        element.addEventListener("click", e => {
            handleNotificationChange(e.target as Element);
        });
    });

    function handleNotificationChange(element: Element) {
        const notificationName = element.getAttribute("data-notification-name");
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

    document.getElementById("save-and-add-browser").addEventListener("click", async () => { await saveAndAddBrowser(); });

    async function saveAndAddBrowser(): Promise<void> {
        const subscription = await subscribeUserToPush();
        ajaxPost(
            "/update-notification-settings",
            "text",
            { notifications: notificationSettings, subscription },
            null,
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
            const registration = await registerServiceWorker();

            const subscribeOptions = {
                applicationServerKey: urlBase64ToUint8Array(notificationSettings.vapidPublic),
                userVisibleOnly: true
            };

            return await registration.pushManager.subscribe(subscribeOptions);
        } catch (e) {
            alert("Error asking for permission");
            throw e;
        }
    }

    function registerServiceWorker(): Promise<ServiceWorkerRegistration> {
        return navigator.serviceWorker.register("/js/service-worker.js");
    }
}
