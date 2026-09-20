# Planejamento de endpoints da API RDF

## Contexto atual

O backend utiliza ASP.NET Core 9 com Controllers (`AddControllers` e `MapControllers`). O `WeatherForecastController` ainda é um placeholder. A infraestrutura já define um dataset QLever chamado `icc_uvv`, exposto na porta `3030`. As consultas SPARQL dos endpoints deverão ser executadas pelo QLever.

A API deverá funcionar como camada de validação, autorização e abstração sobre o armazenamento RDF e o QLever, utilizando dotNetRDF para parsing, validação, serialização e manipulação de grafos RDF. O QLever pode permanecer como camada de persistência enquanto o QLever atua como mecanismo de consulta, desde que essa separação seja mantida na configuração.

## Convenções

- Prefixo dos endpoints de negócio: `/api/v1`.
- Controllers planejados:
  - `RdfDocumentsController`
  - `GraphsController`
  - `ResourcesController`
  - `SparqlController`
  - `OntologiesController`
  - `StatisticsController`
  - `HealthController`
- IRIs devem ser recebidas preferencialmente como query parameter (`iri`) ou codificadas adequadamente quando fizerem parte da rota.
- Operações de escrita devem exigir autenticação e autorização administrativa.
- Consultas devem ter limites de tempo, paginação e quantidade máxima de resultados.

## 1. Importação e persistência de RDF

| Método | Endpoint | Finalidade |
|---|---|---|
| `POST` | `/api/v1/rdf/import` | Receber, validar e persistir um arquivo RDF |
| `POST` | `/api/v1/rdf/import/validate` | Validar RDF sem persistir |
| `GET` | `/api/v1/rdf/documents` | Listar documentos ou conjuntos importados |
| `GET` | `/api/v1/rdf/documents/{id}` | Consultar metadados de uma importação |
| `GET` | `/api/v1/rdf/documents/{id}/content` | Recuperar o conteúdo RDF |
| `DELETE` | `/api/v1/rdf/documents/{id}` | Remover a importação e seus dados |

`/rdf/import` deverá aceitar `multipart/form-data` e formatos como `text/turtle`, `application/ld+json`, `application/rdf+xml`, `application/n-triples` e `application/n-quads`.

Fluxo esperado:

1. Receber o arquivo e identificar o formato.
2. Fazer o parse com dotNetRDF.
3. Validar o grafo ou dataset.
4. Associar o conteúdo a um grafo nomeado.
5. Persistir no armazenamento RDF configurado.
6. Retornar o identificador da importação, o grafo e a quantidade de triplas.

## 2. Gerenciamento de grafos

| Método | Endpoint | Finalidade |
|---|---|---|
| `GET` | `/api/v1/graphs` | Listar grafos disponíveis |
| `POST` | `/api/v1/graphs` | Criar ou registrar um grafo nomeado |
| `GET` | `/api/v1/graphs/{graphName}` | Consultar informações do grafo |
| `DELETE` | `/api/v1/graphs/{graphName}` | Remover um grafo completo |
| `GET` | `/api/v1/graphs/{graphName}/statistics` | Consultar estatísticas do grafo |
| `GET` | `/api/v1/graphs/{graphName}/content` | Exportar o grafo em formato RDF |
| `POST` | `/api/v1/graphs/{graphName}/content` | Inserir RDF no grafo |
| `DELETE` | `/api/v1/graphs/{graphName}/content` | Limpar o conteúdo do grafo |

## 3. Recursos RDF

| Método | Endpoint | Finalidade |
|---|---|---|
| `GET` | `/api/v1/resources?iri={iri}` | Consultar as propriedades de um recurso |
| `GET` | `/api/v1/resources/{iri}/relations` | Consultar recursos relacionados |
| `GET` | `/api/v1/resources/{iri}/types` | Listar as classes RDF do recurso |
| `POST` | `/api/v1/resources` | Criar um recurso e suas propriedades |
| `PATCH` | `/api/v1/resources/{iri}` | Atualizar propriedades específicas |
| `DELETE` | `/api/v1/resources/{iri}` | Remover um recurso e suas relações |

Para evitar ambiguidades e problemas de encoding, a implementação deve preferir:

```text
/api/v1/resources?iri=https%3A%2F%2Fexample.org%2Fresource%2F123
```

## 4. Operações de triplas

