import { afterEach, describe, expect, mock, test } from "bun:test";
import { login, search, executeSparql, API_BASE_URL } from "../../frontend/src/api";
import { extractTriples } from "../../frontend/src/components/RdfGraph";

const originalFetch = globalThis.fetch;

function installStorage(token?: string) {
  const values = new Map<string, string>();
  if (token) values.set("accessToken", token);
  Object.defineProperty(globalThis, "localStorage", {
    configurable: true,
    value: {
      getItem: (key: string) => values.get(key) ?? null,
      setItem: (key: string, value: string) => values.set(key, value),
      removeItem: (key: string) => values.delete(key),
    },
  });
}

afterEach(() => {
  globalThis.fetch = originalFetch;
});

describe("frontend API client", () => {
  test("login sends credentials and parses the JWT response", async () => {
    installStorage();
    globalThis.fetch = mock(async (input, init) => {
      expect(input).toBe(`${API_BASE_URL}/auth/token`);
      expect(init?.method).toBe("POST");
      expect(init?.headers).toBeDefined();
      expect(JSON.parse(String(init?.body))).toEqual({
        username: "rdf-admin",
        password: "ChangeMe123!",
      });
      return Response.json({ accessToken: "jwt", tokenType: "Bearer", expiresIn: 3600 });
    }) as typeof fetch;

    await expect(login("rdf-admin", "ChangeMe123!")).resolves.toEqual({
      accessToken: "jwt",
      tokenType: "Bearer",
      expiresIn: 3600,
    });
  });

  test("authenticated requests include the bearer token", async () => {
    installStorage("jwt-token");
    globalThis.fetch = mock(async (_input, init) => {
      const headers = new Headers(init?.headers);
      expect(headers.get("Authorization")).toBe("Bearer jwt-token");
      expect(headers.get("Content-Type")).toBe("application/json");
      return Response.json({ results: { bindings: [] } });
    }) as typeof fetch;

    await search("person");
    await executeSparql("SELECT * WHERE { ?s ?p ?o } LIMIT 1");
  });

  test("non-success responses become errors", async () => {
    installStorage();
    globalThis.fetch = mock(async () => new Response("backend unavailable", { status: 503 })) as typeof fetch;

    await expect(search("person")).rejects.toThrow("backend unavailable");
  });

  test("HTML error responses are converted to a readable reason", async () => {
    installStorage();
    globalThis.fetch = mock(async () => new Response(
      "<html><head><title>Bad Request</title></head><body>Invalid SPARQL syntax</body></html>",
      { status: 400, headers: { "Content-Type": "text/html" } },
    )) as typeof fetch;

    await expect(executeSparql("invalid")).rejects.toThrow("Bad Request Invalid SPARQL syntax");
  });

  test("graph extraction supports Portuguese directional bindings", () => {
    const triples = extractTriples({
      head: { vars: ["direcao", "predicado", "vizinho"] },
      results: {
        bindings: [
          {
            direcao: { type: "literal", value: "saida" },
            predicado: { type: "uri", value: "http://example.test/related" },
            vizinho: { type: "uri", value: "http://example.test/neighbor" },
            catalogo: { type: "uri", value: "http://example.test/catalog" },
          },
          {
            direcao: { type: "literal", value: "entrada" },
            predicado: { type: "uri", value: "http://example.test/related" },
            vizinho: { type: "uri", value: "http://example.test/source" },
            catalogo: { type: "uri", value: "http://example.test/catalog" },
          },
        ],
      },
    });

    expect(triples).toEqual([
      {
        subject: "http://example.test/catalog",
        predicate: "http://example.test/related",
        object: "http://example.test/neighbor",
      },
      {
        subject: "http://example.test/source",
        predicate: "http://example.test/related",
        object: "http://example.test/catalog",
      },
    ]);
  });
});
