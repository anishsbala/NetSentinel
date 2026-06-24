import { loadDashboard } from "./api.js";
import { renderDashboard, setConnectionState, setLastUpdated } from "./render.js";

const refreshButton = document.getElementById("refresh-button");

async function refresh() {
  refreshButton.disabled = true;
  setConnectionState("loading");
  try {
    const data = await loadDashboard();
    renderDashboard(data);
    setConnectionState("ok");
    setLastUpdated();
  } catch (error) {
    setConnectionState("error", `Could not load dashboard data: ${error.message}`);
  } finally {
    refreshButton.disabled = false;
  }
}

refreshButton.addEventListener("click", refresh);
await refresh();
setInterval(refresh, 30_000);
