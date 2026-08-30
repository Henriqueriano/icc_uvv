import { useState } from "react";
import { DocumentationSection } from "../components/DocumentationSection";
import { NavButtons } from "../components/NavButtons";
import { OntologiesSection } from "../components/OntologiesSection";
import { SearchSection } from "../components/SearchSection";

type MainViewProps = {
  darkMode: boolean;
};

export function MainView({ darkMode }: MainViewProps) {
  const [activeTab, setActiveTab] = useState<"search" | "docs" | "ontologies">("search");
  const [searchView, setSearchView] = useState<"free" | "sparql">("free");

  const shellClass = darkMode
    ? "mx-auto w-full max-w-[1200px] rounded-[22px] border border-[#453F39] bg-[#1F1D1A] p-5 shadow-[0_18px_45px_rgba(0,0,0,0.28)]"
    : "mx-auto w-full max-w-[1200px] rounded-[22px] border border-[#E4D7A2] bg-[#F9F5D8] p-5 shadow-[0_18px_45px_rgba(87,66,35,0.12)]";

  const navClass = darkMode
    ? "mb-6 flex items-end justify-start rounded-2xl border border-[#534C45] bg-[#2A2724] p-5"
    : "mb-6 flex items-end justify-start rounded-2xl border border-[#E7DDB3] bg-[#F2E9C7] p-5";

  const labelClass = darkMode ? "text-[11px] uppercase tracking-[0.08em] text-[#E8DCC2]" : "text-[11px] uppercase tracking-[0.08em] text-[#5B4A35]";

  return (
    <div className="flex min-h-[calc(100vh-64px)] items-center justify-center px-3 py-6">
      <div className={shellClass}>
        <nav className={navClass}>
          <div className="flex w-full flex-col gap-2 md:max-w-[420px]">
            <label className={labelClass}>Menu</label>
            <NavButtons activeTab={activeTab} onChange={setActiveTab} darkMode={darkMode} />
          </div>
        </nav>

        {activeTab === "search" && <SearchSection searchView={searchView} onSearchViewChange={setSearchView} darkMode={darkMode} />}
        {activeTab === "docs" && <DocumentationSection darkMode={darkMode} />}
        {activeTab === "ontologies" && <OntologiesSection darkMode={darkMode} />}
      </div>
    </div>
  );
}

export default MainView;
