import { useEffect, useState } from "react";
import { getStatistics, getSystemStatus, type SystemStatus, type StatisticsOverview } from "../api";

type AdminStatisticsSectionProps = {
  darkMode: boolean;
};

export function AdminStatisticsSection({ darkMode }: AdminStatisticsSectionProps) {
  const [data, setData] = useState<StatisticsOverview>({
    rdfDocuments: 0,
    ontologies: 0,
    sparqlQueries: 0,
    freeSearches: 0,
    averageResponseTimeMs: 0,
    successRate: 100,
    usageIndex: 25,
  });

  const [status, setStatus] = useState<SystemStatus>({
    status: "not_ready",
    integrity: "Carregando...",
    timestamp: new Date().toISOString(),
    server: { status: "Carregando...", isHealthy: false },
    rdfBase: { status: "Carregando...", isHealthy: false },
    database: { status: "Carregando...", isHealthy: false },
    aiService: { status: "Carregando...", isHealthy: false },
    cache: { status: "Carregando...", isHealthy: false },
  });

  const [lastUpdated, setLastUpdated] = useState<string>("");

  useEffect(() => {
    let active = true;

    const refresh = async () => {
      const [statistics, systemStatus] = await Promise.allSettled([
        getStatistics(),
        getSystemStatus(),
      ]);

      if (!active) return;

      if (statistics.status === "fulfilled") {
        setData(statistics.value);
      }

      if (systemStatus.status === "fulfilled") {
        setStatus(systemStatus.value);
        setLastUpdated(new Date().toLocaleTimeString("pt-BR"));
      } else {
        setStatus({
          status: "not_ready",
          integrity: "Degradada",
          timestamp: new Date().toISOString(),
          server: { status: "Indisponível", isHealthy: false },
          rdfBase: { status: "Indisponível", isHealthy: false },
          database: { status: "Indisponível", isHealthy: false },
          aiService: { status: "Indisponível", isHealthy: false },
          cache: { status: "Indisponível", isHealthy: false },
        });
      }
    };

    refresh();
    const interval = window.setInterval(refresh, 4000);

    return () => {
      active = false;
      window.clearInterval(interval);
    };
  }, []);

  const getTone = (isHealthy: boolean) => (isHealthy ? "bg-[#3B8D5A]" : "bg-[#B33A3A]");

  const statusItems = [
    {
      label: "Servidor",
      value: status.server.status || "Indisponível",
      tone: getTone(status.server.isHealthy),
      subtext: status.server.latencyMs !== undefined ? `${status.server.latencyMs} ms` : undefined,
    },
    {
      label: "Base RDF",
      value: status.rdfBase.status || "Indisponível",
      tone: status.rdfBase.isHealthy ? "bg-[#D1B866]" : "bg-[#B33A3A]",
      subtext: status.rdfBase.latencyMs !== undefined ? `${status.rdfBase.latencyMs} ms` : undefined,
    },
    {
      label: "Banco de Dados",
      value: status.database.status || "Indisponível",
      tone: getTone(status.database.isHealthy),
      subtext: status.database.latencyMs !== undefined ? `${status.database.latencyMs} ms` : undefined,
    },
    {
      label: "Integridade",
      value: status.integrity || "Degradada",
      tone: status.integrity === "Normal" ? "bg-[#3B8D5A]" : "bg-[#B33A3A]",
    },
  ];

  const stats = [
    { label: "Arquivos RDF", value: data.rdfDocuments.toLocaleString("pt-BR"), accent: "#B78C5A" },
    { label: "Ontologias", value: data.ontologies.toLocaleString("pt-BR"), accent: "#CDAE7D" },
    { label: "Consultas SPARQL", value: data.sparqlQueries.toLocaleString("pt-BR"), accent: "#D6B88F" },
    { label: "Buscas livres", value: data.freeSearches.toLocaleString("pt-BR"), accent: "#A66F47" },
  ];

  const isLive = status.server.isHealthy;

  return (
    <>
      <div className="mb-6 text-center">
        <h1 className={darkMode ? "text-3xl font-bold text-[#F5F1E6]" : "text-3xl font-bold text-[#2A1F16]"}>
          Estatísticas do sistema
        </h1>
        <div className="mt-3 flex justify-center">
          <hr className={darkMode ? "w-28 border-0 border-t border-[#8C6E4C]" : "w-28 border-0 border-t border-[#A57A4B]"} />
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        {stats.map((stat) => (
          <div
            key={stat.label}
            className={
              darkMode
                ? "rounded-2xl border border-[#453F39] bg-[#2A2724] p-5"
                : "rounded-2xl border border-[#E7DDB3] bg-[#F4EBC9] p-5"
            }
          >
            <div className="mb-4 h-2 w-full rounded-full bg-[#3A322E]">
              <div className="h-2 rounded-full" style={{ width: "72%", backgroundColor: stat.accent }} />
            </div>

            <div className={darkMode ? "text-sm uppercase tracking-[0.08em] text-[#E8DCC2]" : "text-sm uppercase tracking-[0.08em] text-[#5B4A35]"}>
              {stat.label}
            </div>
            <div className={darkMode ? "mt-3 text-3xl font-bold text-[#F5F1E6]" : "mt-3 text-3xl font-bold text-[#2A1F16]"}>
              {stat.value}
            </div>
          </div>
        ))}
      </div>

      <div
        className={
          darkMode
            ? "mt-6 rounded-2xl border border-[#453F39] bg-[#2A2724] p-5"
            : "mt-6 rounded-2xl border border-[#E7DDB3] bg-[#F4EBC9] p-5"
        }
      >
        <div className="mb-3 flex items-center justify-between">
          <h2 className={darkMode ? "text-xl font-semibold text-[#F5F1E6]" : "text-xl font-semibold text-[#2A1F16]"}>
            Status do sistema
          </h2>

          <div className="flex items-center gap-2 text-xs">
            <span className="relative flex h-2.5 w-2.5">
              {isLive && (
                <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-[#3B8D5A] opacity-75" />
              )}
              <span
                className={`relative inline-flex rounded-full h-2.5 w-2.5 ${
                  isLive ? "bg-[#3B8D5A]" : "bg-[#B33A3A]"
                }`}
              />
            </span>
            <span className={darkMode ? "text-[#D1C7BD]" : "text-[#736353]"}>
              {isLive ? "Tempo real" : "Offline"} {lastUpdated ? `• ${lastUpdated}` : ""}
            </span>
          </div>
        </div>

        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
          {statusItems.map((item) => (
            <div key={item.label} className={darkMode ? "rounded-xl bg-[#171614] p-4" : "rounded-xl bg-[#FDFBE8] p-4"}>
              <div className="mb-3 flex items-center justify-between gap-2">
                <div className={darkMode ? "text-sm text-[#E8DCC2]" : "text-sm text-[#524332]"}>{item.label}</div>
                <span className={`inline-block h-2.5 w-2.5 rounded-full ${item.tone}`} />
              </div>
              <div className={darkMode ? "text-xl font-bold text-[#F5F1E6]" : "text-xl font-bold text-[#2A1F16]"}>
                {item.value}
              </div>
              {item.subtext && (
                <div className={`mt-1 text-xs ${darkMode ? "text-[#9E9282]" : "text-[#8C7A68]"}`}>
                  Latência: {item.subtext}
                </div>
              )}
            </div>
          ))}
        </div>
      </div>

      <div
        className={
          darkMode
            ? "mt-6 rounded-2xl border border-[#453F39] bg-[#2A2724] p-5"
            : "mt-6 rounded-2xl border border-[#E7DDB3] bg-[#F4EBC9] p-5"
        }
      >
        <h2 className={darkMode ? "mb-3 text-xl font-semibold text-[#F5F1E6]" : "mb-3 text-xl font-semibold text-[#2A1F16]"}>
          Resumo operacional
        </h2>

        <div className="grid gap-4 md:grid-cols-3">
          <div className={darkMode ? "rounded-xl bg-[#171614] p-4" : "rounded-xl bg-[#FDFBE8] p-4"}>
            <div className={darkMode ? "text-sm text-[#E8DCC2]" : "text-sm text-[#524332]"}>Tempo médio de resposta</div>
            <div className={darkMode ? "mt-2 text-2xl font-bold text-[#F5F1E6]" : "mt-2 text-2xl font-bold text-[#2A1F16]"}>
              {data.averageResponseTimeMs} ms
            </div>
          </div>

          <div className={darkMode ? "rounded-xl bg-[#171614] p-4" : "rounded-xl bg-[#FDFBE8] p-4"}>
            <div className={darkMode ? "text-sm text-[#E8DCC2]" : "text-sm text-[#524332]"}>Taxa de sucesso</div>
            <div className={darkMode ? "mt-2 text-2xl font-bold text-[#F5F1E6]" : "mt-2 text-2xl font-bold text-[#2A1F16]"}>
              {data.successRate}%
            </div>
          </div>

          <div className={darkMode ? "rounded-xl bg-[#171614] p-4" : "rounded-xl bg-[#FDFBE8] p-4"}>
            <div className={darkMode ? "text-sm text-[#E8DCC2]" : "text-sm text-[#524332]"}>Índice de uso</div>
            <div className={darkMode ? "mt-2 text-2xl font-bold text-[#F5F1E6]" : "mt-2 text-2xl font-bold text-[#2A1F16]"}>
              {data.usageIndex}%
            </div>
          </div>
        </div>
      </div>
    </>
  );
}

export default AdminStatisticsSection;