| Método | Endpoint | Finalidade |
|---|---|---|
| `POST` | `/api/v1/triples` | Inserir uma ou mais triplas |
| `GET` | `/api/v1/triples` | Consultar por sujeito, predicado, objeto ou grafo |
| `DELETE` | `/api/v1/triples` | Remover triplas conforme filtros |
| `POST` | `/api/v1/triples/batch` | Executar inserções em lote |
| `DELETE` | `/api/v1/triples/batch` | Executar remoções em lote |

Filtros previstos: `subject`, `predicate`, `object`, `graph`, `limit` e `offset`. Remoções devem exigir critérios restritivos para impedir a limpeza acidental do dataset inteiro.

## 5. Consultas SPARQL com QLever

Os endpoints de consulta SPARQL devem encaminhar as consultas ao QLever, por
meio do endpoint HTTP configurado para o índice RDF da aplicação. A API deve
permanecer como fachada pública: o frontend não deve acessar o QLever
diretamente.

Configuração prevista:

- URL base do QLever configurada por ambiente, sem hardcode no Controller.
- Identificador do índice ou dataset QLever configurado por ambiente.
- Timeout, limite de resultados e paginação controlados pela API.
- Validação da consulta antes do encaminhamento.
- Autenticação e autorização aplicadas na API antes da chamada ao QLever.
- Registro de tempo de resposta, status e quantidade de resultados.

| Método | Endpoint | Finalidade |
|---|---|---|
| `POST` | `/api/v1/sparql/query` | Executar no QLever consultas `SELECT`, `ASK`, `CONSTRUCT` ou `DESCRIBE` |
| `GET` | `/api/v1/sparql/query` | Executar no QLever consultas simples parametrizadas |
| `POST` | `/api/v1/sparql/update` | Encaminhar atualizações somente se o armazenamento configurado suportar escrita |
| `POST` | `/api/v1/sparql/validate` | Validar uma consulta antes do encaminhamento ao QLever |
| `GET` | `/api/v1/sparql/templates` | Listar consultas predefinidas |
| `GET` | `/api/v1/sparql/templates/{name}` | Consultar um template |
| `POST` | `/api/v1/sparql/templates/{name}/execute` | Executar um template parametrizado |

Payload inicial sugerido:

```json
{
  "query": "SELECT ?subject ?predicate ?object WHERE { ?subject ?predicate ?object }",
  "defaultGraph": "https://icc-uvv.org/graph/main",
  "format": "application/sparql-results+json"
}
```

As consultas de leitura devem ser executadas no QLever. Respostas `SELECT` e `ASK` devem usar `application/sparql-results+json`. Respostas `CONSTRUCT` e `DESCRIBE` podem usar Turtle, RDF/XML, JSON-LD ou N-Triples, conforme o formato suportado pelo QLever e negociado pela API.

Consultas SPARQL arbitrárias devem ser controladas por autenticação, autorização, timeout, limite de resultados, paginação e restrição de operações de escrita. Falhas de comunicação, timeout ou respostas inválidas do QLever devem ser convertidas em erros explícitos da API, sem respostas vazias com aparência de sucesso.

## 6. Busca livre

| Método | Endpoint | Finalidade |
|---|---|---|
| `POST` | `/api/v1/search` | Buscar termos, IRIs, labels e propriedades |
| `GET` | `/api/v1/search/suggestions` | Retornar sugestões de recursos e vocabulários |
| `GET` | `/api/v1/search/{iri}` | Recuperar o contexto semântico de um resultado |

Payload sugerido:

```json
{
  "text": "pessoas relacionadas à universidade",
  "graph": "https://icc-uvv.org/graph/main",
  "page": 1,
  "pageSize": 20
}
```

Inicialmente, a busca pode ser convertida para SPARQL por regras determinísticas. Uma eventual interpretação por IA deve permanecer desacoplada, validada e limitada.

## 7. Ontologias e vocabulários

| Método | Endpoint | Finalidade |
|---|---|---|
| `GET` | `/api/v1/ontologies` | Listar ontologias |
| `POST` | `/api/v1/ontologies` | Importar ou registrar uma ontologia |
| `GET` | `/api/v1/ontologies/{iri}` | Consultar informações da ontologia |
| `GET` | `/api/v1/ontologies/{iri}/classes` | Listar classes |
| `GET` | `/api/v1/ontologies/{iri}/properties` | Listar propriedades |
| `GET` | `/api/v1/ontologies/{iri}/classes/{classIri}/instances` | Listar instâncias |
| `DELETE` | `/api/v1/ontologies/{iri}` | Remover uma ontologia administrada |

