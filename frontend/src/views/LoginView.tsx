import { uiText, type Language } from "../i18n";
import { Settings } from 'lucide-react';

type LoginViewProps = {
  darkMode: boolean;
  language: Language;
};

export function LoginView({ darkMode, language }: LoginViewProps) {
  const translation = uiText[language];
  const shellClass = darkMode
    ? "mx-auto w-[min(1000px,calc(100%-24px))] p-5"
    : "mx-auto w-[min(1000px,calc(100%-24px))] p-5";

  const cardClass = darkMode
    ? "grid min-h-[620px] overflow-hidden rounded-[24px] border border-[#4C433D] bg-[#1F1D1A] shadow-[0_20px_60px_rgba(0,0,0,0.35)] md:grid-cols-[minmax(260px,35%)_minmax(0,65%)]"
    : "grid min-h-[620px] overflow-hidden rounded-[24px] border border-[#E7DDB3] bg-[#F9F5D8] shadow-[0_20px_60px_rgba(87,66,35,0.12)] md:grid-cols-[minmax(260px,35%)_minmax(0,65%)]";

  const sideClass = darkMode
    ? "flex flex-col justify-center items-center bg-gradient-to-b from-[#3A342F] to-[#2A2724] p-8 md:p-10"
    : "flex flex-col justify-center items-center bg-gradient-to-b from-[#F5EFC8] to-[#E7D89B] p-8 md:p-10";

  const badgeClass = darkMode
    ? "mb-6 grid h-[74px] w-[74px] place-items-center rounded-[18px] border border-[#574D46] bg-[#342F2B] text-3xl font-bold text-[#F5F1E6]"
    : "mb-6 grid h-[74px] w-[74px] place-items-center rounded-[18px] border border-[#D8C98B] bg-[#FFFDF3] text-3xl font-bold text-[#2A1F16]";

  const labelClass = darkMode ? "text-[11px] uppercase tracking-[0.12em] text-[#E8DCC2]" : "text-[11px] uppercase tracking-[0.12em] text-[#5B4A35]";
  const headingClass = darkMode ? "text-4xl font-bold leading-tight text-[#F5F1E6] md:text-5xl" : "text-4xl font-bold leading-tight text-[#2A1F16] md:text-5xl";
  const bodyClass = darkMode ? "mt-5 text-base leading-7 text-[#D9CBB1]" : "mt-5 text-base leading-7 text-[#4A3B2B]";
  const chipClass = darkMode
    ? "rounded-full border border-[#4C433D] bg-[#2A2724] px-3 py-2 text-sm text-[#F5F1E6]"
    : "rounded-full border border-[#E2D39D] bg-[#FFFDF3] px-3 py-2 text-sm text-[#2A1F16]";
  const formPanelClass = darkMode ? "flex flex-col justify-center bg-[#171614] p-8 md:p-12" : "flex flex-col justify-center bg-[#FDFBE8] p-8 md:p-12";
  const fieldLabelClass = darkMode ? "flex flex-col gap-2.5 text-[#E8DCC2]" : "flex flex-col gap-2.5 text-[#4A3B2B]";
  const inputClass = darkMode
    ? "h-12 w-full rounded-xl border border-[#4C433D] bg-[#2A2724] px-4 text-[#F5F1E6] placeholder:text-[#B7A98E]"
    : "h-12 w-full rounded-xl border border-[#E2D39D] bg-[#FFFDF3] px-4 text-[#2A1F16] placeholder:text-[#7A6854]";
  const helperClass = darkMode ? "flex items-center justify-between gap-4 text-sm text-[#E8DCC2]" : "flex items-center justify-between gap-4 text-sm text-[#5B4A35]";
  const submitClass = darkMode
    ? "mt-2 h-12 rounded-xl bg-[#4A3B32] font-bold text-[#F5F1E6]"
    : "mt-2 h-12 rounded-xl bg-[#E7D89B] font-bold text-[#2A1F16]";

  return (
    <div className={shellClass}>
      <div className={cardClass}>
        <aside className={sideClass}>
          <div className={badgeClass}>
            <Settings size={44}/>
          </div>
          <p className={labelClass}>{translation.internalSystem}</p>
        </aside>

        <section className={formPanelClass}>
          <form className="mt-7 flex flex-col gap-5">
            <label className={fieldLabelClass}>
              <span>{translation.email}</span>
              <input type="email" placeholder="seu@email.com" className={inputClass} />
            </label>

            <label className={fieldLabelClass}>
              <span>{translation.password}</span>
              <input type="password" placeholder="••••••••" className={inputClass} />
            </label>

            <div className={helperClass}>
              <label className="inline-flex items-center gap-2">
                <input type="checkbox" className={darkMode ? "h-4 w-4 accent-[#4A3B32]" : "h-4 w-4 accent-[#D1B866]"} />
                <span>{translation.remember}</span>
              </label>

              <a href="#" className={darkMode ? "text-[#E8DCC2] no-underline" : "text-[#6B543A] no-underline"}>
                {translation.forgot}
              </a>
            </div>

            <button type="submit" className={submitClass}>
              {translation.submit}
            </button>
          </form>
        </section>
      </div>
    </div>
  );
}

export default LoginView;
