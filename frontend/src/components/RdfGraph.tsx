import { useEffect, useMemo, useRef } from "react";
import * as d3 from "d3";

type BindingValue = { value?: unknown; id?: unknown; type?: string };
type QueryBinding = Record<string, BindingValue>;

type RdfGraphProps = {
  result: unknown;
  darkMode: boolean;
};

type GraphNode = d3.SimulationNodeDatum & { id: string; label: string };
type GraphLink = d3.SimulationLinkDatum<GraphNode> & { predicate: string; source: string; target: string };

const roleAliases = {
  subject: ["subject", "sujeito", "subj", "source", "origem", "from", "entity", "entidade", "catalog", "catalogo"],
  predicate: ["predicate", "predicado", "property", "propriedade", "relation", "relacao", "relationship", "edge", "aresta"],
  object: ["object", "objeto", "obj", "target", "destino", "to", "value", "valor", "neighbor", "vizinho", "node", "no"],
  direction: ["direction", "direcao", "dir", "orientation", "orientacao", "sentido"],
};

function getBindings(result: unknown): QueryBinding[] {
  if (Array.isArray(result)) return result.filter(isQueryBinding);
  if (!result || typeof result !== "object") return [];
  const response = result as {
    results?: { bindings?: unknown };
    bindings?: unknown;
    data?: { results?: { bindings?: unknown }; bindings?: unknown };
  };
  const bindings = response.results?.bindings
    ?? response.bindings
    ?? response.data?.results?.bindings
    ?? response.data?.bindings;
  return Array.isArray(bindings) ? bindings.filter((item): item is QueryBinding => !!item && typeof item === "object") : [];
}

function isQueryBinding(item: unknown): item is QueryBinding {
  return !!item && typeof item === "object" && !Array.isArray(item);
}

function getBindingValue(term: BindingValue | string | undefined) {
  if (typeof term === "string") return term;
  if (!term || typeof term !== "object") return undefined;
  if (typeof term.value === "string") return term.value;
  if (typeof term.id === "string") return term.id;
  return undefined;
}

function normalizeVariableName(name: string) {
  return name
    .replace(/^\?/, "")
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .toLowerCase();
}

function findVariable(entries: Array<[string, string]>, role: keyof typeof roleAliases) {
  const aliases = roleAliases[role];
  return entries.find(([name]) => aliases.includes(normalizeVariableName(name)));
}

export function bindingToTriple(binding: QueryBinding) {
  const entries = Object.entries(binding)
    .map(([name, term]) => [name, getBindingValue(term)] as const)
    .filter((entry): entry is [string, string] => Boolean(entry[1]));
  if (entries.length < 3) return null;

  const directionEntry = findVariable(entries, "direction");
  const direction = directionEntry?.[1]?.toLowerCase();
  const graphEntries = entries.filter(([name]) => name !== directionEntry?.[0]);
  const predicate = findVariable(graphEntries, "predicate") ?? graphEntries[1];
  const subject = findVariable(entries, "subject");
  const object = findVariable(entries, "object");

  if (subject && object && subject[0] !== object[0]) {
    return direction === "entrada"
      ? { subject: object[1], predicate: predicate[1], object: subject[1] }
      : { subject: subject[1], predicate: predicate[1], object: object[1] };
  }

  const endpoints = graphEntries.filter(([name]) => name !== predicate[0]);
  if (endpoints.length < 2) return null;
  return direction === "entrada"
    ? { subject: endpoints[1][1], predicate: predicate[1], object: endpoints[0][1] }
    : { subject: endpoints[0][1], predicate: predicate[1], object: endpoints[1][1] };
}

export function extractTriples(result: unknown) {
  return getBindings(result)
    .map(bindingToTriple)
    .filter((triple): triple is NonNullable<typeof triple> => triple !== null);
}

