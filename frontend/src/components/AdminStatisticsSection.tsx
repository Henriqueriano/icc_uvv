type AdminStatisticsSectionProps = {
  darkMode: boolean;
};

const stats = [
  { label: "Arquivos RDF", value: "1.284", accent: "#B78C5A" },
  { label: "Ontologias", value: "42", accent: "#CDAE7D" },
  { label: "Consultas SPARQL", value: "8.9k", accent: "#D6B88F" },
  { label: "Buscas livres", value: "24.3k", accent: "#A66F47" },
];

export function AdminStatisticsSection({ darkMode }: AdminStatisticsSectionProps) {
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
        <h2 className={darkMode ? "mb-3 text-xl font-semibold text-[#F5F1E6]" : "mb-3 text-xl font-semibold text-[#2A1F16]"}>
          Status do sistema
        </h2>

        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
          {[
            { label: "Servidor", value: "Online", tone: "bg-[#3B8D5A]" },
            { label: "Base RDF", value: "Saudável", tone: "bg-[#D1B866]" },
            { label: "Cache", value: "Estável", tone: "bg-[#B78C5A]" },
            { label: "Integridade", value: "Normal", tone: "bg-[#A66F47]" },
          ].map((item) => (
            <div key={item.label} className={darkMode ? "rounded-xl bg-[#171614] p-4" : "rounded-xl bg-[#FDFBE8] p-4"}>
              <div className="mb-3 flex items-center justify-between gap-2">
                <div className={darkMode ? "text-sm text-[#E8DCC2]" : "text-sm text-[#524332]"}>{item.label}</div>
                <span className={`inline-block h-2.5 w-2.5 rounded-full ${item.tone}`} />
              </div>
              <div className={darkMode ? "text-xl font-bold text-[#F5F1E6]" : "text-xl font-bold text-[#2A1F16]"}>{item.value}</div>
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
            <div className={darkMode ? "mt-2 text-2xl font-bold text-[#F5F1E6]" : "mt-2 text-2xl font-bold text-[#2A1F16]"}>182 ms</div>
          </div>

          <div className={darkMode ? "rounded-xl bg-[#171614] p-4" : "rounded-xl bg-[#FDFBE8] p-4"}>
            <div className={darkMode ? "text-sm text-[#E8DCC2]" : "text-sm text-[#524332]"}>Taxa de sucesso</div>
            <div className={darkMode ? "mt-2 text-2xl font-bold text-[#F5F1E6]" : "mt-2 text-2xl font-bold text-[#2A1F16]"}>99.4%</div>
          </div>

          <div className={darkMode ? "rounded-xl bg-[#171614] p-4" : "rounded-xl bg-[#FDFBE8] p-4"}>
            <div className={darkMode ? "text-sm text-[#E8DCC2]" : "text-sm text-[#524332]"}>Índice de uso</div>
            <div className={darkMode ? "mt-2 text-2xl font-bold text-[#F5F1E6]" : "mt-2 text-2xl font-bold text-[#2A1F16]"}>78%</div>
          </div>
        </div>
      </div>
    </>
  );
}

export default AdminStatisticsSection;
