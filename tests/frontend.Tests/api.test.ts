import { afterEach, describe, expect, mock, test } from "bun:test";
import { login, search, executeSparql, API_BASE_URL } from "../../frontend/src/api";

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
});
