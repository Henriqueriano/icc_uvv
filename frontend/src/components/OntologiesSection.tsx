import { useState } from "react";
import { BookOpen } from "lucide-react";
import { uiText, type Language } from "../i18n";

type OntologiesSectionProps = {
  darkMode: boolean;
  language: Language;
};

export function OntologiesSection({ darkMode, language }: OntologiesSectionProps) {
  const translation = uiText[language];
  const ontologies = [
    {
      name: "FOAF",
      description: "Pessoas e relações sociais",
      image: "/logo.svg",
      profile: {
        name: "Marina Costa",
        contact: "marina.costa@example.com",
        role: "Pesquisadora em Web Semântica",
        resume: "Mestranda em Ciência da Computação, com foco em grafos de conhecimento, interoperabilidade e representação de perfis sociais.",
      },
      documentation:
        "A ontologia FOAF descreve pessoas, perfis e conexões sociais em um grafo semântico. Ela permite representar informações como nome, endereço de e-mail, página pessoal e relações de conhecimento entre indivíduos. Neste exemplo, os termos podem ser usados para localizar pessoas e compreender como diferentes recursos se relacionam.",
      terms: ["Person", "name", "knows", "mbox"],
    },
    {
      name: "DCAT",
      description: "Catálogos e conjuntos de dados",
      image: "/react.svg",
      profile: {
        name: "Rafael Mendes",
        contact: "rafael.mendes@example.com",
        role: "Analista de Dados Abertos",
        resume: "Especialista em organização de catálogos, publicação de datasets e integração de fontes públicas com vocabulários semânticos.",
      },
      documentation:
        "A ontologia DCAT organiza catálogos, conjuntos de dados e suas distribuições. Com ela, é possível documentar quem publica um recurso, quais formatos estão disponíveis, onde o conteúdo pode ser baixado e quais conjuntos pertencem a um determinado catálogo. Este texto representa uma documentação resumida para fins de demonstração.",
      terms: ["Dataset", "Catalog", "distribution", "downloadURL"],
    },
    {
      name: "Schema",
      description: "Estruturas e entidades",
      image: "/logo.svg",
      profile: {
        name: "Camila Souza",
        contact: "camila.souza@example.com",
        role: "Arquiteta de Informação",
        resume: "Profissional dedicada à modelagem de entidades, eventos e organizações para melhorar a descoberta e a conexão entre dados.",
      },
      documentation:
        "A ontologia Schema reúne conceitos gerais para descrever entidades, organizações, eventos e locais. Sua estrutura ajuda a conectar informações de diferentes fontes por meio de propriedades comuns, facilitando a navegação e a interpretação dos dados. Os termos abaixo são exemplos simulados de recursos que poderiam aparecer em uma consulta.",
      terms: ["Thing", "Organization", "Event", "location"],
    },
  ];
  const [selectedOntology, setSelectedOntology] = useState(ontologies[0]);

  return (
    <main className="block">
      <section className={darkMode ? "w-full rounded-[18px] border border-[#453F39] bg-[#2A2724] p-6" : "w-full rounded-[18px] border border-[#E7DDB3] bg-[#F4EBC9] p-6"}>
        <div className={darkMode ? "rounded-2xl bg-[#171614] p-6" : "rounded-2xl bg-[#FDFBE8] p-6"}>
          <div className="mb-6 text-center">
            <h1 className={darkMode ? "mb-2 text-3xl font-bold text-[#F5F1E6]" : "mb-2 text-3xl font-bold text-[#2A1F16]"}>{translation.ontologiesTitle}</h1>
            <div className="flex justify-center">
              <hr className={darkMode ? "w-50 border-0 border-t border-[#8C6E4C]" : "w-50 border-0 border-t border-[#A57A4B]"} />
            </div>
          </div>
          <div className={darkMode ? "rounded-xl border border-[#453F39] bg-[#221F1D] p-4" : "rounded-xl border border-[#E7DDB3] bg-[#F7F0D1] p-4"}>
            <div className="mb-4 flex items-center gap-2">
              <BookOpen size={18} className={darkMode ? "text-[#D1B866]" : "text-[#A57A4B]"} aria-hidden="true" />
              <h2 className={darkMode ? "text-xl font-semibold text-[#F5F1E6]" : "text-xl font-semibold text-[#2A1F16]"}>Escolha uma ontologia</h2>
            </div>
            <div className="flex flex-wrap justify-center gap-5">
              {ontologies.map((ontology) => {
                const isSelected = selectedOntology.name === ontology.name;
                return (
                  <button key={ontology.name} type="button" onClick={() => setSelectedOntology(ontology)} className={isSelected ? darkMode ? "flex h-24 w-24 flex-col items-center justify-center rounded-full border-4 border-[#D1B866] bg-[#4A3B32] text-xs font-bold text-[#F5F1E6] shadow-lg" : "flex h-24 w-24 flex-col items-center justify-center rounded-full border-4 border-[#A57A4B] bg-[#E7D89B] text-xs font-bold text-[#2A1F16] shadow-lg" : darkMode ? "flex h-24 w-24 flex-col items-center justify-center rounded-full border border-[#4C433D] bg-[#2A2724] text-xs font-semibold text-[#E8DCC2] transition hover:border-[#D1B866]" : "flex h-24 w-24 flex-col items-center justify-center rounded-full border border-[#D7C99A] bg-[#FDFBE8] text-xs font-semibold text-[#5B4A35] transition hover:border-[#A57A4B]"}>
                    <span className="text-base">{ontology.name}</span>
                  </button>
                );
              })}
            </div>
            <div className="mt-5 grid gap-4 lg:grid-cols-[180px_minmax(0,1fr)]">
              <div className={darkMode ? "flex items-center justify-center rounded-xl bg-[#171614] p-4" : "flex items-center justify-center rounded-xl bg-[#FFFDF3] p-4"}>
                <div className="text-center">
                  <img
                    src={selectedOntology.image}
                    alt=""
                    aria-hidden="true"
                    className="mx-auto h-32 w-32 rounded-full border-4 border-[#D1B866] bg-[#FDFBE8] object-contain p-5"
                  />
                  <ul className={darkMode ? "mt-4 space-y-2 text-left text-xs text-[#E8DCC2]" : "mt-4 space-y-2 text-left text-xs text-[#524332]"}>
                    <li>
                      <strong>Nome:</strong> {selectedOntology.profile.name}
                    </li>
                    <li>
                      <strong>Área:</strong> {selectedOntology.profile.role}
                    </li>
                    <li className="leading-5">
                      <strong>Currículo:</strong> {selectedOntology.profile.resume}
                    </li>
                    <li>
                      <strong>Contato:</strong>{" "}
                      <a
                        href={`mailto:${selectedOntology.profile.contact}`}
                        className={darkMode ? "text-[#D1B866] underline" : "text-[#A57A4B] underline"}
                      >
                        {selectedOntology.profile.contact}
                      </a>
                    </li>
                  </ul>
                </div>
              </div>

              <div className={darkMode ? "rounded-xl bg-[#171614] p-4" : "rounded-xl bg-[#FFFDF3] p-4"}>
                <div className="border-b border-[#D1B866]/40 pb-3">
                  <h3 className={darkMode ? "text-base font-semibold text-[#F5F1E6]" : "text-base font-semibold text-[#2A1F16]"}>Visão geral</h3>
                  <p className={darkMode ? "mt-2 text-sm leading-6 text-[#D9CBB1]" : "mt-2 text-sm leading-6 text-[#524332]"}>
                    {selectedOntology.documentation}
                  </p>
                </div>

                <div className="pt-3">
                  <h3 className={darkMode ? "mb-2 text-base font-semibold text-[#F5F1E6]" : "mb-2 text-base font-semibold text-[#2A1F16]"}>Termos e propriedades</h3>
                  <p className={darkMode ? "mb-3 text-sm text-[#E8DCC2]" : "mb-3 text-sm text-[#524332]"}>Exemplos disponíveis para consulta nesta ontologia:</p>
                  <div className="flex flex-wrap gap-2">
                    {selectedOntology.terms.map((term) => <span key={term} className={darkMode ? "rounded-full border border-[#4C433D] bg-[#2A2724] px-3 py-1 text-xs text-[#E8DCC2]" : "rounded-full border border-[#D7C99A] bg-[#F7F0D1] px-3 py-1 text-xs text-[#5B4A35]"}>{term}</span>)}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>
    </main>
  );
}

export default OntologiesSection;
