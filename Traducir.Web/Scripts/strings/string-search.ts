import { ajaxGet, queryStringFromObject } from "../shared/ajax";
import { clone, spinner } from "../shared/utils";

declare var stringQueryFilters: any;

export default function initializeStringSearch() {
    const queryDropdowns = document.querySelectorAll("select.js-string-query-filter") as NodeListOf<HTMLSelectElement>;
    const queryTextInputs = document.querySelectorAll("input[type=text].js-string-query-filter") as NodeListOf<HTMLInputElement>;
    const queryLinks = document.querySelectorAll("a.js-string-query-filter") as NodeListOf<HTMLAnchorElement>;
    const initialQueryFilters = clone(stringQueryFilters);

    hookDropdowns();
    hookTextboxes();
    hookQuickLinks();
    hookHistoryPopState();

    function setInputsFromCurrentFilters() {
        let i;

        for (i = 0; i < queryDropdowns.length; i++) {
            const dropdown = queryDropdowns[i];
            const queryKey = dropdown.getAttribute("data-string-query-key");
            dropdown.value = stringQueryFilters[queryKey] || 0;
        }

        for (i = 0; i < queryTextInputs.length; i++) {
            const textInput = queryTextInputs[i];
            const queryKey = textInput.getAttribute("data-string-query-key");
            textInput.value = stringQueryFilters[queryKey];
        }
    }

    function hookDropdowns() {
        for (const dropdown of queryDropdowns) {
            dropdown.onchange = e => { updateList(e.target, true); };
        }
    }

    function hookTextboxes() {
        let textInputTimeout = null;
        for (const input of queryTextInputs) {
            input.onkeyup = e => {
                clearTimeout(textInputTimeout);
                textInputTimeout = setTimeout(() => { updateList(e.target); }, 500);
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
                const dropdown = document.querySelector(`select[data-string-query-key=${queryKey}`) as HTMLSelectElement;
                dropdown.value = queryValue;
                updateList(e.target, true);
                e.preventDefault();
            };
        }
    }

    function hookHistoryPopState() {
        window.onpopstate = e => {
            stringQueryFilters = e.state || initialQueryFilters;
            setInputsFromCurrentFilters();
            updateList(null);
        };
    }

    function updateList(triggeringElement, valueIsNumber?) {
        spinner(true);

        if (triggeringElement) {
            const queryKey = triggeringElement.getAttribute("data-string-query-key");
            let queryValue = triggeringElement.getAttribute("data-string-query-value") || triggeringElement.value;
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
            "text",
            queryString,
            html => document.getElementById("strings_list").innerHTML = html
        );
    }
}
