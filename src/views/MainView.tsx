import { useState } from "react";
import { NavButtons } from "../components/NavButtons";
import { SearchTabs } from "../components/SearchTabs";

export function MainView() {
  const [activeTab, setActiveTab] = useState<"search" | "onto_docs">("search");
  const [searchView, setSearchView] = useState<"perguntas" | "livre">("perguntas");

  return (
    <div className="page-shell">
      <nav className="topbar">
        <div className="nav-field nav-actions">
          <label>Menu</label>
          <NavButtons activeTab={activeTab} onChange={setActiveTab} />
        </div>
      </nav>

      {activeTab === "search" ? (
        <main className="main-layout">
          <aside className="sidebar-panel">
            <SearchTabs activeView={searchView} onChange={setSearchView} />
          </aside>

          <section className="content-panel">
            <div className="content-box">
              <h2>{searchView === "perguntas" ? "Perguntas" : "Livre"}</h2>
              <p>Conteúdo principal da busca selecionada.</p>
            </div>
          </section>
        </main>
      ) : (
        <main className="documentation-layout">
          <section className="documentation-panel">
            <div className="content-box">
              <h2>Documentação</h2>
              <p>Conteúdo principal dos relatórios.</p>
            </div>
          </section>
        </main>
      )}
    </div>
  );
}

export default MainView;
