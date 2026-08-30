import { useState } from "react";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import { ToggleLeft, ToggleRight } from "lucide-react";
import "./index.css";
import { AdminView } from "./views/AdminView";
import { MainView } from "./views/MainView";

export function App() {
  const [darkMode, setDarkMode] = useState(false);

  return (
    <BrowserRouter>
      <div
        className={
          darkMode
            ? "min-h-screen bg-[#171614] text-[#F5F1E6] transition-colors duration-200"
            : "min-h-screen bg-[#E9DFB0] text-[#2A1F16] transition-colors duration-200"
        }
      >
        <div className="flex justify-end p-4">
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
          <Route path="/" element={<MainView darkMode={darkMode} />} />
          <Route path="/admin" element={<AdminView darkMode={darkMode} />} />
        </Routes>
      </div>
    </BrowserRouter>
  );
}

export default App;