export function RdfGraph({ result, darkMode }: RdfGraphProps) {
  const svgRef = useRef<SVGSVGElement>(null);
  const triples = useMemo(
    () => extractTriples(result).slice(0, 100),
    [result],
  );

  useEffect(() => {
    const svgElement = svgRef.current;
    if (!svgElement || triples.length === 0) return;

    const width = Math.max(svgElement.clientWidth, 640);
    const height = 420;
    const nodesById = new Map<string, GraphNode>();
    const getNode = (id: string) => {
      const existing = nodesById.get(id);
      if (existing) return existing;
      const node = { id, label: id.length > 42 ? `${id.slice(0, 39)}...` : id };
      nodesById.set(id, node);
      return node;
    };
    const links: GraphLink[] = triples.map(({ subject, predicate, object }) => ({
      source: getNode(subject).id,
      target: getNode(object).id,
      predicate: predicate.length > 32 ? `${predicate.slice(0, 29)}...` : predicate,
    }));
    const nodes = [...nodesById.values()];
    const svg = d3.select(svgElement);
    svg.selectAll("*").remove();
    svg.attr("viewBox", `0 0 ${width} ${height}`);
    const graphLayer = svg.append("g");
    svg.call(
      d3.zoom<SVGSVGElement, unknown>()
        .scaleExtent([0.35, 3])
        .translateExtent([[-width, -height], [width * 2, height * 2]])
        .on("zoom", (event) => graphLayer.attr("transform", event.transform)),
    );

    const simulation = d3.forceSimulation(nodes)
      .force("link", d3.forceLink<GraphNode, GraphLink>(links).id((node) => node.id).distance(120))
      .force("charge", d3.forceManyBody().strength(-280))
      .force("center", d3.forceCenter(width / 2, height / 2))
      .force("collide", d3.forceCollide(28));

    const link = graphLayer.append("g").attr("stroke", darkMode ? "#8C6E4C" : "#A57A4B").selectAll("line").data(links).join("line");
    const edgeLabel = graphLayer.append("g").selectAll("text").data(links).join("text")
      .attr("font-size", 9)
      .attr("fill", darkMode ? "#E8DCC2" : "#524332")
      .text((d) => d.predicate);
    const node = graphLayer.append("g").selectAll("g").data(nodes).join("g").call(
      d3.drag<SVGGElement, GraphNode>()
        .on("start", (event, d) => {
          if (!event.active) simulation.alphaTarget(0.3).restart();
          d.fx = d.x;
          d.fy = d.y;
        })
        .on("drag", (event, d) => {
          d.fx = event.x;
          d.fy = event.y;
        })
        .on("end", (event, d) => {
          if (!event.active) simulation.alphaTarget(0);
          d.fx = null;
          d.fy = null;
        }),
    );
    node.append("circle").attr("r", 20).attr("fill", darkMode ? "#4A3B32" : "#E7D89B").attr("stroke", darkMode ? "#D1B866" : "#A57A4B");
    node.append("title").text((d) => d.id);
    node.append("text").attr("text-anchor", "middle").attr("dy", 4).attr("font-size", 9).attr("fill", darkMode ? "#F5F1E6" : "#2A1F16").text((d) => d.label);

    simulation.on("tick", () => {
      link.attr("x1", (d) => (d.source as GraphNode).x ?? 0).attr("y1", (d) => (d.source as GraphNode).y ?? 0)
        .attr("x2", (d) => (d.target as GraphNode).x ?? 0).attr("y2", (d) => (d.target as GraphNode).y ?? 0);
      edgeLabel.attr("x", (d) => (((d.source as GraphNode).x ?? 0) + ((d.target as GraphNode).x ?? 0)) / 2)
        .attr("y", (d) => (((d.source as GraphNode).y ?? 0) + ((d.target as GraphNode).y ?? 0)) / 2);
      node.attr("transform", (d) => `translate(${d.x ?? 0},${d.y ?? 0})`);
    });

    return () => simulation.stop();
  }, [darkMode, triples]);

  if (triples.length === 0) return null;
  return <div className="mb-4 overflow-hidden rounded-xl border border-[#A57A4B]/40 p-2"><svg ref={svgRef} className="h-[420px] min-h-[420px] w-full cursor-grab touch-none active:cursor-grabbing" role="img" aria-label="Grafo RDF resultante da consulta. Arraste o fundo para navegar e use o scroll para ampliar." /></div>;
}
