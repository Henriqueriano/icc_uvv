type NavButtonsProps = {
  activeTab: "search" | "docs" | "ontologies" ;
  onChange: (tab: "search" | "docs" | "ontologies") => void;
};

export function NavButtons({ activeTab, onChange }: NavButtonsProps) {
  return (
    <div className="nav-button-group">
      <button
        type="button"
        className={activeTab === "search" ? "nav-button active" : "nav-button"}
        onClick={() => onChange("search")}
      >
        Pesquisa
      </button>
      <button
        type="button"
        className={activeTab === "docs" ? "nav-button active" : "nav-button"}
        onClick={() => onChange("docs")}
      >
        Documentação
      </button>
      <button
        type="button"
        className={activeTab === "ontologies" ? "nav-button active" : "nav-button"}
        onClick={() => onChange("ontologies")}
      >
        Ontologias
      </button>
    </div>
  );
}
