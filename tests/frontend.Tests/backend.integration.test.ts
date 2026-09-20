import { describe, expect, test } from "bun:test";

const enabled = process.env.RUN_BACKEND_INTEGRATION === "true";
const baseUrl = process.env.BACKEND_URL ?? "http://localhost:5079";
const username = process.env.AUTH_USERNAME ?? "rdf-admin";
const password = process.env.AUTH_PASSWORD ?? "ChangeMe123!";

let backendAvailable = false;
if (enabled) {
  try {
    backendAvailable = (await fetch(`${baseUrl}/health/live`)).ok;
  } catch {
    backendAvailable = false;
  }
}

async function backendRequest(path: string, init?: RequestInit) {
  return fetch(`${baseUrl}${path}`, init);
}

describe("backend integration", () => {
  test.if(enabled && backendAvailable)("health endpoint responds", async () => {
    const response = await backendRequest("/health/live");
    expect(response.ok).toBe(true);
  });

  test.if(enabled && backendAvailable)("authenticates against the PostgreSQL-backed user", async () => {
    const response = await backendRequest("/api/v1/auth/token", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ username, password }),
    });

    expect(response.status).toBe(200);
    const body = await response.json() as { accessToken?: string; tokenType?: string };
    expect(body.accessToken).toBeString();
    expect(body.tokenType).toBe("Bearer");
  });

  test.if(enabled && backendAvailable)("authenticated request reaches the RDF API", async () => {
    const tokenResponse = await backendRequest("/api/v1/auth/token", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ username, password }),
    });
    expect(tokenResponse.ok).toBe(true);

    const { accessToken } = await tokenResponse.json() as { accessToken: string };
    const response = await backendRequest("/api/v1/statistics/overview", {
      headers: { Authorization: `Bearer ${accessToken}` },
    });

    expect(response.status).toBe(200);
    const body = await response.json() as { rdfDocuments?: number; ontologies?: number };
    expect(body.rdfDocuments).toBeNumber();
    expect(body.ontologies).toBeNumber();
  });
});
