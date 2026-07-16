# caio-matheus-dev

Curriculo web bilingue de Caio Matheus, com frontend em React/Vite e backend em ASP.NET Core.

## O que o projeto demonstra

- Curriculo e portfolio em portugues e ingles.
- API propria em ASP.NET Core com arquitetura separada por Application, Domain, Infrastructure e Presentation.
- Controllers finos chamando services, sem regra de negocio no `Program.cs`.
- Result Pattern com resposta padronizada.
- Integracao com GitHub API.
- Health checks publicos para projetos.
- Integration Lab com ViaCEP, GitHub user, NuGet search e DNS lookup.
- Cache em memoria e worker em background.
- Preparado para deploy gratuito do frontend e deploy da API no Render ou Azure.

## Arquitetura do backend

```text
backend/
  Application/
    Common/          Result Pattern e ApiResponse
    Interfaces/      Contratos dos services
    Services/        Casos de uso simples do portfolio
  Domain/
    Portfolio/       Modelos do curriculo e projetos
    Integrations/    Modelos de GitHub, status e ViaCEP
  Infrastructure/
    Data/            Dados estaticos do portfolio
    GitHub/          Cliente e mapeamento da API do GitHub
    Http/            ViaCEP e health checks
    Workers/         BackgroundService para atualizar cache
  Presentation/
    Controllers/     Controllers HTTP separados por recurso
```

## Rodando localmente

Backend:

```bash
cd backend
dotnet run
```

Frontend:

```bash
cd frontend
npm install
npm run dev
```

O frontend usa `VITE_API_BASE_URL`. Copie `frontend/.env.example` para `frontend/.env` se quiser apontar para outra API.

## Endpoints principais

```text
GET /api/profile
GET /api/projects
GET /api/skills
GET /api/experience
GET /api/github/repos
GET /api/status/projects
GET /api/lab/cep/{cep}
GET /api/lab/github/{username}
GET /api/lab/nuget?query=serilog
GET /api/lab/dns?domain=github.com&recordType=A
GET /api/lab/http-check?url=https://example.com
```

## Deploy sugerido

Frontend:

- Vercel
- Cloudflare Pages
- GitHub Pages

Backend:

- Render Web Service usando `backend/Dockerfile`
- Azure App Service Free F1

No frontend publicado, configure:

```text
VITE_API_BASE_URL=https://sua-api.onrender.com
```
