import { uiText, type Language } from "../i18n";

type AdminInsertSectionProps = {
  darkMode: boolean;
  language: Language;
};

export function AdminInsertSection({ darkMode, language }: AdminInsertSectionProps) {
  const translation = uiText[language];

  return (
    <section
      className={
        darkMode
          ? "rounded-2xl border border-[#453F39] bg-[#2A2724] p-6"
          : "rounded-2xl border border-[#E7DDB3] bg-[#F4EBC9] p-6"
      }
    >
      <div className="mb-6 text-center">
        <h2 className={darkMode ? "text-2xl font-bold text-[#F5F1E6]" : "text-2xl font-bold text-[#2A1F16]"}>
          {translation.statsInsert}
        </h2>
        <div className="mt-3 flex justify-center">
          <hr className={darkMode ? "w-24 border-0 border-t border-[#8C6E4C]" : "w-24 border-0 border-t border-[#A57A4B]"} />
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        <div className={darkMode ? "rounded-2xl bg-[#171614] p-5" : "rounded-2xl bg-[#FDFBE8] p-5"}>
          <h3 className={darkMode ? "mb-2 text-xl font-semibold text-[#F5F1E6]" : "mb-2 text-xl font-semibold text-[#2A1F16]"}>
            {translation.statsImportTitle}
          </h3>
          <p className={darkMode ? "mb-4 text-sm leading-6 text-[#E8DCC2]" : "mb-4 text-sm leading-6 text-[#524332]"}>
            {translation.statsImportDescription}
          </p>
          <button
            type="button"
            className={
              darkMode
                ? "rounded-full bg-[#4A3B32] px-4 py-2 text-sm font-medium text-[#F5F1E6]"
                : "rounded-full bg-[#E7D89B] px-4 py-2 text-sm font-medium text-[#2A1F16]"
            }
          >
            Importar arquivo
          </button>
        </div>

        <div className={darkMode ? "rounded-2xl bg-[#171614] p-5" : "rounded-2xl bg-[#FDFBE8] p-5"}>
          <h3 className={darkMode ? "mb-2 text-xl font-semibold text-[#F5F1E6]" : "mb-2 text-xl font-semibold text-[#2A1F16]"}>
            Validação
          </h3>
          <p className={darkMode ? "text-sm leading-6 text-[#E8DCC2]" : "text-sm leading-6 text-[#524332]"}>
            Os dados importados passam por validação estrutural antes de entrarem na base RDF do sistema.
          </p>
        </div>
      </div>
    </section>
  );
}

export default AdminInsertSection;
