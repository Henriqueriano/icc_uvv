export function LoginView() {
  return (
    <div className="login-shell">
      <div className="login-card">
        <aside className="login-visual">
          <div className="brand-mark">S</div>
          <p className="eyebrow">Sistema interno</p>
          <h1>Bem-vindo</h1>
          <p className="visual-copy">
            Acesse sua conta para continuar monitorando projetos, documentos e relatórios.
          </p>

          <div className="visual-pills">
            <span>Segurança</span>
            <span>Rápido</span>
            <span>Centralizado</span>
          </div>
        </aside>

        <section className="login-form-panel">
          <div className="login-header">
            <p className="eyebrow">Entrar</p>
            <h2>Faça login</h2>
          </div>

          <form className="login-form">
            <label>
              <span>E-mail</span>
              <input type="email" placeholder="seu@email.com" />
            </label>

            <label>
              <span>Senha</span>
              <input type="password" placeholder="••••••••" />
            </label>

            <div className="login-options">
              <label className="remember-me">
                <input type="checkbox" />
                <span>Lembrar-me</span>
              </label>

              <a href="#">Esqueceu a senha?</a>
            </div>

            <button type="submit" className="login-button">
              Entrar
            </button>
          </form>
        </section>
      </div>
    </div>
  );
}

export default LoginView;
