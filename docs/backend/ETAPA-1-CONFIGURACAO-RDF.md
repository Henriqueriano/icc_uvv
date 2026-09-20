# Etapa 1 — Configuração e opções tipadas do backend RDF

## Objetivo

Definir e registrar a configuração do ambiente RDF da aplicação de forma tipada e validada, separando os parâmetros do Fuseki e do QLever da lógica de negócio e dos Controllers.

## Implementação realizada

### 1. Registro das opções em `Program.cs`

Foi adicionado o binding da seção `Rdf` com validação via data annotations e validação na inicialização da aplicação.

Arquivos envolvidos:

- `backend/Program.cs`
- `backend/Options/RdfOptions.cs`

Configuração executada:

```csharp
builder.Services.AddOptions<RdfOptions>()
    .BindConfiguration(RdfOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<RdfOptions>>().Value);
```

Isso garante que:

- a classe `RdfOptions` seja alimentada pela configuração do appsettings;
- valores obrigatórios sejam validados antes da aplicação iniciar;
- a aplicação falhe cedo quando o ambiente estiver configurado de forma inválida.

### 2. Criação da classe de opções

Arquivo criado:

- `backend/Options/RdfOptions.cs`

A classe contém os seguintes parâmetros:

- `FusekiBaseUrl`
- `FusekiDataset`
- `QleverBaseUrl`
- `QleverIndex`
- `QueryTimeoutSeconds`
- `MaxResults`
- `MaxUploadBytes`

Validações aplicadas:

- URLs obrigatórias e em formato válido;
- dataset/index obrigatório;
- timeout positivo;
- quantidade máxima de resultados positiva;
- tamanho máximo de upload positivo e limitado.

### 3. Atualização dos arquivos de configuração

Arquivos atualizados:

- `backend/appsettings.json`
- `backend/appsettings.Development.json`

A seção adicionada segue este padrão:

```json
"Rdf": {
  "FusekiBaseUrl": "http://localhost:3030",
  "FusekiDataset": "icc_uvv",
  "QleverBaseUrl": "http://localhost:7011",
  "QleverIndex": "icc_uvv",
  "QueryTimeoutSeconds": 30,
  "MaxResults": 1000,
  "MaxUploadBytes": 52428800
}
```

## Benefícios da implementação

- evita hardcode de URLs e datasets no código;
- permite ajuste por ambiente (`Development`, produção, staging etc.);
- centraliza parâmetros de RDF em um único ponto;
- facilita futuras integrações com QLever e Fuseki;
- reduz a chance de erros de configuração em produção.

## Observações

- O backend continua estruturado como ASP.NET Core MVC/Controllers.
- A configuração foi adicionada sem mexer na lógica de endpoints existente.
- O projeto permanece em fase de planejamento e preparação para as etapas seguintes.

## Próximo passo recomendado

Implementar a etapa 2, que consiste em organizar a estrutura de camadas e registrar serviços de infraestrutura para Fuseki, QLever e RDF.
