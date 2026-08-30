import { uiText, type Language } from "../i18n";

type OntologiesSectionProps = {
  darkMode: boolean;
  language: Language;
};

export function OntologiesSection({ darkMode, language }: OntologiesSectionProps) {
  const translation = uiText[language];

  return (
    <main className="block">
      <section className={darkMode ? "w-full rounded-[18px] border border-[#453F39] bg-[#2A2724] p-6" : "w-full rounded-[18px] border border-[#E7DDB3] bg-[#F4EBC9] p-6"}>
        <div className={darkMode ? "flex min-h-[220px] flex-col items-center justify-center rounded-2xl bg-[#171614] p-6 text-center" : "flex min-h-[220px] flex-col items-center justify-center rounded-2xl bg-[#FDFBE8] p-6 text-center"}>
          <h1 className={darkMode ? "mb-2 text-3xl font-bold text-[#F5F1E6]" : "mb-2 text-3xl font-bold text-[#2A1F16]"}>{translation.ontologies}</h1>
          <div className="flex justify-center">
            <hr className={darkMode ? "w-50 border-0 border-t border-[#8C6E4C]" : "w-50 border-0 border-t border-[#A57A4B]"} />
          </div>
        </div>
      </section>
    </main>
  );
}

export default OntologiesSection;
