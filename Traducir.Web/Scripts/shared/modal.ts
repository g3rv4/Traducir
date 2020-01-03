interface ModalContent {
    content: string;
}

declare class Modal {
    constructor(container: HTMLElement, content: ModalContent);
    public show(): void;
    public hide(): void;
}

const modal = (() => {
    let currentModal: Modal | undefined;
    let onClose: (() => void) | undefined;

    function modalContainer(): HTMLElement {
        const container = document.getElementById("modal-container");
        if (container) {
            return container;
        }
        throw new Error("Could not find container for modal");
    }

    function showModal(title: string, contents: string, closeCallback?: () => void) {
        hideModal();

        const modalHtml =
            `<div class="modal-header">
                <h5 class="modal-title">${title}</h5>
                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                    <span aria-hidden="true">×</span>
                </button>
            </div>
            <div class="modal-body" id="modal-body">
                ${contents}
            </div>`;

        currentModal = new Modal(modalContainer(), { content: modalHtml });
        onClose = closeCallback;

        modalContainer().addEventListener("hidden.bs.modal", _ => {
            onClose?.();
            currentModal = undefined;
            onClose = undefined;
        }, false);

        currentModal.show();
    }

    function modalContents() {
        modalContainer().querySelector(".modal-body");
    }

    function hideModal() {
        currentModal?.hide();
    }

    return {
        show: showModal,
        hide: hideModal,
        contents: modalContents
    };
})();

export default modal;
