const endpoints = {
  devices: "/api/devices",
  ports: "/api/open-ports",
  alerts: "/api/alerts",
  scans: "/api/scans",
  heartbeats: "/api/agents/heartbeats",
  firewallRules: "/api/firewall-rules",
};

async function getJson(url) {
  const response = await fetch(url, { headers: { Accept: "application/json" } });
  if (!response.ok) {
    throw new Error(`${url} returned HTTP ${response.status}`);
  }
  return response.json();
}

export async function loadDashboard() {
  const entries = await Promise.all(
    Object.entries(endpoints).map(async ([name, url]) => [name, await getJson(url)]),
  );
  return Object.fromEntries(entries);
}
