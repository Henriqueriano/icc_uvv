type NavButtonsProps = {
  activeTab: "search" | "docs" | "ontologies";
  onChange: (tab: "search" | "docs" | "ontologies") => void;
  darkMode: boolean;
};

export function NavButtons({ activeTab, onChange, darkMode }: NavButtonsProps) {
  const baseButton = darkMode
    ? "rounded-xl border border-[#4C433D] bg-[#2A2724] px-4 py-3 text-[#F5F1E6] transition hover:bg-[#342F2B]"
    : "rounded-xl border border-[#E2D39D] bg-[#FDFBE8] px-4 py-3 text-[#3C2E1F] transition hover:bg-[#F5EFC8]";

  const activeButton = darkMode
    ? "rounded-xl bg-[#4A3B32] px-4 py-3 font-bold text-[#F5F1E6] shadow-sm"
    : "rounded-xl bg-[#E7D89B] px-4 py-3 font-bold text-[#2A1F16] shadow-sm";

  return (
    <div className="flex flex-wrap gap-3">
      <button
        type="button"
        className={activeTab === "search" ? activeButton : baseButton}
        onClick={() => onChange("search")}
      >
        Search
      </button>
      <button
        type="button"
        className={activeTab === "docs" ? activeButton : baseButton}
        onClick={() => onChange("docs")}
      >
        Documentation
      </button>
      <button
        type="button"
        className={activeTab === "ontologies" ? activeButton : baseButton}
        onClick={() => onChange("ontologies")}
      >
        Ontologies
      </button>
    </div>
  );
}
