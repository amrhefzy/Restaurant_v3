window.appShell = (() => {
  const toastHost = document.getElementById("toastHost");
  const appLoader = document.getElementById("app-loader");

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

  return {
    toast,
    showLoader,
    hideLoader,
    postJson
  };
})();

document.addEventListener("DOMContentLoaded", () => {
  document.querySelectorAll("[data-toast]").forEach((button) => {
    button.addEventListener("click", () => {
      window.appShell.toast(button.getAttribute("data-toast"));
    });
  });
});
