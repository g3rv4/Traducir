import { ajaxPost } from "../shared/ajax";
import { dynamicEventHook } from "../shared/utils";

export default function run() {
    initializeUserEdit();
}

function initializeUserEdit() {
    dynamicEventHook("click", "[data-change-to-user-type]", (e: Event) => {
        changeUserTypeFor(e.target as HTMLElement);
    });

    function changeUserTypeFor(element: HTMLElement) {
        const userSummaryContainer = element.closest("[data-user-id]");
        if (!userSummaryContainer) {
            throw Error("Could not get the summary container DOM element");
        }

        const userId = userSummaryContainer.getAttribute("data-user-id");
        const newUserType = element.getAttribute("data-change-to-user-type");

        ajaxPost(
            "/users/change-type",
            { userId, userType: newUserType },
            html => userSummaryContainer.outerHTML = html
        );
    }
}
