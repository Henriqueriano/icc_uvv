using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Scripts;

public static class OntologySampleSeeder
{
    private sealed record Sample(
        string Name,
        string Iri,
        string Description,
        string Documentation,
        string Terms,
        string ProfileArea,
        string ProfileResume,
        string ProfileSource,
        (string Name, string Portfolio)[] Authors,
        (string Link, string Description)[] Documents);

    private static readonly Sample[] Samples =
    [
        new(
            "Ontologia de Registro Arquivístico (ARO)",
            "https://example.org/ontology/aro",
            "Ontologia para representação semântica de documentos arquivísticos.",
            "A Ontologia de Registro Arquivístico (Archival Record Ontology - ARO) representa semanticamente documentos arquivísticos físicos, digitais e nato-digitais. A pesquisa busca melhorar a interoperabilidade semântica entre sistemas de informação da administração pública, apoiar a preservação de longo prazo, a acessibilidade e a autenticidade dos registros. A construção integra OntoUML, SABiO, UFO e gUFO; a verificação foi realizada no Protégé e validada por consultas SPARQL baseadas em questões de integração.",
            "Archival Record\nRecord\nDocumento arquivístico\nArquivologia\nOntoUML\nSABiO\nUFO\ngUFO\nProtégé\nSPARQL",
            "Arquivologia e modelagem conceitual",
            "Dissertação de mestrado apresentada à UFES em 2025, orientada por Vítor E. Silva Souza e coorientada por João Paulo A. Almeida.",
            "Universidade Federal do Espírito Santo",
            [("Jussara Teixeira", "https://www.google.com/search?q=Jussara+Teixeira")],
            [("https://scholar.google.com/scholar?q=%22Ontological+Representation+of+Archival+Records%22", "Dissertação que apresenta a Ontologia de Registro Arquivístico (ARO).")]),
        new(
            "Jurisprudência Brasileira",
            "https://example.org/ontology/brazilian-jurisprudence",
            "Ontologia para exposição semântica de jurisprudências federais brasileiras.",
            "O trabalho propõe uma camada ontológica para repositórios de jurisprudências federais brasileiras. A abordagem remodela uma ontologia existente e incorpora características do contexto federal para permitir interoperabilidade com vocabulários jurídicos e consultas semânticas sobre decisões judiciais. O estudo concentra-se no direito criminal federal e descreve o percurso desde a denúncia e a instrução até sentenças, recursos, votos e acórdãos.",
            "Jurisprudência\nProcesso criminal\nSentença\nAcórdão\nVoto\nRelator\nTRF\nSTF\nSTJ\nLexML",
            "Modelagem conceitual e ontologias jurídicas",
            "Artigo sobre a exposição semântica de jurisprudências brasileiras.",
            "UFES e TRF da 2ª Região",
            [("Jean-Rémi Bourguet", "https://www.google.com/search?q=Jean-Remi+Bourguet"), ("Melissa Zorzanelli Costa", "https://www.google.com/search?q=Melissa+Zorzanelli+Costa")],
            [("https://scholar.google.com/scholar?q=%22About+The+Exposition+of+Brazilian+Jurisprudences%22", "Artigo que propõe uma camada ontológica para jurisprudências brasileiras.")])
    ];

    public static async Task SeedAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        foreach (var sample in Samples)
        {
            var ontology = await dbContext.Ontologies
                .SingleOrDefaultAsync(item => item.Iri == sample.Iri, cancellationToken);

            if (ontology is null)
            {
                ontology = new Ontology { Id = Guid.NewGuid(), Iri = sample.Iri, Name = sample.Name };
                dbContext.Ontologies.Add(ontology);
            }

            if (string.IsNullOrWhiteSpace(ontology.Description)) ontology.Description = sample.Description;
            if (string.IsNullOrWhiteSpace(ontology.Documentation)) ontology.Documentation = sample.Documentation;
            if (string.IsNullOrWhiteSpace(ontology.Terms)) ontology.Terms = sample.Terms;
            if (string.IsNullOrWhiteSpace(ontology.SourceDocument)) ontology.SourceDocument = sample.Name.Contains("ARO", StringComparison.Ordinal) ? "jussara.pdf" : "melissa.pdf";
            if (string.IsNullOrWhiteSpace(ontology.ProfileArea)) ontology.ProfileArea = sample.ProfileArea;
            if (string.IsNullOrWhiteSpace(ontology.ProfileResume)) ontology.ProfileResume = sample.ProfileResume;
            if (string.IsNullOrWhiteSpace(ontology.ProfileSource)) ontology.ProfileSource = sample.ProfileSource;

            await dbContext.SaveChangesAsync(cancellationToken);
            if (!await dbContext.OntologyAuthorPortfolios.AnyAsync(item => item.OntologyId == ontology.Id, cancellationToken))
            {
                dbContext.OntologyAuthorPortfolios.AddRange(sample.Authors.Select(author => new OntologyAuthorPortfolio
                {
                    Id = Guid.NewGuid(),
                    OntologyId = ontology.Id,
                    AuthorName = author.Name,
                    PortfolioUrl = author.Portfolio
                }));
            }
            if (!await dbContext.OntologyBaseDocuments.AnyAsync(item => item.OntologyId == ontology.Id, cancellationToken))
            {
                dbContext.OntologyBaseDocuments.AddRange(sample.Documents.Select(document => new OntologyBaseDocument
                {
                    Id = Guid.NewGuid(),
                    OntologyId = ontology.Id,
                    Link = document.Link,
                    Description = document.Description
                }));
            }
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
