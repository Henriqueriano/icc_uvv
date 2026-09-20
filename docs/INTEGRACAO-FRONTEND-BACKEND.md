# Integração entre frontend e backend

## Visão geral

O frontend React executado pelo Bun consome a API ASP.NET Core por HTTP. As
chamadas estão centralizadas em `frontend/src/api.ts`, evitando URLs,
headers e tratamento de erros duplicados nos componentes.

O endereço da API é definido por `FRONTEND_API_URL` no arquivo
`frontend/.env`. O exemplo local usa:

```text
http://localhost:5079/api/v1
```

O servidor Bun do frontend usa a porta definida no ambiente. O backend aceita
requisições CORS da origem definida por `Frontend__Url`.

```text
http://localhost:4040
http://127.0.0.1:4040
```

## Autenticação

O login da tela administrativa envia:

```http
POST /api/v1/auth/token
Content-Type: application/json

{
  "username": "rdf-admin",
  "password": "ChangeMe123!"
}
```

O token JWT retornado é salvo em `localStorage` com a chave
`accessToken`. As requisições autenticadas incluem:

```http
Authorization: Bearer <token>
```

O estado visual de administrador também é mantido em `localStorage` com a
chave `isAdmin`. O token é a credencial efetiva; `isAdmin` apenas controla a
rota e a apresentação da interface.

Ao sair do painel, o frontend remove o token e o estado administrativo.

## Integrações disponíveis no frontend

| Funcionalidade | Método e endpoint | Componente |
|---|---|---|
| Login | `POST /api/v1/auth/token` | `LoginView` |
| Busca livre | `POST /api/v1/search` | `SearchSection` |
| Consulta SPARQL | `POST /api/v1/sparql/query` | `SearchSection` |
| Ontologias | `GET /api/v1/ontologies` | `OntologiesSection` |
| Estatísticas | `GET /api/v1/statistics/overview` | `AdminStatisticsSection` |
| Importação RDF | `POST /api/v1/rdf/import` | `AdminInsertSection` |

Busca livre e consulta SPARQL exibem o JSON retornado pela API em um bloco de
resultado. Falhas HTTP são convertidas em mensagens exibidas na própria tela.

## Upload RDF

O componente administrativo cria um `FormData` com:

- `file`: arquivo RDF selecionado;
- `graphName`: atualmente `icc_uvv`;
- `format`: opcional.

O token JWT é enviado automaticamente pelo cliente HTTP. A resposta informa o
status da validação, o grafo, o formato e a quantidade de triplas.

## Executando localmente

Backend:

```bash
dotnet run --project backend/backend.csproj --launch-profile http
```

Frontend:

```bash
cd frontend
bun install
bun dev
```

O PostgreSQL e o QLever podem ser iniciados com:

```bash
docker compose -f backend/docker-compose.yml up -d
```

## Limitações conhecidas

- URLs, credenciais e parâmetros de ambiente ficam em `.env`; use
  `.env.example` como referência e não versione arquivos `.env`.
- Para outro ambiente, altere `FRONTEND_API_URL` em `frontend/.env` sem
  modificar o código.
- A rota de importação atualmente valida o arquivo e registra a operação,
  mas a persistência definitiva no QLever ainda deve ser concluída.
- Algumas métricas de estatísticas não possuem fonte histórica persistida e
  podem retornar zero.
- A tela de ontologias usa dados de apresentação locais para perfil e imagem;
  os metadados principais são carregados da API quando existem.