As respostas devem incluir, quando disponíveis, IRI, label, descrição, namespace, versão, classes, propriedades e quantidade de recursos associados.

## 8. Estatísticas e monitoramento

| Método | Endpoint | Finalidade |
|---|---|---|
| `GET` | `/api/v1/statistics/overview` | Resumo geral da aplicação |
| `GET` | `/api/v1/statistics/graphs` | Estatísticas por grafo |
| `GET` | `/api/v1/statistics/queries` | Volume, duração e sucesso das consultas |
| `GET` | `/api/v1/statistics/imports` | Histórico e desempenho das importações |
| `GET` | `/api/v1/statistics/ontologies` | Quantidade e distribuição de ontologias |

O resumo deve suportar os indicadores exibidos pela área administrativa:

```json
{
  "rdfDocuments": 1284,
  "ontologies": 42,
  "sparqlQueries": 8900,
  "freeSearches": 24300,
  "averageResponseTimeMs": 182,
  "successRate": 99.4
}
```

## 9. Saúde dos serviços

| Método | Endpoint | Finalidade |
|---|---|---|
| `GET` | `/health` | Verificar se a API responde |
| `GET` | `/health/ready` | Verificar API, QLever, QLever e dependências |
| `GET` | `/health/live` | Verificar se o processo está ativo |
| `GET` | `/api/v1/system/status` | Retornar status detalhado ao painel administrativo |

O readiness deve testar a conectividade com o QLever, a disponibilidade do
índice configurado e a execução de uma consulta SPARQL simples no QLever. Caso
o QLever permaneça como armazenamento, sua conectividade também deverá ser
verificada. Se o PostgreSQL for utilizado pela aplicação, sua conectividade
deverá igualmente ser testada.

## Respostas e erros

Status HTTP esperados:

- `200 OK`: consulta concluída.
- `201 Created`: recurso, grafo ou importação criado.
- `202 Accepted`: importação processada de forma assíncrona.
- `204 No Content`: remoção concluída sem corpo.
- `400 Bad Request`: RDF ou SPARQL inválido.
- `404 Not Found`: recurso, grafo ou importação inexistente.
- `409 Conflict`: duplicidade ou conflito de versão.
- `413 Payload Too Large`: arquivo acima do limite.
- `422 Unprocessable Entity`: RDF sintaticamente válido, mas semanticamente inválido.
- `429 Too Many Requests`: excesso de consultas.
- `503 Service Unavailable`: QLever ou o armazenamento RDF indisponível.

Os erros devem usar um formato único baseado em `ProblemDetails`:

```json
{
  "type": "https://icc-uvv.org/errors/invalid-rdf",
  "title": "RDF inválido",
  "status": 400,
  "detail": "Não foi possível interpretar o conteúdo como Turtle.",
  "traceId": "..."
}
```

## Ordem sugerida de implementação

### Primeira fase

1. `POST /api/v1/rdf/import`
2. `POST /api/v1/rdf/import/validate`
3. `GET /api/v1/graphs`
4. `GET /api/v1/graphs/{graphName}/content`
5. `POST /api/v1/sparql/query` utilizando QLever
6. `POST /api/v1/search`
7. `GET /health`
8. `GET /health/ready`

### Segunda fase

1. CRUD de recursos RDF.
2. Operações em lote de triplas.
3. Gerenciamento completo de grafos.
4. Exportação em diferentes formatos RDF.
5. Templates SPARQL.
6. Estatísticas reais para a área administrativa.

### Terceira fase

1. Endpoints de ontologias.
2. Autenticação e autorização administrativa.
3. Consultas assíncronas.
4. Histórico de importações e consultas.
5. Cache de consultas de leitura.
6. Paginação e indexação otimizada para buscas.

## Guia de implementação para uma IA desenvolvedora

Esta seção define a ordem de execução esperada. A IA deve implementar uma
etapa por vez, validar o resultado antes de avançar e não alterar o frontend
ou endpoints fora do escopo da etapa atual sem necessidade explícita.

### Etapa 0 — Inspecionar o repositório e estabelecer limites

1. Ler `backend/backend.csproj`, `backend/Program.cs`,
   `backend/appsettings.json`, `backend/appsettings.Development.json` e
   `backend/docker-compose.yml`.
2. Confirmar que a API usa ASP.NET Core 9 com Controllers.
3. Confirmar que o QLever disponibiliza o dataset `icc_uvv`.
4. Verificar se já existem serviços, repositórios, DTOs, testes ou referências
   a dotNetRDF e QLever antes de criar novos arquivos.
