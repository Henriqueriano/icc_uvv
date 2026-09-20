export const API_BASE_URL = "/api/v1";

type AuthResponse = {
  accessToken: string;
  tokenType: string;
  expiresIn: number;
};

export class ApiError extends Error {
  readonly ollamaResponse?: string;
  readonly status: number;

  constructor(message: string, status: number, ollamaResponse?: string) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.ollamaResponse = ollamaResponse;
  }
}

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
    if (response.status === 401) {
      localStorage.removeItem("accessToken");
      localStorage.removeItem("isAdmin");
    }
    const body = await response.text();
    try {
      const problem = JSON.parse(body) as { detail?: string; ollamaResponse?: string };
      throw new ApiError(problem.detail || "Sessão expirada. Faça login novamente.", response.status, problem.ollamaResponse);
    } catch (error) {
      if (error instanceof ApiError) {
        throw error;
      }
      throw new ApiError(body || `Erro ${response.status}`, response.status);
    }
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

export type ComponentStatus = {
  status: string;
  isHealthy: boolean;
  latencyMs?: number;
  details?: string;
};

export type SystemStatus = {
  status: "ready" | "not_ready" | "degraded";
  integrity: string;
  timestamp: string;
  server: ComponentStatus;
  rdfBase: ComponentStatus;
  database: ComponentStatus;
  aiService: ComponentStatus;
  cache: ComponentStatus;
};

export type StatisticsOverview = {
  rdfDocuments: number;
  ontologies: number;
  sparqlQueries: number;
  freeSearches: number;
  averageResponseTimeMs: number;
  successRate: number;
  usageIndex: number;
};

export function getStatistics() {
  return request<StatisticsOverview>("/statistics/overview");
}

export async function getSystemStatus(): Promise<SystemStatus> {
  try {
    return await request<SystemStatus>("/health/status");
  } catch {
    return {
      status: "not_ready",
      integrity: "Degradada",
      timestamp: new Date().toISOString(),
      server: { status: "Indisponível", isHealthy: false },
      rdfBase: { status: "Indisponível", isHealthy: false },
      database: { status: "Indisponível", isHealthy: false },
      aiService: { status: "Indisponível", isHealthy: false },
      cache: { status: "Indisponível", isHealthy: false },
    };
  }
}

export async function getSystemHealth() {
  const status = await getSystemStatus();
  return { status: status.status === "ready" ? "ready" : "not_ready" };
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

export function importPdf(file: File, graphName: string) {
  const body = new FormData();
  body.append("file", file);
  body.append("graphName", graphName);
  return request<{ status: string; graphName: string; tripleCount: number; format: string }>("/pdf/import", {
    method: "POST",
    body,
  });
}
