export function clone(obj: object) {
    return JSON.parse(JSON.stringify(obj));
}

export function spinner(show: boolean) {
    const spinnerObj = document.getElementById("spinner");
    if (!spinnerObj) {
        throw Error("Could not find a spinner object");
    }
    spinnerObj.style.display = show ? "block" : "none";
}

export function dynamicEventHook(events: string[] | string, selector: string, handler: (e: Event) => void) {
    if (!Array.isArray(events)) {
        events = [events];
    }

    for (const event of events) {
        document.addEventListener(event, e => {
            if (e.target && Array.from(document.querySelectorAll(selector)).indexOf(e.target as Element) !== -1) {
                handler.call(e.target, e);
            }
        });
    }
}

export function toCamelCase(s: string) {
    return s.replace(/([-_][a-z])/ig, ($1: string) => {
        return $1.toUpperCase()
            .replace("-", "")
            .replace("_", "");
    });
}

export function urlBase64ToUint8Array(base64String: string) {
    const padding = "=".repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding)
        .replace(/\-/g, "+")
        .replace(/_/g, "/");
    const rawData = window.atob(base64);
    return Uint8Array.from([...rawData].map(char => char.charCodeAt(0)));
}