5. Não reutilizar `WeatherForecastController` para funcionalidades RDF.
6. Preservar alterações não relacionadas já existentes no worktree.
7. Antes de codificar, registrar as decisões de infraestrutura:
   - QLever será o armazenamento de escrita?
   - QLever será somente leitura?
   - Qual URL HTTP do QLever será utilizada?
   - O índice QLever será atualizado por importação, processo externo ou
     pipeline separado?

Se a resposta sobre escrita no QLever não estiver definida, implementar
`/sparql/update` como não suportado e retornar `501 Not Implemented`, em vez de
simular uma atualização bem-sucedida.

### Etapa 1 — Definir configuração e opções tipadas

Criar uma seção de configuração, por exemplo `Rdf`, contendo:

```json
{
  "Rdf": {
    "QLeverBaseUrl": "http://localhost:3030",
    "QLeverDataset": "icc_uvv",
    "QleverBaseUrl": "http://localhost:7011",
    "QleverIndex": "icc_uvv",
    "QueryTimeoutSeconds": 30,
    "MaxResults": 1000,
    "MaxUploadBytes": 52428800
  }
}
```

Regras:

1. Nunca fixar URLs, credenciais, dataset ou índice dentro de Controllers.
2. Criar uma classe de opções validada na inicialização.
3. Permitir sobrescrita por variáveis de ambiente.
4. Não registrar credenciais nem consultas completas que possam conter dados
   sensíveis.
5. Falhar explicitamente na inicialização quando uma configuração obrigatória
   estiver ausente ou inválida.
6. Ajustar o `docker-compose.yml` somente se for necessário disponibilizar o
   QLever; nesse caso, definir volume, porta, dependências e health check sem
   remover o QLever.

Critério de conclusão: a aplicação inicia com configuração válida e falha com
mensagem clara quando a URL ou o dataset obrigatório estiverem inválidos.

### Etapa 2 — Adicionar dependências e organizar camadas

Adicionar somente os pacotes necessários, preferindo versões compatíveis com
`net9.0`. A estrutura recomendada é:

```text
backend/
├── Controllers/
├── Contracts/
│   ├── Rdf/
│   ├── Sparql/
│   ├── Search/
│   └── Statistics/
├── Services/
│   ├── Rdf/
│   ├── Sparql/
│   ├── Search/
│   └── Health/
├── Infrastructure/
│   ├── QLever/
│   └── Qlever/
├── Options/
├── Middleware/
└── Tests/
```

Responsabilidades:

- `Controllers`: HTTP, model binding, códigos de status e autorização.
- `Contracts`: requests e responses públicos da API.
- `Services`: regras de negócio e orquestração.
- `Infrastructure`: clientes HTTP, dotNetRDF e integração externa.
- `Options`: configuração tipada.
- `Middleware`: erros padronizados e correlação.
- `Tests`: testes unitários e de integração.

Não colocar chamadas HTTP ao QLever ou QLever diretamente em Controllers.

### Etapa 3 — Criar o tratamento de erros

1. Configurar `ProblemDetails` para respostas de erro.
2. Criar exceções específicas para:
   - RDF inválido;
   - SPARQL inválido;
   - recurso inexistente;
   - dependência indisponível;
   - timeout;
   - payload excedido.
3. Mapear cada exceção para o status HTTP definido neste documento.
4. Incluir `traceId` em toda resposta de erro.
5. Não capturar `Exception` de forma ampla sem registrar e transformar o erro
   corretamente.
6. Não retornar lista vazia quando QLever, QLever ou o parser falharem.

Critério de conclusão: uma falha de dependência retorna `503`, um timeout
retorna `504` e uma consulta inválida retorna `400`, todos com o mesmo formato.

### Etapa 4 — Implementar clientes de infraestrutura

#### Cliente QLever

1. Criar uma abstração como `IQleverClient`.
2. Implementar métodos para executar consultas SPARQL de leitura.
3. Enviar a consulta ao endpoint HTTP configurado do QLever.
4. Propagar o formato solicitado somente entre formatos permitidos.
5. Aplicar `CancellationToken` e timeout configurado.
6. Validar código HTTP, `Content-Type` e corpo da resposta.
7. Converter falhas de rede, timeout e respostas malformadas em exceções
   específicas.
8. Não permitir que o cliente aceite uma URL arbitrária enviada pelo usuário.

