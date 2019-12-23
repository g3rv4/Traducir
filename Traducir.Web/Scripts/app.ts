import { init as initAjax } from "./shared/ajax";

export function init(csrfToken: string) {
    initAjax(csrfToken);
    initLogInLogOutLinks();
}

function initLogInLogOutLinks() {
    const elements = Array.from(document.getElementsByClassName("js-add-return-url"));
    for (const element of elements) {
        element.addEventListener("click", e => {
            const target = e.target as HTMLAnchorElement;
            window.location.href = `${target.href}?returnUrl=${encodeURIComponent(document.location.href)}`;
            return false;
        });
    }
}
