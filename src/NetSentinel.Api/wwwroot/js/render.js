const byId = (id) => document.getElementById(id);

function escapeHtml(value) {
  return String(value ?? "")
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#039;");
}

function date(value) {
  return value ? new Date(value).toLocaleString() : "-";
}

function renderTable(id, columns, rows) {
  const table = byId(id);
  const headings = columns.map((column) => `<th>${escapeHtml(column.label)}</th>`).join("");
  if (!rows.length) {
    table.innerHTML = `<thead><tr>${headings}</tr></thead><tbody><tr><td class="empty" colspan="${columns.length}">No data reported</td></tr></tbody>`;
    return;
  }
  const body = rows
    .map((row) => `<tr>${columns.map((column) => `<td>${column.render(row)}</td>`).join("")}</tr>`)
    .join("");
  table.innerHTML = `<thead><tr>${headings}</tr></thead><tbody>${body}</tbody>`;
}

export function renderDashboard(data) {
  byId("device-count").textContent = data.devices.length;
  byId("port-count").textContent = data.ports.length;
  byId("alert-count").textContent = data.alerts.filter((alert) => !alert.isAcknowledged).length;
  byId("heartbeat-count").textContent = data.heartbeats.length;

  renderTable("devices-table", [
    { label: "Hostname", render: (x) => escapeHtml(x.hostname) },
    { label: "IP address", render: (x) => escapeHtml(x.ipAddress) },
    { label: "Operating system", render: (x) => escapeHtml(x.osType) },
    { label: "Ports", render: (x) => escapeHtml(x.openPortCount) },
    { label: "Last seen", render: (x) => date(x.lastSeenAtUtc) },
  ], data.devices);

  renderTable("ports-table", [
    { label: "Host", render: (x) => escapeHtml(x.hostname) },
    { label: "Port", render: (x) => `${escapeHtml(x.portNumber)}/${escapeHtml(x.protocol)}` },
    { label: "Service", render: (x) => escapeHtml(x.serviceName || "unknown") },
    { label: "Source", render: (x) => escapeHtml(x.observationSource) },
  ], data.ports.slice(0, 20));

  renderTable("alerts-table", [
    { label: "Severity", render: (x) => `<span class="severity-${escapeHtml(x.severity).toLowerCase()}">${escapeHtml(x.severity)}</span>` },
    { label: "Finding", render: (x) => escapeHtml(x.title) },
    { label: "Created", render: (x) => date(x.createdAtUtc) },
  ], data.alerts.slice(0, 20));

  renderTable("scans-table", [
    { label: "Target", render: (x) => escapeHtml(x.target) },
    { label: "Status", render: (x) => escapeHtml(x.status) },
    { label: "Hosts", render: (x) => escapeHtml(x.hostsDiscovered) },
    { label: "Completed", render: (x) => date(x.completedAtUtc) },
  ], data.scans.slice(0, 20));

  renderTable("heartbeats-table", [
    { label: "Agent", render: (x) => escapeHtml(x.agentId) },
    { label: "Host", render: (x) => escapeHtml(x.hostname) },
    { label: "Firewall", render: (x) => x.firewallEnabled ? '<span class="ok">Enabled</span>' : "Disabled" },
    { label: "CPU", render: (x) => `${escapeHtml(x.cpuPercent)}%` },
    { label: "Reported", render: (x) => date(x.reportedAtUtc) },
  ], data.heartbeats.slice(0, 20));

  renderTable("firewall-table", [
    { label: "Rule", render: (x) => escapeHtml(x.name) },
    { label: "Action", render: (x) => escapeHtml(x.action) },
    { label: "Traffic", render: (x) => `${escapeHtml(x.protocol)} ${escapeHtml(x.portNumber ?? "Any")}` },
    { label: "Source", render: (x) => escapeHtml(x.sourceCidr) },
    { label: "Findings", render: (x) => escapeHtml(x.findingCount) },
  ], data.firewallRules);
}

export function setConnectionState(state, message = "") {
  const status = byId("api-status");
  status.className = `status status-${state}`;
  status.textContent = state === "ok" ? "API connected" : state === "error" ? "API unavailable" : "Refreshing";
  byId("error-message").hidden = !message;
  byId("error-message").textContent = message;
}

export function setLastUpdated() {
  byId("last-updated").textContent = `Updated ${new Date().toLocaleTimeString()}`;
}
