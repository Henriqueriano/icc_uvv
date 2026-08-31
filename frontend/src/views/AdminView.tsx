import { useState } from "react";
import { uiText, type Language } from "../i18n";
import { AdminInsertSection } from "../components/AdminInsertSection";
import { AdminStatisticsSection } from "../components/AdminStatisticsSection";

type AdminViewProps = {
  darkMode: boolean;
  language: Language;
};

type AdminSection = "insert" | "statistics";

export function AdminView({ darkMode, language }: AdminViewProps) {
  const [activeSection, setActiveSection] = useState<AdminSection>("statistics");
  const translation = uiText[language];

  const shellClass = darkMode
    ? "mx-auto w-full max-w-[1200px] rounded-[22px] border border-[#453F39] bg-[#1F1D1A] p-5 shadow-[0_18px_45px_rgba(0,0,0,0.28)]"
    : "mx-auto w-full max-w-[1200px] rounded-[22px] border border-[#E4D7A2] bg-[#F9F5D8] p-5 shadow-[0_18px_45px_rgba(87,66,35,0.12)]";

  const navClass = darkMode
    ? "mb-6 flex items-end justify-start rounded-2xl border border-[#534C45] bg-[#2A2724] p-5"
    : "mb-6 flex items-end justify-start rounded-2xl border border-[#E7DDB3] bg-[#F2E9C7] p-5";

  const baseButtonClass = darkMode
    ? "rounded-xl border border-[#4C433D] bg-[#2A2724] px-4 py-3 text-[#F5F1E6] transition hover:bg-[#342F2B]"
    : "rounded-xl border border-[#E2D39D] bg-[#FDFBE8] px-4 py-3 text-[#3C2E1F] transition hover:bg-[#F5EFC8]";

  const activeButtonClass = darkMode
    ? "rounded-xl bg-[#4A3B32] px-4 py-3 font-bold text-[#F5F1E6] shadow-sm"
    : "rounded-xl bg-[#E7D89B] px-4 py-3 font-bold text-[#2A1F16] shadow-sm";

  return (
    <div className="flex min-h-[calc(100vh-64px)] items-center justify-center px-3 py-6">
      <div className={shellClass}>
        <nav className={navClass}>
          <div className="flex w-full flex-col gap-2 md:max-w-[420px]">
            <div className="flex flex-wrap gap-3">
              <button
                type="button"
                onClick={() => setActiveSection("insert")}
                className={activeSection === "insert" ? activeButtonClass : baseButtonClass}
              >
                {translation.statsInsert}
              </button>

              <button
                type="button"
                onClick={() => setActiveSection("statistics")}
                className={activeSection === "statistics" ? activeButtonClass : baseButtonClass}
              >
                {translation.statsMetrics}
              </button>
            </div>
          </div>
        </nav>

        {activeSection === "insert" ? <AdminInsertSection darkMode={darkMode} language={language} /> : <AdminStatisticsSection darkMode={darkMode} />}
      </div>
    </div>
  );
}

export default AdminView;
