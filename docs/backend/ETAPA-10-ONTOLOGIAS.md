# Etapa 10 — Ontologias e vocabulários RDF

## Objetivo

Implementar a base para listagem e consulta de ontologias e vocabulários disponíveis no ecossistema RDF, alinhando a API ao painel de ontologias do frontend e ao planejamento do sistema semântico.

## Implementação realizada

### 1. Contratos de ontologia

Arquivo criado:

- `backend/Contracts/Ontologies/OntologySummaryDto.cs`

O DTO representa:

- `Iri`
- `Name`
- `Description`
- `Namespace`
- `ClassCount`
- `PropertyCount`

### 2. Serviço de ontologias

Arquivos criados:

- `backend/Services/Ontologies/IOntologyService.cs`
- `backend/Services/Ontologies/OntologyService.cs`

O serviço:

- lista ontologias principais conhecidas (FOAF, DCAT e Schema.org);
- recupera a ontologia por IRI;
- consulta classes e propriedades por meio do QLever.

Essa etapa mantém a implementação inicial e limpa, em nível de estrutura funcional, sem exigir integração com ontology registry externo.

### 3. Controller de ontologias

Arquivo criado:

- `backend/Controllers/OntologiesController.cs`

Endpoints implementados:

- `GET /api/v1/ontologies`
- `GET /api/v1/ontologies/{iri}`
- `GET /api/v1/ontologies/{iri}/classes`
- `GET /api/v1/ontologies/{iri}/properties`

### 4. Registro do serviço

Arquivo atualizado:

- `backend/Program.cs`

Foi adicionado:

```csharp
builder.Services.AddScoped<IOntologyService, OntologyService>();
```

## Benefícios desta etapa

- a API já expõe a estrutura básica de ontologias do sistema;
- a área de frontend pode mostrar vocabulários disponíveis;
- o backend amplia sua camada de semântica, explorando classes e propriedades;
- a arquitetura continua separada em contrato, serviço e Controller.

## Observações importantes

- esta implementação é inicial e não substitui um registry completo de ontologias externas;
- o sistema continua contando com QLever como mecanismo principal de consulta semântica;
- a lista inicial funciona como base para expansão futura, incluindo importação real de vocabulários e indexação por namespace.

## Próximo passo recomendado

Implementar a etapa 11, voltada para estatísticas e monitoramento operacional do backend RDF.