#### Cliente QLever/dotNetRDF

1. Criar uma abstração como `IRdfStore`.
2. Usar dotNetRDF para parsear e serializar os formatos RDF.
3. Encaminhar inserções e remoções ao QLever, caso ele seja o armazenamento
   oficial.
4. Associar cada importação ao grafo nomeado definido pela aplicação.
5. Manter a escrita separada da leitura via QLever.
6. Documentar como o índice QLever será atualizado após uma escrita.

Critério de conclusão: clientes podem ser substituídos por mocks nos testes e
nenhum Controller conhece detalhes de URL, headers ou formato de transporte.

### Etapa 5 — Registrar dependências no `Program.cs`

1. Registrar `HttpClient` nomeado ou tipado para QLever.
2. Registrar cliente do QLever.
3. Registrar opções com validação.
4. Registrar serviços RDF, SPARQL, busca, estatísticas e health checks.
5. Configurar `ProblemDetails` e o middleware de exceções.
6. Configurar limites de upload e tamanho máximo de request.
7. Manter `AddControllers`, `MapControllers` e OpenAPI ativos.
8. Adicionar políticas de CORS somente conforme a origem real do frontend.

### Etapa 6 — Implementar health checks

1. Implementar `/health/live` sem consultar dependências externas.
2. Implementar `/health/ready` com:
   - chamada controlada ao QLever;
   - verificação do índice configurado;
   - consulta SPARQL mínima;
   - verificação do QLever quando ele for necessário para escrita;
   - verificação do PostgreSQL somente se usado pela aplicação.
3. Retornar `503` quando uma dependência obrigatória estiver indisponível.
4. Evitar expor credenciais, queries internas ou detalhes de rede na resposta.
5. Testar startup, liveness, readiness e timeout das dependências.

### Etapa 7 — Implementar validação e importação RDF

1. Criar DTO ou modelo de formulário para arquivo, formato, grafo e metadados.
2. Validar tamanho, extensão, `Content-Type` e nome do arquivo.
3. Aceitar apenas formatos explicitamente suportados.
4. Fazer parse usando dotNetRDF sem carregar entrada ilimitada na memória.
5. Retornar erro `400` para RDF malformado.
6. Retornar `422` para RDF sintaticamente válido, mas semanticamente inválido,
   quando houver regra de validação aplicável.
7. Para `/rdf/import/validate`, não escrever no QLever.
8. Para `/rdf/import`, persistir no grafo escolhido após a validação.
9. Gerar identificador de importação, registrar metadados e retornar `201` ou
   `202` conforme o processamento seja síncrono ou assíncrono.
10. Definir explicitamente como o conteúdo persistido chegará ao índice QLever.
11. Não informar sucesso se a persistência terminar, mas a atualização
    obrigatória do índice falhar.

### Etapa 8 — Implementar grafos e recursos

1. Implementar primeiro listagem e consulta de grafos.
2. Implementar exportação de conteúdo com serializadores dotNetRDF.
3. Implementar inserção e limpeza de conteúdo somente com autorização.
4. Validar `graphName`, IRI, paginação e limites.
5. Implementar recursos RDF por consultas SPARQL executadas no QLever.
6. Para criação ou alteração, escrever no QLever e aplicar a estratégia de
   atualização do índice.
7. Usar operações parametrizadas ou construção segura de SPARQL; nunca
   concatenar valores sem escaping correto.
8. Exigir confirmação ou escopo explícito para remoções amplas.

### Etapa 9 — Implementar endpoints SPARQL com QLever

1. Criar contratos separados para consulta, atualização e resposta.
2. Aceitar somente os tipos de consulta previstos:
   `SELECT`, `ASK`, `CONSTRUCT` e `DESCRIBE` em `/sparql/query`.
3. Validar a sintaxe e classificar a operação antes de chamar o QLever.
4. Encaminhar toda leitura ao `IQleverClient`.
5. Aplicar grafo padrão, paginação, timeout e limite máximo de resultados.
6. Negociar somente formatos de resposta suportados.
7. Preservar o formato SPARQL Results para `SELECT` e `ASK`.
8. Encaminhar `/sparql/update` ao armazenamento de escrita somente se esse
   fluxo estiver comprovadamente suportado.
9. Caso a escrita no QLever não esteja disponível, retornar `501`.
10. Criar templates SPARQL com parâmetros tipados e allowlist de nomes.
11. Nunca executar uma query gerada por IA sem validação, limites e
    autorização.

