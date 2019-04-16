function hookFilterResultsForEdit() {
    let i;

    const originalStringCells = document.querySelectorAll(".string-list tr td:first-of-type");
    for (i = 0; i < originalStringCells.length; i++) {
        originalStringCells[i].onclick = e => {
            loadStringEditorFor(e.target.parentElement);
        };
    }

    dynamicEventHook("click", "[data-string-action]", e => { doStringAction(e.target); });

    function loadStringEditorFor(searchResultsRow) {
        const stringId = searchResultsRow.getAttribute("data-string-id");
        ajaxGet(
            "/string_edit_ui",
            "text",
            queryStringFromObject({ stringId: stringId }),
            html => {
                modal.show('Suggestions', html);
                //TODO: Bind controls
            }
        );
    }

    function doStringAction(button) {
        const stringRow = button.closest("[data-string-id]");
        const stringId = stringRow.getAttribute("data-string-id");
        const stringAction = button.getAttribute("data-string-action");

        let doIgnore;
        if (stringAction === "ignore") {
            doIgnore = true;
        }
        else if (stringAction === "stop-ignoring") {
            doIgnore = false;
        }
        else {
            console.error(`Unknown string action: ${stringAction}`);
            return;
        }

        ajaxPut(
            "/manage-ignore",
            "text",
            { stringId: stringId, ignored: doIgnore },
            text => {
                stringRow.outerHTML = text;
            }
        );
    }
}
