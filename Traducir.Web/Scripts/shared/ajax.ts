import { spinner } from "./utils";

let csrfToken: string;

export function init(token: string) {
    csrfToken = token;
}

export function ajaxGet(url: string, queryString?: string, onSuccess?: (_: string) => void, onErrorResponse?: (_: Response) => void, onFailure?: (_: Error) => void) {
    ajax("GET", url, queryString, undefined, onSuccess, onErrorResponse, onFailure);
}

export function ajaxPost(url: string, body: object, onSuccess?: (_: string) => void, onErrorResponse?: (_: Response) => void, onFailure?: (_: Error) => void) {
    ajax("POST", url, undefined, body, onSuccess, onErrorResponse, onFailure);
}

async function ajax(method: "GET" | "POST", url: string, queryString?: string, body?: object, onSuccess?: (_: string) => void, onErrorResponse?: (_: Response) => void, onFailure?: (_: Error) => void) {
    spinner(true);

    const headers: Record<string, string> = body ? { "Content-Type": "application/json" } : {};

    if (method === "POST") {
        headers["X-CSRF-TOKEN"] = csrfToken;
    }

    try {
        const requestInit: RequestInit = { method, headers };
        if (body) {
            requestInit.body = JSON.stringify(body);
        }

        const response = await fetch(url + (queryString || ""), requestInit);
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

export function queryStringFromObject(obj: Record<string, string | number>) {
    if (!obj) {
        return "";
    }

    let query = Object
        .keys(obj)
        .filter(k => !!obj[k])
        .map(k => encodeURIComponent(k) + "=" + encodeURIComponent(obj[k]))
        .join("&");

    if (query) {
        query = "?" + query;
    }

    return query;
}

export function defaultAjaxOnErrorResponse(response: Response) {
    alert(`Error returned by server:\r\n${response.status} - ${response.statusText}`);
}

function defaultAjaxOnFailure(error: Error) {
    alert(error);
}
