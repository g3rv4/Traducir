import { ajaxGet, queryStringFromObject } from "../shared/ajax";
import { clone, spinner } from "../shared/utils";

declare var stringQueryFilters: any;

export default function initializeStringSearch() {
    const queryDropdowns = document.querySelectorAll("select.js-string-query-filter") as NodeListOf<HTMLSelectElement>;
    const queryTextInputs = document.querySelectorAll("input[type=text].js-string-query-filter") as NodeListOf<HTMLInputElement>;
    const queryLinks = document.querySelectorAll("a.js-string-query-filter") as NodeListOf<HTMLAnchorElement>;

    hookDropdowns();
    hookTextboxes();
    hookQuickLinks();
    hookHistoryPopState();

    function hookDropdowns() {
        for (const dropdown of queryDropdowns) {
            dropdown.onchange = e => { updateList(e.target as Element, true); };
        }
    }

    function hookTextboxes() {
        let textInputTimeout: number | undefined;
        for (const input of queryTextInputs) {
            input.onkeyup = e => {
                clearTimeout(textInputTimeout);
                textInputTimeout = setTimeout(() => { updateList(e.target as Element); }, 500);
            };
        }
    }

    function hookQuickLinks() {
        for (let i = 0; i < queryLinks.length; i++) {
            const link = queryLinks.item(i);
            link.onclick = e => {
                const target = e.target as HTMLElement;
                const queryKey = target.getAttribute("data-string-query-key");
                const queryValue = target.getAttribute("data-string-query-value");
                if (queryValue == null) {
                    throw Error("Could not get a queryValue");
                }

                const dropdown = document.querySelector(`select[data-string-query-key=${queryKey}`) as HTMLSelectElement;
                if (!dropdown) {
                    throw Error("Could not find the dropdown");
                }
                dropdown.value = queryValue;
                updateList(e.target as Element, true);
                e.preventDefault();
            };
        }
    }

    function hookHistoryPopState() {
        window.onpopstate = (_: PopStateEvent) => {
            location.reload();
        };
    }

    function updateList(triggeringElement: Element, valueIsNumber?: boolean) {
        spinner(true);

        if (triggeringElement) {
            const queryKey = triggeringElement.getAttribute("data-string-query-key");
            if (!queryKey) {
                throw Error("Could not find the queryKey");
            }

            // TODO: Check what things are passed here that have a value
            let value = null;
            const elementAsAny = triggeringElement as any;
            if (elementAsAny.value) {
                value = elementAsAny.value;
            }

            let queryValue = triggeringElement.getAttribute("data-string-query-value") ?? value;
            if (valueIsNumber) {
                queryValue = parseInt(queryValue, 10);
            }
            stringQueryFilters[queryKey] = queryValue;
        }

        const queryString = queryStringFromObject(stringQueryFilters);

        if (triggeringElement) {
            history.pushState(clone(stringQueryFilters), "", queryString ? "filters" + queryString : "/");
        }

        ajaxGet(
            "/strings_list",
            queryString,
            html => {
                const stringsList = document.getElementById("strings_list");
                if (!stringsList) {
                    throw Error("Could not get the strings list DOM element");
                }

                stringsList.innerHTML = html;
            }
        );
    }
}
