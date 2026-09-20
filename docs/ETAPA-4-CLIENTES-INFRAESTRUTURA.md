# Etapa 4 — Clientes de infraestrutura para Fuseki e QLever

## Objetivo

Fortalecer os clientes e serviços de infraestrutura do backend RDF para que suportem validação de configuração, timeout, tratamento de falhas e respostas consistentes em caso de indisponibilidade de dependências.

## Implementação realizada

### 1. Configuração de timeout por cliente

Arquivo atualizado:

- `backend/Program.cs`

Os clientes HTTP de Fuseki e QLever agora recebem tempo de expiração com base na configuração global:

```csharp
builder.Services.AddHttpClient<IFusekiClient, FusekiClient>((sp, client) =>
{
    var options = sp.GetRequiredService<RdfOptions>();
    client.BaseAddress = new Uri(options.FusekiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.QueryTimeoutSeconds);
});

builder.Services.AddHttpClient<IQleverClient, QleverClient>((sp, client) =>
{
    var options = sp.GetRequiredService<RdfOptions>();
    client.BaseAddress = new Uri(options.QleverBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.QueryTimeoutSeconds);
});
```

Isso centraliza a política de timeout e evita que cada cliente tenha comportamento imprevisível.

### 2. Validação de URLs e parâmetros

Os clientes agora validam a URL base e os parâmetros obrigatórios antes de realizar qualquer chamada externa.

Exemplos:

- `FusekiClient` rejeita `FusekiBaseUrl` inválida;
- `QleverClient` rejeita `QleverBaseUrl` inválida;
- `QleverClient` exige query não vazia;
- `QleverClient` exige grafo padrão ou índice configurado.

### 3. Tratamento de indisponibilidade de dependências

Arquivos atualizados:

- `backend/Infrastructure/Fuseki/FusekiClient.cs`
- `backend/Infrastructure/Qlever/QleverClient.cs`

Os clientes agora convertem falhas de rede e indisponibilidade em exceções explícitas:

- `DependencyUnavailableException` para comunicação falha e ausência de serviço;
- `TimeoutException` para timeout do cliente;
- `SparqlException` para consultas rejeitadas pelo QLever.

Dessa forma, a camada de infraestrutura impede que a aplicação continue em estado inconsistente ou devolva respostas ambíguas.

### 4. Respostas de erro mais informativas

No caso do QLever, quando a query recebe resposta com status diferente de sucesso, o cliente:

- lê o corpo da resposta;
- inclui o status HTTP e a mensagem do provedor;
- lança `SparqlException` com detalhes relevantes.

No caso do Fuseki, quando `/$/ping` falha, o cliente lança `DependencyUnavailableException` com o código HTTP e motivo.

## Benefícios desta etapa

- maior resiliência às falhas de infraestrutura;
- melhor diagnósticos em ambientes de desenvolvimento e produção;
- respostas consistentes através do middleware global;
- preparo para implementação dos endpoints finais da API RDF;
- melhor isolamento entre infraestrutura e lógica de domínio.

## Observações

- os clientes ainda são uma base inicial e serão expandidos conforme os endpoints reais forem implementados;
- o desacoplamento da lógica de integração continua preservado;
- os clientes continuam dependentes apenas de `RdfOptions`, sem acoplamento ao `Program.cs` ou `Controllers`.

## Próximo passo recomendado

Implementar a etapa 5, que consiste em registrar dependências no pipeline da aplicação e preparar os health checks e inicialização com validação de serviços externos.
