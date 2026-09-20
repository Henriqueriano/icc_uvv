# Alterações e integrações do backend

## Banco de usuários com Entity Framework Core

O backend agora usa Entity Framework Core com PostgreSQL por meio do pacote
`Npgsql.EntityFrameworkCore.PostgreSQL`.

Arquivos principais:

- `backend/Data/User.cs`: entidade de usuário;
- `backend/Data/AppDbContext.cs`: contexto EF Core e mapeamento da tabela;
- `backend/Data/DatabaseInitializer.cs`: aplicação de migrations e criação do
  usuário administrativo inicial;
- `backend/Data/Migrations/CreateUsers`: migration da tabela `users`.

A tabela `users` possui:

| Coluna | Descrição |
|---|---|
| `id` | Identificador UUID |
| `username` | Nome de usuário único |
| `password_hash` | Hash gerado por `PasswordHasher<User>` |
| `role` | Papel usado nas claims JWT |
| `is_active` | Permite desativar o acesso |
| `created_at_utc` | Data de criação em UTC |

Na inicialização, a aplicação executa `Database.MigrateAsync()`. Se não
houver usuários, cria um usuário com `Auth:DefaultUsername` e
`Auth:DefaultPassword`, mas salva apenas o hash da senha.

## PostgreSQL no Docker Compose

O arquivo `backend/docker-compose.yml` disponibiliza:

- PostgreSQL 16 Alpine;
- banco `ic_uvv`;
- usuário `admin`;
- volume persistente `postgres_data`;
- healthcheck com `pg_isready`;
- porta local `5432`.

A conexão da aplicação está definida em `ConnectionStrings:DefaultConnection`
por variável de ambiente. O arquivo `.env.example` documenta essa variável.

## Integração com Fuseki

O serviço Fuseki do Compose usa o dataset `icc_uvv`, volume persistente
`fuseki_data` e porta local `3030`. As credenciais administrativas são
configuradas por `ADMIN_USERNAME` e `ADMIN_PASSWORD`.

O cliente `FusekiClient`:

- configura autenticação Basic;
- verifica disponibilidade com `GET /$/ping`;
- executa consultas SPARQL com `POST /{dataset}/query`;
- aceita resposta `application/sparql-results+json`;
- traduz falhas de rede, timeout e respostas inválidas para as exceções de
  infraestrutura do projeto.

As credenciais usadas pela aplicação ficam em:

```dotenv
Rdf__FusekiBaseUrl=http://localhost:3030
Rdf__FusekiDataset=icc_uvv
Rdf__FusekiUsername=admin
Rdf__FusekiPassword=...
```

Em produção, esses valores devem ser substituídos por secrets ou variáveis de
ambiente. Não se deve reutilizar as credenciais de desenvolvimento.

## Variáveis de ambiente

O backend carrega o arquivo `.env` local com `DotNetEnv` e também aceita as
variáveis padrão do ASP.NET Core. Os nomes usam `__` para representar seções
de configuração, por exemplo:

```dotenv
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=ic_uvv;Username=admin;Password=...
Auth__JwtKey=...
Auth__DefaultUsername=rdf-admin
Auth__DefaultPassword=...
Frontend__Url=http://localhost:4040
```

O Compose usa o mesmo `.env` para configurar PostgreSQL e Fuseki. Arquivos
`.env` são ignorados pelo Git; somente `.env.example` deve ser versionado.

## Serviços RDF conectados ao Fuseki

As implementações atuais usam consultas reais do dataset para:

- listar grafos e contar triplas;
- obter estatísticas de um grafo;
- recuperar conteúdo de um grafo;
- listar ontologias declaradas como `owl:Ontology`;
- listar classes `owl:Class`;
- listar propriedades `rdf:Property`;
- calcular a visão geral básica de estatísticas RDF.

As consultas incluem os prefixes RDF/OWL necessários para funcionar
independentemente de declarações externas no dataset.

## Autenticação e autorização

`AuthController` consulta a tabela `users` e valida a senha com hash. O JWT
contém:

- `sub`: ID do usuário;
- `name`: nome do usuário;
- `role`: papel do usuário;
- `jti`: identificador único do token.

As políticas existentes são:

- `RdfRead`: exige usuário autenticado;
- `RdfWrite`: exige usuário autenticado com role `RdfAdmin`.

## CORS e integração HTTP

O backend registra a política `Frontend`, permitindo métodos e headers para as
origens locais do Bun:

```text
http://localhost:4040
http://127.0.0.1:4040
```

Swagger UI e OpenAPI continuam disponíveis somente no ambiente de
desenvolvimento:

```text
/swagger
/swagger/v1/swagger.json
/openapi/v1.json
```

## Estado atual e próximos pontos técnicos

- `RdfDocumentsController` ainda valida o upload e audita a operação; a
  gravação efetiva do arquivo no Fuseki não está concluída.
- Estatísticas de consultas, buscas, tempo médio e taxa de sucesso dependem de
  armazenamento de métricas; sem histórico, esses campos podem retornar zero.
- O QLever continua disponível como cliente separado para os fluxos que ainda
  dependem dele.
- A validação de URLs e o rate limiting permanecem ativos antes do acesso aos
  serviços RDF.

## Validação

Comandos usados para validar as alterações:

```bash
dotnet build backend/backend.csproj --no-restore
docker compose -f backend/docker-compose.yml config --quiet
cd frontend && bun run build
```
