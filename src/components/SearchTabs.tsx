import { uiText, type Language } from "../i18n";

type SearchTabsProps = {
  activeView: "free" | "sparql";
  onChange: (view: "free" | "sparql") => void;
  darkMode: boolean;
  language: Language;
};

export function SearchTabs({ activeView, onChange, darkMode, language }: SearchTabsProps) {
  const translation = uiText[language];
  const baseButton = darkMode
    ? "w-full rounded-xl border border-[#4C433D] bg-[#2A2724] px-4 py-3 text-left text-[#F5F1E6] transition hover:bg-[#342F2B]"
    : "w-full rounded-xl border border-[#E2D39D] bg-[#FDFBE8] px-4 py-3 text-left text-[#3C2E1F] transition hover:bg-[#F5EFC8]";

  const activeButton = darkMode
    ? "w-full rounded-xl bg-[#4A3B32] px-4 py-3 text-left font-bold text-[#F5F1E6] shadow-sm"
    : "w-full rounded-xl bg-[#E7D89B] px-4 py-3 text-left font-bold text-[#2A1F16] shadow-sm";

  return (
    <div className="flex w-full flex-col gap-3">
      <button
        type="button"
        className={activeView === "free" ? activeButton : baseButton}
        onClick={() => onChange("free")}
      >
        {translation.freeSearch}
      </button>
      <button
        type="button"
        className={activeView === "sparql" ? activeButton : baseButton}
        onClick={() => onChange("sparql")}
      >
        {translation.sparql}
      </button>
    </div>
  );
}
