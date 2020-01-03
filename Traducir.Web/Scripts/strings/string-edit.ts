import { ajaxGet, ajaxPost, queryStringFromObject, defaultAjaxOnErrorResponse } from "../shared/ajax";
import { dynamicEventHook, toCamelCase } from "../shared/utils";
import modal from "../shared/modal";

export function showString(stringId: string, reuseModal?: boolean) {
    ajaxGet(
        "/string_edit_ui",
        queryStringFromObject({ stringId }),
        html => {
            if (reuseModal) {
                const content = document.getElementById("modal-body");
                if (!content) {
                    throw Error("Couldn't find the modal-body element");
                }
                content.innerHTML = html;
            } else {
                // when people visit the string url by doing a request, we don't need to pushState
                const newPath = `/strings/${stringId}`;
                if (window.location.pathname !== newPath) {
                    history.pushState({ stringId, prevUrl: window.location.href, prevState: history.state }, "", newPath);
                }

                modal.show("Suggestions", html, () => {
                    history.pushState(history.state?.prevState, "", history.state?.prevUrl ?? "/");
                });
            }
        }
    );
}

export function init() {
    const suggestionCreationErrors: Record<number, string> = {
        2: "We couldn't find the id you send, did you need to refresh your page?",
        3: "The suggestion you are sending is the same as the actual translation",
        4: "You sent an empty suggestion, please try to send a suggestion next time",
        5: "The suggestion you are sending is already suggested. Maybe you need to refresh?",
        6: "Failed sending the suggestion. You are missing some variables",
        7: "Failed sending the suggestion. You have included unrecognized variables",
        8: "A database error has ocurred, please try again."
    };

    dynamicEventHook("click", ".string-list tr td:first-of-type", (e: Event) => {
        const tgt = e.target as HTMLElement;
        if (tgt && tgt.parentElement) {
            loadStringEditorFor(tgt.parentElement);
        }
    });

    function loadStringEditorFor(searchResultsRow: HTMLElement) {
        const stringId = searchResultsRow.getAttribute("data-string-id");
        if (!stringId) {
            throw Error("Could not find the string id to load");
        }
        showString(stringId);
    }

    dynamicEventHook("click", "button[data-string-action]", (e: Event) => {
        executeStringActionFor(e.target as HTMLElement);
    });

    // TODO: Find a better way to hook the thumb up/down icon clicks
    dynamicEventHook("click", "button[data-string-action] *", (e: Event) => {
        if (e.target) {
            const tgt = e.target as HTMLElement;
            const action = tgt.closest("[data-string-action]");
            if (!action) {
                throw Error("Could not find the action to perform");
            }

            executeStringActionFor(action);
        }
    });

    dynamicEventHook(["change", "keyup", "paste"], "textarea[data-string-action]", (e: Event) => {
        if (e.target) {
            executeStringActionFor(e.target as HTMLElement);
        }
    });

    function executeStringActionFor(element: Element) {
        const stringContainer = element.closest("[data-string-id]");
        if (!stringContainer) {
            throw Error("Could not find the string id container");
        }

        const stringId = stringContainer.getAttribute("data-string-id");
        if (!stringId) {
            throw Error("Could not find the string id");
        }

        const actionNameAttr = element.getAttribute("data-string-action");
        if (!actionNameAttr) {
            throw Error("Could not find the action name attribute");
        }

        const actionName = toCamelCase(actionNameAttr);
        const action = getAction(actionName);

        action(stringId, element);
    }

    function getAction(actionName: string): (a: string, b: Element) => void {
        switch (actionName) {
            case "copyAsSuggestion": return (_: string, button: Element) => {
                const buttonTargetId = button.getAttribute("data-string-action-target");
                if (!buttonTargetId) {
                    throw Error("Could not find the target id");
                }

                const target = document.getElementById(buttonTargetId);
                if (!target) {
                    throw Error("Could not find the target");
                }

                const text = target.innerText;
                const suggestionBox = document.getElementById("suggestion") as HTMLTextAreaElement;
                suggestionBox.value = text;
            };
            case "manageIgnore": return (stringId: string, button: Element) => {
                const doIgnore = button.getAttribute("data-string-action-argument") === "ignore";

                ajaxPost(
                    "/manage-ignore",
                    { stringId, ignored: doIgnore },
                    text => {
                        const stringContainer = button.closest("[data-string-id]");
                        if (!stringContainer) {
                            throw Error("Could not find the string id container");
                        }

                        stringContainer.outerHTML = text;
                    }
                );
            };
            case "manageUrgency": return (stringId: string, button: Element) => {
                const mustBeUrgent = button.getAttribute("data-string-action-argument") === "make-urgent";

                ajaxForStringAction(
                    stringId,
                    "/manage-urgency",
                    { stringId, isUrgent: mustBeUrgent },
                    undefined,
                    true
                );
            };
            case "handleSuggestionTextChanged": return (_: string, element: Element) => {
                const textarea = element as HTMLTextAreaElement;
                const replaceButtons = document.querySelectorAll(".js-replace-suggestion") as NodeListOf<HTMLButtonElement>;
                for (const button of replaceButtons) {
                    button.disabled = !textarea.value.trim();
                }
            };
            case "replaceSuggestion": return (stringId: string, button: Element) => {
                const suggestionElement = button.closest("[data-suggestion-id]");
                if (!suggestionElement) {
                    throw Error("Could not find the suggestion");
                }

                const suggestionId = suggestionElement.getAttribute("data-suggestion-id");
                const newSuggestion = (document.getElementById("suggestion") as HTMLTextAreaElement).value.trim();

                ajaxForStringAction(
                    stringId,
                    "/replace-suggestion",
                    { suggestionId, newSuggestion }
                );
            };
            case "deleteSuggestion": return (stringId: string, button: Element) => {
                const suggestionElement = button.closest("[data-suggestion-id]");
                if (!suggestionElement) {
                    throw Error("Could not find the suggestion");
                }
                const suggestionId = suggestionElement.getAttribute("data-suggestion-id");

                ajaxForStringAction(
                    stringId,
                    "/delete-suggestion",
                    { suggestionId }
                );
            };
            case "reviewSuggestion": return (stringId: string, button: Element) => {
                const suggestionElement = button.closest("[data-suggestion-id]");
                if (!suggestionElement) {
                    throw Error("Could not find the suggestion");
                }

                const suggestionId = suggestionElement.getAttribute("data-suggestion-id");
                const approve = button.getAttribute("data-review-action") === "approve";

                ajaxForStringAction(
                    stringId,
                    "/review-suggestion",
                    { suggestionId, approve }
                );
            };
            case "createSuggestion": return (stringId: string, button: Element) => {
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
                            const errorMessage = suggestionCreationErrors[parseInt(errorCode, 10)] ?? "The server encountered an error, but we don't know what happened";
                            alert(errorMessage);
                        });
                    }
                );
            };
        }

        throw Error("Unknown action: " + actionName);
    }

    function ajaxForStringAction(stringId: string, url: string, body: object, onErrorResponse?: (_: Response) => void, keepModalOpen?: boolean) {
        ajaxPost(
            url,
            body,
            text => {
                const stringSummaryContainer = document.querySelector(`.js-string-summary[data-string-id='${stringId}']`);
                if (stringSummaryContainer) {
                    stringSummaryContainer.outerHTML = text;
                }
                if (keepModalOpen) {
                    showString(stringId, true);
                } else {
                    modal.hide();
                }
            },
            onErrorResponse
        );
    }
}
