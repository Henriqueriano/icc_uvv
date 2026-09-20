# Etapa 14 — Testes obrigatórios para o backend RDF

## Objetivo

Garantir que a API RDF continue segura, estável e previsível, cobrindo os cenários críticos de validação, autenticação, proteção operacional e tratamento de falhas.

## Implementação realizada

### 1. Estrutura de testes

Diretório criado:

- `tests/backend.Tests/`

Projeto de testes adicionado:

- `tests/backend.Tests/backend.Tests.csproj`

A suíte usa xUnit e referência o projeto principal `backend/backend.csproj`.

### 2. Testes de validação RDF

Arquivo criado:

- `tests/backend.Tests/RdfValidationServiceTests.cs`

Cobertura implementada:

- RDF Turtle válido retorna sucesso;
- RDF inválido dispara `RdfException`;
- arquivo acima do limite de payload dispara `ArgumentException`.

### 3. Testes de segurança e SSRF

No mesmo arquivo de testes, foram adicionados cenários para:

- URLs públicas HTTPS são aceitas;
- localhost e hosts loopback são rejeitados por padrão;
- URLs privadas configuradas disparam a aplicação com `InvalidOperationException`.

### 4. Testes de SPARQL

Também em `RdfValidationServiceTests.cs`:

- consulta `SELECT` válida delega corretamente para o cliente QLever;
- operação de escrita (`INSERT`) dispara `SparqlException`.

### 5. Teste de middleware de exceções

Arquivo criado na suíte:

- `ExceptionHandlingMiddlewareTests`

Valida que:

- falha de dependência retorna `503 Service Unavailable`;
- a resposta é emitida em `application/problem+json`;
- o `traceId` é preservado na resposta.

## Benefícios desta etapa

- reduz o risco de regressão em cenários críticos do backend RDF;
- valida a integração entre QLever, validação RDF e middleware de erro;
- documenta o comportamento esperado para entradas válidas e inválidas;
- fornece base de confiança para evoluções futuras do sistema.

## Observações importantes

- esta suíte representa a base obrigatória de testes para o backend em desenvolvimento;
- a cobertura ainda pode ser ampliada com testes de integração contra serviços reais de Fuseki e QLever;
- em ambientes de produção, recomenda-se executar testes em containers descartáveis para evitar impacto em infraestrutura compartilhada.

## Próximo passo recomendado

Avançar para testes de integração reais com containers ou serviços controlados, cobrindo readiness, importação válida/inválida e falha de QLever/Fuseki de forma end-to-end.