### Etapa 10 — Implementar busca livre

1. Criar request com `text`, `graph`, `page` e `pageSize`.
2. Validar texto não vazio e limites de tamanho.
3. Converter a busca para uma consulta SPARQL segura e determinística.
4. Executar a consulta gerada pelo QLever.
5. Retornar resultados paginados, total quando disponível e metadados mínimos.
6. Implementar sugestões usando consultas controladas.
7. Não aceitar SPARQL no endpoint de busca livre.
8. Registrar métricas sem armazenar texto sensível desnecessariamente.

### Etapa 11 — Implementar ontologias

1. Detectar ou registrar ontologias por IRI.
2. Importar ontologias pelo mesmo pipeline de validação RDF.
3. Consultar classes, propriedades e instâncias via QLever.
4. Separar ontologia de dados de aplicação por grafo ou metadados.
5. Validar IRIs de ontologia e classe.
6. Garantir paginação em listagens potencialmente grandes.
7. Permitir remoção somente para usuários autorizados.

### Etapa 12 — Implementar estatísticas

1. Criar um serviço de métricas para importações, buscas e consultas SPARQL.
2. Medir duração, sucesso, falha, timeout e quantidade de resultados.
3. Definir onde os contadores serão persistidos; não usar variáveis estáticas
   em memória para estatísticas de produção.
4. Implementar os endpoints de overview, grafos, queries, imports e ontologias.
5. Não expor a consulta SPARQL completa em estatísticas públicas.
6. Retornar valores reais ou indicar indisponibilidade; não manter números
   fictícios da interface.

### Etapa 13 — Autenticação, autorização e proteção operacional

1. Definir autenticação antes de expor escrita, importação ou SPARQL arbitrário.
2. Criar políticas distintas para leitura, importação, atualização e remoção.
3. Aplicar rate limiting em consultas e buscas.
4. Definir limites de payload, timeout e concorrência.
5. Proteger endpoints contra SSRF: URLs externas nunca devem vir do request.
6. Sanitizar logs e não registrar tokens, senhas ou dados sensíveis.
7. Auditar operações destrutivas com usuário, grafo, data e resultado.

### Etapa 14 — Testes obrigatórios

Implementar testes unitários para:

- validação de formatos e tamanho de arquivo;
- validação de IRI, grafo e paginação;
- classificação de consultas SPARQL;
- criação segura de requests para QLever;
- mapeamento de exceções para `ProblemDetails`;
- tratamento de timeout e respostas inválidas.

Implementar testes de integração para:

- `/health/live` e `/health/ready`;
- importação válida e inválida;
- consulta `SELECT` executada no QLever;
- consulta `ASK` executada no QLever;
- resposta `CONSTRUCT` ou `DESCRIBE`;
- QLever indisponível;
- QLever indisponível durante uma escrita;
- limites de upload e paginação;
- tentativa de atualização não suportada.

Os testes de integração devem usar serviços controlados ou containers
descartáveis. Não executar testes destrutivos contra um ambiente compartilhado.

### Etapa 15 — Documentação e verificação final

1. Atualizar OpenAPI com requests, responses, formatos e códigos de erro.
2. Documentar variáveis de ambiente do QLever e QLever.
3. Atualizar `backend.http` com exemplos não destrutivos.
4. Explicar no README como iniciar dependências e validar readiness.
5. Executar restore, build, testes e lint disponíveis.
6. Conferir que nenhuma URL ou credencial foi fixada no código.
7. Conferir que todos os endpoints SPARQL de leitura usam QLever.
8. Conferir que o fluxo de escrita não declara sucesso sem persistência e
   atualização de índice concluídas.
9. Conferir respostas `ProblemDetails`, códigos HTTP e logs.
10. Resumir no final quais etapas foram concluídas e quais ficaram bloqueadas.

## Critérios gerais de conclusão

A implementação somente deve ser considerada concluída quando:

- a API inicia com configuração documentada;
- o QLever é usado por todos os endpoints de leitura SPARQL;
- a escrita está claramente separada e não é simulada;
- RDF inválido, query inválida e dependência indisponível geram erros explícitos;
- importações e consultas têm limites de tamanho, tempo e resultados;
- health checks distinguem processo ativo de dependências prontas;
- testes cobrem sucesso, falha e timeout;
- OpenAPI e exemplos refletem o comportamento real;
- nenhuma funcionalidade retorna dados fictícios ou sucesso silencioso.
