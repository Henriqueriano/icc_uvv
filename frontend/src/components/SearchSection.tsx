import { useState, type FormEvent } from "react";
import { uiText, type Language } from "../i18n";
import { ArrowRight } from "lucide-react";
import { SearchTabs } from "./SearchTabs";
import { executeSparql, search } from "../api";

type SearchSectionProps = {
  searchView: "free" | "sparql";
  onSearchViewChange: (view: "free" | "sparql") => void;
  darkMode: boolean;
  language: Language;
};

export function SearchSection({ searchView, onSearchViewChange, darkMode, language }: SearchSectionProps) {
  const [searchResult, setSearchResult] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
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
  const submitClass = darkMode
    ? "inline-flex shrink-0 items-center justify-center rounded-full border border-[#4C433D] bg-[#4A3B32] p-3 text-[#F5F1E6] transition hover:bg-[#5A493E] focus:outline-none focus:ring-2 focus:ring-[#D1B866]/70"
    : "inline-flex shrink-0 items-center justify-center rounded-full border border-[#D7C99A] bg-[#E7D89B] p-3 text-[#2A1F16] transition hover:bg-[#F0E4AA] focus:outline-none focus:ring-2 focus:ring-[#D1B866]/70";

  const lowerSectionClass = darkMode
    ? "mt-5 min-h-[220px] w-full rounded-[18px] border border-[#453F39] bg-[#2A2724] p-6"
    : "mt-5 min-h-[220px] w-full rounded-[18px] border border-[#E7DDB3] bg-[#F4EBC9] p-6";

  const handleSearchSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const formData = new FormData(event.currentTarget);
    const query = String(formData.get("query") ?? "").trim();
    setLoading(true);
    setError("");
    try {
      const result = searchView === "free" ? await search(query) : await executeSparql(query);
      setSearchResult(JSON.stringify(result, null, 2));
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : "Falha ao consultar a API.");
    } finally {
      setLoading(false);
    }
  };

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
              <form onSubmit={handleSearchSubmit} className="flex w-full max-w-[900px] flex-1 items-center justify-center gap-3">
                <textarea id="free-search" name="query" placeholder={translation.freeSearchInput} className={inputClass} />
                <button type="submit" className={submitClass} aria-label={translation.freeSearch}>
                  {loading ? "..." : <ArrowRight size={22} aria-hidden="true" />}
                </button>
              </form>
            </div>
          ) : (
            <div className={innerPanelClass}>
              <div className="w-full" />
              <form onSubmit={handleSearchSubmit} className="flex w-full max-w-[900px] flex-1 items-center justify-center gap-3">
                <textarea
                  id="sparql-search"
                  name="query"
                  placeholder={"SELECT ?subject ?predicate ?object\nWHERE {\n  ?subject ?predicate ?object .\n}"}
                  className={inputClass}
                />
                <button type="submit" className={submitClass} aria-label={translation.sparql}>
                  {loading ? "..." : <ArrowRight size={22} aria-hidden="true" />}
                </button>
              </form>
            </div>
          )}
        </section>
      </main>

      <div className={lowerSectionClass}>
        <h3 className={darkMode ? "mb-2 text-xl font-semibold text-[#F5F1E6]" : "mb-2 text-xl font-semibold text-[#2A1F16]"}>{translation.lowerSection}</h3>
        {error ? <p className="whitespace-pre-wrap text-sm text-red-700">{error}</p> : searchResult ? <pre className="max-h-96 overflow-auto whitespace-pre-wrap text-sm">{searchResult}</pre> : <p className={secondaryTextClass}>{translation.lowerSectionText}</p>}
      </div>
    </>
  );
}

export default SearchSection;
