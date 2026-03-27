window.appShell = (() => {
  const toastHost = document.getElementById("toastHost");
  const appLoader = document.getElementById("app-loader");
  const confirmModalElement = document.getElementById("confirmActionModal");
  const confirmMessage = document.getElementById("confirmActionMessage");
  const confirmActionBtn = document.getElementById("confirmActionBtn");
  const confirmModal =
    confirmModalElement && window.bootstrap
      ? bootstrap.Modal.getOrCreateInstance(confirmModalElement)
      : null;

  const showLoader = () => {
    if (!appLoader) return;
    appLoader.classList.remove("d-none");
  };

  const hideLoader = () => {
    if (!appLoader) return;
    appLoader.classList.add("d-none");
  };

  const toast = (message, tone = "success") => {
    if (!toastHost || !message) return;

    const wrapper = document.createElement("div");
    wrapper.className = `toast align-items-center text-bg-${tone} border-0 mb-2`;
    wrapper.role = "alert";
    wrapper.ariaLive = "assertive";
    wrapper.ariaAtomic = "true";
    wrapper.innerHTML = `<div class="d-flex"><div class="toast-body">${message}</div><button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button></div>`;

    toastHost.appendChild(wrapper);
    const instance = bootstrap.Toast.getOrCreateInstance(wrapper, { delay: 2400 });
    instance.show();
    wrapper.addEventListener("hidden.bs.toast", () => wrapper.remove());
  };

  const postJson = async (url, payload) => {
    const csrf =
      document.querySelector("input[name='__RequestVerificationToken']")?.value || "";

    showLoader();
    try {
      const response = await fetch(url, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "X-Requested-With": "XMLHttpRequest",
          RequestVerificationToken: csrf
        },
        body: JSON.stringify(payload)
      });

      const json = await response.json();
      if (!response.ok) {
        const message = json?.message || "Request failed.";
        toast(message, "danger");
      }

      return json;
    } finally {
      hideLoader();
    }
  };

  const confirm = ({
    message,
    onConfirm,
    confirmText,
    confirmClass = "btn-danger"
  }) => {
    if (!confirmModal || !confirmActionBtn || !confirmMessage) {
      if (window.confirm(message || "Are you sure?")) {
        onConfirm?.();
      }
      return;
    }

    confirmMessage.textContent = message || "Are you sure you want to continue?";
    confirmActionBtn.textContent = confirmText || confirmActionBtn.dataset.defaultText || confirmActionBtn.textContent;
    confirmActionBtn.className = `btn ${confirmClass}`;
    confirmActionBtn.onclick = () => {
      confirmModal.hide();
      onConfirm?.();
    };
    confirmModal.show();
  };

  return {
    toast,
    showLoader,
    hideLoader,
    postJson,
    confirm
  };
})();

document.addEventListener("DOMContentLoaded", () => {
  const defaultConfirmText = document.getElementById("confirmActionBtn")?.textContent;
  const confirmBtn = document.getElementById("confirmActionBtn");
  if (confirmBtn && !confirmBtn.dataset.defaultText) {
    confirmBtn.dataset.defaultText = defaultConfirmText || "Confirm";
  }

  document.querySelectorAll("[data-toast]").forEach((button) => {
    button.addEventListener("click", () => {
      window.appShell.toast(button.getAttribute("data-toast"));
    });
  });

  document.querySelectorAll("form").forEach((form) => {
    form.addEventListener("submit", (event) => {
      const submitter = event.submitter;
      const confirmMessage = submitter?.getAttribute("data-confirm") || form.getAttribute("data-confirm");

      if (confirmMessage && !form.dataset.confirmed) {
        event.preventDefault();
        window.appShell.confirm({
          message: confirmMessage,
          confirmText: submitter?.getAttribute("data-confirm-text") || undefined,
          confirmClass: submitter?.getAttribute("data-confirm-class") || "btn-danger",
          onConfirm: () => {
            form.dataset.confirmed = "true";
            form.requestSubmit(submitter || undefined);
          }
        });
        return;
      }

      form.dataset.confirmed = "";

      if ((form.method || "get").toLowerCase() !== "get") {
        window.appShell.showLoader();
        submitter?.setAttribute("disabled", "disabled");
      }
    });
  });

  window.addEventListener("pageshow", () => {
    window.appShell.hideLoader();
    document.querySelectorAll("form button[disabled], form input[type='submit'][disabled]").forEach((button) => {
      button.removeAttribute("disabled");
    });
  });
});
