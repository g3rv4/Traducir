function clone(object) {
    return JSON.parse(JSON.stringify(stringQueryFilters));
}

function spinner(show) {
    document.getElementById('spinner').style.display = show ? 'block' : 'none';
}
