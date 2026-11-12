(() => {
    "use strict";

    // Character counters for textareas with maxlength.
    document.querySelectorAll("[data-counter]").forEach((field) => {
        const target = document.querySelector(`[data-counter-for="${field.id}"]`);
        if (!target) return;

        const max = field.getAttribute("maxlength");
        const update = () => {
            target.textContent = max ? `${field.value.length} / ${max}` : `${field.value.length}`;
            target.classList.toggle("text-danger", !!max && field.value.length >= Number(max));
        };

        field.addEventListener("input", update);
        update();
    });

    // Shared Bootstrap confirmation modal for forms carrying data-confirm.
    const modalElement = document.getElementById("confirmModal");
    if (modalElement) {
        const modal = new bootstrap.Modal(modalElement);
        const message = document.getElementById("confirmMessage");
        const accept = document.getElementById("confirmAccept");
        let pendingForm = null;

        document.querySelectorAll("form[data-confirm]").forEach((form) => {
            form.addEventListener("submit", (event) => {
                if (form.dataset.confirmed === "true") return;

                event.preventDefault();
                pendingForm = form;
                message.textContent = form.getAttribute("data-confirm") || "Are you sure?";
                modal.show();
            });
        });

        accept.addEventListener("click", () => {
            if (!pendingForm) return;

            const form = pendingForm;
            pendingForm = null;
            form.dataset.confirmed = "true";
            modal.hide();
            form.requestSubmit();
        });

        modalElement.addEventListener("hidden.bs.modal", () => {
            if (pendingForm) delete pendingForm.dataset.confirmed;
            pendingForm = null;
        });
    }

    // Auto-show server-side flash toasts.
    document.querySelectorAll(".toast").forEach((toast) => new bootstrap.Toast(toast).show());
})();
