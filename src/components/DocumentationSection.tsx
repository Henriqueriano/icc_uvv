type DocumentationSectionProps = {
  darkMode: boolean;
};

export function DocumentationSection({ darkMode }: DocumentationSectionProps) {
  return (
    <main className="block">
      <section className={darkMode ? "w-full rounded-[18px] border border-[#453F39] bg-[#2A2724] p-6" : "w-full rounded-[18px] border border-[#E7DDB3] bg-[#F4EBC9] p-6"}>
        <div className={darkMode ? "flex min-h-[220px] flex-col justify-center rounded-2xl bg-[#171614] p-6" : "flex min-h-[220px] flex-col justify-center rounded-2xl bg-[#FDFBE8] p-6"}>
          <h2 className={darkMode ? "mb-3 text-3xl font-bold text-[#F5F1E6]" : "mb-3 text-3xl font-bold text-[#2A1F16]"}>Documentation</h2>
          <p className={darkMode ? "text-[#E8DCC2]" : "text-[#524332]"}>Main content of the project documentation.</p>
        </div>
      </section>
    </main>
  );
}

export default DocumentationSection;
