(function () {
    Array.prototype.isArray = true;
})();

function clone(object) {
    return JSON.parse(JSON.stringify(object));
}

function spinner(show) {
    document.getElementById('spinner').style.display = show ? 'block' : 'none';
}

function dynamicEventHook(events, selector, handler) {
    if (!events.isArray) events = [events];

    events.forEach(event =>
        document.addEventListener(event, e => {
            if (Array.from(document.querySelectorAll(selector)).includes(e.target)) {
                handler.call(e.target, e);
            }
        })
    );
}

const toCamelCase = (s) => {
    return s.replace(/([-_][a-z])/ig, ($1) => {
        return $1.toUpperCase()
            .replace('-', '')
            .replace('_', '');
    });
};
