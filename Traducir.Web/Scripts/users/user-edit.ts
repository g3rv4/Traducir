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
        if (!userId) {
            throw Error("Could not get the user Id");
        }

        const newUserType = element.getAttribute("data-change-to-user-type");
        if (!newUserType) {
            throw Error("Could not get the new user type");

        }

        ajaxPost(
            "/users/change-type",
            {
                userId: parseInt(userId, 10),
                userType: parseInt(newUserType, 10)
            },
            html => userSummaryContainer.outerHTML = html
        );
    }
}
