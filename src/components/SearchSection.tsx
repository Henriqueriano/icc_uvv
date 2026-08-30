import { uiText, type Language } from "../i18n";
import { SearchTabs } from "./SearchTabs";

type SearchSectionProps = {
  searchView: "free" | "sparql";
  onSearchViewChange: (view: "free" | "sparql") => void;
  darkMode: boolean;
  language: Language;
};

export function SearchSection({ searchView, onSearchViewChange, darkMode, language }: SearchSectionProps) {
  const translation = uiText[language];
  const panelClass = darkMode
    ? "rounded-[18px] border border-[#453F39] bg-[#2A2724] p-5"
    : "rounded-[18px] border border-[#E7DDB3] bg-[#F4EBC9] p-5";

  const contentPanelClass = darkMode
    ? "min-h-[100px] rounded-[18px] border border-[#453F39] bg-[#2A2724] p-6"
    : "min-h-[100px] rounded-[18px] border border-[#E7DDB3] bg-[#F4EBC9] p-6";

  const innerPanelClass = darkMode
    ? "flex h-full min-h-[360px] flex-col items-center justify-center gap-2 rounded-2xl bg-[#1F1D1A] p-6"
    : "flex h-full min-h-[360px] flex-col items-center justify-center gap-2 rounded-2xl bg-[#FDFBE8] p-6";

  const titleClass = darkMode ? "text-3xl font-bold text-[#F5F1E6]" : "text-3xl font-bold text-[#2A1F16]";
  const secondaryTextClass = darkMode ? "text-[#E8DCC2]" : "text-[#524332]";
  const inputClass = darkMode
    ? "h-full min-h-[270px] w-full flex-1 resize-none rounded-xl border border-[#4C433D] bg-[#171614] px-4 py-3 text-left align-top text-[#F5F1E6] placeholder:text-left placeholder:align-top placeholder:text-[#B7A98E] focus:outline-none focus:ring-2 focus:ring-[#D1B866]/70"
    : "h-full min-h-[270px] w-full flex-1 resize-none rounded-xl border border-[#E2D39D] bg-[#FFFDF3] px-4 py-3 text-left align-top text-[#2A1F16] placeholder:text-left placeholder:align-top placeholder:text-[#7A6854] focus:outline-none focus:ring-2 focus:ring-[#D1B866]/70";

  const lowerSectionClass = darkMode
    ? "mt-5 min-h-[220px] w-full rounded-[18px] border border-[#453F39] bg-[#2A2724] p-6"
    : "mt-5 min-h-[220px] w-full rounded-[18px] border border-[#E7DDB3] bg-[#F4EBC9] p-6";

  return (
    <>
      <main className="grid gap-5 md:grid-cols-[minmax(220px,30%)_minmax(0,70%)]">
        <aside className={panelClass}>
          <SearchTabs activeView={searchView} onChange={onSearchViewChange} darkMode={darkMode} language={language} />
        </aside>

        <section className={contentPanelClass}>
          {searchView === "free" ? (
            <div className={innerPanelClass}>
              <div className="w-full" />
              <div className="flex w-full max-w-[900px] flex-1 items-center justify-center">
                <textarea id="free-search" placeholder={translation.freeSearchInput} className={inputClass} />
              </div>
            </div>
          ) : (
            <div className={innerPanelClass}>
              <div className="w-full" />
              <div className="flex w-full max-w-[900px] flex-1 items-center justify-center">
                <textarea id="sparql-search" placeholder={translation.sparqlInput} className={inputClass} />
              </div>
            </div>
          )}
        </section>
      </main>

      <div className={lowerSectionClass}>
        <h3 className={darkMode ? "mb-2 text-xl font-semibold text-[#F5F1E6]" : "mb-2 text-xl font-semibold text-[#2A1F16]"}>{translation.lowerSection}</h3>
        <p className={secondaryTextClass}>{translation.lowerSectionText}</p>
      </div>
    </>
  );
}

export default SearchSection;
