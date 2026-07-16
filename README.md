# caio-matheus-dev

Curriculo web bilingue de Caio Matheus, com frontend em React/Vite e backend em ASP.NET Core.

## O que o projeto demonstra

- Curriculo e portfolio em portugues e ingles.
- API propria em ASP.NET Core com arquitetura separada por Application, Domain, Infrastructure e Presentation.
- Controllers finos chamando services, sem regra de negocio no `Program.cs`.
- Result Pattern com resposta padronizada.
- Integracao com GitHub API.
- Integration Lab com ViaCEP, GitHub user, busca de anime, clima por estado e autenticacao JWT.
- Auth Lab com cadastro, login, JWT, senha com hash, Neon Postgres e recuperacao de senha por Gmail SMTP.
- Cache em memoria e worker em background.
- Preparado para deploy no Render usando `render.yaml`.

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
    Persistence/     EF Core, Neon Postgres e migrations
    Auth/            JWT, store em memoria e contratos de auth
    Email/           SMTP para recuperacao de senha
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
GET /api/lab/anime?query=naruto
GET /api/lab/weather?city=Sao%20Paulo&latitude=-23.5505&longitude=-46.6333
POST /api/auth/register
POST /api/auth/login
GET /api/auth/me
POST /api/auth/forgot-password
POST /api/auth/reset-password
```

## Deploy no Render

O repositorio possui `render.yaml` com dois servicos:

- `caio-matheus-dev-api`: API ASP.NET Core em Docker.
- `caio-matheus-dev-web`: frontend Vite como static site.

Variaveis secretas para preencher no Render:

```text
ConnectionStrings__DefaultConnection=postgresql://...
Email__Password=app-password-do-gmail
```

O Render gera automaticamente:

```text
AuthLab__Secret
```

O backend aceita connection string do Neon tanto no formato `postgresql://...` quanto no formato Npgsql `Host=...;Database=...`.
