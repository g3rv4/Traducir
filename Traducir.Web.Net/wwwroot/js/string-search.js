function initializeStringSearch () {
    const queryDropdowns = document.querySelectorAll("select.js-string-query-filter");
    const queryTextInputs = document.querySelectorAll("input[type=text].js-string-query-filter");
    const queryLinks = document.querySelectorAll("a.js-string-query-filter");
    let initialQueryFilters;

    window.onload = function () {
        initialQueryFilters = clone(stringQueryFilters);

        hookDropdowns();
        hookTextboxes();
        hookQuickLinks();
        hookHistoryPopState();
    };

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
        for (var i = 0; i < queryDropdowns.length; i++) {
            queryDropdowns[i].onchange = e => { updateList(e.target, true); };
        }
    }

    function hookTextboxes() {
        var textInputTimeout = null;
        for (var i = 0; i < queryTextInputs.length; i++) {
            queryTextInputs[i].onkeyup = function (e) {
                clearTimeout(textInputTimeout);
                textInputTimeout = setTimeout(() => { updateList(e.target); }, 500);
            };
        };
    }

    function hookQuickLinks() {
        for (var i = 0; i < queryLinks.length; i++) {
            const link = queryLinks.item(i);
            link.onclick = e => {
                const queryKey = e.target.getAttribute("data-string-query-key");
                const queryValue = e.target.getAttribute("data-string-query-value");
                const dropdown = document.querySelector(`select[data-string-query-key=${queryKey}`);
                dropdown.value = queryValue;
                updateList(e.target, true);
                e.preventDefault();
            };
        }
    }

    function hookHistoryPopState() {
        window.onpopstate = function (e) {
            stringQueryFilters = e.state || initialQueryFilters;
            setInputsFromCurrentFilters();
            updateList(null);
        };
    }

    function updateList(triggeringElement, valueIsNumber) {
        spinner(true);

        if (triggeringElement) {
            const queryKey = triggeringElement.getAttribute("data-string-query-key");
            var queryValue = triggeringElement.getAttribute("data-string-query-value") || triggeringElement.value;
            if (valueIsNumber) queryValue = parseInt(queryValue);
            stringQueryFilters[queryKey] = queryValue;
        }

        const queryString = queryStringFromObject(stringQueryFilters);

        if (triggeringElement) {
            history.pushState(clone(stringQueryFilters), "", queryString ? 'filters' + queryString : '/');
        }

        ajaxGet(
            '/strings_list',
            'text',
            queryString,
            html => { document.getElementById("strings_list").innerHTML = html; }
        );
    }
}
