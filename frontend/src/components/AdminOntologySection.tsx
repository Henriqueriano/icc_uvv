import { useEffect, useState } from "react";
import { createOntology, deleteOntology, listAdminOntologies, updateOntology, type OntologyAdminPayload } from "../api";

type Props = { darkMode: boolean };
type AdminOntology = OntologyAdminPayload & { id?: string };
type Author = { authorName: string; portfolioUrl: string };
type BaseDocument = { link: string; description: string };

const empty: OntologyAdminPayload = {
  name: "",
  iri: "",
  description: "",
  documentation: "",
  profileArea: "",
  profileResume: "",
  profileSource: "",
  sourceDocument: "",
  terms: [""],
  authors: [{ authorName: "", portfolioUrl: "" }],
  baseDocuments: [{ link: "", description: "" }],
};

export function AdminOntologySection({ darkMode }: Props) {
  const [items, setItems] = useState<AdminOntology[]>([]);
  const [form, setForm] = useState<OntologyAdminPayload>(empty);
  const [editingId, setEditingId] = useState<string>();
  const [message, setMessage] = useState("");
  const panel = darkMode ? "rounded-2xl bg-[#171614] p-5" : "rounded-2xl bg-[#FDFBE8] p-5";
  const input = darkMode
    ? "w-full rounded-xl border border-[#4C433D] bg-[#2A2724] p-3 text-[#F5F1E6]"
    : "w-full rounded-xl border border-[#E2D39D] bg-white p-3 text-[#2A1F16]";
  const muted = darkMode ? "text-[#E8DCC2]" : "text-[#524332]";

  const load = async () => setItems(await listAdminOntologies());
  useEffect(() => {
    load().catch((error) => setMessage(error instanceof Error ? error.message : "Falha ao carregar ontologias."));
  }, []);

  const reset = () => {
    setForm(empty);
    setEditingId(undefined);
  };

  const edit = (item: AdminOntology) => {
    setEditingId(item.id);
    setForm({
      name: item.name,
      iri: item.iri,
      description: item.description,
      documentation: item.documentation,
      profileArea: item.profileArea,
      profileResume: item.profileResume,
      profileSource: item.profileSource,
      sourceDocument: item.sourceDocument,
      terms: item.terms.length > 0 ? item.terms : [""],
      authors: item.authors.map((author) => ({ authorName: author.authorName, portfolioUrl: author.portfolioUrl })),
      baseDocuments: item.baseDocuments.length > 0 ? item.baseDocuments : [{ link: "", description: "" }],
    });
  };

  const submit = async () => {
    const hasEmptyScalar = [form.name, form.description, form.documentation, form.profileArea, form.profileResume, form.profileSource].some((value) => !value.trim());
    const hasEmptyTerm = form.terms.length === 0 || form.terms.some((term) => !term.trim());
    const authors = form.authors as Author[];
    const documents = form.baseDocuments as BaseDocument[];
    const hasEmptyAuthor = authors.length === 0 || authors.some((author) => !author.authorName.trim() || !author.portfolioUrl.trim());
    const hasEmptyDocument = documents.length === 0 || documents.some((document) => !document.link.trim() || !document.description.trim());
    if (hasEmptyScalar || hasEmptyTerm || hasEmptyAuthor || hasEmptyDocument) {
      setMessage("Preencha todos os campos antes de salvar.");
      return;
    }

    const payload: OntologyAdminPayload = {
      ...form,
      terms: form.terms.map((term) => term.trim()),
      authors: authors.map((author) => ({ authorName: author.authorName.trim(), portfolioUrl: author.portfolioUrl.trim() })),
      baseDocuments: documents.map((document) => ({ link: document.link.trim(), description: document.description.trim() })),
    };
    try {
      if (editingId) {
        const updated = await updateOntology(editingId, payload) as AdminOntology;
        setForm({
          name: updated.name,
          iri: updated.iri,
          description: updated.description,
          documentation: updated.documentation,
          profileArea: updated.profileArea,
          profileResume: updated.profileResume,
          profileSource: updated.profileSource,
          sourceDocument: updated.sourceDocument,
          terms: updated.terms.length > 0 ? updated.terms : [""],
          authors: updated.authors.map((author) => ({ authorName: author.authorName, portfolioUrl: author.portfolioUrl })),
          baseDocuments: updated.baseDocuments.length > 0 ? updated.baseDocuments : [{ link: "", description: "" }],
        });
        setMessage("Documentação atualizada.");
      } else {
        await createOntology(payload);
        setMessage("Documentação cadastrada.");
        reset();
      }
      await load();
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "Falha ao salvar documentação.");
    }
  };

  const remove = async (id?: string) => {
    if (!id) return;
    try {
      await deleteOntology(id);
      await load();
      setMessage("Documentação removida.");
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "Falha ao remover documentação.");
    }
  };

  const updateTerm = (index: number, value: string) => setForm((current) => ({ ...current, terms: current.terms.map((term, position) => position === index ? value : term) }));
  const updateAuthor = (index: number, field: keyof Author, value: string) => setForm((current) => ({ ...current, authors: current.authors.map((author, position) => position === index ? { ...author, [field]: value } : author) }));
  const updateDocument = (index: number, field: keyof BaseDocument, value: string) => setForm((current) => ({ ...current, baseDocuments: current.baseDocuments.map((document, position) => position === index ? { ...document, [field]: value } : document) }));

  return (
    <section className="grid gap-5 lg:grid-cols-[minmax(0,1fr)_minmax(0,1fr)]">
      <div className={panel}>
        <div className="mb-4 flex items-center justify-between gap-3">
          <h2 className="text-xl font-semibold">{editingId ? "Editar documentação" : "Cadastro de documentação"}</h2>
          {editingId && (
            <button type="button" onClick={reset} className="rounded-xl border border-[#A57A4B] px-3 py-2 text-sm font-semibold text-[#A57A4B]">
              Nova documentação
            </button>
          )}
        </div>
        <div className="space-y-3">
          <fieldset className={`${darkMode ? "border-[#4C433D]" : "border-[#E2D39D]"} space-y-3 rounded-xl border p-4`}>
            <legend className={`px-2 font-semibold ${muted}`}>Identificação</legend>
            {([
              ["name", "Título da aba"],
              ["description", "Descrição"],
            ] as const).map(([key, placeholder]) => (
              <input key={key} required className={input} placeholder={placeholder} value={form[key]} onChange={(event) => setForm({ ...form, [key]: event.target.value })} />
            ))}
          </fieldset>

          <fieldset className={`${darkMode ? "border-[#4C433D]" : "border-[#E2D39D]"} space-y-3 rounded-xl border p-4`}>
            <legend className={`px-2 font-semibold ${muted}`}>Dados do criador</legend>
            <input required className={input} placeholder="Área" value={form.profileArea} onChange={(event) => setForm({ ...form, profileArea: event.target.value })} />
            <textarea required className={`${input} min-h-24`} placeholder="Currículo" value={form.profileResume} onChange={(event) => setForm({ ...form, profileResume: event.target.value })} />
            <input required className={input} placeholder="Fonte" value={form.profileSource} onChange={(event) => setForm({ ...form, profileSource: event.target.value })} />
            {form.authors.map((author, index) => (
              <div key={`author-${index}`} className="grid gap-2 md:grid-cols-2">
                <input required className={input} placeholder="Nome do autor" value={author.authorName} onChange={(event) => updateAuthor(index, "authorName", event.target.value)} />
                <div className="flex gap-2">
                  <input required type="url" className={input} placeholder="Link do portfólio" value={author.portfolioUrl} onChange={(event) => updateAuthor(index, "portfolioUrl", event.target.value)} />
                  <button type="button" aria-label="Remover autor" onClick={() => setForm({ ...form, authors: form.authors.filter((_, position) => position !== index) })} className="rounded-xl bg-[#B33A3A] px-3 text-white">-</button>
                </div>
              </div>
            ))}
            <button type="button" onClick={() => setForm({ ...form, authors: [...form.authors, { authorName: "", portfolioUrl: "" }] })} className="rounded-xl bg-[#E7D89B] px-3 py-2 text-sm text-[#2A1F16]">+ Autor</button>
          </fieldset>

          <fieldset className={`${darkMode ? "border-[#4C433D]" : "border-[#E2D39D]"} space-y-3 rounded-xl border p-4`}>
            <legend className={`px-2 font-semibold ${muted}`}>Visão geral</legend>
            <textarea required className={`${input} min-h-28`} placeholder="Visão geral" value={form.documentation} onChange={(event) => setForm({ ...form, documentation: event.target.value })} />
          </fieldset>

          <fieldset className={`${darkMode ? "border-[#4C433D]" : "border-[#E2D39D]"} space-y-2 rounded-xl border p-4`}>
            <legend className={`px-2 font-semibold ${muted}`}>Termos</legend>
            {form.terms.map((term, index) => (
              <div key={`term-${index}`} className="flex gap-2">
                <input required className={input} placeholder={`Termo ${index + 1}`} value={term} onChange={(event) => updateTerm(index, event.target.value)} />
                <button type="button" aria-label="Remover termo" onClick={() => setForm({ ...form, terms: form.terms.filter((_, position) => position !== index) })} className="rounded-xl bg-[#B33A3A] px-3 text-white">-</button>
              </div>
            ))}
            <button type="button" onClick={() => setForm({ ...form, terms: [...form.terms, ""] })} className="rounded-xl bg-[#E7D89B] px-3 py-2 text-sm text-[#2A1F16]">+ Termo</button>
          </fieldset>

          <fieldset className={`${darkMode ? "border-[#4C433D]" : "border-[#E2D39D]"} space-y-2 rounded-xl border p-4`}>
            <legend className={`px-2 font-semibold ${muted}`}>Documentos-base</legend>
            {form.baseDocuments.map((document, index) => (
              <div key={`document-${index}`} className="grid gap-2 md:grid-cols-2">
                <input required type="url" className={input} placeholder="Link" value={document.link} onChange={(event) => updateDocument(index, "link", event.target.value)} />
                <div className="flex gap-2">
                  <input required className={input} placeholder="Descrição" value={document.description} onChange={(event) => updateDocument(index, "description", event.target.value)} />
                  <button type="button" aria-label="Remover documento" onClick={() => setForm({ ...form, baseDocuments: form.baseDocuments.filter((_, position) => position !== index) })} className="rounded-xl bg-[#B33A3A] px-3 text-white">-</button>
                </div>
              </div>
            ))}
            <button type="button" onClick={() => setForm({ ...form, baseDocuments: [...form.baseDocuments, { link: "", description: "" }] })} className="rounded-xl bg-[#E7D89B] px-3 py-2 text-sm text-[#2A1F16]">+ Documento-base</button>
          </fieldset>

          <button type="button" onClick={submit} className="rounded-xl bg-[#E7D89B] px-4 py-3 font-bold text-[#2A1F16]">{editingId ? "Atualizar" : "Cadastrar"}</button>
          {message && <p role="status" className="text-sm">{message}</p>}
        </div>
      </div>
      <div className={panel}>
        <h2 className="mb-4 text-xl font-semibold">Documentações cadastradas</h2>
        <div className="space-y-3">{items.map((item) => <div key={item.id ?? item.iri} className={darkMode ? "rounded-xl bg-[#2A2724] p-4" : "rounded-xl bg-[#F4EBC9] p-4"}><strong>{item.name}</strong><p className="mt-1 text-sm">{item.description}</p><div className="mt-3 flex gap-2"><button type="button" onClick={() => edit(item)} className="rounded-lg bg-[#E7D89B] px-3 py-2 text-sm text-[#2A1F16]">Editar</button><button type="button" onClick={() => remove(item.id)} className="rounded-lg bg-[#B33A3A] px-3 py-2 text-sm text-white">Excluir</button></div></div>)}</div>
      </div>
    </section>
  );
}
