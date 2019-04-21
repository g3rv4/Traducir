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

    events.forEach(event =>
        document.addEventListener(event, e => {
            if (Array.from(document.querySelectorAll(selector)).indexOf(e.target) === -1) {
                handler.call(e.target, e);
            }
        })
    );
}

export function toCamelCase(s) {
    return s.replace(/([-_][a-z])/ig, $1 => {
        return $1.toUpperCase()
            .replace("-", "")
            .replace("_", "");
    });
}
