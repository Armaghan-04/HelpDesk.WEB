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

    // Reveal/hide password inputs (sign-in and registration forms).
    document.querySelectorAll("[data-password-toggle]").forEach((toggle) => {
        const input = document.getElementById(toggle.dataset.passwordToggle);
        if (!input) return;

        toggle.addEventListener("click", () => {
            const revealing = input.type === "password";
            input.type = revealing ? "text" : "password";

            const label = revealing ? "Hide password" : "Show password";
            toggle.setAttribute("aria-pressed", String(revealing));
            toggle.setAttribute("aria-label", label);
            toggle.setAttribute("title", label);
        });
    });

    // Demo accounts: fill the sign-in form from the selected card.
    const demoFillStatus = document.getElementById("demo-fill-status");
    document.querySelectorAll("[data-demo-email]").forEach((card) => {
        card.addEventListener("click", () => {
            const email = document.getElementById("Input_Email");
            const password = document.getElementById("Input_Password");
            if (!email || !password) return;

            const demoEmail = card.dataset.demoEmail || "";
            email.value = demoEmail;
            password.value = card.dataset.demoPassword || "";

            // Let client-side validation react to the programmatic values.
            [email, password].forEach((field) => {
                field.dispatchEvent(new Event("input", { bubbles: true }));
                field.dispatchEvent(new Event("change", { bubbles: true }));
            });

            // Keep the password hidden after filling, even if it was being revealed.
            if (password.type === "text") {
                document
                    .querySelector(`[data-password-toggle="${password.id}"]`)
                    ?.click();
            }

            document.querySelectorAll("[data-demo-email]").forEach((item) => {
                const selected = item.dataset.demoEmail === demoEmail;
                item.classList.toggle("is-selected", selected);
                item.setAttribute("aria-pressed", String(selected));
            });

            if (demoFillStatus) {
                demoFillStatus.textContent = `${card.dataset.demoRole || "Demo"} account selected.`;
            }

            document.getElementById("login-submit")?.focus();
        });
    });

    // Auto-show server-side flash toasts.
    document.querySelectorAll(".toast").forEach((toast) => new bootstrap.Toast(toast).show());
})();
