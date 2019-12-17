export function clone(object) {
    return JSON.parse(JSON.stringify(object));
}

export function spinner(show) {
    document.getElementById("spinner").style.display = show ? "block" : "none";
}

export function dynamicEventHook(events, selector, handler) {
    if (!Array.isArray(events)) {
        events = [events];
    }

    for (const event of events) {
        document.addEventListener(event, e => {
            if (Array.from(document.querySelectorAll(selector)).indexOf(e.target) !== -1) {
                handler.call(e.target, e);
            }
        });
    }
}

export function toCamelCase(s) {
    return s.replace(/([-_][a-z])/ig, $1 => {
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
