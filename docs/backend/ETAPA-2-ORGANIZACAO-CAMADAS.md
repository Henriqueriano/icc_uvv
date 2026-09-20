# Etapa 2 — Organização das camadas e dependências de infraestrutura

## Objetivo

Organizar a aplicação em camadas claras para que o backend RDF respeite a separação entre configuração, infraestrutura, serviços e lógica de controle. A ideia é manter Controllers enxutos e evitar acoplamento direto com HTTP, QLever, QLever ou dotNetRDF.

## Implementação realizada

### 1. Criação da estrutura de camadas

Foram criados os diretórios iniciais:

- `backend/Infrastructure/QLever`
- `backend/Infrastructure/Qlever`
- `backend/Services/Rdf`

Essa estrutura representa a divisão proposta na etapa anterior, alinhando-se ao padrão de desenvolvimento do projeto e preparando a base para integração com RDF, QLever e consultas híbridas.

### 2. Cliente de infraestrutura do QLever

Arquivo criado:

- `backend/Infrastructure/QLever/IQLeverClient.cs`
- `backend/Infrastructure/QLever/QLeverClient.cs`

A abstração expõe um contrato mínimo para verificação de disponibilidade do QLever:

```csharp
public interface IQLeverClient
{
    Task<string> PingAsync(CancellationToken cancellationToken = default);
}
```

A implementação usa `HttpClient` com a URL base configurada em `RdfOptions.QLeverBaseUrl` e executa um simples ping ao endpoint do ambiente RDF.

### 3. Cliente de infraestrutura do QLever

Arquivos criados:

- `backend/Infrastructure/Qlever/IQleverClient.cs`
- `backend/Infrastructure/Qlever/QleverClient.cs`

A abstração define a execução de consultas SPARQL:

```csharp
public interface IQleverClient
{
    Task<string> ExecuteQueryAsync(string query, string? defaultGraph = null, CancellationToken cancellationToken = default);
}
```

A implementação:

- usa a URL base configurada em `RdfOptions.QleverBaseUrl`;
- envia a query para o endpoint `/sparql`;
- usa o `defaultGraph` configurado ou o índice do ambiente;
- valida a query antes de enviar;
- dispara exceção se a resposta HTTP não for bem-sucedida.

### 4. Serviço de RDF

Arquivos criados:

- `backend/Services/Rdf/IRdfService.cs`
- `backend/Services/Rdf/RdfService.cs`

O serviço atua como camada de orquestração entre a infraestrutura do QLever e a do QLever. A implementação atual é um ponto de partida com um método de resumo de configuração:

```csharp
public interface IRdfService
{
    Task<string> GetConfigurationSummaryAsync(CancellationToken cancellationToken = default);
}
```

Ele:

- chama o ping do QLever;
- executa uma query de teste no QLever;
- retorna uma string resumida para uso futuro em health checks ou diagnóstico.

### 5. Registro de dependências em `Program.cs`

Arquivo atualizado:

- `backend/Program.cs`

Foram adicionados os registros:

```csharp
builder.Services.AddHttpClient<IQLeverClient, QLeverClient>();
builder.Services.AddHttpClient<IQleverClient, QleverClient>();
builder.Services.AddScoped<IRdfService, RdfService>();
```

Essas linhas deixam claro o padrão de injeção de dependência e evitam que Controllers conheçam detalhes de infraestrutura externa.

## Benefícios desta etapa

- separação clara entre configuração, clientes HTTP e serviços de domínio;
- preparação para futuras Controllers e contratos da API RDF;
- redução do acoplamento da aplicação com implementações concretas;
- melhor testabilidade por meio de mocks ou clientes em memória;
- alinhamento com a arquitetura recomendada para a próxima etapa de implementação.

## Observações importantes

- os clientes e serviços criados nesta etapa são estruturas iniciais e ainda não implementam o CRUD completo de RDF;
- o código foi organizado para suportar a etapa seguinte, em que serão adicionados contratos HTTP mais completos e endpoints de consulta;
- a lógica de escrita em QLever ainda precisa ser definida de forma explícita pela arquitetura da aplicação;
- o acoplamento com `RdfOptions` foi mantido em infraestrutura e serviços, não em Controllers.

## Próximo passo recomendado

Implementar a etapa 3, que consiste em criar o tratamento de erros padronizado, `ProblemDetails`, exceções específicas e a conversão de falhas internas para códigos HTTP consistentes.
