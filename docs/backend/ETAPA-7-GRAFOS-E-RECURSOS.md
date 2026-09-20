# Etapa 7 — Grafos e recursos RDF

## Objetivo

Implementar a base para gestão dos grafos RDF e a consulta de conteúdo e metadados dos recursos armazenados, alinhando o backend às operações de leitura, listagem e exportação da estrutura semântica.

## Implementação realizada

### 1. Contratos inicializados para grafos

Arquivos criados:

- `backend/Contracts/Rdf/GraphMetadataDto.cs`
- `backend/Contracts/Rdf/GraphStatisticsDto.cs`
- `backend/Contracts/Rdf/GraphContentResult.cs`

Esses contratos representam a estrutura inicial do retorno da API para:

- listagem de grafos;
- estatísticas por grafo;
- exportação de conteúdo RDF.

### 2. Serviço de grafos

Arquivos criados:

- `backend/Services/Graphs/IGraphService.cs`
- `backend/Services/Graphs/GraphService.cs`

O serviço expõe operações básicas para:

- consultar a lista de grafos disponíveis;
- localizar um grafo por nome;
- recuperar estatísticas do grafo;
- recuperar conteúdo RDF do grafo usando o QLever.

A implementação atual baseia-se em metadata configurada e em consultas QLever seguras em baixo volume, respeitando o limite inicial da etapa.

### 3. Controller de grafos

Arquivo criado:

- `backend/Controllers/GraphsController.cs`

Endpoints expostos:

- `GET /api/v1/graphs`
- `GET /api/v1/graphs/{graphName}`
- `GET /api/v1/graphs/{graphName}/statistics`
- `GET /api/v1/graphs/{graphName}/content`

Esses endpoints formam a base da API para visualização e consulta de grafos RDF, sem ainda implementar escritas complexas de persistência plena.

### 4. Registro do serviço no container de DI

Arquivo atualizado:

- `backend/Program.cs`

Foi adicionado:

```csharp
builder.Services.AddScoped<IGraphService, GraphService>();
```

Isso permite injeção do serviço diretamente nos Controllers, preservando a arquitetura em camadas.

## Benefícios desta etapa

- disponibiliza a base para visualização de grafos RDF;
- permite consulta do conteúdo RDF por grafo;
- prepara a aplicação para expandir para recursos, triplas e ontologias;
- mantém a arquitetura do backend alinhada ao planejamento de endpoints;
- reduz o acoplamento entre Controller e infraestrutura.

## Observações importantes

- os endpoints agora existem como base funcional mínima e seguem o modelo de arquitetura proposto;
- a busca por conteúdo ainda depende da estratégia de atualização do índice do QLever e do armazenamento principal;
- esta etapa implementa a visão de leitura e metadados, não a escrita em lote nem a manipulação de triplas avançada.

## Próximo passo recomendado

Implementar a etapa 8, que consiste em criar os endpoints de consulta SPARQL com foco em leitura, validação e parametros de consulta no QLever.
