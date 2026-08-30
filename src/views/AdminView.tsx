import { LoginView } from "./LoginView";

export function AdminView() {
  return (
    <div className="admin-page-shell">
      <header className="admin-topbar">
        <div className="admin-brand">Painel administrativo</div>
      </header>

      <LoginView />
    </div>
  );
}

export default AdminView;
