import { uiText, type Language } from "../i18n";

type DocumentationSectionProps = {
  darkMode: boolean;
  language: Language;
};

export function DocumentationSection({ darkMode, language }: DocumentationSectionProps) {
  const translation = uiText[language];

  return (
    <main className="block">
      <section className={darkMode ? "w-full rounded-[18px] border border-[#453F39] bg-[#2A2724] p-6" : "w-full rounded-[18px] border border-[#E7DDB3] bg-[#F4EBC9] p-6"}>
        <div className={darkMode ? "rounded-2xl bg-[#171614] p-6" : "rounded-2xl bg-[#FDFBE8] p-6"}>
          <div className="mb-6 text-center">
            <h1 className={darkMode ? "mb-2 text-3xl font-bold text-[#F5F1E6]" : "mb-2 text-3xl font-bold text-[#2A1F16]"}>{translation.documentation}</h1>
            <div className="flex justify-center">
              <hr className={darkMode ? "w-50 border-0 border-t border-[#8C6E4C]" : "w-50 border-0 border-t border-[#A57A4B]"} />
            </div>
          </div>

          <div className="space-y-5 text-left">
            <div className={darkMode ? "rounded-xl border border-[#453F39] bg-[#221F1D] p-4" : "rounded-xl border border-[#E7DDB3] bg-[#F7F0D1] p-4"}>
              <h2 className={darkMode ? "mb-2 text-xl font-semibold text-[#F5F1E6]" : "mb-2 text-xl font-semibold text-[#2A1F16]"}>{translation.documentationOverviewTitle}</h2>
              <p className={darkMode ? "text-sm leading-7 text-[#E8DCC2]" : "text-sm leading-7 text-[#524332]"}>
                {translation.documentationOverviewText}
              </p>
            </div>

            <div className="grid gap-4 md:grid-cols-2">
              <div className={darkMode ? "rounded-xl border border-[#453F39] bg-[#221F1D] p-4" : "rounded-xl border border-[#E7DDB3] bg-[#F7F0D1] p-4"}>
                <h3 className={darkMode ? "mb-2 text-base font-semibold text-[#F5F1E6]" : "mb-2 text-base font-semibold text-[#2A1F16]"}>{translation.documentationSearchTitle}</h3>
                <p className={darkMode ? "text-sm leading-6 text-[#E8DCC2]" : "text-sm leading-6 text-[#524332]"}>
                  {translation.documentationSearchText}
                </p>
              </div>

              <div className={darkMode ? "rounded-xl border border-[#453F39] bg-[#221F1D] p-4" : "rounded-xl border border-[#E7DDB3] bg-[#F7F0D1] p-4"}>
                <h3 className={darkMode ? "mb-2 text-base font-semibold text-[#F5F1E6]" : "mb-2 text-base font-semibold text-[#2A1F16]"}>{translation.documentationSparqlTitle}</h3>
                <p className={darkMode ? "text-sm leading-6 text-[#E8DCC2]" : "text-sm leading-6 text-[#524332]"}>
                  {translation.documentationSparqlText}
                </p>
              </div>
            </div>

            <div className={darkMode ? "rounded-xl border border-[#453F39] bg-[#221F1D] p-4" : "rounded-xl border border-[#E7DDB3] bg-[#F7F0D1] p-4"}>
              <h3 className={darkMode ? "mb-2 text-base font-semibold text-[#F5F1E6]" : "mb-2 text-base font-semibold text-[#2A1F16]"}>{translation.documentationGuideTitle}</h3>
              <ul className={darkMode ? "list-disc space-y-2 pl-5 text-sm leading-6 text-[#E8DCC2]" : "list-disc space-y-2 pl-5 text-sm leading-6 text-[#524332]"}>
                {translation.documentationGuideItems.map((item) => (
                  <li key={item}>{item}</li>
                ))}
              </ul>
            </div>
          </div>
        </div>
      </section>
    </main>
  );
}

export default DocumentationSection;
