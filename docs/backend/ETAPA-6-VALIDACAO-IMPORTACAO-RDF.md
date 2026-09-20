# Etapa 6 — Validação e importação de RDF

## Objetivo

Implementar a validação inicial de arquivos RDF e disponibilizar os endpoints mínimos para aceitar e validar conteúdo RDF antes da persistência final no armazenamento principal.

## Implementação realizada

### 1. Criação dos contratos de entrada/saída

Arquivos criados:

- `backend/Contracts/Rdf/RdfImportRequest.cs`
- `backend/Contracts/Rdf/RdfValidationResult.cs`

Esses contratos representam a base da API para:

- entrada de metadata de importação;
- saída de resultado da validação;
- retorno de indicadores como `IsValid`, `TripleCount`, `Format` e `Message`.

### 2. Serviço de validação RDF

Arquivos criados:

- `backend/Services/RdfValidation/IRdfValidationService.cs`
- `backend/Services/RdfValidation/RdfValidationService.cs`

A implementação faz a validação de conteúdo RDF com dotNetRDF, usando uma fábrica de parsers por formato. Ela:

- aceita conteúdo em texto ou arquivo `IFormFile`;
- normaliza o formato (`text/turtle`, `application/rdf+xml`, `application/n-triples`, `application/n-quads` e `application/ld+json`);
- detecta o formato a partir do `Content-Type` ou extensão do arquivo;
- tenta interpretar o conteúdo com o parser correto;
- retorna `RdfValidationResult` quando a sintaxe do RDF for válida;
- lança `RdfException` quando a estrutura for inválida.

### 3. Registro do serviço

Arquivo atualizado:

- `backend/Program.cs`

Foi adicionado o registro:

```csharp
builder.Services.AddScoped<IRdfValidationService, RdfValidationService>();
```

Isso permite que os Controllers usem o validador sem acoplamento direto ao parser RDF.

### 4. Controller de importação e validação

Arquivo criado:

- `backend/Controllers/RdfDocumentsController.cs`

Endpoints implementados:

- `POST /api/v1/rdf/import/validate`
- `POST /api/v1/rdf/import`

A lógica atual é controlada e segura: a importação só valida o arquivo e retorna uma resposta resumida. Na prática, a persistência no armazenamento real ainda precisa ser integrada à etapa seguinte, mas a estrutura do endpoint foi montada conforme o planejamento da API.

### 5. Dependência dotNetRDF

Arquivo atualizado:

- `backend/backend.csproj`

Foi adicionada a dependência:

```xml
<PackageReference Include="dotNetRDF" Version="3.1.0" />
```

Essa biblioteca foi adotada para o parsing e validação de RDF, alinhando a implementação ao objetivo do projeto.

## Benefícios desta etapa

- a API agora é capaz de validar conteúdo RDF antes de persistir;
- o processo está organizado em camada de serviço e contrato;
- a aplicação mantém a estrutura de Controllers enxuta;
- a validação responde cedo diante de arquivos inválidos;
- é possível evoluir para importação real no Fuseki ou outro armazenamento RDF.

## Observações importantes

- ainda não há persistência física em banco RDF nesta etapa; o processamento de importação é validado e retornado como resposta da API;
- os endpoints seguem a convenção `/api/v1/rdf` e respeitam o desenho arquitetural do projeto;
- a próxima etapa deve avançar para a criação de endpoints de grafos, busca e SPARQL com QLever.

## Próximo passo recomendado

Implementar a etapa 7, que consiste em criar a camada de grafos e ampliar a API para listagem, consulta e exportação dos grafos RDF.
