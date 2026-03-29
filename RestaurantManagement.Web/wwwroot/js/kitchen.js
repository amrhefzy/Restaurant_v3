(function () {
  const app = document.getElementById("kitchenApp");
  if (!app) {
    return;
  }

  const board = document.getElementById("kitchenBoard");
  const antiForgeryToken =
    document.querySelector("input[name='__RequestVerificationToken']")?.value || "";

  const labels = {
    noActive: app.dataset.noActive || "No active orders.",
    start: app.dataset.start || "Start",
    ready: app.dataset.ready || "Mark Ready",
    complete: app.dataset.complete || "Complete",
    startConfirm: app.dataset.startConfirm || "Start this kitchen order now?",
    readyConfirm: app.dataset.readyConfirm || "Mark this kitchen order as ready?",
    completeConfirm: app.dataset.completeConfirm || "Complete this kitchen order now?"
  };

  let orders = JSON.parse(app.dataset.seed || "[]");
  const warningMinutes = 6;
  const criticalMinutes = 10;
  const newHighlightDurationMs = 25000;
  let knownOrderIds = new Set(orders.map((order) => order.salesOrderId));
  let newOrderUntil = new Map();
  let audioContext = null;

  function getAgeClass(minutesSinceCreated) {
    if (minutesSinceCreated > criticalMinutes) {
      return "age-red";
    }

    if (minutesSinceCreated >= warningMinutes) {
      return "age-yellow";
    }

    return "age-green";
  }

  function unlockAudio() {
    if (audioContext) {
      return;
    }

    const AudioCtx = window.AudioContext || window.webkitAudioContext;
    if (!AudioCtx) {
      return;
    }

    audioContext = new AudioCtx();
    if (audioContext.state === "suspended") {
      audioContext.resume().catch(() => {});
    }
  }

  function playNewOrderAlert() {
    try {
      if (!audioContext) {
        unlockAudio();
      }

      if (!audioContext) {
        return;
      }

      const now = audioContext.currentTime;
      const sequence = [880, 1175];
      sequence.forEach((frequency, index) => {
        const oscillator = audioContext.createOscillator();
        const gainNode = audioContext.createGain();
        oscillator.type = "sine";
        oscillator.frequency.value = frequency;
        oscillator.connect(gainNode);
        gainNode.connect(audioContext.destination);
        const startAt = now + index * 0.14;
        gainNode.gain.setValueAtTime(0.001, startAt);
        gainNode.gain.exponentialRampToValueAtTime(0.12, startAt + 0.015);
        gainNode.gain.exponentialRampToValueAtTime(0.001, startAt + 0.11);
        oscillator.start(startAt);
        oscillator.stop(startAt + 0.12);
      });
    } catch {
      // Keep polling/flow resilient if browser blocks audio.
    }
  }

  function render() {
    if (!orders.length) {
      board.innerHTML = `<div class="empty-state">${labels.noActive}</div>`;
      return;
    }

    board.innerHTML = orders
      .map((order) => {
        const items = (order.items || [])
          .map(
            (item) =>
              `<li><strong>${item.productName}</strong> x ${item.quantity}${
                item.note ? ` <small class="text-muted">(${item.note})</small>` : ""
              }</li>`
          )
          .join("");

        const canStart = order.status === "New";
        const canReady = order.status === "InKitchen" || order.status === "New";
        const canComplete = order.status === "Ready";

        const ageClass = getAgeClass(order.minutesSinceCreated);
        const isOverdue = order.minutesSinceCreated > criticalMinutes;

        const isNewArrival = (newOrderUntil.get(order.salesOrderId) || 0) > Date.now();

        return `<article class="kitchen-order-card ${
          order.status === "Ready" ? "ready" : ""
        } ${ageClass} ${isOverdue ? "overdue" : ""} ${isNewArrival ? "new-arrival" : ""}" data-order-id="${order.salesOrderId}">
            <header class="kitchen-order-head">
              <div>
                <h5>${order.orderNumber}</h5>
                <small>${order.status}</small>
              </div>
              <span class="kitchen-age-badge">${order.minutesSinceCreated} min</span>
            </header>
            <ul class="kitchen-items">${items}</ul>
            ${order.notes ? `<p class="kitchen-note">${order.notes}</p>` : ""}
            <div class="kitchen-actions">
              <button type="button" class="btn btn-lg btn-outline-dark start-btn" ${
                canStart ? "" : "disabled"
              }>${labels.start}</button>
              <button type="button" class="btn btn-lg btn-primary ready-btn" ${
                canReady ? "" : "disabled"
              }>${labels.ready}</button>
              <button type="button" class="btn btn-lg btn-success complete-btn" ${
                canComplete ? "" : "disabled"
              }>${labels.complete}</button>
            </div>
          </article>`;
      })
      .join("");
  }

  async function callAction(url, salesOrderId) {
    window.appShell.showLoader();
    try {
      const response = await fetch(url, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          RequestVerificationToken: antiForgeryToken
        },
        body: JSON.stringify(salesOrderId)
      });

      const json = await response.json();
      if (!response.ok || !json.success) {
        window.appShell.toast(json.message || "Action failed.", "danger");
        return false;
      }

      if (json.message) {
        window.appShell.toast(json.message, "success");
      }

      return true;
    } finally {
      window.appShell.hideLoader();
    }
  }

  async function refresh() {
    const response = await fetch("/Kitchen/Feed");
    const json = await response.json();
    if (!response.ok || !json.success) {
      return;
    }

    const incoming = json.data || [];
    const incomingIds = new Set(incoming.map((order) => order.salesOrderId));
    const newOrders = incoming.filter((order) => !knownOrderIds.has(order.salesOrderId));
    if (newOrders.length) {
      const highlightUntil = Date.now() + newHighlightDurationMs;
      newOrders.forEach((order) => newOrderUntil.set(order.salesOrderId, highlightUntil));
      playNewOrderAlert();
    }

    knownOrderIds = incomingIds;
    orders = incoming;
    render();
  }

  board.addEventListener("click", async (event) => {
    const card = event.target.closest("[data-order-id]");
    if (!card) {
      return;
    }

    const salesOrderId = card.getAttribute("data-order-id");

    if (event.target.classList.contains("start-btn")) {
      window.appShell.confirm({
        message: labels.startConfirm,
        confirmText: labels.start,
        confirmClass: "btn-outline-dark",
        onConfirm: async () => {
          const ok = await callAction("/Kitchen/Start", salesOrderId);
          if (ok) {
            await refresh();
          }
        }
      });
      return;
    }

    if (event.target.classList.contains("ready-btn")) {
      window.appShell.confirm({
        message: labels.readyConfirm,
        confirmText: labels.ready,
        confirmClass: "btn-primary",
        onConfirm: async () => {
          const ok = await callAction("/Kitchen/Ready", salesOrderId);
          if (ok) {
            await refresh();
          }
        }
      });
      return;
    }

    if (event.target.classList.contains("complete-btn")) {
      window.appShell.confirm({
        message: labels.completeConfirm,
        confirmText: labels.complete,
        confirmClass: "btn-success",
        onConfirm: async () => {
          const ok = await callAction("/Kitchen/Complete", salesOrderId);
          if (ok) {
            await refresh();
          }
        }
      });
    }
  });

  window.addEventListener("pointerdown", unlockAudio, { once: true });
  window.addEventListener("keydown", unlockAudio, { once: true });

  render();
  window.setInterval(refresh, 10000);
})();
