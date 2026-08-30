import { useEffect, useState } from "react";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import { ToggleLeft, ToggleRight } from "lucide-react";
import "./index.css";
import type { Language } from "./i18n";
import { AdminView } from "./views/AdminView";
import { MainView } from "./views/MainView";
import { NotFoundView } from "./views/NotFoundView";

export function App() {
  const [darkMode, setDarkMode] = useState<boolean>(() => {
    if (typeof window === "undefined") return false;
    return localStorage.getItem("theme") === "dark";
  });

  const [language, setLanguage] = useState<Language>(() => {
    if (typeof window === "undefined") return "pt-BR";
    const current = localStorage.getItem("language");
    return current === "en-US" || current === "es-ES" ? current : "pt-BR";
  });

  useEffect(() => {
    localStorage.setItem("theme", darkMode ? "dark" : "light");
  }, [darkMode]);

  useEffect(() => {
    localStorage.setItem("language", language);
  }, [language]);

  return (
    <BrowserRouter>
      <div
        className={
          darkMode
            ? "min-h-screen bg-[#171614] text-[#F5F1E6] transition-colors duration-200"
            : "min-h-screen bg-[#E9DFB0] text-[#2A1F16] transition-colors duration-200"
        }
      >
        <div className="flex justify-end gap-3 p-4">
          <select
            aria-label="Select system language"
            value={language}
            onChange={(event) => setLanguage(event.target.value as Language)}
            className={
              darkMode
                ? "rounded-full border border-[#4C433D] bg-[#2A2724] px-3 py-2 text-sm text-[#F5F1E6] outline-none"
                : "rounded-full border border-[#D7C99A] bg-[#FDFBE8] px-3 py-2 text-sm text-[#2A1F16] outline-none"
            }
          >
            <option value="pt-BR">PT-BR</option>
            <option value="en-US">EN-US</option>
            <option value="es-ES">ES-ES</option>
          </select>

          <button
            type="button"
            onClick={() => setDarkMode((prev) => !prev)}
            className={
              darkMode
                ? "rounded-full border border-[#4C433D] bg-[#2A2724] px-4 py-2 text-sm font-medium text-[#F5F1E6] shadow-sm transition hover:bg-[#342F2B]"
                : "rounded-full border border-[#D7C99A] bg-[#FDFBE8] px-4 py-2 text-sm font-medium text-[#2A1F16] shadow-sm transition hover:bg-[#F3E8B9]"
            }
          >
            {darkMode ? <ToggleRight size={44} /> : <ToggleLeft size={44} />}
          </button>
        </div>

        <Routes>
          <Route path="/" element={<MainView darkMode={darkMode} language={language} />} />
          <Route path="/admin" element={<AdminView darkMode={darkMode} language={language} />} />
          <Route path="*" element={<NotFoundView darkMode={darkMode} language={language} />} />
        </Routes>
      </div>
    </BrowserRouter>
  );
}

export default App;
