import InitStringSearch from "./string-search";
import { init as initStringEdit } from "./string-edit";

export function init() {
    InitStringSearch();
    initStringEdit();
}

export { showString as show } from "./string-edit";
