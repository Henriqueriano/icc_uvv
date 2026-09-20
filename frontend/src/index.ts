import { serve } from "bun";
import index from "./index.html";

const proxyToBackend = async (request: Request) => {
  const backendUrl = process.env.BACKEND_URL;
  if (!backendUrl) {
    return Response.json({ error: "BACKEND_URL is required." }, { status: 500 });
  }

  const target = new URL(request.url);
  const upstreamUrl = `${backendUrl.replace(/\/$/, "")}${target.pathname}${target.search}`;
  const headers = new Headers(request.headers);
  headers.delete("host");

  const body = request.method === "GET" || request.method === "HEAD"
    ? undefined
    : await request.arrayBuffer();

  return fetch(upstreamUrl, {
    method: request.method,
    headers,
    body,
  });
};

const server = serve({
  port: Number(process.env.FRONTEND_PORT ?? 4040),
  routes: {
    "/api/*": proxyToBackend,
    "/health/*": proxyToBackend,
    "/health": proxyToBackend,
    "/*": index,
  },

  development: process.env.NODE_ENV !== "production" && {
    // Enable browser hot reloading in development
    hmr: true,

    // Echo console logs from the browser to the server
    console: true,
  },
});

console.log(`🚀 Server running at ${server.url}`);
