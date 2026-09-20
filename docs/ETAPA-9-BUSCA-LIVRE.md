# Etapa 9 — Busca livre e templates semânticos

## Objetivo

Implementar a capa de busca textual sobre os dados RDF, transformando termos livres em consultas SPARQL seguras e reutilizáveis, alinhando a API ao que o frontend de busca livre já espera.

## Implementação realizada

### 1. Contratos de busca

Arquivo criado:

- `backend/Contracts/Search/SearchRequest.cs`

O modelo define:

- `Text`
- `Graph`
- `Page`
- `PageSize`

Esses campos permitem controlar a busca textual, o grafo alvo e a paginação.

### 2. Serviço de busca

Arquivos criados:

- `backend/Services/Search/ISearchService.cs`
- `backend/Services/Search/SearchService.cs`

O serviço:

- exige `Text` preenchido;
- valida página e tamanho máximo;
- monta uma query SPARQL básica de busca por termos em `?subject`, `?predicate` e `?object`;
- executa a consulta no QLever;
- gera sugestões usando `rdfs:label` e filtro textual.

### 3. Controller de busca

Arquivo criado:

- `backend/Controllers/SearchController.cs`

Endpoints implementados:

- `POST /api/v1/search`
- `GET /api/v1/search/suggestions`

A busca livre retorna resultado em formato SPARQL JSON, mantendo compatibilidade com a camada semântica da aplicação.

### 4. Registro do serviço no DI

Arquivo atualizado:

- `backend/Program.cs`

Foi adicionado:

```csharp
builder.Services.AddScoped<ISearchService, SearchService>();
```

## Benefícios desta etapa

- a API começa a atender o fluxo de busca livre do frontend;
- os termos livres são convertidos em consultas SPARQL seguras;
- a etapa prepara a aplicação para templates, ontologias e refinamentos de busca;
- o processamento segue o princípio de uso do QLever como motor de leitura.

## Observações importantes

- a busca livre é uma implementação inicial e determinística;
- a geração de consultas ainda precisa ser refinada com filtros mais semânticos e indexação real;
- a rota continua sendo somente leitura e segura por construção;
- não há execução de SPARQL arbitrária vindas do cliente sem validação.

## Próximo passo recomendado

Implementar a etapa 10, focada em ontologias, vocabulários e consultas por classes/propriedades do RDF.
