import { useEffect, useMemo, useRef } from "react";
import * as d3 from "d3";

type BindingValue = { value?: string };
type QueryBinding = Record<string, BindingValue>;

type RdfGraphProps = {
  result: unknown;
  darkMode: boolean;
};

type GraphNode = d3.SimulationNodeDatum & { id: string; label: string };
type GraphLink = d3.SimulationLinkDatum<GraphNode> & { predicate: string; source: string; target: string };

function getBindings(result: unknown): QueryBinding[] {
  if (!result || typeof result !== "object") return [];
  const bindings = (result as { results?: { bindings?: unknown } }).results?.bindings;
  return Array.isArray(bindings) ? bindings.filter((item): item is QueryBinding => !!item && typeof item === "object") : [];
}

function asTriple(binding: QueryBinding) {
  const subject = binding.subject?.value;
  const predicate = binding.predicate?.value;
  const object = binding.object?.value;
  return subject && predicate && object ? { subject, predicate, object } : null;
}

export function RdfGraph({ result, darkMode }: RdfGraphProps) {
  const svgRef = useRef<SVGSVGElement>(null);
  const triples = useMemo(
    () => getBindings(result)
      .map(asTriple)
      .filter((triple): triple is NonNullable<typeof triple> => triple !== null)
      .slice(0, 100),
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
