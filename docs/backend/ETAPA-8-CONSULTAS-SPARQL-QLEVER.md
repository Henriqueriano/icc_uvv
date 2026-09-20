# Etapa 8 — Consultas SPARQL com QLever

## Objetivo

Implementar a base dos endpoints de consulta SPARQL usando QLever como mecanismo de leitura, mantendo a API como fachada pública e proibindo operações de escrita nessa rota.

## Implementação realizada

### 1. Contrato da requisição de consulta

Arquivo criado:

- `backend/Contracts/Sparql/SparqlQueryRequest.cs`

O modelo expõe:

- `Query`
- `DefaultGraph`
- `Format`

Esses dados serão utilizados para validar a operação, configurar o grafo padrão e responder no formato acordado.

### 2. Serviço de consultas SPARQL

Arquivos criados:

- `backend/Services/Sparql/ISparqlService.cs`
- `backend/Services/Sparql/SparqlService.cs`

O serviço:

- valida se a query foi informada;
- limita tamanho máximo da string;
- bloqueia operações de escrita (`INSERT`, `DELETE`, `UPDATE`);
- encaminha a query para o `IQleverClient`;
- converte falhas em `SparqlException`.

Essa regra é importante porque os endpoints de leitura devem ser seguros e previsíveis.

### 3. Controller SPARQL

Arquivo criado:

- `backend/Controllers/SparqlController.cs`

Endpoints implementados:

- `POST /api/v1/sparql/query`
- `POST /api/v1/sparql/validate`

A rota de consulta envia ao QLever e retorna o conteúdo como `application/sparql-results+json`.
A rota de validação apenas avalia a query, sem executá-la.

### 4. Registro do serviço no DI

Arquivo atualizado:

- `backend/Program.cs`

Foi adicionado:

```csharp
builder.Services.AddScoped<ISparqlService, SparqlService>();
```

## Benefícios desta etapa

- a API já pode receber consultas SPARQL publicamente;
- a leitura passa a usar o QLever como mecanismo central de busca semântica;
- há proteção inicial contra operações de escrita nas rotas de leitura;
- o desenho arquitetural continua separado em camada de contrato, serviço e Controller.

## Observações importantes

- a implementação atual é leitura-only por design, conforme o planejamento da etapa 5;
- ainda não foram implementados templates, queries parametrizadas ou buscas livres avançadas;
- as respostas do QLever são retornadas diretamente, mas ainda sem um enriquecimento de formato completo para todas as modalidades de consulta.

## Próximo passo recomendado

Implementar a etapa 9, focada em busca livre e templates de consulta, conectando a API RDF à experiência do frontend com busca textual e consultas semânticas reutilizáveis.
