(function () {
  const trend = document.getElementById("reportTrend");
  if (!trend) return;
  const app = document.getElementById("reportsApp");

  const gross = document.getElementById("repGross");
  const orders = document.getElementById("repOrders");
  const avg = document.getElementById("repAvg");
  const low = document.getElementById("repLow");
  const cats = document.getElementById("repCategories");
  const buttons = document.querySelectorAll(".rep-range");
  const noDataText = app?.dataset.noData || "No data available yet.";
  const loadErrorText = app?.dataset.loadError || "Unable to load report data.";

  function drawTrend(points) {
    if (!points || !points.length) {
      trend.innerHTML = '<span style="height:15%"></span>';
      return;
    }

    const max = Math.max(...points.map((p) => p.salesAmount), 1);
    trend.innerHTML = points
      .slice(-12)
      .map((p) => {
        const h = Math.max(12, Math.round((p.salesAmount / max) * 100));
        return `<span title="${p.label}: ${p.salesAmount.toFixed(2)}" style="height:${h}%"></span>`;
      })
      .join("");
  }

  async function load(days) {
    window.appShell.showLoader();
    try {
      const res = await fetch(`/Reports/SummaryJson?days=${days}`);
      const json = await res.json();
      if (!json.success) return;

      const d = json.data;
      gross.textContent = Number(d.grossSales || 0).toFixed(2);
      orders.textContent = d.ordersCount || 0;
      avg.textContent = Number(d.averageTicket || 0).toFixed(2);
      low.textContent = d.lowStockCount || 0;

      drawTrend(d.salesTrend || []);

      const top = d.topCategories || [];
      if (!top.length) {
        cats.innerHTML = `<tr><td colspan="2" class="empty-state">${noDataText}</td></tr>`;
      } else {
        cats.innerHTML = top
          .map((x) => `<tr><td>${x.categoryName}</td><td class="text-end">${Number(x.salesAmount).toFixed(2)}</td></tr>`)
          .join("");
      }
    } catch (_error) {
      if (window.appShell?.toast) {
        window.appShell.toast(loadErrorText, "danger");
      }
    } finally {
      window.appShell.hideLoader();
    }
  }

  buttons.forEach((b) => {
    b.addEventListener("click", () => {
      buttons.forEach((x) => x.classList.remove("active"));
      b.classList.add("active");
      load(Number(b.dataset.days || "30"));
    });
  });

  load(30);
})();
