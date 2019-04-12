function clone(object) {
    return JSON.parse(JSON.stringify(stringQueryFilters));
}

function spinner(show) {
    document.getElementById('spinner').style.display = show ? 'block' : 'none';
}

function dynamicEventHook(event, selector, handler) {
    document.addEventListener(event, e => {
        if (Array.from(document.querySelectorAll(selector)).includes(e.target)) {
            handler.call(e.target, e);
        }
    });
}
