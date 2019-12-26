declare var Modal: any;

const modal = (() => {
    let currentModal = null;
    let onClose: () => void;

    function modalContainer() {
        return document.getElementById("modal-container");
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

        modalContainer().addEventListener("hidden.bs.modal", event => {
            onClose?.();
            currentModal = null;
            onClose = null;
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
