# Etapa 12 — Autenticação e autorização para acesso RDF

## Objetivo

Implementar a camada de autenticação e autorização mínima para proteger os endpoints críticos do backend RDF, especialmente importação, escrita e consultas sensíveis que podem expor ou alterar dados do sistema.

## Implementação realizada

### 1. Configuração de autenticação JWT

Arquivo criado:

- `backend/Options/AuthOptions.cs`

A classe centraliza os parâmetros do JWT:

- emissor (`JwtIssuer`);
- audiência (`JwtAudience`);
- chave de assinatura (`JwtKey`);
- usuário padrão (`DefaultUsername`);
- senha padrão (`DefaultPassword`);
- expiração do token (`TokenExpirationMinutes`).

Ela também usa validação por data annotations para falhar cedo na inicialização quando a configuração estiver incompleta ou insegura.

### 2. Contratos de autenticação

Arquivos criados:

- `backend/Contracts/Auth/AuthTokenRequest.cs`
- `backend/Contracts/Auth/AuthTokenResponse.cs`

Esses contratos definem a requisição de login e a resposta com o token JWT emitido pela API.

### 3. Endpoint de emissão de token

Arquivo criado:

- `backend/Controllers/AuthController.cs`

Endpoint implementado:

- `POST /api/v1/auth/token`

Comportamento:

- aceita `username` e `password` em JSON;
- valida as credenciais configuradas no appsettings;
- emite um JWT assinado com HMAC SHA256;
- inclui claims de usuário e papel (`RdfAdmin`).

### 4. Proteção de endpoints críticos

Arquivo atualizado:

- `backend/Controllers/RdfDocumentsController.cs`

A ação de importação foi protegida com autorização:

- `POST /api/v1/rdf/import`

Também foi adicionada a configuração de políticas de autorização no `Program.cs` e a chamada ao `UseAuthentication()` antes do `UseAuthorization()`.

### 5. Política de autorização

Arquivo atualizado:

- `backend/Program.cs`

As alterações incluem:

- registro de `AddAuthentication` com JWT bearer;
- registro de `AddAuthorization` com a política `RdfWrite`;
- configuração do `TokenValidationParameters` com issuer, audience, key e validade;
- associação da política `RdfWrite` aos endpoints exigidos.

### 6. Configuração dos valores padrão

Arquivos atualizados:

- `backend/appsettings.json`
- `backend/appsettings.Development.json`

Foi incluída a seção `Auth` com credenciais e chaves de exemplo para ambiente local. A chave deve ser substituída por um valor forte em produção.

### 7. Dependência do pacote JWT

Arquivo atualizado:

- `backend/backend.csproj`

Foi adicionada a referência:

- `Microsoft.AspNetCore.Authentication.JwtBearer`

## Benefícios desta etapa

- a API deixa de aceitar operações de escrita anônimas;
- o backend passa a emitir tokens para autenticação do frontend ou serviços internos;
- a arquitetura continua coerente com a separação entre leitura, escrita e infraestrutura RDF;
- os endpoints críticos ficam protegidos por políticas de autorização e podem evoluir para perfis mais granulares.

## Observações importantes

- a implementação atual é uma base segura para ambiente local e integração inicial;
- as credenciais são configuráveis, mas ainda devem ser movidas para segredos reais em produção;
- a validação é feita por JWT e roles, mas a gestão de usuários ainda pode evoluir para um provedor externo, banco de dados ou identidade centralizada;
- o endpoint de token é permiti do anonimamente apenas para a emissão inicial do JWT, e os pontos de escrita exigem autenticação.

## Próximo passo recomendado

Prosseguir para testes automatizados de autenticação, autorização e proteção de endpoints críticos, incluindo cenários de token inválido, token expirado e tentativa de escrita sem autenticação.
