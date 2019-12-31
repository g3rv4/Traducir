import { spinner } from "./utils";

let csrfToken: string;

export function init(token: string) {
    csrfToken = token;
}

export function ajaxGet(url: string, queryString, onSuccess?, onErrorResponse?, onFailure?) {
    ajax("GET", url, queryString, null, onSuccess, onErrorResponse, onFailure);
}

export function ajaxPost(url, body, onSuccess?, onErrorResponse?, onFailure?) {
    ajax("POST", url, null, body, onSuccess, onErrorResponse, onFailure);
}

async function ajax(method, url, queryString, body, onSuccess, onErrorResponse, onFailure) {
    spinner(true);

    if (body) {
        body = JSON.stringify(body);
    }
    const headers = body ? { "Content-Type": "application/json" } : {};

    if (method === "POST") {
        headers["X-CSRF-TOKEN"] = csrfToken;
    }

    try {
        const response = await fetch(url + (queryString || ""), { method, body, headers });
        spinner(false);
        if (response.ok) {
            const value = await response.text();
            onSuccess?.(value);
        } else {
            const errorHandler = onErrorResponse ?? defaultAjaxOnErrorResponse;
            errorHandler(response);
        }
    } catch (error) {
        spinner(false);
        const errorHandler = (onFailure || defaultAjaxOnFailure);
        errorHandler(error);
    }
}

export function queryStringFromObject(object) {
    if (!object) {
        return "";
    }

    let query = Object
        .keys(object)
        .filter(k => !!object[k])
        .map(k => encodeURIComponent(k) + "=" + encodeURIComponent(object[k]))
        .join("&");

    if (query) {
        query = "?" + query;
    }

    return query;
}

export function defaultAjaxOnErrorResponse(response) {
    alert(`Error returned by server:\r\n${response.status} - ${response.statusText}`);
}

function defaultAjaxOnFailure(error) {
    alert(error);
}
