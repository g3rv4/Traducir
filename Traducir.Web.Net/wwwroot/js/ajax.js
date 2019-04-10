function ajaxGet(url, responseType, queryString, onSuccess, onErrorResponse, onFailure) {
    ajax("GET", url, responseType, queryString, null, onSuccess, onErrorResponse, onFailure);
}

function ajax(method, url, responseType, queryString, body, onSuccess, onErrorResponse, onFailure) {
    spinner(true);

    fetch(url + queryString, { method: method, body: body })
        .then(response => {
            if (response.ok) {
                response[responseType]()
                    .then(value => {
                        if (onSuccess) onSuccess(value);
                        spinner(false);
                    });
            }
            else {
                spinner(false);
                (onErrorResponse || defaultAjaxOnErrorResponse)(response);
            }
        })
        .catch(function (error) {
            spinner(false);
            (onFailure || defaultAjaxOnFailure)(error);
        });
}

function queryStringFromObject(object) {
    if (!object) return '';

    let query = Object
        .keys(object)
        .filter(k => !!object[k])
        .map(k => encodeURIComponent(k) + '=' + encodeURIComponent(object[k]))
        .join('&');

    if (query) query = '?' + query;

    return query;
}

function defaultAjaxOnErrorResponse(response) {
    alert(`Error returned by server:\r\n${response.status} - ${response.statusText}`);
}

function defaultAjaxOnFailure(error) {
    alert(error);
}
