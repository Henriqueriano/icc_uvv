# Etapa 3 — Tratamento padronizado de erros

## Objetivo

Estabelecer um mecanismo consistente de respostas de erro para a API RDF, cobrindo falhas de validação, dependências externas, tempo limite e problemas de formato RDF/SPARQL.

## Implementação realizada

### 1. Criação das exceções específicas

Arquivos criados:

- `backend/Infrastructure/Exceptions/RdfException.cs`
- `backend/Infrastructure/Exceptions/SparqlException.cs`
- `backend/Infrastructure/Exceptions/DependencyUnavailableException.cs`

Essas exceções foram criadas para diferenciar:

- RDF inválido;
- SPARQL inválido;
- dependência externa indisponível;
- falhas genéricas de infraestrutura.

### 2. Middleware global de exceções

Arquivo criado:

- `backend/Middleware/ExceptionHandlingMiddleware.cs`

O middleware:

- captura exceções não tratadas do pipeline;
- registra o erro em log com `ILogger`;
- monta um `ProblemDetails`;
- atribui `traceId` com base em `Activity.Current` ou `context.TraceIdentifier`;
- devolve `application/problem+json`;
- converte códigos de erro para status HTTP consistentes.

Mapeamento implementado:

- `ArgumentException` -> `400 Bad Request`
- `RdfException` -> `422 Unprocessable Entity`
- `SparqlException` -> `400 Bad Request`
- `DependencyUnavailableException` -> `503 Service Unavailable`
- `TimeoutException` -> `504 Gateway Timeout`
- demais exceções -> `500 Internal Server Error`

### 3. Registro do middleware no pipeline

Arquivo atualizado:

- `backend/Program.cs`

Foi adicionado:

```csharp
builder.Services.AddProblemDetails();
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

Isso assegura que toda falha inesperada passe pelo mesmo mecanismo de resposta.

## Benefícios desta etapa

- respostas uniformes para frontends e inteligências artificiais;
- melhor diagnósticos ao integrar com QLever, QLever e validações de RDF;
- padronização consistente com `ProblemDetails`;
- menor chance de erros de camada de apresentação mascararem falhas reais;
- estrutura pronta para as próximas etapas de endpoints e health check.

## Observações importantes

- o middleware foi criado para servir como base da API, mas a lógica específica de cada endpoint ainda será adicionada nas etapas seguintes;
- o registro de logs foi mantido sem expor dados sensíveis;
- `ProblemDetails` centraliza o formato das respostas, mesmo sem implementar ainda todos os contratos de negócio da aplicação.

## Próximo passo recomendado

Implementar a etapa 4, que consiste em criar os clientes de infraestrutura de maneira mais robusta, com validação de chamada, timeout e integração explícita com QLever e QLever.
