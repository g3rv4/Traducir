import { ajaxGet, ajaxPost, queryStringFromObject, defaultAjaxOnErrorResponse } from "../shared/ajax";
import { dynamicEventHook, toCamelCase } from "../shared/utils";
import modal from "../shared/modal";

export function showString(stringId: number) {
    ajaxGet(
        "/string_edit_ui",
        "text",
        queryStringFromObject({ stringId }),
        html => {
            // when people visit the string url by doing a request, we don't need to pushState
            const newPath = `/strings/${stringId}`;
            if (window.location.pathname !== newPath) {
                history.pushState({ stringId, prevUrl: window.location.href, prevState: history.state }, "", newPath);
            }

            modal.show("Suggestions", html, () => {
                history.pushState(history.state?.prevState, "", history.state?.prevUrl ?? "/");
            });
        }
    );
}

export function init() {
    const suggestionCreationErrors = {
        2: "We couldn't find the id you send, did you need to refresh your page?",
        3: "The suggestion you are sending is the same as the actual translation",
        4: "You sent an empty suggestion, please try to send a suggestion next time",
        5: "The suggestion you are sending is already suggested. Maybe you need to refresh?",
        6: "Failed sending the suggestion. You are missing some variables",
        7: "Failed sending the suggestion. You have included unrecognized variables",
        8: "A database error has ocurred, please try again."
    };

    dynamicEventHook("click", ".string-list tr td:first-of-type", e => {
        loadStringEditorFor(e.target.parentElement);
    });

    function loadStringEditorFor(searchResultsRow) {
        const stringId = searchResultsRow.getAttribute("data-string-id");
        showString(stringId);
    }

    dynamicEventHook("click", "button[data-string-action]", e => {
        executeStringActionFor(e.target);
    });

    // TODO: Find a better way to hook the thumb up/down icon clicks
    dynamicEventHook("click", "button[data-string-action] *", e => {
        executeStringActionFor(e.target.closest("[data-string-action]"));
    });

    dynamicEventHook(["change", "keyup", "paste"], "textarea[data-string-action]", e => {
        executeStringActionFor(e.target);
    });

    function executeStringActionFor(element) {
        const stringContainer = element.closest("[data-string-id]");
        const stringId = stringContainer.getAttribute("data-string-id");
        const actionName = toCamelCase(element.getAttribute("data-string-action"));
        stringActions[actionName](stringId, element);
    }

    const stringActions = {
        copyAsSuggestion: (stringId, button) => {
            const buttonTargetId = button.getAttribute("data-string-action-target");
            const text = document.getElementById(buttonTargetId).innerText;
            const suggestionBox = document.getElementById("suggestion") as HTMLTextAreaElement;
            suggestionBox.value = text;
        },

        manageIgnore: (stringId, button) => {
            const doIgnore = button.getAttribute("data-string-action-argument") === "ignore";

            ajaxPost(
                "/manage-ignore",
                "text",
                { stringId, ignored: doIgnore },
                text => {
                    const stringContainer = button.closest("[data-string-id]");
                    stringContainer.outerHTML = text;
                }
            );
        },

        manageUrgency: (stringId, button) => {
            const mustBeUrgent = button.getAttribute("data-string-action-argument") === "make-urgent";

            ajaxForStringAction(
                stringId,
                "/manage-urgency",
                { stringId, isUrgent: mustBeUrgent }
            );
        },

        handleSuggestionTextChanged: (stringId, textarea) => {
            const replaceButtons = document.querySelectorAll(".js-replace-suggestion") as NodeListOf<HTMLButtonElement>;
            for (const button of replaceButtons) {
                button.disabled = !textarea.value.trim();
            }
        },

        replaceSuggestion: (stringId, button) => {
            const suggestionId = button.closest("[data-suggestion-id]").getAttribute("data-suggestion-id");
            const newSuggestion = (document.getElementById("suggestion") as HTMLTextAreaElement).value.trim();

            ajaxForStringAction(
                stringId,
                "/replace-suggestion",
                { suggestionId, newSuggestion }
            );
        },

        deleteSuggestion: (stringId, button) => {
            const suggestionId = button.closest("[data-suggestion-id]").getAttribute("data-suggestion-id");

            ajaxForStringAction(
                stringId,
                "/delete-suggestion",
                suggestionId
            );
        },

        reviewSuggestion: (stringId, button) => {
            const suggestionId = button.closest("[data-suggestion-id]").getAttribute("data-suggestion-id");
            const approve = button.getAttribute("data-review-action") === "approve";

            ajaxForStringAction(
                stringId,
                "/review-suggestion",
                { suggestionId, approve }
            );
        },

        createSuggestion: (stringId, button) => {
            const rawStringCheckbox = document.getElementById("is-raw-string") as HTMLInputElement;
            const suggestionElement = document.getElementById("suggestion") as HTMLTextAreaElement;
            const body = {
                stringId,
                suggestion: suggestionElement.value.trim(),
                approve: button.getAttribute("data-create-approved-suggestion") === "yes",
                rawString: rawStringCheckbox ? !!rawStringCheckbox.checked : false
            };

            ajaxForStringAction(
                stringId,
                "/create-suggestion",
                body,
                errorResponse => {
                    if (errorResponse.status !== 400) {
                        defaultAjaxOnErrorResponse(errorResponse);
                        return;
                    }

                    errorResponse.text().then(errorCode => {
                        const errorMessage = suggestionCreationErrors[errorCode] || "The server encountered an error, but we don't know what happened";
                        alert(errorMessage);
                    });
                }
            );
        }
    };

    function ajaxForStringAction(stringId, url, body, onErrorResponse?) {
        ajaxPost(
            url,
            "text",
            body,
            text => {
                const stringSummaryContainer = document.querySelector(`.js-string-summary[data-string-id='${stringId}']`);
                stringSummaryContainer.outerHTML = text;
                modal.hide();
            },
            onErrorResponse
        );
    }
}
