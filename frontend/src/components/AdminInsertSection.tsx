import { useState } from "react";
import { ArrowUpToLine } from "lucide-react";
import { uiText, type Language } from "../i18n";
import { importRdf } from "../api";

type AdminInsertSectionProps = {
  darkMode: boolean;
  language: Language;
};

export function AdminInsertSection({ darkMode, language }: AdminInsertSectionProps) {
  const translation = uiText[language];
  const [file, setFile] = useState<File | null>(null);
  const [message, setMessage] = useState("");
  const [loading, setLoading] = useState(false);
  const handleImport = async () => {
    if (!file) {
      setMessage("Selecione um arquivo RDF antes de importar.");
      return;
    }

    setLoading(true);
    setMessage("");
    try {
      const formatByExtension: Record<string, string> = {
        ttl: "text/turtle",
        rdf: "application/rdf+xml",
        xml: "application/rdf+xml",
        nt: "application/n-triples",
        nq: "application/n-quads",
      };
      const extension = file.name.split(".").pop()?.toLowerCase() ?? "";
      const result = await importRdf(file, "icc_uvv", formatByExtension[extension]);
      setMessage(`Arquivo validado: ${result.tripleCount} triplas.`);
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "Falha na importação.");
    } finally {
      setLoading(false);
    }
  };

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
          <input
            type="file"
            accept=".rdf,.ttl,.nt,.nq,.n3,.jsonld,.xml"
            onChange={(event) => {
              setFile(event.target.files?.[0] ?? null);
              setMessage("");
            }}
            className="mb-3 block w-full text-sm"
          />
          <button
            type="button"
            onClick={handleImport}
            disabled={loading}
            className={
              darkMode
                ? "inline-flex items-center gap-2 rounded-xl border border-dashed border-[#8C6E4C] bg-[#2A2724] px-4 py-2.5 text-sm font-medium text-[#F5F1E6] transition hover:bg-[#342F2B]"
                : "inline-flex items-center gap-2 rounded-xl border border-dashed border-[#A57A4B] bg-[#FDFBE8] px-4 py-2.5 text-sm font-medium text-[#2A1F16] transition hover:bg-[#F5EFC8]"
            }
          >
            <ArrowUpToLine size={16} />
            {loading ? "Enviando..." : "Importar arquivo"}
          </button>
          {message && <p className="mt-3 text-sm">{message}</p>}
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
