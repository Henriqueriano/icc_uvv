import { useRef, useState } from "react";
import { ArrowUpToLine } from "lucide-react";
import { uiText, type Language } from "../i18n";
import { importPdf, importRdf } from "../api";

type AdminInsertSectionProps = {
  darkMode: boolean;
  language: Language;
};

export function AdminInsertSection({ darkMode, language }: AdminInsertSectionProps) {
  const translation = uiText[language];
  const [files, setFiles] = useState<File[]>([]);
  const [pdfFile, setPdfFile] = useState<File | null>(null);
  const [message, setMessage] = useState("");
  const [loading, setLoading] = useState(false);
  const [pdfLoading, setPdfLoading] = useState(false);
  const [pdfMessage, setPdfMessage] = useState("");

  // Qualquer importação em andamento trava as duas áreas de envio.
  const isBusy = loading || pdfLoading;

  // Guarda síncrona: o estado só vale no próximo render, então dois cliques
  // muito rápidos ainda passariam por `disabled`.
  const busyRef = useRef(false);

  const handlePdfImport = async () => {
    if (busyRef.current) return;
    if (!pdfFile) {
      setPdfMessage("Selecione um arquivo PDF antes de importar.");
      return;
    }

    busyRef.current = true;
    setPdfLoading(true);
    setPdfMessage("");
    try {
      const result = await importPdf(pdfFile, "icc_uvv");
      setPdfMessage(`PDF processado pelo Ollama: ${result.tripleCount} triplas persistidas.`);
    } catch (error) {
      setPdfMessage(error instanceof Error ? error.message : "Falha no processamento do PDF.");
    } finally {
      busyRef.current = false;
      setPdfLoading(false);
    }
  };

  const handleImport = async () => {
    if (busyRef.current) return;
    if (files.length === 0) {
      setMessage("Selecione um arquivo ou uma pasta RDF antes de importar.");
      return;
    }

    busyRef.current = true;
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
      const supportedFiles = files.filter((file) => {
        const extension = file.name.split(".").pop()?.toLowerCase() ?? "";
        return extension in formatByExtension;
      });
      if (supportedFiles.length === 0) {
        throw new Error("Nenhum arquivo RDF compatível foi encontrado.");
      }

      let totalTriples = 0;
      for (const file of supportedFiles) {
        const extension = file.name.split(".").pop()?.toLowerCase() ?? "";
        const result = await importRdf(file, "icc_uvv", formatByExtension[extension]);
        totalTriples += result.tripleCount;
      }
      setMessage(`${supportedFiles.length} arquivo(s) validado(s): ${totalTriples} triplas.`);
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "Falha na importação.");
    } finally {
      busyRef.current = false;
      setLoading(false);
    }
  };

  // Aplicado aos <label> dos inputs de arquivo: sem clique e visualmente apagado.
  const lockedLabelClass = isBusy ? "pointer-events-none opacity-50" : "";

  return (
    <section
      className={
        darkMode
          ? "rounded-2xl border border-[#453F39] bg-[#2A2724] p-6"
          : "rounded-2xl border border-[#E7DDB3] bg-[#F4EBC9] p-6"
      }
      aria-busy={isBusy}
    >
      <div className="mb-6 text-center">
        <h2 className={darkMode ? "text-2xl font-bold text-[#F5F1E6]" : "text-2xl font-bold text-[#2A1F16]"}>
          {translation.statsInsert}
        </h2>
        <div className="mt-3 flex justify-center">
          <hr className={darkMode ? "w-24 border-0 border-t border-[#8C6E4C]" : "w-24 border-0 border-t border-[#A57A4B]"} />
        </div>
      </div>

      <div className="mb-4">
        <fieldset disabled={isBusy} className={darkMode ? "min-w-0 rounded-2xl border-0 bg-[#171614] p-5" : "min-w-0 rounded-2xl border-0 bg-[#FDFBE8] p-5"}>
          <h3 className={darkMode ? "mb-2 text-xl font-semibold text-[#F5F1E6]" : "mb-2 text-xl font-semibold text-[#2A1F16]"}>
            Importar PDF com Ollama
          </h3>
          <p className={darkMode ? "mb-4 text-sm leading-6 text-[#E8DCC2]" : "mb-4 text-sm leading-6 text-[#524332]"}>
            O texto do PDF será extraído, convertido em RDF pelo Ollama e persistido no QLever.
          </p>
          <label
            htmlFor="pdf-file"
            aria-disabled={isBusy}
            className={`${darkMode
              ? "mb-3 flex min-h-12 w-full cursor-pointer items-center rounded-xl border border-[#8C6E4C] bg-[#2A2724] px-4 py-3 text-sm text-[#E8DCC2] transition hover:bg-[#342F2B]"
              : "mb-3 flex min-h-12 w-full cursor-pointer items-center rounded-xl border border-[#A57A4B] bg-[#FDFBE8] px-4 py-3 text-sm text-[#524332] transition hover:bg-[#F5EFC8]"} ${lockedLabelClass}`}
          >
            <span className="truncate">{pdfFile?.name ?? "Selecionar arquivo PDF"}</span>
            <input
              id="pdf-file"
              type="file"
              accept="application/pdf,.pdf"
              disabled={isBusy}
              onChange={(event) => {
                setPdfFile(event.target.files?.[0] ?? null);
                setPdfMessage("");
              }}
              className="sr-only"
            />
          </label>
          <button
            type="button"
            onClick={handlePdfImport}
            disabled={isBusy || !pdfFile}
            className={darkMode
              ? "inline-flex items-center gap-2 rounded-xl border border-dashed border-[#8C6E4C] bg-[#2A2724] px-4 py-2.5 text-sm font-medium text-[#F5F1E6] transition hover:bg-[#342F2B] disabled:cursor-not-allowed disabled:opacity-50"
              : "inline-flex items-center gap-2 rounded-xl border border-dashed border-[#A57A4B] bg-[#FDFBE8] px-4 py-2.5 text-sm font-medium text-[#2A1F16] transition hover:bg-[#F5EFC8] disabled:cursor-not-allowed disabled:opacity-50"}
          >
            <ArrowUpToLine size={16} />
            {pdfLoading ? "Processando..." : "Processar e importar PDF"}
          </button>
          {pdfMessage && <p className="mt-3 text-sm" role="status">{pdfMessage}</p>}
        </fieldset>
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        <fieldset disabled={isBusy} className={darkMode ? "min-w-0 rounded-2xl border-0 bg-[#171614] p-5" : "min-w-0 rounded-2xl border-0 bg-[#FDFBE8] p-5"}>
          <h3 className={darkMode ? "mb-2 text-xl font-semibold text-[#F5F1E6]" : "mb-2 text-xl font-semibold text-[#2A1F16]"}>
            {translation.statsImportTitle}
          </h3>
          <p className={darkMode ? "mb-4 text-sm leading-6 text-[#E8DCC2]" : "mb-4 text-sm leading-6 text-[#524332]"}>
            {translation.statsImportDescription}
          </p>
          <label
            htmlFor="rdf-file"
            aria-disabled={isBusy}
            className={`${
              darkMode
                ? "mb-3 flex min-h-12 w-full cursor-pointer items-center rounded-xl border border-[#8C6E4C] bg-[#2A2724] px-4 py-3 text-sm text-[#E8DCC2] transition hover:bg-[#342F2B]"
                : "mb-3 flex min-h-12 w-full cursor-pointer items-center rounded-xl border border-[#A57A4B] bg-[#FDFBE8] px-4 py-3 text-sm text-[#524332] transition hover:bg-[#F5EFC8]"
            } ${lockedLabelClass}`}
          >
            <span className="truncate">{files.length === 1 ? files[0].name : files.length > 1 ? `${files.length} arquivos selecionados` : "Selecionar arquivo RDF"}</span>
            <input
              id="rdf-file"
              type="file"
              multiple
              accept=".rdf,.ttl,.nt,.nq,.xml"
              disabled={isBusy}
              onChange={(event) => {
                setFiles(event.target.files ? Array.from(event.target.files) : []);
                setMessage("");
              }}
              className="sr-only"
            />
          </label>
          <button
            type="button"
            onClick={handleImport}
            disabled={isBusy || files.length === 0}
            className={
              darkMode
                ? "inline-flex items-center gap-2 rounded-xl border border-dashed border-[#8C6E4C] bg-[#2A2724] px-4 py-2.5 text-sm font-medium text-[#F5F1E6] transition hover:bg-[#342F2B] disabled:cursor-not-allowed disabled:opacity-50"
                : "inline-flex items-center gap-2 rounded-xl border border-dashed border-[#A57A4B] bg-[#FDFBE8] px-4 py-2.5 text-sm font-medium text-[#2A1F16] transition hover:bg-[#F5EFC8] disabled:cursor-not-allowed disabled:opacity-50"
            }
          >
            <ArrowUpToLine size={16} />
            {loading ? "Enviando..." : files.length > 1 ? "Importar arquivos" : "Importar arquivo"}
          </button>
          {message && <p className="mt-3 text-sm" role="status">{message}</p>}
        </fieldset>

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
