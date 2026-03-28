(function () {
  const app = document.getElementById("posApp");
  if (!app) {
    return;
  }

  const seed = JSON.parse(app.dataset.pos || "{}");
  const categories = seed.categories || [];
  const products = seed.products || [];

  const messages = {
    addAtLeastOneItem:
      app.dataset.msgAddItem || "Add at least one item to the cart.",
    selectTableForDineIn:
      app.dataset.msgSelectTable || "Please select a table for dine-in.",
    selectPaymentType:
      app.dataset.msgSelectPayment || "Please select a payment type.",
    cartEmpty: app.dataset.msgCartEmpty || "Cart cannot be empty.",
    noProductsMatch: "No products match the current filter.",
    cartEmptyState: "Cart is empty.",
    noHeldOrders: "No held orders.",
    shortcutsHint: "Keyboard shortcuts ready.",
    showingProducts: "Showing",
    ofProducts: "of",
    productsLabel: "products",
    tableOptionalHint: "Table is optional for takeaway and delivery.",
    networkProcessError: "Network error while processing order.",
    networkHeldError: "Network error while loading held order.",
    heldLoadError: "Unable to load held order.",
    orderProcessError: "Unable to process order.",
    itemNotePrompt: "Item note",
    heldOrderSuccessPrefix: "Order held",
    submittedSuccessPrefix: "Order submitted",
    resumedSuccessPrefix: "Held order resumed"
  };

  const state = {
    activeCategoryId: null,
    search: "",
    filteredProducts: [],
    renderedProductCount: 0,
    productPageSize: 48,
    cart: [],
    heldOrders: seed.heldOrders || [],
    activeOrders: seed.activeOrders || [],
    activeCartProductId: null
  };

  const refs = {
    categories: document.getElementById("posCategories"),
    products: document.getElementById("posProducts"),
    productsMeta: document.getElementById("posProductsMeta"),
    loadMoreBtn: document.getElementById("posLoadMoreBtn"),
    validationSummary: document.getElementById("posValidationSummary"),
    cartLines: document.getElementById("cartLines"),
    cartValidationMessage: document.getElementById("cartValidationMessage"),
    cartItemCount: document.getElementById("cartItemCount"),
    subtotalValue: document.getElementById("subtotalValue"),
    taxValue: document.getElementById("taxValue"),
    totalValue: document.getElementById("totalValue"),
    search: document.getElementById("posSearch"),
    orderTypeGroup: document.getElementById("orderTypeGroup"),
    tableId: document.getElementById("tableId"),
    paymentTypeGroup: document.getElementById("paymentTypeGroup"),
    tableHint: document.getElementById("tableHint"),
    tableValidationMessage: document.getElementById("tableValidationMessage"),
    paymentValidationMessage: document.getElementById("paymentValidationMessage"),
    orderNotes: document.getElementById("orderNotes"),
    checkoutBtn: document.getElementById("checkoutBtn"),
    holdOrderBtn: document.getElementById("holdOrderBtn"),
    clearCartBtn: document.getElementById("clearCartBtn"),
    heldOrdersList: document.getElementById("heldOrdersList"),
    statusFeed: document.getElementById("posStatusFeed")
  };

  const antiForgeryToken =
    document.querySelector("input[name='__RequestVerificationToken']")?.value || "";

  function round2(value) {
    return Math.round((value + Number.EPSILON) * 100) / 100;
  }

  function money(value) {
    return Number(value || 0).toFixed(2);
  }

  function currentOrderType() {
    const checked = document.querySelector("input[name='orderType']:checked");
    return checked ? Number(checked.value) : 2;
  }

  function setOrderType(value) {
    const candidate = document.querySelector(`input[name='orderType'][value='${value}']`);
    if (candidate) {
      candidate.checked = true;
    }
    syncTableControl();
  }

  function currentPaymentType() {
    const checked = document.querySelector("input[name='paymentType']:checked");
    return checked ? Number(checked.value) : null;
  }

  function syncTableControl() {
    const dineIn = currentOrderType() === 1;
    refs.tableId.disabled = !dineIn;
    refs.tableHint.classList.toggle("text-danger", dineIn);
    refs.tableHint.textContent = dineIn
      ? messages.selectTableForDineIn
      : messages.tableOptionalHint;

    if (!dineIn) {
      refs.tableId.value = "";
      refs.tableId.classList.remove("is-invalid");
      refs.tableValidationMessage.textContent = "";
      refs.tableValidationMessage.classList.add("d-none");
      refs.orderTypeGroup.classList.remove("pos-invalid");
    }
  }

  function clearValidationState() {
    refs.validationSummary.classList.add("d-none");
    refs.validationSummary.textContent = "";
    refs.tableId.classList.remove("is-invalid");
    refs.orderTypeGroup.classList.remove("pos-invalid");
    refs.tableValidationMessage.classList.add("d-none");
    refs.tableValidationMessage.textContent = "";
    refs.paymentTypeGroup.classList.remove("pos-invalid");
    refs.paymentValidationMessage.classList.add("d-none");
    refs.paymentValidationMessage.textContent = "";
    refs.cartValidationMessage.classList.add("d-none");
    refs.cartValidationMessage.textContent = "";
  }

  function showValidationErrors(errors) {
    if (!errors.length) {
      return;
    }

    refs.validationSummary.textContent = errors.join(" ");
    refs.validationSummary.classList.remove("d-none");
  }

  function validateBeforeSubmit(showErrors) {
    const errors = [];
    const dineIn = currentOrderType() === 1;
    const hasItems = state.cart.length > 0;
    const isHoldMode = false;
    const paymentType = currentPaymentType();

    if (!hasItems) {
      errors.push(messages.addAtLeastOneItem);
      if (showErrors) {
        refs.cartValidationMessage.textContent = messages.cartEmpty;
        refs.cartValidationMessage.classList.remove("d-none");
      }
    }

    if (dineIn && !refs.tableId.value) {
      errors.push(messages.selectTableForDineIn);
      if (showErrors) {
        refs.tableId.classList.add("is-invalid");
        refs.orderTypeGroup.classList.add("pos-invalid");
        refs.tableValidationMessage.textContent = messages.selectTableForDineIn;
        refs.tableValidationMessage.classList.remove("d-none");
      }
    }

    if (!isHoldMode && !paymentType) {
      errors.push(messages.selectPaymentType);
      if (showErrors) {
        refs.paymentTypeGroup.classList.add("pos-invalid");
        refs.paymentValidationMessage.textContent = messages.selectPaymentType;
        refs.paymentValidationMessage.classList.remove("d-none");
      }
    }

    if (!showErrors) {
      return errors.length === 0;
    }

    showValidationErrors(errors);
    return errors.length === 0;
  }

  function filteredProducts() {
    const q = state.search.trim().toLowerCase();

    return products.filter((p) => {
      if (!p.isActive) {
        return false;
      }

      if (state.activeCategoryId && p.categoryId !== state.activeCategoryId) {
        return false;
      }

      if (!q) {
        return true;
      }

      return (
        (p.nameEn || "").toLowerCase().includes(q) ||
        (p.nameAr || "").toLowerCase().includes(q) ||
        (p.sku || "").toLowerCase().includes(q)
      );
    });
  }

  function refreshProductResults() {
    state.filteredProducts = filteredProducts();
    state.renderedProductCount = Math.min(
      state.productPageSize,
      state.filteredProducts.length
    );
    renderProducts();
  }

  function loadMoreProducts() {
    if (state.renderedProductCount >= state.filteredProducts.length) {
      return;
    }

    state.renderedProductCount = Math.min(
      state.renderedProductCount + state.productPageSize,
      state.filteredProducts.length
    );
    renderProducts();
  }

  function cartTotals() {
    const subtotal = round2(
      state.cart.reduce((sum, item) => sum + item.quantity * item.salePrice, 0)
    );
    const tax = round2(subtotal * 0.15);
    const total = round2(subtotal + tax);

    return { subtotal, tax, total };
  }

  function renderCategories() {
    const html = [
      `<button type="button" class="pos-category-chip ${
        state.activeCategoryId ? "" : "active"
      }" data-category-id="">All</button>`
    ];

    categories.forEach((category) => {
      html.push(
        `<button type="button" class="pos-category-chip ${
          state.activeCategoryId === category.id ? "active" : ""
        }" data-category-id="${category.id}">${category.nameEn}</button>`
      );
    });

    refs.categories.innerHTML = html.join("");
  }

  function renderProducts() {
    const list = state.filteredProducts.slice(0, state.renderedProductCount);
    if (list.length === 0) {
      refs.products.innerHTML = `<div class="empty-state">${messages.noProductsMatch}</div>`;
      refs.productsMeta.textContent = `${messages.showingProducts} 0 ${messages.productsLabel}`;
      refs.loadMoreBtn.classList.add("d-none");
      return;
    }

    refs.products.innerHTML = list
      .map(
        (product) => `<button type="button" class="pos-product-card" data-product-id="${product.id}">
            <span class="sku">${product.sku}</span>
            <strong class="name">${product.nameEn}</strong>
            <small class="name-alt">${product.nameAr}</small>
            <span class="price">${money(product.salePrice)}</span>
          </button>`
      )
      .join("");

    refs.productsMeta.textContent = `${messages.showingProducts} ${list.length} ${messages.ofProducts} ${state.filteredProducts.length} ${messages.productsLabel}`;
    const hasMore = state.renderedProductCount < state.filteredProducts.length;
    refs.loadMoreBtn.classList.toggle("d-none", !hasMore);
  }

  function renderCart() {
    if (state.cart.length === 0) {
      refs.cartLines.innerHTML = `<div class="empty-state">${messages.cartEmptyState}</div>`;
      state.activeCartProductId = null;
    } else {
      if (
        !state.activeCartProductId ||
        !state.cart.some((x) => x.productId === state.activeCartProductId)
      ) {
        state.activeCartProductId = state.cart[0].productId;
      }

      refs.cartLines.innerHTML = state.cart
        .map((item) => {
          const lineTotal = round2(item.quantity * item.salePrice);
          return `<div class="cart-line ${
            item.productId === state.activeCartProductId ? "active" : ""
          }" data-product-id="${item.productId}">
              <div class="cart-line-main">
                <strong>${item.name}</strong>
                <small>Unit ${item.salePrice.toFixed(2)} | Line ${lineTotal.toFixed(2)}</small>
                ${
                  item.note
                    ? `<span class="cart-note">${item.note}</span>`
                    : `<span class="cart-note muted">No note</span>`
                }
              </div>
              <div class="cart-line-actions">
                <button type="button" class="btn btn-sm btn-light qty-dec" title="Decrease quantity">-</button>
                <input type="number" class="form-control form-control-sm qty-input" value="${item.quantity}" min="0.001" step="0.001" />
                <button type="button" class="btn btn-sm btn-light qty-inc" title="Increase quantity">+</button>
                <button type="button" class="btn btn-sm btn-outline-secondary add-note" title="Add note">Note</button>
                <button type="button" class="btn btn-sm btn-danger remove-item" title="Remove item">Remove</button>
              </div>
            </div>`;
        })
        .join("");
    }

    const totals = cartTotals();
    const quantitySum = state.cart.reduce((sum, item) => sum + item.quantity, 0);
    refs.cartItemCount.textContent = `${quantitySum} item(s)`;
    refs.subtotalValue.textContent = money(totals.subtotal);
    refs.taxValue.textContent = money(totals.tax);
    refs.totalValue.textContent = money(totals.total);

    const validWithoutMessages = validateBeforeSubmit(false);
    refs.checkoutBtn.disabled = !validWithoutMessages;
    refs.holdOrderBtn.disabled = !validWithoutMessages;
    refs.clearCartBtn.disabled = state.cart.length === 0;
  }

  function renderHeldOrders() {
    if (!state.heldOrders.length) {
      refs.heldOrdersList.innerHTML = `<div class="empty-state">${messages.noHeldOrders}</div>`;
      return;
    }

    refs.heldOrdersList.innerHTML = state.heldOrders
      .map(
        (held) => `<div class="held-order-item">
            <div>
              <strong>${held.heldReference}</strong>
              <small>${money(held.total)}</small>
            </div>
            <button type="button" class="btn btn-sm btn-outline-dark resume-held" data-held-id="${held.salesOrderId}">
              Resume
            </button>
          </div>`
      )
      .join("");
  }

  function renderStatusFeed() {
    if (!refs.statusFeed) {
      return;
    }

    if (!state.activeOrders.length) {
      refs.statusFeed.innerHTML = `<div class="empty-state">${messages.noHeldOrders}</div>`;
      return;
    }

    refs.statusFeed.innerHTML = state.activeOrders
      .map(
        (order) => `<div class="held-order-item ${order.isReady ? "status-ready" : ""}">
            <div>
              <strong>${order.orderNumber}</strong>
              <small>${order.status}</small>
            </div>
            <span class="badge ${order.isReady ? "text-bg-success" : "text-bg-secondary"}">${order.status}</span>
          </div>`
      )
      .join("");
  }

  function addToCart(productId) {
    const product = products.find((p) => p.id === productId);
    if (!product) {
      return;
    }

    const line = state.cart.find((x) => x.productId === productId);
    if (line) {
      line.quantity += 1;
    } else {
      state.cart.push({
        productId: product.id,
        name: product.nameEn,
        salePrice: Number(product.salePrice),
        quantity: 1,
        note: ""
      });
    }

    state.activeCartProductId = productId;
    renderCart();
  }

  function changeQty(productId, delta) {
    const line = state.cart.find((x) => x.productId === productId);
    if (!line) {
      return;
    }

    line.quantity = round2(line.quantity + delta);
    if (line.quantity <= 0) {
      state.cart = state.cart.filter((x) => x.productId !== productId);
    }

    renderCart();
  }

  function removeItem(productId) {
    state.cart = state.cart.filter((x) => x.productId !== productId);
    if (state.activeCartProductId === productId) {
      state.activeCartProductId = state.cart[0]?.productId || null;
    }
    renderCart();
  }

  function editLineNote(productId) {
    const line = state.cart.find((x) => x.productId === productId);
    if (!line) {
      return;
    }

    const note = window.prompt(messages.itemNotePrompt, line.note || "");
    if (note === null) {
      return;
    }

    line.note = note.trim();
    renderCart();
  }

  function clearCart() {
    state.cart = [];
    state.activeCartProductId = null;
    refs.orderNotes.value = "";
    renderCart();
  }

  function isTypingTarget(target) {
    if (!(target instanceof HTMLElement)) {
      return false;
    }

    if (target.isContentEditable) {
      return true;
    }

    const tag = target.tagName;
    return tag === "INPUT" || tag === "TEXTAREA" || tag === "SELECT";
  }

  function getShortcutTargetProductId() {
    if (state.activeCartProductId) {
      return state.activeCartProductId;
    }

    return state.cart[0]?.productId || null;
  }

  function createPayload(isHold) {
    if (state.cart.length === 0) {
      throw new Error(messages.addAtLeastOneItem);
    }

    const orderType = currentOrderType();
    const tableId = refs.tableId.value || null;
    if (orderType === 1 && !tableId) {
      throw new Error(messages.selectTableForDineIn);
    }

    const totals = cartTotals();
    const paymentType = isHold ? null : currentPaymentType();
    if (!isHold && !paymentType) {
      throw new Error(messages.selectPaymentType);
    }

    return {
      orderType,
      tableId,
      paymentType,
      notes: refs.orderNotes.value || null,
      discountAmount: 0,
      taxAmount: totals.tax,
      items: state.cart.map((line) => ({
        productId: line.productId,
        quantity: line.quantity,
        note: line.note || null
      })),
      isHold
    };
  }

  async function submitOrder(isHold) {
    clearValidationState();
    if (!validateBeforeSubmit(true)) {
      return;
    }

    let payload;
    try {
      payload = createPayload(isHold);
    } catch (error) {
      window.appShell.toast(error.message, "danger");
      return;
    }

    const endpoint = isHold ? "/POS/Hold" : "/POS/Checkout";
    refs.checkoutBtn.disabled = true;
    refs.holdOrderBtn.disabled = true;
    window.appShell.showLoader();

    try {
      const response = await fetch(endpoint, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          RequestVerificationToken: antiForgeryToken
        },
        body: JSON.stringify(payload)
      });

      const result = await response.json();
      if (!response.ok || !result.success) {
        const message = result.message || messages.orderProcessError;
        refs.validationSummary.textContent = message;
        refs.validationSummary.classList.remove("d-none");
        window.appShell.toast(message, "danger");
        return;
      }

      if (isHold) {
        state.heldOrders.unshift({
          salesOrderId: result.data.salesOrderId,
          heldReference: result.data.heldReference || result.data.orderNumber,
          total: result.data.total
        });
        renderHeldOrders();
      }

      clearCart();
      refs.orderNotes.value = "";
      setOrderType(2);
      clearValidationState();

      const label = isHold
        ? `${messages.heldOrderSuccessPrefix} (${result.data.heldReference})`
        : `${messages.submittedSuccessPrefix} (${result.data.orderNumber})`;
      window.appShell.toast(label, "success");
    } catch (error) {
      window.appShell.toast(messages.networkProcessError, "danger");
    } finally {
      window.appShell.hideLoader();
      renderCart();
    }
  }

  async function resumeHeldOrder(salesOrderId) {
    window.appShell.showLoader();

    try {
      const response = await fetch(`/POS/Held/${salesOrderId}`);
      const result = await response.json();
      if (!response.ok || !result.success) {
        window.appShell.toast(result.message || messages.heldLoadError, "danger");
        return;
      }

      const held = result.data;
      state.cart = (held.items || []).map((item) => ({
        productId: item.productId,
        name: item.productName,
        salePrice: Number(item.unitPrice),
        quantity: Number(item.quantity),
        note: item.note || ""
      }));

      setOrderType(Number(held.orderType || 2));
      refs.tableId.value = held.tableId || "";
      refs.orderNotes.value = held.notes || "";

      renderCart();
      window.appShell.toast(
        `${messages.resumedSuccessPrefix} (${held.heldReference || held.orderNumber})`,
        "success"
      );
    } catch (error) {
      window.appShell.toast(messages.networkHeldError, "danger");
    } finally {
      window.appShell.hideLoader();
    }
  }

  async function refreshStatusFeed() {
    if (!refs.statusFeed) {
      return;
    }

    try {
      const response = await fetch("/POS/StatusFeed");
      const json = await response.json();
      if (!response.ok || !json.success) {
        return;
      }

      state.activeOrders = json.data || [];
      renderStatusFeed();
    } catch (_error) {
      // Silent polling failure to avoid interrupting cashier flow.
    }
  }

  refs.categories.addEventListener("click", (event) => {
    const button = event.target.closest("[data-category-id]");
    if (!button) {
      return;
    }

    const value = button.getAttribute("data-category-id");
    state.activeCategoryId = value || null;
    renderCategories();
    refreshProductResults();
  });

  refs.products.addEventListener("click", (event) => {
    const button = event.target.closest("[data-product-id]");
    if (!button) {
      return;
    }

    addToCart(button.getAttribute("data-product-id"));
  });

  refs.cartLines.addEventListener("click", (event) => {
    const row = event.target.closest("[data-product-id]");
    if (!row) {
      return;
    }

    const productId = row.getAttribute("data-product-id");
    state.activeCartProductId = productId;

    if (event.target.classList.contains("qty-inc")) {
      changeQty(productId, 1);
    } else if (event.target.classList.contains("qty-dec")) {
      changeQty(productId, -1);
    } else if (event.target.classList.contains("remove-item")) {
      removeItem(productId);
    } else if (event.target.classList.contains("add-note")) {
      editLineNote(productId);
    }
  });

  refs.cartLines.addEventListener("input", (event) => {
    if (!event.target.classList.contains("qty-input")) {
      return;
    }

    const row = event.target.closest("[data-product-id]");
    if (!row) {
      return;
    }

    const productId = row.getAttribute("data-product-id");
    const parsed = Number(event.target.value);
    if (!Number.isFinite(parsed)) {
      return;
    }

    const line = state.cart.find((x) => x.productId === productId);
    if (!line) {
      return;
    }

    if (parsed <= 0) {
      removeItem(productId);
      return;
    }

    line.quantity = round2(parsed);
    renderCart();
  });

  refs.heldOrdersList.addEventListener("click", (event) => {
    const button = event.target.closest(".resume-held");
    if (!button) {
      return;
    }

    resumeHeldOrder(button.getAttribute("data-held-id"));
  });

  refs.search.addEventListener("input", () => {
    state.search = refs.search.value || "";
    window.clearTimeout(refs.search._debounceTimerId);
    refs.search._debounceTimerId = window.setTimeout(() => {
      refreshProductResults();
    }, 120);
  });

  refs.loadMoreBtn.addEventListener("click", loadMoreProducts);

  if ("IntersectionObserver" in window) {
    const observer = new IntersectionObserver((entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting && !refs.loadMoreBtn.classList.contains("d-none")) {
          loadMoreProducts();
        }
      });
    });

    observer.observe(refs.loadMoreBtn);
  }

  document.addEventListener("keydown", (event) => {
    const target = event.target;
    const typing = isTypingTarget(target);

    if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "f") {
      event.preventDefault();
      refs.search.focus();
      refs.search.select();
      return;
    }

    if (typing) {
      return;
    }

    if (event.key === "Enter") {
      event.preventDefault();
      submitOrder(false);
      return;
    }

    if (event.key === "Escape") {
      event.preventDefault();
      clearCart();
      return;
    }

    const targetProductId = getShortcutTargetProductId();
    if (!targetProductId) {
      return;
    }

    const isPlus = event.key === "+" || event.key === "=" || event.code === "NumpadAdd";
    if (isPlus) {
      event.preventDefault();
      changeQty(targetProductId, 1);
      return;
    }

    const isMinus = event.key === "-" || event.code === "NumpadSubtract";
    if (isMinus) {
      event.preventDefault();
      changeQty(targetProductId, -1);
    }
  });

  document.querySelectorAll("input[name='orderType']").forEach((input) => {
    input.addEventListener("change", () => {
      clearValidationState();
      syncTableControl();
      renderCart();
    });
  });

  refs.tableId.addEventListener("change", () => {
    clearValidationState();
    renderCart();
  });

  document.querySelectorAll("input[name='paymentType']").forEach((input) => {
    input.addEventListener("change", () => {
      clearValidationState();
      renderCart();
    });
  });

  refs.checkoutBtn.addEventListener("click", () => submitOrder(false));
  refs.holdOrderBtn.addEventListener("click", () => submitOrder(true));
  refs.clearCartBtn.addEventListener("click", clearCart);

  renderCategories();
  refreshProductResults();
  renderCart();
  renderHeldOrders();
  renderStatusFeed();
  syncTableControl();
  clearValidationState();

  window.setInterval(refreshStatusFeed, 10000);
})();
