initializeUserEdit();

function initializeUserEdit() {
    dynamicEventHook("click", "[data-change-to-user-type]", e => {
        changeUserTypeFor(e.target);
    });

    function changeUserTypeFor(element) {
        const userSummaryContainer = element.closest("[data-user-id]");
        const userId = userSummaryContainer.getAttribute("data-user-id");
        const newUserType = element.getAttribute("data-change-to-user-type");

        ajaxPost(
            "/change-user-type",
            "text",
            { userId: userId, userType: newUserType },
            html => {
                userSummaryContainer.outerHTML = html;
            }
        );
    }
}
