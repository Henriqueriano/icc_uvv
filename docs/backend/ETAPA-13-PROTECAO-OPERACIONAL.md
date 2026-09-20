# Etapa 13 — Proteção operacional e hardening da API RDF

## Objetivo

Fortalecer a aplicação RDF contra abuso, uso indevido de URLs externas, excesso de requisições e ausência de auditoria, preservando a estrutura já implementada em camadas e mantendo compatibilidade com o restante do backend.

## Implementação realizada

### 1. Configuração de segurança

Arquivo criado:

- `backend/Options/SecurityOptions.cs`

A nova seção `Security` centraliza:

- `RateLimitPerMinute`
- `AuthRateLimitPerMinute`
- `SearchRateLimitPerMinute`
- `AllowLoopbackUrls`
- `AllowedHosts`

Esses valores tornam as políticas de proteção explícitas e fáceis de ajustar por ambiente.

### 2. Validação contra SSRF

Arquivo criado:

- `backend/Infrastructure/Security/SecurityUrlValidator.cs`

A validação implementada:

- rejeita URLs inválidas;
- aceita apenas `http` e `https`;
- bloqueia hosts loopback e privados por padrão;
- permite hosts explícitos somente quando configurados;
- falha de forma explícita quando uma configuração obrigatória aponta para um destino proibido.

Essa camada é usada na inicialização para validar as URLs do Fuseki e do QLever, reduzindo o risco de SSRF via configuração ou entrada maliciosa.

### 3. Middleware de validação de entrada

Arquivo criado:

- `backend/Middleware/SecurityValidationMiddleware.cs`

O middleware verifica parâmetros e headers como:

- `url`
- `uri`
- `sourceUrl`
- `targetUrl`
- `redirectUrl`
- `X-Forwarded-Uri`
- `Referer`
- `Origin`

Se qualquer valor contiver um destino externo não permitido, a requisição é recusada com erro explícito.

### 4. Rate limiting

Arquivo atualizado:

- `backend/Program.cs`

A API agora usa `AddRateLimiter` com políticas:

- `default`: limite geral por cliente/IP;
- `auth`: limite especial para emissão de token;
- `search`: limite para buscas e consultas de leitura.

Os controllers relevantes foram marcados com atributos `EnableRateLimiting` para reforçar o controle.

### 5. Auditoria sanitizada

Arquivos criados:

- `backend/Services/Auditing/IAuditService.cs`
- `backend/Services/Auditing/AuditService.cs`

A auditoria registra eventos sem expor dados sensíveis. A sanitização remove ou mascará valores como:

- `password`
- `secret`
- `token`
- `authorization`
- `jwt`
- `apiKey`

### 6. Registro de auditoria em fluxos críticos

Arquivos atualizados:

- `backend/Controllers/AuthController.cs`
- `backend/Controllers/RdfDocumentsController.cs`

A autenticação e o processo de importação RDF passam a registrar ações de sucesso e falha com:

- ação;
- recurso;
- ator;
- detalhamento sanitizado;
- resultado da operação.

### 7. Configuração de segurança no ambiente

Arquivos atualizados:

- `backend/appsettings.json`
- `backend/appsettings.Development.json`

A seção `Security` foi adicionada para manter a política de segurança configurável por ambiente.

## Benefícios desta etapa

- proteção contra abuso por excesso de requisições;
- redução do risco de SSRF em headers, query strings e metadados de URL;
- registro de operações sensíveis com rastreabilidade sem expor segredos;
- continuidade da padronização da API em camadas e do uso de middlewares;
- preparação para ambientes mais seguros e governados.

## Observações importantes

- este é um hardening inicial, não um substituto para WAF, gateway de API ou provider de identidade externo;
- URLs externas vindas do cliente são rejeitadas por padrão e não devem ser aceitas em endpoints de integração;
- a auditoria ficou focada em ações críticas, e pode evoluir para persistência em banco ou logs centralizados;
- a proteção por rate limit deve continuar ajustada conforme volume real do sistema.

## Próximo passo recomendado

Implementar testes automatizados de segurança para validar:

- rate limit por IP;
- rejeição de URL externa em query string;
- auditoria sanitizada de tokens e senhas;
- falha explícita ao configurar URLs de infraestrutura fora do padrão permitido.
