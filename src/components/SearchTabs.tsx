type SearchTabsProps = {
  activeView: "perguntas" | "livre";
  onChange: (view: "perguntas" | "livre") => void;
};

export function SearchTabs({ activeView, onChange }: SearchTabsProps) {
  return (
    <div className="search-tabs">
      <button
        type="button"
        className={activeView === "perguntas" ? "search-tab active" : "search-tab"}
        onClick={() => onChange("perguntas")}
      >
        Perguntas
      </button>
      <button
        type="button"
        className={activeView === "livre" ? "search-tab active" : "search-tab"}
        onClick={() => onChange("livre")}
      >
        Livre
      </button>
    </div>
  );
}
