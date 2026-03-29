(() => {
  const app = document.getElementById("posApp");
  if (!app) return;

  const refs = {
    categories: document.getElementById("posCategories"),
    products: document.getElementById("posProducts"),
    productsMeta: document.getElementById("posProductsMeta"),
    loadMoreBtn: document.getElementById("posLoadMoreBtn"),
    search: document.getElementById("posSearch"),
    cartLines: document.getElementById("cartLines"),
    cartItemCount: document.getElementById("cartItemCount"),
    clearCartBtn: document.getElementById("clearCartBtn"),
    subtotalValue: document.getElementById("subtotalValue"),
    taxValue: document.getElementById("taxValue"),
    serviceChargeValue: document.getElementById("serviceChargeValue"),
    totalValue: document.getElementById("totalValue"),
    orderTypeGroup: document.getElementById("orderTypeGroup"),
    tableId: document.getElementById("tableId"),
    tableLabel: document.getElementById("tableLabel"),
    tableHint: document.getElementById("tableHint"),
    orderNotes: document.getElementById("orderNotes"),
    paymentTypeGroup: document.getElementById("paymentTypeGroup"),
    checkoutBtn: document.getElementById("checkoutBtn"),
    holdOrderBtn: document.getElementById("holdOrderBtn"),
    heldOrdersList: document.getElementById("heldOrdersList"),
    statusFeed: document.getElementById("posStatusFeed"),
    validationSummary: document.getElementById("posValidationSummary"),
    cartValidationMessage: document.getElementById("cartValidationMessage"),
    tableValidationMessage: document.getElementById("tableValidationMessage"),
    paymentValidationMessage: document.getElementById("paymentValidationMessage")
  };

  const data = JSON.parse(app.dataset.pos || "{}");
  const messages = {
    addItem: app.dataset.msgAddItem || "Add at least one item.",
    selectTable: app.dataset.msgSelectTable || "Select a table for dine-in orders.",
    cartEmpty: app.dataset.msgCartEmpty || "Cart is empty.",
    selectPayment: app.dataset.msgSelectPayment || "Select a payment type."
  };

  const currencyCode = app.dataset.currencyCode || "EGP";
  const taxRate = Number.parseFloat(app.dataset.taxRate || "15") / 100;
  const serviceChargeRate = Number.parseFloat(app.dataset.serviceChargeRate || "0") / 100;
  const token = app.querySelector('input[name="__RequestVerificationToken"]')?.value ?? "";

  const state = {
    products: data.products || [],
    tables: data.tables || [],
    heldOrders: data.heldOrders || [],
    statuses: [],
    activeCategory: null,
    searchTerm: "",
    cart: [],
    visibleCount: 16
  };

  function round2(value) {
    return Math.round((value + Number.EPSILON) * 100) / 100;
  }

  function money(value) {
    return round2(value).toFixed(2);
  }

  function getSelectedOrderType() {
    return Number(document.querySelector('input[name="orderType"]:checked')?.value || 2);
  }

  function getSelectedPaymentType() {
    return Number(document.querySelector('input[name="paymentType"]:checked')?.value || 1);
  }

  function setValidation(message) {
    if (!refs.validationSummary) return;
    refs.validationSummary.textContent = message || "";
    refs.validationSummary.classList.toggle("d-none", !message);
  }

  function setFieldValidation(target, message) {
    if (!target) return;
    target.textContent = message || "";
    target.classList.toggle("d-none", !message);
  }

  function clearFieldValidation() {
    setFieldValidation(refs.cartValidationMessage, "");
    setFieldValidation(refs.tableValidationMessage, "");
    setFieldValidation(refs.paymentValidationMessage, "");
  }

  function renderCategories() {
    const categories = Array.from(new Map(state.products.map(x => [x.categoryId, x.categoryName || "Uncategorized"]))).map(([id, name]) => ({ id, name }));
    const allActive = state.activeCategory === null ? "active" : "";
    refs.categories.innerHTML = `
      <button type="button" class="btn btn-sm btn-outline-dark ${allActive}" data-category="">All</button>
      ${categories.map(cat => `<button type="button" class="btn btn-sm btn-outline-dark ${state.activeCategory === String(cat.id) ? "active" : ""}" data-category="${cat.id}">${cat.name}</button>`).join("")}
    `;
  }

  function getFilteredProducts() {
    const term = state.searchTerm.trim().toLowerCase();
    return state.products.filter(product => {
      const matchesCategory = state.activeCategory === null || String(product.categoryId) === state.activeCategory;
      const haystack = `${product.name} ${product.categoryName || ""}`.toLowerCase();
      const matchesSearch = !term || haystack.includes(term);
      return matchesCategory && matchesSearch;
    });
  }

  function renderProducts() {
    const filtered = getFilteredProducts();
    const visible = filtered.slice(0, state.visibleCount);
    refs.products.innerHTML = visible.map(product => `
      <button type="button" class="pos-product-card" data-product-id="${product.id}">
        <div class="pos-product-name">${product.name}</div>
        <div class="pos-product-meta">${product.categoryName || "—"}</div>
        <div class="pos-product-price">${currencyCode} ${money(product.price || 0)}</div>
      </button>
    `).join("");

    refs.productsMeta.textContent = `${visible.length} / ${filtered.length}`;
    refs.loadMoreBtn.classList.toggle("d-none", visible.length >= filtered.length);
  }

  function getCartItem(productId) {
    return state.cart.find(item => item.productId === productId);
  }

  function addToCart(productId) {
    const product = state.products.find(x => x.id === productId);
    if (!product) return;

    const existing = getCartItem(productId);
    if (existing) {
      existing.quantity += 1;
      existing.lineTotal = round2(existing.quantity * existing.unitPrice);
    } else {
      state.cart.push({
        productId: product.id,
        name: product.name,
        quantity: 1,
        unitPrice: round2(product.price || 0),
        lineTotal: round2(product.price || 0)
      });
    }

    renderCart();
  }

  function updateQuantity(productId, delta) {
    const item = getCartItem(productId);
    if (!item) return;

    item.quantity += delta;
    if (item.quantity <= 0) {
      state.cart = state.cart.filter(x => x.productId !== productId);
    } else {
      item.lineTotal = round2(item.quantity * item.unitPrice);
    }

    renderCart();
  }

  function calculateTotals() {
    const subtotal = round2(state.cart.reduce((sum, item) => sum + item.lineTotal, 0));
    const tax = round2(subtotal * taxRate);
    const serviceCharge = round2(subtotal * serviceChargeRate);
    const total = round2(subtotal + tax + serviceCharge);

    return { subtotal, tax, serviceCharge, total };
  }

  function renderCart() {
    const totals = calculateTotals();

    refs.cartLines.innerHTML = state.cart.map(item => `
      <div class="cart-line">
        <div>
          <div class="fw-semibold">${item.name}</div>
          <small class="text-muted">${currencyCode} ${money(item.unitPrice)}</small>
        </div>
        <div class="d-flex align-items-center gap-2">
          <button type="button" class="btn btn-sm btn-outline-secondary" data-qty-change="-1" data-product-id="${item.productId}">−</button>
          <span>${item.quantity}</span>
          <button type="button" class="btn btn-sm btn-outline-secondary" data-qty-change="1" data-product-id="${item.productId}">+</button>
        </div>
      </div>
    `).join("");

    refs.cartItemCount.textContent = `${state.cart.length} item(s)`;
    refs.subtotalValue.textContent = money(totals.subtotal);
    refs.taxValue.textContent = money(totals.tax);
    if (refs.serviceChargeValue) {
      refs.serviceChargeValue.textContent = money(totals.serviceCharge);
    }
    refs.totalValue.textContent = money(totals.total);
  }

  function resetCart() {
    state.cart = [];
    refs.orderNotes.value = "";
    setValidation("");
    clearFieldValidation();
    renderCart();
  }

  async function submitOrder(isHold) {
    setValidation("");
    clearFieldValidation();

    if (!state.cart.length) {
      setValidation(messages.addItem);
      setFieldValidation(refs.cartValidationMessage, messages.cartEmpty);
      return;
    }

    const orderType = getSelectedOrderType();
    const tableId = refs.tableId.value || null;
    if (orderType === 1 && !tableId) {
      setValidation(messages.selectTable);
      setFieldValidation(refs.tableValidationMessage, messages.selectTable);
      return;
    }

    const paymentType = getSelectedPaymentType();
    if (!paymentType) {
      setValidation(messages.selectPayment);
      setFieldValidation(refs.paymentValidationMessage, messages.selectPayment);
      return;
    }

    const totals = calculateTotals();
    const payload = {
      orderType,
      tableId,
      paymentType,
      notes: refs.orderNotes.value || null,
      items: state.cart.map(item => ({
        productId: item.productId,
        quantity: item.quantity,
        unitPrice: item.unitPrice
      })),
      subtotalAmount: totals.subtotal,
      taxAmount: totals.tax,
      totalAmount: totals.total,
      isHold
    };

    const endpoint = isHold ? "/POS/Hold" : "/POS/Checkout";
    const response = await fetch(endpoint, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "RequestVerificationToken": token
      },
      body: JSON.stringify(payload)
    });

    const result = await response.json();
    if (!response.ok || !result.success) {
      setValidation(result.message || "Unable to complete the POS action.");
      return;
    }

    if (isHold) {
      state.heldOrders.unshift(result.data);
      renderHeldOrders();
    }

    resetCart();
  }

  function renderHeldOrders() {
    refs.heldOrdersList.innerHTML = (state.heldOrders || []).map(order => `
      <button type="button" class="held-order-card" data-held-id="${order.id}">
        <div class="fw-semibold">${order.orderNumber || "Held order"}</div>
        <small>${order.notes || "—"}</small>
      </button>
    `).join("");
  }

  function renderStatusFeed() {
    refs.statusFeed.innerHTML = (state.statuses || []).map(status => `
      <div class="held-order-card">
        <div class="fw-semibold">${status.orderNumber}</div>
        <small>${status.status} · ${status.orderType}</small>
      </div>
    `).join("");
  }

  async function loadHeldOrder(id) {
    const response = await fetch(`/POS/Held/${id}`);
    const result = await response.json();
    if (!response.ok || !result.success) {
      setValidation(result.message || "Unable to load held order.");
      return;
    }

    const held = result.data;
    state.cart = (held.items || []).map(item => ({
      productId: item.productId,
      name: item.productName,
      quantity: item.quantity,
      unitPrice: round2(item.unitPrice),
      lineTotal: round2(item.quantity * item.unitPrice)
    }));
    refs.orderNotes.value = held.notes || "";
    renderCart();
  }

  async function refreshStatusFeed() {
    const response = await fetch("/POS/StatusFeed");
    const result = await response.json();
    if (response.ok && result.success) {
      state.statuses = result.data || [];
      renderStatusFeed();
    }
  }

  refs.categories.addEventListener("click", event => {
    const button = event.target.closest("button[data-category]");
    if (!button) return;

    state.activeCategory = button.dataset.category || null;
    renderCategories();
    renderProducts();
  });

  refs.products.addEventListener("click", event => {
    const card = event.target.closest("[data-product-id]");
    if (!card) return;
    addToCart(card.dataset.productId);
  });

  refs.cartLines.addEventListener("click", event => {
    const button = event.target.closest("button[data-qty-change]");
    if (!button) return;
    updateQuantity(button.dataset.productId, Number(button.dataset.qtyChange));
  });

  refs.loadMoreBtn.addEventListener("click", () => {
    state.visibleCount += 16;
    renderProducts();
  });

  refs.search.addEventListener("input", event => {
    state.searchTerm = event.target.value;
    state.visibleCount = 16;
    renderProducts();
  });

  refs.clearCartBtn.addEventListener("click", resetCart);
  refs.checkoutBtn.addEventListener("click", () => submitOrder(false));
  refs.holdOrderBtn.addEventListener("click", () => submitOrder(true));
  refs.heldOrdersList.addEventListener("click", event => {
    const button = event.target.closest("[data-held-id]");
    if (!button) return;
    loadHeldOrder(button.dataset.heldId);
  });

  document.addEventListener("keydown", event => {
    if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "f") {
      event.preventDefault();
      refs.search.focus();
      refs.search.select();
    }

    if (event.key === "Enter" && document.activeElement?.tagName !== "TEXTAREA") {
      event.preventDefault();
      submitOrder(false);
    }

    if (event.key === "Escape") {
      resetCart();
    }
  });

  renderCategories();
  renderProducts();
  renderCart();
  renderHeldOrders();
  refreshStatusFeed();
  setInterval(refreshStatusFeed, 20000);
})();
