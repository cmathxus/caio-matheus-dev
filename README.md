# caio-matheus-dev

Curriculo web bilingue de Caio Matheus, com frontend em React/Vite e backend em ASP.NET Core.

## O que o projeto demonstra

- Curriculo e portfolio em portugues e ingles.
- API propria em ASP.NET Core com arquitetura separada por Application, Domain, Infrastructure e Presentation.
- Controllers finos chamando services, sem regra de negocio no `Program.cs`.
- Result Pattern com resposta padronizada.
- Integracao com GitHub API.
- Integration Lab com ViaCEP, GitHub user, busca de anime, clima por estado e autenticacao JWT.
- Auth Lab com cadastro, login, JWT, senha com hash, Neon Postgres e recuperacao de senha por Resend API HTTP.
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
    Email/           Resend API e fallback SMTP para recuperacao de senha
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

- `caio-matheus-dev`: API ASP.NET Core em Docker.
- `caio-matheus-dev-web`: frontend Vite como static site.

Variaveis secretas para preencher no Render:

```text
ConnectionStrings__DefaultConnection=postgresql://...
Resend__ApiKey=re_...
```

O Render gera automaticamente:

```text
AuthLab__Secret
```

O backend aceita connection string do Neon tanto no formato `postgresql://...` quanto no formato Npgsql `Host=...;Database=...`.

## Email de recuperacao no Render Free

O Render Free bloqueia portas SMTP como `587`, entao o backend usa Resend via HTTP quando `Resend__ApiKey` esta configurado.
Sem `Resend__ApiKey`, ele ainda tenta o fallback SMTP para ambiente local ou hospedagens que liberam SMTP.

Variaveis para o Resend:

```text
Resend__ApiKey=re_...
Resend__From=Caio Matheus Dev <noreply@yahub.com.br>
Email__ResetGifUrl=https://cmathxus.github.io/caio-matheus-dev/kirito-reset.gif
```

Com o dominio `yahub.com.br` verificado no Resend, use:

```text
Caio Matheus Dev <noreply@yahub.com.br>
```

## Deploy do frontend no GitHub Pages

O workflow `.github/workflows/deploy-pages.yml` publica o frontend automaticamente a cada push na branch `main`.

No GitHub:

1. Abra `Settings` > `Pages`.
2. Em `Build and deployment`, selecione `Source: GitHub Actions`.
3. Abra `Settings` > `Secrets and variables` > `Actions` > `Variables`.
4. Crie a variavel `VITE_API_BASE_URL` com a URL publica da API no Render.
5. Faca push na `main` ou rode manualmente em `Actions` > `Deploy frontend to GitHub Pages`.

URL esperada do Pages:

```text
https://cmathxus.github.io/caio-matheus-dev/
```

## Deploy do backend no Render

Caminho recomendado para a API:

1. No Render, clique em `New` > `Web Service`.
2. Escolha `Git Provider` e conecte o repositorio `cmathxus/caio-matheus-dev`.
3. Configure:
   - Name: `caio-matheus-dev`
   - Branch: `main`
   - Root Directory: `backend`
   - Runtime: `Docker`
   - Instance Type: `Free`
4. Em Environment, preencha:
   - `ASPNETCORE_ENVIRONMENT`: `Production`
   - `AuthLab__Issuer`: `caio-matheus-dev`
   - `AuthLab__Audience`: `portfolio-auth-lab`
   - `AuthLab__Secret`: gere uma string longa e aleatoria
   - `AuthLab__FrontendBaseUrl`: `https://cmathxus.github.io/caio-matheus-dev`
   - `ConnectionStrings__DefaultConnection`: connection string do Neon
   - `Resend__ApiKey`: API key do Resend
   - `Resend__From`: `Caio Matheus Dev <noreply@yahub.com.br>`
   - `Email__ResetGifUrl`: `https://cmathxus.github.io/caio-matheus-dev/kirito-reset.gif`
5. Em `Auto-Deploy`, deixe `On Commit`.
6. Clique em `Create Web Service`.

Depois do deploy, teste:

```text
https://caio-matheus-dev.onrender.com/api/profile
```
