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

function stripHtml(value: string) {
  return value
    .replace(/<script\b[^>]*>[\s\S]*?<\/script>/gi, " ")
    .replace(/<style\b[^>]*>[\s\S]*?<\/style>/gi, " ")
    .replace(/<[^>]+>/g, " ")
    .replace(/&nbsp;/gi, " ")
    .replace(/&amp;/gi, "&")
    .replace(/&lt;/gi, "<")
    .replace(/&gt;/gi, ">")
    .replace(/&quot;/gi, "\"")
    .replace(/&#39;|&apos;/gi, "'")
    .replace(/\s+/g, " ")
    .trim();
}

function getErrorMessage(body: string, status: number) {
  try {
    const problem = JSON.parse(body) as {
      detail?: unknown;
      message?: unknown;
      title?: unknown;
      errors?: Record<string, unknown>;
    };
    const detail = [problem.detail, problem.message, problem.title]
      .find((value): value is string => typeof value === "string" && value.trim().length > 0);
    if (detail) return detail.trim();
    if (problem.errors) {
      const validationErrors = Object.values(problem.errors)
        .flatMap((value) => Array.isArray(value) ? value : [value])
        .filter((value): value is string => typeof value === "string")
        .join(" ");
      if (validationErrors) return validationErrors;
    }
  } catch {
    // Some reverse proxies return an HTML error page instead of ProblemDetails.
  }

  const text = /<html[\s>]/i.test(body) || /<\/?[a-z][^>]*>/i.test(body)
    ? stripHtml(body)
    : body.trim();
  return text || (status === 401 ? "Sessão expirada. Faça login novamente." : `Erro ${status} ao consultar a API.`);
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
    let ollamaResponse: string | undefined;
    try {
      const problem = JSON.parse(body) as { ollamaResponse?: unknown };
      if (typeof problem.ollamaResponse === "string") {
        ollamaResponse = problem.ollamaResponse;
      }
    } catch {
      // The user-facing message is extracted below from text or HTML responses.
    }
    throw new ApiError(getErrorMessage(body, response.status), response.status, ollamaResponse);
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
    body: JSON.stringify({ text, graph }),
  });
}

export function executeSparql(query: string) {
  return request<Record<string, unknown>>("/sparql/query", {
    method: "POST",
    body: JSON.stringify({ query }),
  });
}

export function listOntologies() {
  return request<Array<{
    iri: string;
    name: string;
    description: string;
    namespace: string;
    classCount: number;
    propertyCount: number;
    authors: Array<{ authorName: string; portfolioUrl: string }>;
    baseDocuments: Array<{ link: string; description: string }>;
    documentation: string;
    profileArea: string;
    profileResume: string;
    profileSource: string;
    terms: string[];
  }>>("/ontologies");
}

export type OntologyAdminPayload = {
  name: string;
  iri: string;
  description: string;
  documentation: string;
  profileArea: string;
  profileResume: string;
  profileSource: string;
  sourceDocument: string;
  terms: string[];
  authors: Array<{ authorName: string; portfolioUrl: string }>;
  baseDocuments: Array<{ link: string; description: string }>;
};

export function listAdminOntologies() {
  return request<Array<OntologyAdminPayload & { id?: string; authors: Array<{ authorName: string; portfolioUrl: string }> }>>("/admin/ontologies");
}

export function createOntology(payload: OntologyAdminPayload) {
  return request("/admin/ontologies", { method: "POST", body: JSON.stringify(payload) });
}

export function updateOntology(id: string, payload: OntologyAdminPayload) {
  return request(`/admin/ontologies/${id}`, { method: "PUT", body: JSON.stringify(payload) });
}

export function deleteOntology(id: string) {
  return request<void>(`/admin/ontologies/${id}`, { method: "DELETE" });
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
