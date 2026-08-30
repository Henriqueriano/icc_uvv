import { LoginView } from "./LoginView";

type AdminViewProps = {
  darkMode: boolean;
};

export function AdminView({ darkMode }: AdminViewProps) {
  return (
    <div className="mx-auto w-[min(1200px,calc(100%-20px))] py-6">
      <header className="mx-auto mb-4 w-[min(1000px,calc(100%-24px))] px-2">
        <div className={darkMode ? "text-[11px] uppercase tracking-[0.12em] text-[#E8DCC2]" : "text-[11px] uppercase tracking-[0.12em] text-[#5B4A35]"}>
          Painel administrativo
        </div>
      </header>

      <LoginView darkMode={darkMode} />
    </div>
  );
}

export default AdminView;
