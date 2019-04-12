function ajaxGet(url, responseType, queryString, onSuccess, onErrorResponse, onFailure) {
    ajax("GET", url, responseType, queryString, null, onSuccess, onErrorResponse, onFailure);
}

function ajaxPut(url, responseType, body, onSuccess, onErrorResponse, onFailure) {
    ajax("PUT", url, responseType, null, body, onSuccess, onErrorResponse, onFailure);
}

function ajax(method, url, responseType, queryString, body, onSuccess, onErrorResponse, onFailure) {
    spinner(true);

    if (body) body = JSON.stringify(body);
    headers = body ? { 'Content-Type': 'application/json' } : {};

    fetch(url + (queryString || ''), { method: method, body: body, headers: headers })
        .then(response => {
            if (response.ok) {
                response[responseType]()
                    .then(value => {
                        if (onSuccess) {
                            try {
                                onSuccess(value);
                            }
                            finally {
                                spinner(false);
                            }
                        }
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
