const API_BASE_URL = "http://localhost:5079/api/v1";

type AuthResponse = {
  accessToken: string;
  tokenType: string;
  expiresIn: number;
};

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = localStorage.getItem("accessToken");
  const headers = new Headers(options.headers);
  if (options.body && !(options.body instanceof FormData)) {
    headers.set("Content-Type", "application/json");
  }
  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const response = await fetch(`${API_BASE_URL}${path}`, { ...options, headers });
  if (!response.ok) {
    const detail = await response.text();
    throw new Error(detail || `Erro ${response.status}`);
  }
  return response.status === 204 ? (undefined as T) : response.json();
}

export function login(username: string, password: string) {
  return request<AuthResponse>("/auth/token", {
    method: "POST",
    body: JSON.stringify({ username, password }),
  });
}

export function search(text: string, graph?: string) {
  return request<Record<string, unknown>>("/search", {
    method: "POST",
    body: JSON.stringify({ text, graph, page: 1, pageSize: 20 }),
  });
}

export function executeSparql(query: string) {
  return request<Record<string, unknown>>("/sparql/query", {
    method: "POST",
    body: JSON.stringify({ query }),
  });
}

export function listOntologies() {
  return request<Array<{ iri: string; name: string; description: string; namespace: string; classCount: number; propertyCount: number }>>("/ontologies");
}

export function getStatistics() {
  return request<{ rdfDocuments: number; ontologies: number; sparqlQueries: number; freeSearches: number; averageResponseTimeMs: number; successRate: number }>("/statistics/overview");
}

export function importRdf(file: File, graphName: string, format?: string) {
  const body = new FormData();
  body.append("file", file);
  body.append("graphName", graphName);
  if (format) body.append("format", format);
  return request<{ status: string; graphName: string; tripleCount: number; format: string }>("/rdf/import", {
    method: "POST",
    body,
  });
}
