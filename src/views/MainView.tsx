import { useState } from "react";
import { NavButtons } from "../components/NavButtons";

export function MainView() {
  const [activeTab, setActiveTab] = useState<"search" | "onto_docs">("search");

  return (
    <div className="page-shell">
      <nav className="topbar">
        <div className="nav-field nav-actions">
          <label>Menu</label>
          <NavButtons activeTab={activeTab} onChange={setActiveTab} />
        </div>
      </nav>

      <main className="main-layout">
        <aside className="sidebar-panel"></aside>

        <section className="content-panel">
          {activeTab === "search" ? (
            <div className="content-box">
              <h2>Buscas</h2>
              <p>Conteúdo principal do dashboard.</p>
            </div>
          ) : (
            <div className="content-box">
              <h2>Documentação</h2>
              <p>Conteúdo principal dos relatórios.</p>
            </div>
          )}
        </section>
      </main>
    </div>
  );
}

export default MainView;
