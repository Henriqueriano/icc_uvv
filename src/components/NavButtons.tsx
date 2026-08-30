type NavButtonsProps = {
  activeTab: "search" | "onto_docs";
  onChange: (tab: "search" | "onto_docs") => void;
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
        className={activeTab === "onto_docs" ? "nav-button active" : "nav-button"}
        onClick={() => onChange("onto_docs")}
      >
        Documentação
      </button>
    </div>
  );
}
