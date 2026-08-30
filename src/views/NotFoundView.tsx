import { Link } from "react-router-dom";
import { uiText, type Language } from "../i18n";

type NotFoundViewProps = {
  darkMode: boolean;
  language: Language;
};

export function NotFoundView({ darkMode, language }: NotFoundViewProps) {
  const translation = uiText[language];

  return (
    <div className="mx-auto flex min-h-[calc(100vh-120px)] w-full max-w-[980px] items-center justify-center px-4 py-10">
      <div
        className={
          darkMode
            ? "w-full rounded-[28px] border border-[#453F39] bg-[#2A2724] p-8 shadow-[0_18px_45px_rgba(0,0,0,0.28)]"
            : "w-full rounded-[28px] border border-[#E4D7A2] bg-[#F9F5D8] p-8 shadow-[0_18px_45px_rgba(87,66,35,0.12)]"
        }
      >
        <div className={darkMode ? "mb-3 text-[11px] uppercase tracking-[0.12em] text-[#E8DCC2]" : "mb-3 text-[11px] uppercase tracking-[0.12em] text-[#5B4A35]"}>
          404
        </div>

        <h1 className={darkMode ? "mb-3 text-4xl font-bold text-[#F5F1E6]" : "mb-3 text-4xl font-bold text-[#2A1F16]"}>
          {translation.notFound} ¯\_(ツ)_/¯
        </h1>

        <p className={darkMode ? "mb-6 max-w-xl text-base leading-7 text-[#E8DCC2]" : "mb-6 max-w-xl text-base leading-7 text-[#524332]"}>
          {translation.notFoundMessage}
        </p>

        <Link
          to="/"
          className={
            darkMode
              ? "inline-flex items-center rounded-full border border-[#4C433D] bg-[#171614] px-5 py-3 text-sm font-medium text-[#F5F1E6] transition hover:bg-[#221F1D]"
              : "inline-flex items-center rounded-full border border-[#D7C99A] bg-[#FDFBE8] px-5 py-3 text-sm font-medium text-[#2A1F16] transition hover:bg-[#F3E8B9]"
          }
        >
          {translation.goHome}
        </Link>
      </div>
    </div>
  );
}

export default NotFoundView;
