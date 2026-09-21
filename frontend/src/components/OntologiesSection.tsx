import { useEffect, useState } from "react";
import { uiText, type Language } from "../i18n";
import { listOntologies } from "../api";

type Ontology = {
  name: string;
  description: string;
  profile: { name: string; contact: string; role: string; resume: string };
  documentation: string;
  terms: string[];
  authors: Array<{ authorName: string; portfolioUrl: string }>;
  baseDocuments: Array<{ link: string; description: string }>;
};

type OntologiesSectionProps = {
  darkMode: boolean;
  language: Language;
};

export function OntologiesSection({ darkMode, language }: OntologiesSectionProps) {
  const translation = uiText[language];
  const [ontologies, setOntologies] = useState<Ontology[]>([]);
  const [selectedOntology, setSelectedOntology] = useState<Ontology>();

  useEffect(() => {
    listOntologies().then((items) => {
      const loaded = items.map((item) => ({
        name: item.name || item.iri,
        description: item.description || "Ontologia carregada do QLever.",
        profile: {
          name: item.authors.map((author) => author.authorName).filter(Boolean).join(" e "),
          role: item.profileArea,
          resume: item.profileResume,
          contact: item.profileSource,
        },
        documentation: item.documentation,
        terms: item.terms ?? [],
        authors: item.authors ?? [],
        baseDocuments: item.baseDocuments ?? [],
      }));
      setOntologies(loaded);
      setSelectedOntology(loaded[0]);
    }).catch(() => undefined);
  }, []);

  const panel = darkMode ? "rounded-[18px] border border-[#453F39] bg-[#2A2724]" : "rounded-[18px] border border-[#E7DDB3] bg-[#F4EBC9]";
  const inner = darkMode ? "rounded-2xl bg-[#171614]" : "rounded-2xl bg-[#FDFBE8]";
  const text = darkMode ? "text-[#E8DCC2]" : "text-[#524332]";
  const heading = darkMode ? "text-[#F5F1E6]" : "text-[#2A1F16]";

  return (
    <main className="block">
      <section className={`${panel} w-full p-6`}>
        <div className={`${inner} p-6`}>
          <div className="mb-6 text-center">
            <h1 className={`mb-2 text-3xl font-bold ${heading}`}>{translation.ontologiesTitle}</h1>
            <div className="flex justify-center"><hr className="w-50 border-0 border-t border-[#A57A4B]" /></div>
          </div>
          <div className={`${darkMode ? "border-[#453F39] bg-[#221F1D]" : "border-[#E7DDB3] bg-[#F7F0D1]"} rounded-xl border p-4`}>
            <div className="flex flex-wrap justify-center gap-5">
              {ontologies.map((ontology) => (
                <button key={ontology.name} type="button" onClick={() => setSelectedOntology(ontology)} className={selectedOntology?.name === ontology.name ? `flex h-24 w-32 flex-col items-center justify-center rounded-xl px-4 py-3 font-bold shadow-sm ${darkMode ? "bg-[#4A3B32] text-[#F5F1E6]" : "bg-[#E7D89B] text-[#2A1F16]"}` : `flex h-24 w-32 flex-col items-center justify-center rounded-xl border ${darkMode ? "border-[#4C433D] bg-[#2A2724] text-[#F5F1E6] transition hover:bg-[#342F2B]" : "border-[#E2D39D] bg-[#FDFBE8] text-[#3C2E1F] transition hover:bg-[#F5EFC8]"}`}>
                  <span className="text-center text-xs">{ontology.name}</span>
                </button>
              ))}
            </div>
            {selectedOntology && (<div className="mt-5 grid gap-4 lg:grid-cols-[180px_minmax(0,1fr)]">
              <div className={`${inner} flex items-center justify-center p-4`}>
                <div className="text-center">
                    <ul className={`mt-4 space-y-2 text-left text-xs ${text}`}>
                    <li><strong>Área:</strong> {selectedOntology.profile.role}</li>
                    <li className="leading-5"><strong>Currículo:</strong> {selectedOntology.profile.resume}</li>
                    <li><strong>Fonte:</strong> {selectedOntology.profile.contact}</li>
                  </ul>
                  {selectedOntology.authors.length > 0 && (
                    <div className={`mt-4 space-y-2 text-left text-xs ${text}`}>
                      <strong>Autores</strong>
                      {selectedOntology.authors.map((author) => (
                        <div key={author.authorName}>
                          <a href={author.portfolioUrl} target="_blank" rel="noreferrer" className="underline">{author.authorName}</a>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </div>
              <div className={`${inner} p-4`}>
                <div className="border-b border-[#D1B866]/40 pb-3">
                  <h3 className={`text-base font-semibold ${heading}`}>Visão geral</h3>
                  <p className={`mt-2 text-sm leading-6 ${text}`}>{selectedOntology.documentation}</p>
                </div>
                <div className="pt-3">
                  <h3 className={`mb-2 text-base font-semibold ${heading}`}>Termos e propriedades</h3>
                  <div className="flex flex-wrap gap-2">
                    {selectedOntology.terms.map((term) => <span key={term} className={`rounded-full border border-[#D7C99A] px-3 py-1 text-xs ${text}`}>{term}</span>)}
                  </div>
                </div>
                {selectedOntology.baseDocuments.length > 0 && (
                  <div className="mt-4 border-t border-[#D1B866]/40 pt-3">
                    <h3 className={`mb-2 text-base font-semibold ${heading}`}>Documentos-base da ontologia</h3>
                    <div className={`grid gap-x-4 gap-y-2 rounded-xl p-3 text-sm ${darkMode ? "bg-[#2A2724] text-[#E8DCC2]" : "bg-[#F2E9C7] text-[#5B4A35]"}`}>
                      <div className="grid grid-cols-[minmax(0,1fr)_minmax(0,1fr)] gap-4 px-3 py-2 text-xs font-semibold uppercase tracking-wide">
                        <span>Link</span>
                        <span>Descrição</span>
                      </div>
                      {selectedOntology.baseDocuments.map((document) => (
                        <div key={document.link} className="grid grid-cols-[minmax(0,1fr)_minmax(0,1fr)] gap-4 border-b border-[#D1B866]/20 pb-2 last:border-b-0">
                          <a href={document.link} target="_blank" rel="noreferrer" className="min-w-0 break-words font-medium underline">{document.link}</a>
                          <p className="min-w-0 break-words">{document.description}</p>
                        </div>
                      ))}
                    </div>
                  </div>
                )}
              </div>
            </div>)}
          </div>
        </div>
      </section>
    </main>
  );
}

export default OntologiesSection;
