# Etapa 5 — Health checks e registro de dependências externas

## Objetivo

Preparar a aplicação para verificar disponibilidade de serviços externos e expor endpoints mínimos de saúde com separação correta entre liveness e readiness.

## Implementação realizada

### 1. Criação da camada de health check para RDF

Arquivos criados:

- `backend/Infrastructure/Health/IRdfHealthCheck.cs`
- `backend/Infrastructure/Health/RdfHealthCheck.cs`

A interface define a responsabilidade de verificar se o backend RDF está pronto:

```csharp
public interface IRdfHealthCheck
{
    Task<bool> IsReadyAsync(CancellationToken cancellationToken = default);
}
```

A implementação realiza dois testes essenciais:

- ping no Fuseki;
- consulta mínima no QLever.

Se qualquer dependência falhar, retorna `false` e indica que a API não está pronta para receber tráfego operacional completo.

### 2. Registro do serviço no container de DI

Arquivo atualizado:

- `backend/Program.cs`

Foi adicionado o registro:

```csharp
builder.Services.AddScoped<IRdfHealthCheck, RdfHealthCheck>();
```

Isso permite que o `HealthController` e quaisquer outros pontos da aplicação consultem o estado da infraestrutura sem acoplamento direto ao cliente HTTP.

### 3. Criação do Controller de saúde

Arquivo criado:

- `backend/Controllers/HealthController.cs`

O controller expõe:

- `GET /health`
- `GET /health/ready`

A resposta de `/health` é um indicador simples de liveness, enquanto `/health/ready` invoca a verificação de dependências finais.

### 4. Mapeamento dos endpoints de health

Arquivo atualizado:

- `backend/Program.cs`

Foi adicionado:

```csharp
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
```

Essa estratégia foi aplicada como base de readiness para a aplicação, com a camada de controller adicional para respostas e status mais explícitos.

## Observações importantes

- `/health` atende ao propósito de processo ativo;
- `/health/ready` valida a disponibilidade do armazém RDF e do QLever;
- a verificação ainda é inicial, mas já segue o padrão sugerido pela etapa de planejamento;
- a lógica de readiness permanece separada da lógica de endpoints de negócio, em conformidade com a arquitetura proposta.

## Benefícios desta etapa

- diagnóstico mais rápido de falhas externas;
- base para implementar operação e monitoramento;
- separação entre liveness e readiness;
- maior clareza para governança de infraestrutura RDF e consulta semântica.

## Próximo passo recomendado

Implementar a etapa 6, que consiste em criar a validação e importação de RDF e preparar os DTOs iniciais para arquivos, formatos e grafos.
