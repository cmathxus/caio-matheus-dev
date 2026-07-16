import { useEffect, useMemo, useRef, useState } from 'react'
import type { CSSProperties, FormEvent, MouseEvent, PointerEvent as ReactPointerEvent } from 'react'
import './App.css'

type Language = 'pt' | 'en'
type Page = 'home' | 'lab' | 'auth' | 'authReset' | 'certifications' | 'about'
type NavKey = 'home' | 'projects' | 'api' | 'lab' | 'certifications' | 'about'
type Theme = 'light' | 'dark'

type ApiResponse<T> = {
  success: boolean
  data: T | null
  error: { code: string; message: string } | null
  respondedAt: string
}

type Profile = {
  name: string
  role: string
  location: string
  summaryPt: string
  summaryEn: string
  gitHubUrl: string
  linkedInUrl: string
  highlights: string[]
}

type Project = {
  id: string
  name: string
  summaryPt: string
  summaryEn: string
  stack: string
  impactPt: string
  impactEn: string
  repositoryUrl: string
  production: boolean
}

type SkillGroup = {
  name: string
  items: string[]
}

type ExperienceItem = {
  id: string
  contextPt: string
  contextEn: string
  rolePt: string
  roleEn: string
  descriptionPt: string
  descriptionEn: string
  detailsPt: string[]
  detailsEn: string[]
}

type Certification = {
  id: string
  namePt: string
  nameEn: string
  issuer: string
  summaryPt: string
  summaryEn: string
  imageUrl: string
  credentialUrl: string
}

type CertificationLightbox = {
  src: string
  alt: string
  origin: {
    top: number
    left: number
    width: number
    height: number
  }
  target: {
    top: number
    left: number
    width: number
    height: number
  }
  closing: boolean
}

type GitHubRepository = {
  name: string
  htmlUrl: string
  description: string | null
  language: string | null
  stars: number
  forks: number
  updatedAt: string
}

type IntegrationEnvelope<T> = {
  items: T
  fromCache: boolean
  fetchedAt: string
}

type PortfolioData = {
  profile: Profile
  projects: Project[]
  skills: SkillGroup[]
  experience: ExperienceItem[]
}

type ConsoleEntry = {
  type: 'command' | 'output' | 'error'
  text: string
  id?: string
}

type AuthUserProfile = {
  id: string
  name: string
  email: string
  createdAt: string
}

type AuthSession = {
  accessToken: string
  tokenType: string
  expiresAt: string
  user: AuthUserProfile
}

type PasswordResetRequestResult = {
  message: string
  emailConfigured: boolean
  expiresAt: string | null
  developmentToken: string | null
  resetUrl: string | null
}

type PasswordResetConfirmation = {
  message: string
}

type BackendRoomNote = {
  id: string
  userId: string
  content: string
  createdAt: string
  updatedAt: string
}

type BackendRoomDrawing = {
  id: string
  userId: string
  name: string
  dataUrl: string
  createdAt: string
  updatedAt: string
}

type BackendRoomCommunityPost = {
  id: string
  userId: string
  authorName: string
  caption: string
  dataUrl: string
  createdAt: string
  expiresAt: string
  likesCount: number
  likedByCurrentUser: boolean
}

type BackendRoomSnapshot = {
  user: AuthUserProfile
  notes: BackendRoomNote[]
  drawing: BackendRoomDrawing | null
  communityPosts: BackendRoomCommunityPost[]
  loadedAt: string
}

type BackendRoomActionResult = {
  message: string
}

type BackendRoomLikeResult = {
  postId: string
  likesCount: number
  likedByCurrentUser: boolean
}

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5089'
const publicBaseUrl = import.meta.env.BASE_URL

const assetUrl = (path: string) => `${publicBaseUrl}${path.replace(/^\/+/, '')}`

const normalizePublicAssetUrl = (url: string) => {
  if (!url.startsWith('/')) {
    return url
  }

  return assetUrl(url)
}

const normalizeCertificationAssets = (certifications: Certification[]) =>
  certifications.map((certification) => ({
    ...certification,
    imageUrl: normalizePublicAssetUrl(certification.imageUrl),
  }))

const bootEntries: ConsoleEntry[] = [
  { type: 'output', text: 'Microsoft Windows [Portfolio Terminal]' },
  { type: 'output', text: 'Inicializando perfil de Caio Matheus...' },
  { type: 'output', text: 'Digite "help" para ver os comandos disponíveis.' },
]

const weatherStates = {
  AC: { label: 'Acre', capital: 'Rio Branco', latitude: -9.97499, longitude: -67.8243 },
  AL: { label: 'Alagoas', capital: 'Maceió', latitude: -9.66599, longitude: -35.735 },
  AP: { label: 'Amapá', capital: 'Macapá', latitude: 0.0349, longitude: -51.0694 },
  AM: { label: 'Amazonas', capital: 'Manaus', latitude: -3.119, longitude: -60.0217 },
  BA: { label: 'Bahia', capital: 'Salvador', latitude: -12.9777, longitude: -38.5016 },
  CE: { label: 'Ceará', capital: 'Fortaleza', latitude: -3.7319, longitude: -38.5267 },
  DF: { label: 'Distrito Federal', capital: 'Brasília', latitude: -15.7939, longitude: -47.8828 },
  ES: { label: 'Espírito Santo', capital: 'Vitória', latitude: -20.3155, longitude: -40.3128 },
  GO: { label: 'Goiás', capital: 'Goiânia', latitude: -16.6869, longitude: -49.2648 },
  MA: { label: 'Maranhão', capital: 'São Luís', latitude: -2.5307, longitude: -44.3068 },
  MT: { label: 'Mato Grosso', capital: 'Cuiabá', latitude: -15.6014, longitude: -56.0979 },
  MS: { label: 'Mato Grosso do Sul', capital: 'Campo Grande', latitude: -20.4697, longitude: -54.6201 },
  MG: { label: 'Minas Gerais', capital: 'Belo Horizonte', latitude: -19.9167, longitude: -43.9345 },
  PA: { label: 'Pará', capital: 'Belém', latitude: -1.4558, longitude: -48.4902 },
  PB: { label: 'Paraíba', capital: 'João Pessoa', latitude: -7.1195, longitude: -34.845 },
  PR: { label: 'Paraná', capital: 'Curitiba', latitude: -25.4284, longitude: -49.2733 },
  PE: { label: 'Pernambuco', capital: 'Recife', latitude: -8.0476, longitude: -34.877 },
  PI: { label: 'Piauí', capital: 'Teresina', latitude: -5.0892, longitude: -42.8019 },
  RJ: { label: 'Rio de Janeiro', capital: 'Rio de Janeiro', latitude: -22.9068, longitude: -43.1729 },
  RN: { label: 'Rio Grande do Norte', capital: 'Natal', latitude: -5.7945, longitude: -35.211 },
  RS: { label: 'Rio Grande do Sul', capital: 'Porto Alegre', latitude: -30.0346, longitude: -51.2177 },
  RO: { label: 'Rondônia', capital: 'Porto Velho', latitude: -8.7612, longitude: -63.9004 },
  RR: { label: 'Roraima', capital: 'Boa Vista', latitude: 2.8235, longitude: -60.6758 },
  SC: { label: 'Santa Catarina', capital: 'Florianópolis', latitude: -27.5949, longitude: -48.5482 },
  SP: { label: 'São Paulo', capital: 'São Paulo', latitude: -23.5505, longitude: -46.6333 },
  SE: { label: 'Sergipe', capital: 'Aracaju', latitude: -10.9472, longitude: -37.0731 },
  TO: { label: 'Tocantins', capital: 'Palmas', latitude: -10.184, longitude: -48.3336 },
} as const

type WeatherStateKey = keyof typeof weatherStates

const wait = (ms: number) => new Promise((resolve) => window.setTimeout(resolve, ms))

const fallbackData: PortfolioData = {
  profile: {
    name: 'Caio Matheus',
    role: 'Backend Developer C#/.NET',
    location: 'Guarulhos, SP',
    summaryPt:
      'Desenvolvedor back-end com foco em C#, .NET, ASP.NET Core e SQL, atuando na construção de APIs REST, aplicações internas e integrações entre sistemas corporativos.',
    summaryEn:
      'Back-end developer focused on C#, .NET, ASP.NET Core and SQL, building REST APIs, internal applications and integrations between corporate systems.',
    gitHubUrl: 'https://github.com/cmathxus',
    linkedInUrl: 'https://www.linkedin.com/in/caio-matheus-b68977236',
    highlights: ['C#', '.NET 8', 'ASP.NET Core', 'SQL', 'Azure', 'GitHub Actions'],
  },
  projects: [
    {
      id: 'idsupport',
      name: 'iDSupport / RmaWorker',
      summaryPt: 'Aplicação interna em produção na Control iD para apoiar fluxos de suporte, RMA e ordens de serviço.',
      summaryEn: 'Internal production application at Control iD supporting support, RMA and service order workflows.',
      stack: 'C#, TypeScript, CSS',
      impactPt: 'Reduziu a abertura de ordens de serviço de aproximadamente 10 minutos para cerca de 1 minuto.',
      impactEn: 'Reduced service order opening time from around 10 minutes to about 1 minute.',
      repositoryUrl: 'https://github.com/ya-labs/RmaWorker',
      production: true,
    },
    {
      id: 'rental-manager',
      name: 'Rental Manager',
      summaryPt: 'API solo para organizar imóveis, contratos, locatários e pagamentos.',
      summaryEn: 'Solo API for organizing properties, contracts, tenants, and payments.',
      stack: 'ASP.NET Core, .NET 9, PostgreSQL, DDD, Clean Architecture',
      impactPt: 'Projeto focado em modelagem de domínio, persistência e organização de camadas.',
      impactEn: 'Project focused on domain modeling, persistence, and layered architecture.',
      repositoryUrl: 'https://github.com/cmathxus/rental-manager',
      production: false,
    },
    {
      id: 'cade-o-dano',
      name: 'Cade o Dano',
      summaryPt: 'Aplicação com dados de League of Legends e integração com API externa.',
      summaryEn: 'Application with League of Legends data and external API integration.',
      stack: 'TypeScript, CSS, C#',
      impactPt: 'Backend usado para intermediar chamadas, proteger detalhes de integração e normalizar respostas.',
      impactEn: 'Backend used to intermediate calls, protect integration details, and normalize responses.',
      repositoryUrl: 'https://github.com/ya-labs/CADE-O-DANO',
      production: false,
    },
    {
      id: 'yahub',
      name: 'YAHub',
      summaryPt: 'Site oficial da YA LABS, usado como presença pública dos projetos.',
      summaryEn: 'Official YA LABS website, used as the public presence for projects.',
      stack: 'JavaScript, TypeScript',
      impactPt: 'Organização de produto, documentação visual e publicação em produção.',
      impactEn: 'Product organization, visual documentation, and production publishing.',
      repositoryUrl: 'https://github.com/ya-labs/YAHub',
      production: true,
    },
  ],
  skills: [
    { name: 'Backend', items: ['C#', '.NET 8', 'ASP.NET Core', 'APIs REST'] },
    { name: 'Architecture', items: ['Clean Architecture', 'DDD', 'DTOs', 'REST'] },
    { name: 'Data', items: ['PostgreSQL', 'MySQL', 'SQL', 'Modelagem', 'Cache'] },
    { name: 'Delivery', items: ['GitHub', 'GitHub Actions', 'Docker', 'Azure', 'Linux'] },
  ],
  experience: [
    {
      id: 'control-id',
      contextPt: 'Control iD',
      contextEn: 'Control iD',
      rolePt: 'Suporte técnico de sistemas + desenvolvimento interno',
      roleEn: 'Technical systems support + internal development',
      descriptionPt:
        'Desde 06/2024, atuando em suporte técnico especializado, diagnóstico de falhas e desenvolvimento de aplicações internas em produção.',
      descriptionEn:
        'Since 06/2024, working with specialized technical support, failure diagnosis, and production internal applications.',
      detailsPt: [
        'Automação de processos internos e redução de trabalho manual.',
        'Apoio técnico em IDSecure, IDSecure Cloud, bancos de dados e ambientes Linux.',
        'Integrações com APIs de ERP legado e fluxos corporativos reais.',
      ],
      detailsEn: [
        'Internal process automation and manual-work reduction.',
        'Technical support for IDSecure, IDSecure Cloud, databases and Linux environments.',
        'Integrations with legacy ERP APIs and real corporate workflows.',
      ],
    },
    {
      id: 'automation',
      contextPt: 'Operação e automação',
      contextEn: 'Operations and automation',
      rolePt: 'Integrações corporativas',
      roleEn: 'Corporate integrations',
      descriptionPt:
        'Automação de processos, consulta de notas fiscais, fluxos de RMA, integrações com ERP legado, VPN e Playwright.',
      descriptionEn:
        'Process automation, invoice lookup, RMA flows, legacy ERP integrations, VPN and Playwright.',
      detailsPt: [
        'iDSupport/RmaWorker em produção para apoiar fluxos de suporte e RMA.',
        'Abertura de ordem de serviço reduzida de aproximadamente 10 minutos para cerca de 1 minuto.',
        'Uso de C#, .NET 8, TypeScript, Playwright e GitHub Actions.',
      ],
      detailsEn: [
        'iDSupport/RmaWorker in production supporting support and RMA workflows.',
        'Service-order opening time reduced from around 10 minutes to about 1 minute.',
        'Use of C#, .NET 8, TypeScript, Playwright and GitHub Actions.',
      ],
    },
    {
      id: 'education',
      contextPt: 'Formação',
      contextEn: 'Education',
      rolePt: 'Análise e desenvolvimento de sistemas',
      roleEn: 'Systems analysis and development',
      descriptionPt: 'Centro Universitário ENIAC, conclusão em 12/2024. Certificação Microsoft Azure Fundamentals AZ-900.',
      descriptionEn: 'Centro Universitário ENIAC, completed in 12/2024. Microsoft Azure Fundamentals AZ-900 certification.',
      detailsPt: [
        'Base em desenvolvimento back-end, bancos de dados e arquitetura de aplicações.',
        'Inglês avançado para leitura técnica, documentação e comunicação profissional.',
        'Interesse atual em C#, ASP.NET Core, APIs REST, cloud e automações.',
      ],
      detailsEn: [
        'Foundation in back-end development, databases and application architecture.',
        'Advanced English for technical reading, documentation and professional communication.',
        'Current focus on C#, ASP.NET Core, REST APIs, cloud and automation.',
      ],
    },
  ],
}

const fallbackCertifications: Certification[] = [
  {
    id: 'az-900',
    namePt: 'Microsoft Certified: Azure Fundamentals (AZ-900)',
    nameEn: 'Microsoft Certified: Azure Fundamentals (AZ-900)',
    issuer: 'Microsoft',
    summaryPt: 'Certificação de fundamentos de cloud, serviços Azure, segurança, governança e modelos de cobrança.',
    summaryEn: 'Certification covering cloud fundamentals, Azure services, security, governance and pricing models.',
    imageUrl: assetUrl('certifications/microsoft-az-900.png'),
    credentialUrl:
      'https://learn.microsoft.com/pt-br/users/caiomatheusqueiroz-7788/credentials/94eaa0e05ee099e?ref=https%3A%2F%2Fwww.linkedin.com%2F',
  },
  {
    id: 'tivit-dotnet-copilot',
    namePt: 'TIVIT - .NET com GitHub Copilot',
    nameEn: 'TIVIT - .NET with GitHub Copilot',
    issuer: 'TIVIT',
    summaryPt: 'Bootcamp focado em desenvolvimento .NET com uso de GitHub Copilot no fluxo de construção de aplicações.',
    summaryEn: 'Bootcamp focused on .NET development using GitHub Copilot in the application-building workflow.',
    imageUrl: assetUrl('certifications/tivit-dotnet-copilot.png'),
    credentialUrl:
      'https://www.linkedin.com/posts/caio-matheus-b68977236_finalizei-hoje-o-bootcamp-da-dio-tivit-activity-7399581791495512064-x5M4?utm_source=social_share_send&utm_medium=member_desktop_web&rcm=ACoAADrgOMABjnHamE70unLi5JMtxUjA3G1Ooyo',
  },
]

const copy = {
  pt: {
    navHome: 'Início',
    navProjects: 'Projetos',
    navApi: 'API',
    navLab: 'Lab',
    navCertifications: 'Certificações',
    themeToLight: 'Alternar para tema claro',
    themeToDark: 'Alternar para tema escuro',
    availability: 'Backend C#/.NET',
    heroTitle: 'Caio Matheus',
    heroRole: 'APIs, integrações e automações para sistemas internos.',
    heroText:
      'Hoje na Control iD, desenvolvo soluções para automação de processos, fluxos de RMA, consulta de notas fiscais e integração com APIs de ERP legado.',
    github: 'GitHub',
    linkedin: 'LinkedIn',
    projectsEyebrow: 'Selecionados',
    projectsTitle: 'Projetos com entrega real',
    projectsText: 'Projetos onde dá para ver backend, integração, produto interno e organização de código.',
    production: 'Produção',
    repository: 'Repositório',
    viewJson: 'Ver JSON',
    apiTitle: 'API Explorer',
    apiText: 'Alguns dados desta página vêm da API ASP.NET Core criada para o portfólio.',
    apiNoteTitle: 'JSON sob demanda',
    apiNoteText: 'Clique em um endpoint para abrir a resposta real da API, sem sair da página.',
    contextTitle: 'Trajetória',
    experienceTitle: 'Experiência profissional',
    stackTitle: 'Stack',
    skillsTitle: 'Competências técnicas',
    labIntro: 'Integration Lab',
    labText: 'Integrações públicas e gratuitas para demonstrar consumo, normalização e resposta padronizada.',
    openLab: 'Abrir Lab',
    cepTitle: 'CEP',
    cepHint: 'ViaCEP: normaliza CEP e retorna endereço.',
    gitHubUserTitle: 'GitHub User',
    gitHubUserHint: 'Busca um perfil público do GitHub.',
    animeTitle: 'Busca de anime',
    animeHint: 'Kitsu: busca animes por nome sem autenticação.',
    weatherTitle: 'Clima',
    weatherHint: 'Open-Meteo: consulta clima atual pela capital do estado.',
    authTitle: 'Auth JWT',
    authHint: 'Registro, login, token assinado e endpoint protegido em ASP.NET Core.',
    authOpen: 'Abrir Auth Lab',
    authPageTitle: 'Auth Lab',
    authPageText: 'Fluxo simples para demonstrar registro, login, geração de JWT e consumo de rota protegida com Bearer token.',
    authMemoryNote: 'Usuários persistidos em Neon Postgres, com senha protegida por hash e token JWT.',
    authRegister: 'Registrar',
    authLogin: 'Login',
    authValidate: 'Validar token',
    authName: 'Nome',
    authEmail: 'E-mail',
    authPassword: 'Senha',
    authForgot: 'Esqueci a senha',
    authReset: 'Redefinir',
    authNewPassword: 'Nova senha',
    authConfirmPassword: 'Confirmar senha',
    authResetToken: 'Token de recuperação',
    authRecoveryHint: 'O link de recuperação é enviado para o e-mail preenchido no login.',
    authResetPageTitle: 'Nova senha',
    authResetPageText: 'Informe uma nova senha. O email e o token já vieram pelo link de recuperação.',
    authPasswordMismatch: 'As senhas não conferem.',
    authResetSuccess: 'Senha redefinida e login efetuado.',
    authToken: 'Token JWT',
    authProtected: 'Rota protegida',
    cepPlaceholder: '07090000',
    githubPlaceholder: 'cmathxus',
    animePlaceholder: 'Fullmetal Alchemist',
    stateLabel: 'Estado',
    run: 'Executar',
    close: 'Fechar',
    response: 'Resposta',
    endpoint: 'Endpoint',
    details: 'Detalhes',
    projectAction: 'Abrir repositório',
    certificationTitle: 'Certificações',
    certificationText: 'Credenciais e estudos que reforçam minha base em cloud e backend.',
    openCredential: 'Abrir credencial',
  },
  en: {
    navHome: 'Home',
    navProjects: 'Projects',
    navApi: 'API',
    navLab: 'Lab',
    navCertifications: 'Certifications',
    themeToLight: 'Switch to light theme',
    themeToDark: 'Switch to dark theme',
    availability: 'C#/.NET Backend',
    heroTitle: 'Caio Matheus',
    heroRole: 'APIs, integrations and automation for internal systems.',
    heroText:
      'At Control iD, I build solutions for process automation, RMA flows, invoice lookup and integrations with legacy ERP APIs.',
    github: 'GitHub',
    linkedin: 'LinkedIn',
    projectsEyebrow: 'Selected',
    projectsTitle: 'Projects with real delivery',
    projectsText: 'Projects that show backend, integrations, internal products and code organization.',
    production: 'Production',
    repository: 'Repository',
    viewJson: 'View JSON',
    apiTitle: 'API Explorer',
    apiText: 'Some data on this page comes from the ASP.NET Core API built for the portfolio.',
    apiNoteTitle: 'JSON on demand',
    apiNoteText: 'Click an endpoint to open the real API response without leaving the page.',
    contextTitle: 'Trajectory',
    experienceTitle: 'Professional experience',
    stackTitle: 'Stack',
    skillsTitle: 'Technical skills',
    labIntro: 'Integration Lab',
    labText: 'Free public integrations that demonstrate consumption, normalization and standardized responses.',
    openLab: 'Open Lab',
    cepTitle: 'ZIP',
    cepHint: 'ViaCEP: normalizes Brazilian ZIP and returns address data.',
    gitHubUserTitle: 'GitHub User',
    gitHubUserHint: 'Fetches a public GitHub profile.',
    animeTitle: 'Anime search',
    animeHint: 'Kitsu: searches anime by name without authentication.',
    weatherTitle: 'Weather',
    weatherHint: 'Open-Meteo: fetches current weather using the state capital.',
    authTitle: 'JWT Auth',
    authHint: 'Register, login, signed token and protected ASP.NET Core endpoint.',
    authOpen: 'Open Auth Lab',
    authPageTitle: 'Auth Lab',
    authPageText: 'Simple flow demonstrating registration, login, JWT generation and protected route consumption with a Bearer token.',
    authMemoryNote: 'Users are persisted in Neon Postgres, with hashed passwords and JWT tokens.',
    authRegister: 'Register',
    authLogin: 'Login',
    authValidate: 'Validate token',
    authName: 'Name',
    authEmail: 'Email',
    authPassword: 'Password',
    authForgot: 'Forgot password',
    authReset: 'Reset',
    authNewPassword: 'New password',
    authConfirmPassword: 'Confirm password',
    authResetToken: 'Recovery token',
    authRecoveryHint: 'The recovery link is sent to the email filled in the login form.',
    authResetPageTitle: 'New password',
    authResetPageText: 'Set a new password. The email and token came from the recovery link.',
    authPasswordMismatch: 'Passwords do not match.',
    authResetSuccess: 'Password reset and login completed.',
    authToken: 'JWT token',
    authProtected: 'Protected route',
    cepPlaceholder: '07090000',
    githubPlaceholder: 'cmathxus',
    animePlaceholder: 'Fullmetal Alchemist',
    stateLabel: 'State',
    run: 'Run',
    close: 'Close',
    response: 'Response',
    endpoint: 'Endpoint',
    details: 'Details',
    projectAction: 'Open repository',
    certificationTitle: 'Certifications',
    certificationText: 'Credentials and studies that strengthen my cloud and backend foundation.',
    openCredential: 'Open credential',
  },
}

const endpointOptions = [
  { label: 'Profile', path: '/api/profile' },
  { label: 'Projects', path: '/api/projects' },
  { label: 'Experience', path: '/api/experience' },
  { label: 'GitHub repos', path: '/api/github/repos' },
]

const authSessionStorageKey = 'caio-matheus-dev-auth-session'
const authPageActiveStorageKey = 'caio-matheus-dev-auth-page-active'

function getStoredAuthSession(): AuthSession | null {
  try {
    const rawSession = localStorage.getItem(authSessionStorageKey)

    if (!rawSession) {
      return null
    }

    const session = JSON.parse(rawSession) as AuthSession
    const expiresAt = new Date(session.expiresAt).getTime()

    if (!session.accessToken || !session.user?.id || Number.isNaN(expiresAt) || expiresAt <= Date.now()) {
      clearStoredAuthSession()
      return null
    }

    return session
  } catch {
    clearStoredAuthSession()
    return null
  }
}

function saveAuthSession(session: AuthSession) {
  localStorage.setItem(authSessionStorageKey, JSON.stringify(session))
  sessionStorage.setItem(authPageActiveStorageKey, 'true')
}

function clearStoredAuthSession() {
  localStorage.removeItem(authSessionStorageKey)
  sessionStorage.removeItem(authPageActiveStorageKey)
}

function getInitialPage(): Page {
  const searchParams = new URLSearchParams(window.location.search)

  if (searchParams.has('authResetToken')) {
    return 'authReset'
  }

  if (sessionStorage.getItem(authPageActiveStorageKey) === 'true' && getStoredAuthSession()) {
    return 'auth'
  }

  return 'home'
}

async function fetchApi<T>(path: string): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`)
  const payload = (await response.json()) as ApiResponse<T>

  if (!response.ok || !payload.success || payload.data === null) {
    throw new Error(payload.error?.message ?? `Request failed: ${response.status}`)
  }

  return payload.data
}

async function fetchRaw(path: string): Promise<unknown> {
  const response = await fetch(`${apiBaseUrl}${path}`)
  return response.json()
}

async function postApi<T>(path: string, body: unknown): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
  const payload = (await response.json()) as ApiResponse<T>

  if (!response.ok || !payload.success || payload.data === null) {
    throw new Error(payload.error?.message ?? `Request failed: ${response.status}`)
  }

  return payload.data
}

async function getProtectedApi<T>(path: string, token: string): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    headers: { Authorization: `Bearer ${token}` },
  })
  const payload = (await response.json()) as ApiResponse<T>

  if (!response.ok || !payload.success || payload.data === null) {
    throw new Error(payload.error?.message ?? `Request failed: ${response.status}`)
  }

  return payload.data
}

async function sendProtectedApi<T>(
  path: string,
  token: string,
  method: 'POST' | 'PUT' | 'DELETE',
  body?: unknown,
): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    method,
    headers: {
      Authorization: `Bearer ${token}`,
      ...(body === undefined ? {} : { 'Content-Type': 'application/json' }),
    },
    body: body === undefined ? undefined : JSON.stringify(body),
  })
  const payload = (await response.json()) as ApiResponse<T>

  if (!response.ok || !payload.success || payload.data === null) {
    throw new Error(payload.error?.message ?? `Request failed: ${response.status}`)
  }

  return payload.data
}

function getInitialTheme(): Theme {
  const stored = localStorage.getItem('theme')
  if (stored === 'light' || stored === 'dark') {
    return stored
  }

  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
}

function App() {
  const [language, setLanguage] = useState<Language>('pt')
  const [theme, setTheme] = useState<Theme>(getInitialTheme)
  const [page, setPage] = useState<Page>(getInitialPage)
  const [activeNav, setActiveNav] = useState<NavKey>('home')
  const [data, setData] = useState<PortfolioData>(fallbackData)
  const [certifications, setCertifications] = useState<Certification[]>(fallbackCertifications)
  const [repos, setRepos] = useState<GitHubRepository[]>([])
  const [jsonPreview, setJsonPreview] = useState<{ title: string; body: unknown } | null>(null)
  const [cep, setCep] = useState('07090000')
  const [githubUser, setGithubUser] = useState('cmathxus')
  const [animeQuery, setAnimeQuery] = useState('Fullmetal Alchemist')
  const [weatherState, setWeatherState] = useState<WeatherStateKey>('SP')
  const [labResult, setLabResult] = useState<unknown>(null)
  const navRef = useRef<HTMLElement | null>(null)
  const navButtonRefs = useRef<Partial<Record<NavKey, HTMLButtonElement | null>>>({})
  const pendingScrollTarget = useRef<NavKey | null>(null)
  const [navIndicatorStyle, setNavIndicatorStyle] = useState<CSSProperties>({ opacity: 0 })

  const t = copy[language]

  useEffect(() => {
    const searchParams = new URLSearchParams(window.location.search)

    if (searchParams.has('authResetToken')) {
      pendingScrollTarget.current = null
      setActiveNav('lab')
      setPage('authReset')
      window.setTimeout(() => window.scrollTo({ top: 0, behavior: 'smooth' }), 0)
    }
  }, [])

  useEffect(() => {
    if (page === 'auth' || page === 'authReset') {
      sessionStorage.setItem(authPageActiveStorageKey, 'true')
      return
    }

    sessionStorage.removeItem(authPageActiveStorageKey)
  }, [page])

  useEffect(() => {
    document.documentElement.dataset.theme = theme
    document.documentElement.style.colorScheme = theme
    localStorage.setItem('theme', theme)
  }, [theme])

  useEffect(() => {
    if (page !== 'home') {
      pendingScrollTarget.current = null
      setActiveNav(page === 'auth' || page === 'authReset' ? 'lab' : page)
      return
    }

    const updateActiveSection = () => {
      if (pendingScrollTarget.current) {
        setActiveNav(pendingScrollTarget.current)
        return
      }

      const scrollMarker = window.scrollY + 150
      const current = (['projects', 'api'] as const).reduce<NavKey>((active, sectionId) => {
        const section = document.getElementById(sectionId)
        return section && section.offsetTop <= scrollMarker ? sectionId : active
      }, 'home')

      setActiveNav(current)
    }

    updateActiveSection()
    window.addEventListener('scroll', updateActiveSection, { passive: true })
    window.addEventListener('resize', updateActiveSection)

    return () => {
      window.removeEventListener('scroll', updateActiveSection)
      window.removeEventListener('resize', updateActiveSection)
    }
  }, [page])

  useEffect(() => {
    const updateIndicator = () => {
      const activeButton = navButtonRefs.current[activeNav]

      if (!navRef.current || !activeButton) {
        setNavIndicatorStyle({ opacity: 0 })
        return
      }

      setNavIndicatorStyle({
        width: activeButton.offsetWidth,
        transform: `translateX(${activeButton.offsetLeft}px)`,
        opacity: 1,
      })
    }

    const frameId = window.requestAnimationFrame(updateIndicator)
    window.addEventListener('resize', updateIndicator)

    return () => {
      window.cancelAnimationFrame(frameId)
      window.removeEventListener('resize', updateIndicator)
    }
  }, [activeNav, language, page])

  useEffect(() => {
    let cancelled = false

    async function loadPortfolio() {
      try {
        const [profile, projects, skills, experience, certificationsData, repoEnvelope] =
          await Promise.all([
            fetchApi<Profile>('/api/profile'),
            fetchApi<Project[]>('/api/projects'),
            fetchApi<SkillGroup[]>('/api/skills'),
            fetchApi<ExperienceItem[]>('/api/experience'),
            fetchApi<Certification[]>('/api/certifications'),
            fetchApi<IntegrationEnvelope<GitHubRepository[]>>('/api/github/repos'),
          ])

        if (!cancelled) {
          setData({ profile, projects, skills, experience })
          setCertifications(normalizeCertificationAssets(certificationsData))
          setRepos(repoEnvelope.items)
        }
      } catch {
        if (!cancelled) {
          setData(fallbackData)
        }
      }
    }

    loadPortfolio()

    return () => {
      cancelled = true
    }
  }, [])

  function navigateHome(sectionId?: string) {
    const target = (sectionId as NavKey | undefined) ?? 'home'
    pendingScrollTarget.current = target
    setActiveNav(target)
    setPage('home')

    window.setTimeout(() => {
      if (pendingScrollTarget.current === target) {
        pendingScrollTarget.current = null
      }
    }, 900)

    if (sectionId) {
      window.setTimeout(() => {
        document.getElementById(sectionId)?.scrollIntoView({ behavior: 'smooth', block: 'start' })
      }, 0)
    } else {
      window.scrollTo({ top: 0, behavior: 'smooth' })
    }
  }

  function navigatePage(nextPage: Extract<Page, 'lab' | 'certifications' | 'about'>) {
    pendingScrollTarget.current = null
    setActiveNav(nextPage)
    setPage(nextPage)
    window.setTimeout(() => window.scrollTo({ top: 0, behavior: 'smooth' }), 0)
  }

  function navigateAuthLab() {
    pendingScrollTarget.current = null
    setActiveNav('lab')
    setPage('auth')
    window.setTimeout(() => window.scrollTo({ top: 0, behavior: 'smooth' }), 0)
  }

  function setNavButtonRef(key: NavKey) {
    return (node: HTMLButtonElement | null) => {
      navButtonRefs.current[key] = node
    }
  }

  async function showJson(title: string, path: string) {
    try {
      setJsonPreview({ title, body: await fetchRaw(path) })
    } catch {
      setJsonPreview({
        title,
        body: { success: false, error: { code: 'offline', message: 'API unavailable.' } },
      })
    }
  }

  async function runLab(path: string) {
    try {
      setLabResult(await fetchRaw(path))
    } catch {
      setLabResult({ success: false, error: { code: 'offline', message: 'API unavailable.' } })
    }
  }

  return (
    <main className="site-shell">
      <header className="topbar">
        <button className="brand-lockup" type="button" onClick={() => navigateHome()} aria-label="Caio Matheus">
          <strong>Caio Matheus</strong>
        </button>

        <nav className="main-nav" ref={navRef} aria-label="Primary navigation">
          <span className="nav-indicator" style={navIndicatorStyle} aria-hidden="true" />
          <button
            ref={setNavButtonRef('home')}
            className={activeNav === 'home' ? 'active' : ''}
            type="button"
            aria-current={activeNav === 'home' ? 'page' : undefined}
            onClick={() => navigateHome()}
          >
            {t.navHome}
          </button>
          <button
            ref={setNavButtonRef('projects')}
            className={activeNav === 'projects' ? 'active' : ''}
            type="button"
            aria-current={activeNav === 'projects' ? 'page' : undefined}
            onClick={() => navigateHome('projects')}
          >
            {t.navProjects}
          </button>
          <button
            ref={setNavButtonRef('api')}
            className={activeNav === 'api' ? 'active' : ''}
            type="button"
            aria-current={activeNav === 'api' ? 'page' : undefined}
            onClick={() => navigateHome('api')}
          >
            {t.navApi}
          </button>
          <button
            ref={setNavButtonRef('certifications')}
            className={activeNav === 'certifications' ? 'active' : ''}
            type="button"
            aria-current={activeNav === 'certifications' ? 'page' : undefined}
            onClick={() => navigatePage('certifications')}
          >
            {t.navCertifications}
          </button>
          <button
            ref={setNavButtonRef('about')}
            className={`nav-help ${activeNav === 'about' ? 'active' : ''}`}
            type="button"
            aria-current={activeNav === 'about' ? 'page' : undefined}
            aria-label={language === 'pt' ? 'Como este site foi feito' : 'How this site was built'}
            title={language === 'pt' ? 'Como este site foi feito' : 'How this site was built'}
            onClick={() => navigatePage('about')}
          >
            ?
          </button>
          <button
            ref={setNavButtonRef('lab')}
            className={`nav-lab ${activeNav === 'lab' ? 'active' : ''}`}
            type="button"
            aria-current={activeNav === 'lab' ? 'page' : undefined}
            onClick={() => navigatePage('lab')}
          >
            {t.navLab}
          </button>
        </nav>

        <div className="toolbar" aria-label="Preferences">
          <button
            className={`account-shortcut ${page === 'auth' || page === 'authReset' ? 'active' : ''}`}
            type="button"
            onClick={navigateAuthLab}
            aria-label={t.authOpen}
            title={t.authOpen}
          >
            <span className="account-glyph" aria-hidden="true" />
          </button>
          <div
            className={`segmented-control language-switch ${language === 'en' ? 'is-en' : 'is-pt'}`}
            aria-label="Language selector"
          >
            <span className="language-switch-indicator" aria-hidden="true" />
            <button className={language === 'pt' ? 'active' : ''} type="button" onClick={() => setLanguage('pt')}>
              PT
            </button>
            <button className={language === 'en' ? 'active' : ''} type="button" onClick={() => setLanguage('en')}>
              EN
            </button>
          </div>
          <button
            className="theme-toggle"
            type="button"
            onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')}
            aria-label={theme === 'dark' ? t.themeToLight : t.themeToDark}
          >
            <span className="theme-toggle-track" aria-hidden="true">
              <span className="sun-icon"></span>
              <span className="moon-icon"></span>
            </span>
          </button>
        </div>
      </header>

      {page === 'home' ? (
        <>
          <section className="hero">
            <div className="hero-copy">
              <p className="eyebrow">{t.availability}</p>
              <h1>{t.heroTitle}</h1>
              <p className="role-line">{t.heroRole}</p>
              <p className="lead">{language === 'pt' ? data.profile.summaryPt : data.profile.summaryEn}</p>
              <p>{t.heroText}</p>
              <div className="actions">
                <a className="button primary" href={data.profile.gitHubUrl} target="_blank" rel="noreferrer">{t.github}</a>
                <a className="button" href={data.profile.linkedInUrl} target="_blank" rel="noreferrer">{t.linkedin}</a>
              </div>
            </div>

            <InteractiveConsole
              data={data}
              language={language}
              repos={repos}
              setPage={setPage}
              showJson={showJson}
            />
          </section>

          <section className="capability-strip" aria-label="Highlights">
            {data.profile.highlights.map((highlight) => (
              <span key={highlight}>{highlight}</span>
            ))}
          </section>

          <section className="section" id="projects">
            <SectionHeading eyebrow={t.projectsEyebrow} title={t.projectsTitle} text={t.projectsText} />

            <div className="project-showcase">
              {data.projects.map((project, index) => (
                <article className="project-card" key={project.id}>
                  <span className="project-index">{String(index + 1).padStart(2, '0')}</span>
                  <div className="card-head">
                    <div>
                      <h3>{project.name}</h3>
                      {project.production && <span>{t.production}</span>}
                    </div>
                    <a className="text-link" href={project.repositoryUrl} target="_blank" rel="noreferrer">
                      {t.projectAction}
                    </a>
                  </div>
                  <p>{language === 'pt' ? project.summaryPt : project.summaryEn}</p>
                  <p className="project-impact">{language === 'pt' ? project.impactPt : project.impactEn}</p>
                  <div className="stack-list">
                    {project.stack.split(',').map((item) => (
                      <span key={`${project.id}-${item.trim()}`}>{item.trim()}</span>
                    ))}
                  </div>
                </article>
              ))}
            </div>
          </section>

          <section className="section" id="api">
            <SectionHeading eyebrow={t.navApi} title={t.apiTitle} text={t.apiText} />

            <div className="api-grid compact-api">
              <article className="endpoint-panel">
                <div className="panel-title">
                  <span>{t.endpoint}</span>
                  <code>{apiBaseUrl}</code>
                </div>
                <div className="endpoint-list">
                  {endpointOptions.map((endpoint) => (
                    <button
                      type="button"
                      key={endpoint.path}
                      onClick={() => showJson(`GET ${endpoint.path}`, endpoint.path)}
                    >
                      <span>GET</span>
                      <strong>{endpoint.path}</strong>
                    </button>
                  ))}
                </div>
              </article>

              <article className="api-note">
                <h3>{t.apiNoteTitle}</h3>
                <p>{t.apiNoteText}</p>
              </article>
            </div>
          </section>

          <section className="section details-grid">
            <div>
              <SectionHeading eyebrow={t.contextTitle} title={t.experienceTitle} />
              <ExperienceList items={data.experience} language={language} />
            </div>

            <div>
              <SectionHeading eyebrow={t.stackTitle} title={t.skillsTitle} />
              <div className="skills-grid">
                {data.skills.map((group) => (
                  <article className="skill-card" key={group.name}>
                    <h3>{group.name}</h3>
                    <div>
                      {group.items.filter((item) => item.toLowerCase() !== 'result pattern').map((item) => (
                        <span key={item}>{item}</span>
                      ))}
                    </div>
                  </article>
                ))}
              </div>
            </div>
          </section>

          <section className="lab-teaser">
            <div>
              <p className="eyebrow">{t.labIntro}</p>
              <h2>{t.labText}</h2>
            </div>
            <button className="button primary" type="button" onClick={() => setPage('lab')}>{t.openLab}</button>
          </section>
        </>
      ) : page === 'lab' ? (
        <LabPage
          t={t}
          cep={cep}
          githubUser={githubUser}
          animeQuery={animeQuery}
          weatherState={weatherState}
          labResult={labResult}
          setCep={setCep}
          setGithubUser={setGithubUser}
          setAnimeQuery={setAnimeQuery}
          setWeatherState={setWeatherState}
          runLab={runLab}
          setPage={setPage}
        />
      ) : page === 'auth' ? (
        <AuthLabPage t={t} language={language} />
      ) : page === 'authReset' ? (
        <AuthResetPage t={t} language={language} />
      ) : page === 'about' ? (
        <AboutSitePage language={language} />
      ) : (
        <CertificationsPage
          t={t}
          language={language}
          certifications={certifications}
        />
      )}

      {jsonPreview && (
        <div className="dialog-backdrop" role="presentation" onMouseDown={() => setJsonPreview(null)}>
          <section className="json-dialog" role="dialog" aria-modal="true" aria-label={jsonPreview.title} onMouseDown={(event) => event.stopPropagation()}>
            <div className="panel-title">
              <h3>{jsonPreview.title}</h3>
              <button type="button" onClick={() => setJsonPreview(null)}>{t.close}</button>
            </div>
            <pre>{JSON.stringify(jsonPreview.body, null, 2)}</pre>
          </section>
        </div>
      )}
    </main>
  )
}

function AuthLabPage({ t, language }: { t: typeof copy.pt; language: Language }) {
  const [mode, setMode] = useState<'register' | 'login'>('register')
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [session, setSession] = useState<AuthSession | null>(getStoredAuthSession)
  const [authResult, setAuthResult] = useState<unknown>({ waiting: true })
  const [loading, setLoading] = useState(false)
  const [forgotLoading, setForgotLoading] = useState(false)

  useEffect(() => {
    if (!session) {
      return
    }

    const timeLeft = new Date(session.expiresAt).getTime() - Date.now()

    if (timeLeft <= 0) {
      clearStoredAuthSession()
      setSession(null)
      setAuthResult({ waiting: true })
      return
    }

    const timeoutId = window.setTimeout(() => {
      clearStoredAuthSession()
      setSession(null)
      setAuthResult({
        success: false,
        error: language === 'pt' ? 'Sessão expirada. Faça login novamente.' : 'Session expired. Please log in again.',
      })
    }, Math.min(timeLeft, 2_147_483_647))

    return () => window.clearTimeout(timeoutId)
  }, [language, session])

  async function submitAuth(event: FormEvent) {
    event.preventDefault()
    setLoading(true)

    try {
      const result = mode === 'register'
        ? await postApi<AuthSession>('/api/auth/register', { name, email, password })
        : await postApi<AuthSession>('/api/auth/login', { email, password })

      setSession(result)
      saveAuthSession(result)
      setAuthResult({
        success: true,
        flow: mode,
        user: result.user,
        tokenType: result.tokenType,
        expiresAt: result.expiresAt,
        preview: `${result.accessToken.slice(0, 42)}...`,
      })
    } catch (error) {
      setAuthResult({
        success: false,
        error: error instanceof Error ? error.message : 'Request failed',
      })
    } finally {
      setLoading(false)
    }
  }

  async function requestPasswordReset() {
    setForgotLoading(true)

    try {
      const result = await postApi<PasswordResetRequestResult>('/api/auth/forgot-password', { email })
      setAuthResult({
        success: true,
        flow: 'forgot-password',
        email,
        message: result.message,
        emailConfigured: result.emailConfigured,
        expiresAt: result.expiresAt,
        resetUrl: result.emailConfigured ? undefined : result.resetUrl,
      })
    } catch (error) {
      setAuthResult({
        success: false,
        error: error instanceof Error ? error.message : 'Request failed',
      })
    } finally {
      setForgotLoading(false)
    }
  }

  if (session) {
    return (
      <BackendRoom
        session={session}
        language={language}
        onLogout={() => {
          clearStoredAuthSession()
          setSession(null)
          setPassword('')
          setAuthResult({ waiting: true })
        }}
      />
    )
  }

  return (
    <section className="lab-page auth-page">
      <SectionHeading title={t.authPageTitle} text={t.authPageText} />

      <div className="auth-grid">
        <article className="auth-panel">
          <div className="auth-tabs" aria-label="Auth mode">
            <button
              className={mode === 'register' ? 'active' : ''}
              type="button"
              onClick={() => setMode('register')}
            >
              {t.authRegister}
            </button>
            <button
              className={mode === 'login' ? 'active' : ''}
              type="button"
              onClick={() => setMode('login')}
            >
              {t.authLogin}
            </button>
          </div>

          <form className="auth-form" onSubmit={submitAuth}>
            {mode === 'register' && (
              <label>
                {t.authName}
                <input
                  value={name}
                  onChange={(event) => setName(event.target.value)}
                  autoComplete="name"
                  placeholder="Sasuke Uchiha"
                />
              </label>
            )}

            <label>
              {t.authEmail}
              <input
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                autoComplete="email"
                placeholder="sasuke@email.com"
              />
            </label>

            <PasswordField
              label={t.authPassword}
              value={password}
              onChange={setPassword}
              autoComplete={mode === 'register' ? 'new-password' : 'current-password'}
            />

            <div className="auth-actions-row">
              <button className="button primary" type="submit" disabled={loading}>
                {loading ? '...' : mode === 'register' ? t.authRegister : t.authLogin}
              </button>
              {mode === 'login' && (
                <button
                  className="auth-forgot-link"
                  type="button"
                  disabled={forgotLoading}
                  onClick={requestPasswordReset}
                >
                  {forgotLoading ? '...' : t.authForgot}
                </button>
              )}
            </div>
          </form>

          <p className="auth-note">{t.authMemoryNote}</p>
          {mode === 'login' && <p className="auth-note">{t.authRecoveryHint}</p>}
        </article>

        <article className="auth-panel auth-token-panel">
          <div className="panel-title">
            <h3>Backend Room</h3>
            <span>JWT</span>
          </div>

          <pre className="auth-token">
            {language === 'pt'
              ? 'Depois do login, você entra em uma área protegida com notas privadas, canvas pessoal e chamadas autenticadas.'
              : 'After login, you enter a protected area with private notes, personal canvas and authenticated requests.'}
          </pre>

          <button className="button" type="button" disabled>
            {language === 'pt' ? 'Faça login para abrir' : 'Login to open'}
          </button>
        </article>
      </div>

      <article className="lab-response auth-response">
        <div className="panel-title">
          <h3>{t.authProtected}</h3>
          <code>GET /api/auth/me</code>
        </div>
        <pre>{JSON.stringify(authResult, null, 2)}</pre>
      </article>
    </section>
  )
}

function AboutSitePage({ language }: { language: Language }) {
  const isPt = language === 'pt'
  const statusText = `status: ${isPt ? 'em produção' : 'in production'}
api: ASP.NET Core
auth: JWT + password hash
db: Neon PostgreSQL
email: Resend
deploy: GitHub Pages + Render`
  const [typedStatus, setTypedStatus] = useState('')

  useEffect(() => {
    let index = 0
    setTypedStatus('')

    const intervalId = window.setInterval(() => {
      index += 1
      setTypedStatus(statusText.slice(0, index))

      if (index >= statusText.length) {
        window.clearInterval(intervalId)
      }
    }, 18)

    return () => window.clearInterval(intervalId)
  }, [statusText])

  const workflow = [
    {
      number: '01',
      title: 'Front-end',
      text: isPt
        ? 'React, TypeScript e Vite montam a experiência pública, tema claro/escuro, console interativo e telas do Auth Lab.'
        : 'React, TypeScript and Vite power the public experience, light/dark themes, interactive console and Auth Lab screens.',
      command: 'npm run build',
    },
    {
      number: '02',
      title: 'API',
      text: isPt
        ? 'ASP.NET Core em C# organiza controllers, services, integrações públicas, respostas padronizadas e validações.'
        : 'ASP.NET Core in C# organizes controllers, services, public integrations, standardized responses and validation.',
      command: 'dotnet run',
    },
    {
      number: '03',
      title: isPt ? 'Autenticação' : 'Authentication',
      text: isPt
        ? 'JWT protege o Backend Room. O login fica salvo no navegador até o token expirar, sem perder sessão no F5.'
        : 'JWT protects the Backend Room. Login persists in the browser until the token expires, surviving refreshes.',
      command: 'Authorization: Bearer',
    },
    {
      number: '04',
      title: isPt ? 'Persistência' : 'Persistence',
      text: isPt
        ? 'Neon Postgres + Entity Framework Core salvam usuários, notas, canvas, posts da comunidade, likes e expiração de 24h.'
        : 'Neon Postgres + Entity Framework Core store users, notes, canvas data, community posts, likes and 24h expiration.',
      command: 'ef migrations',
    },
    {
      number: '05',
      title: isPt ? 'Automação' : 'Automation',
      text: isPt
        ? 'GitHub Actions publica o front no GitHub Pages e o Render hospeda o backend em produção.'
        : 'GitHub Actions publishes the front-end to GitHub Pages and Render hosts the backend in production.',
      command: 'git push main',
    },
    {
      number: '06',
      title: isPt ? 'E-mails + integrações' : 'Email + integrations',
      text: isPt
        ? 'ViaCEP, GitHub API, Kitsu e Open-Meteo mostram consumo de APIs públicas; Resend envia os links de recuperação de senha.'
        : 'ViaCEP, GitHub API, Kitsu and Open-Meteo show public API consumption; Resend sends password recovery links.',
      command: 'Resend + public APIs',
    },
  ]

  const stack = ['C#', 'ASP.NET Core', 'React', 'TypeScript', 'Vite', 'PostgreSQL', 'EF Core', 'JWT', 'Resend', 'Render', 'GitHub Pages']

  return (
    <section className="about-page">
      <div className="about-terminal-line">
        <strong>portfolio:~$</strong>
        <span>explain --architecture</span>
      </div>

      <div className="about-hero">
        <div>
          <p className="eyebrow">{isPt ? 'Por baixo do capô' : 'Under the hood'}</p>
          <h1>{isPt ? 'Como este site foi feito.' : 'How this site was built.'}</h1>
          <p className="lead">
            {isPt
              ? 'Um portfólio com cara de front-end, mas feito para demonstrar backend: API em camadas, autenticação JWT, banco real, integrações públicas e deploy automatizado.'
              : 'A portfolio that looks like a front-end project, but was built to demonstrate backend skills: layered API, JWT auth, real database, public integrations and automated deploys.'}
          </p>
        </div>

        <aside className="about-status-card" aria-label={isPt ? 'Resumo técnico' : 'Technical summary'}>
          <pre aria-live="polite">{typedStatus}<span className="about-status-cursor" aria-hidden="true" /></pre>
        </aside>
      </div>

      <div className="about-stack-strip" aria-label="Stack">
        {stack.map((item) => (
          <span key={item}>{item}</span>
        ))}
      </div>

      <div className="about-flow">
        {workflow.map((item) => (
          <article className="about-flow-card" key={item.number}>
            <span>{item.number}</span>
            <h3>{item.title}</h3>
            <p>{item.text}</p>
            <code>{item.command}</code>
          </article>
        ))}
      </div>
    </section>
  )
}

function BackendRoom({
  session,
  language,
  onLogout,
}: {
  session: AuthSession
  language: Language
  onLogout: () => void
}) {
  const [room, setRoom] = useState<BackendRoomSnapshot | null>(null)
  const [noteContent, setNoteContent] = useState('')
  const [noteDrafts, setNoteDrafts] = useState<Record<string, string>>({})
  const [communityCaption, setCommunityCaption] = useState('')
  const [roomResult, setRoomResult] = useState<unknown>({ loading: true })
  const [loading, setLoading] = useState(false)
  const [communityActionPostId, setCommunityActionPostId] = useState<string | null>(null)
  const [drawingStatus, setDrawingStatus] = useState(language === 'pt' ? 'Canvas pronto.' : 'Canvas ready.')
  const [brushColor, setBrushColor] = useState('#111111')
  const [brushSize, setBrushSize] = useState(5)
  const [isDrawing, setIsDrawing] = useState(false)
  const canvasRef = useRef<HTMLCanvasElement>(null)
  const communityRefreshInFlightRef = useRef(false)
  const communityActionInFlightRef = useRef(false)
  const mountedRef = useRef(true)
  const locale = language === 'pt' ? 'pt-BR' : 'en-US'
  const tokenPreview = `${session.accessToken.slice(0, 28)}...${session.accessToken.slice(-12)}`

  useEffect(() => {
    mountedRef.current = true

    return () => {
      mountedRef.current = false
    }
  }, [])

  useEffect(() => {
    void loadRoom()
  }, [session.accessToken])

  useEffect(() => {
    const refreshIfVisible = () => {
      if (document.hidden) return
      void refreshCommunityPosts()
    }

    const intervalId = window.setInterval(refreshIfVisible, 4_000)

    window.addEventListener('focus', refreshIfVisible)
    document.addEventListener('visibilitychange', refreshIfVisible)

    return () => {
      window.clearInterval(intervalId)
      window.removeEventListener('focus', refreshIfVisible)
      document.removeEventListener('visibilitychange', refreshIfVisible)
    }
  }, [session.accessToken])

  useEffect(() => {
    if (!room) return

    setNoteDrafts(Object.fromEntries(room.notes.map((note) => [note.id, note.content])))
  }, [room])

  useEffect(() => {
    const canvas = canvasRef.current
    if (!canvas || !room) return

    const context = canvas.getContext('2d')
    if (!context) return

    if (!room.drawing?.dataUrl) {
      resetCanvas()
      return
    }

    const image = new Image()
    image.onload = () => {
      context.clearRect(0, 0, canvas.width, canvas.height)
      context.drawImage(image, 0, 0, canvas.width, canvas.height)
    }
    image.src = room.drawing.dataUrl
  }, [room?.drawing?.dataUrl])

  async function loadRoom() {
    setLoading(true)

    try {
      const result = await getProtectedApi<BackendRoomSnapshot>('/api/backend-room', session.accessToken)
      setRoom(result)
      setRoomResult({
        success: true,
        loadedAt: result.loadedAt,
        notes: result.notes.length,
        drawingSaved: Boolean(result.drawing),
        communityPosts: result.communityPosts.length,
      })
    } catch (error) {
      setRoomResult({ success: false, error: error instanceof Error ? error.message : 'Request failed' })
    } finally {
      setLoading(false)
    }
  }

  async function refreshCommunityPosts(reportErrors = false) {
    if (communityRefreshInFlightRef.current) {
      return
    }

    communityRefreshInFlightRef.current = true

    try {
      const posts = await getProtectedApi<BackendRoomCommunityPost[]>('/api/backend-room/community', session.accessToken)

      if (!mountedRef.current) {
        return
      }

      setRoom((current) => current && { ...current, communityPosts: posts })
    } catch (error) {
      if (reportErrors) {
        setRoomResult({ success: false, error: error instanceof Error ? error.message : 'Request failed' })
      }
    } finally {
      communityRefreshInFlightRef.current = false
    }
  }

  async function createNote(event: FormEvent) {
    event.preventDefault()

    if (!noteContent.trim()) return

    setLoading(true)

    try {
      const note = await sendProtectedApi<BackendRoomNote>(
        '/api/backend-room/notes',
        session.accessToken,
        'POST',
        { content: noteContent },
      )
      setRoom((current) => current && { ...current, notes: [note, ...current.notes] })
      setNoteContent('')
      setRoomResult({ success: true, action: 'note.created', note })
    } catch (error) {
      setRoomResult({ success: false, error: error instanceof Error ? error.message : 'Request failed' })
    } finally {
      setLoading(false)
    }
  }

  async function saveNote(note: BackendRoomNote) {
    const content = noteDrafts[note.id]?.trim()

    if (!content || content === note.content) return

    setLoading(true)

    try {
      const updated = await sendProtectedApi<BackendRoomNote>(
        `/api/backend-room/notes/${note.id}`,
        session.accessToken,
        'PUT',
        { content },
      )
      setRoom((current) =>
        current && {
          ...current,
          notes: current.notes.map((item) => (item.id === updated.id ? updated : item)),
        },
      )
      setRoomResult({ success: true, action: 'note.updated', note: updated })
    } catch (error) {
      setRoomResult({ success: false, error: error instanceof Error ? error.message : 'Request failed' })
    } finally {
      setLoading(false)
    }
  }

  async function deleteNote(noteId: string) {
    setLoading(true)

    try {
      const result = await sendProtectedApi<BackendRoomActionResult>(
        `/api/backend-room/notes/${noteId}`,
        session.accessToken,
        'DELETE',
      )
      setRoom((current) =>
        current && {
          ...current,
          notes: current.notes.filter((note) => note.id !== noteId),
        },
      )
      setRoomResult({ success: true, action: 'note.deleted', result })
    } catch (error) {
      setRoomResult({ success: false, error: error instanceof Error ? error.message : 'Request failed' })
    } finally {
      setLoading(false)
    }
  }

  function getCanvasPoint(event: ReactPointerEvent<HTMLCanvasElement>) {
    const canvas = event.currentTarget
    const rect = canvas.getBoundingClientRect()

    return {
      x: ((event.clientX - rect.left) / rect.width) * canvas.width,
      y: ((event.clientY - rect.top) / rect.height) * canvas.height,
    }
  }

  function startDrawing(event: ReactPointerEvent<HTMLCanvasElement>) {
    const canvas = event.currentTarget
    const context = canvas.getContext('2d')

    if (!context) return

    const point = getCanvasPoint(event)
    canvas.setPointerCapture(event.pointerId)
    context.beginPath()
    context.moveTo(point.x, point.y)
    setIsDrawing(true)
  }

  function draw(event: ReactPointerEvent<HTMLCanvasElement>) {
    if (!isDrawing) return

    const context = event.currentTarget.getContext('2d')

    if (!context) return

    const point = getCanvasPoint(event)
    context.lineWidth = brushSize
    context.lineCap = 'round'
    context.lineJoin = 'round'
    context.strokeStyle = brushColor
    context.lineTo(point.x, point.y)
    context.stroke()
  }

  function stopDrawing(event: ReactPointerEvent<HTMLCanvasElement>) {
    if (!isDrawing) return

    event.currentTarget.releasePointerCapture(event.pointerId)
    setIsDrawing(false)
  }

  function resetCanvas() {
    const canvas = canvasRef.current
    const context = canvas?.getContext('2d')

    if (!canvas || !context) return

    context.fillStyle = '#ffffff'
    context.fillRect(0, 0, canvas.width, canvas.height)
  }

  async function saveDrawing() {
    const canvas = canvasRef.current

    if (!canvas) return

    setLoading(true)
    setDrawingStatus(language === 'pt' ? 'Salvando desenho...' : 'Saving drawing...')

    try {
      const drawing = await sendProtectedApi<BackendRoomDrawing>(
        '/api/backend-room/drawing',
        session.accessToken,
        'PUT',
        {
          name: 'backend-room-canvas',
          dataUrl: canvas.toDataURL('image/png'),
        },
      )
      setRoom((current) => current && { ...current, drawing })
      setRoomResult({ success: true, action: 'drawing.saved', drawing: { ...drawing, dataUrl: '[canvas png]' } })
      setDrawingStatus(language === 'pt' ? 'Canvas salvo no seu usuário.' : 'Canvas saved to your user.')
    } catch (error) {
      setRoomResult({ success: false, error: error instanceof Error ? error.message : 'Request failed' })
      setDrawingStatus(language === 'pt' ? 'Falha ao salvar desenho.' : 'Failed to save drawing.')
    } finally {
      setLoading(false)
    }
  }

  async function shareDrawing(event: FormEvent) {
    event.preventDefault()

    const canvas = canvasRef.current

    if (!canvas) return

    setLoading(true)
    setDrawingStatus(language === 'pt' ? 'Publicando no feed...' : 'Publishing to feed...')

    try {
      const post = await sendProtectedApi<BackendRoomCommunityPost>(
        '/api/backend-room/community',
        session.accessToken,
        'POST',
        {
          caption: communityCaption,
          dataUrl: canvas.toDataURL('image/png'),
        },
      )
      setRoom((current) =>
        current && {
          ...current,
          communityPosts: [post, ...current.communityPosts].slice(0, 24),
        },
      )
      setCommunityCaption('')
      setRoomResult({ success: true, action: 'community.post.created', post: { ...post, dataUrl: '[canvas png]' } })
      setDrawingStatus(language === 'pt' ? 'Publicado no feed da comunidade.' : 'Published to the community feed.')
    } catch (error) {
      setRoomResult({ success: false, error: error instanceof Error ? error.message : 'Request failed' })
      setDrawingStatus(language === 'pt' ? 'Falha ao publicar no feed.' : 'Failed to publish to feed.')
    } finally {
      setLoading(false)
    }
  }

  function getCommunityPostExpiry(post: BackendRoomCommunityPost) {
    const diffInMs = new Date(post.expiresAt).getTime() - Date.now()
    const hoursLeft = Math.max(0, Math.ceil(diffInMs / 3_600_000))

    if (hoursLeft <= 0) {
      return language === 'pt' ? 'expirando agora' : 'expiring now'
    }

    if (hoursLeft === 1) {
      return language === 'pt' ? 'expira em 1h' : 'expires in 1h'
    }

    return language === 'pt' ? `expira em ${hoursLeft}h` : `expires in ${hoursLeft}h`
  }

  function isOwnCommunityPost(post: BackendRoomCommunityPost) {
    return post.userId.toLowerCase() === session.user.id.toLowerCase()
  }

  async function toggleCommunityLike(post: BackendRoomCommunityPost) {
    if (communityActionInFlightRef.current) return

    communityActionInFlightRef.current = true
    setCommunityActionPostId(post.id)

    try {
      const like = await sendProtectedApi<BackendRoomLikeResult>(
        `/api/backend-room/community/${post.id}/like`,
        session.accessToken,
        'POST',
      )

      setRoom((current) =>
        current && {
          ...current,
          communityPosts: current.communityPosts.map((item) =>
            item.id === like.postId
              ? {
                  ...item,
                  likesCount: like.likesCount,
                  likedByCurrentUser: like.likedByCurrentUser,
                }
              : item,
          ),
        },
      )
      setRoomResult({
        success: true,
        action: like.likedByCurrentUser ? 'community.post.liked' : 'community.post.unliked',
        postId: like.postId,
        likesCount: like.likesCount,
      })
    } catch (error) {
      setRoomResult({ success: false, error: error instanceof Error ? error.message : 'Request failed' })
      void refreshCommunityPosts(true)
    } finally {
      communityActionInFlightRef.current = false
      setCommunityActionPostId(null)
    }
  }

  async function deleteCommunityPost(post: BackendRoomCommunityPost) {
    if (communityActionInFlightRef.current) return

    communityActionInFlightRef.current = true
    setCommunityActionPostId(post.id)

    try {
      const result = await sendProtectedApi<BackendRoomActionResult>(
        `/api/backend-room/community/${post.id}`,
        session.accessToken,
        'DELETE',
      )

      setRoom((current) =>
        current && {
          ...current,
          communityPosts: current.communityPosts.filter((item) => item.id !== post.id),
        },
      )
      setRoomResult({
        success: true,
        action: 'community.post.deleted',
        postId: post.id,
        result,
      })
    } catch (error) {
      setRoomResult({ success: false, error: error instanceof Error ? error.message : 'Request failed' })
      void refreshCommunityPosts(true)
    } finally {
      communityActionInFlightRef.current = false
      setCommunityActionPostId(null)
    }
  }

  return (
    <section className="lab-page backend-room-page">
      <div className="backend-room-shell">
        <header className="backend-room-top">
          <div className="backend-room-title">
            <p className="eyebrow">{language === 'pt' ? 'Área autenticada' : 'Authenticated area'}</p>
            <h2>Backend Room</h2>
            <p>
              {language === 'pt'
                ? 'Um espaço privado para testar autenticação JWT com dados salvos por usuário.'
                : 'A private space to test JWT authentication with user-owned saved data.'}
            </p>
          </div>

          <div className="backend-room-session">
            <div className="user-avatar" aria-hidden="true">
              {session.user.name.slice(0, 1).toUpperCase()}
            </div>
            <div>
              <span>{language === 'pt' ? 'Sessão ativa' : 'Active session'}</span>
              <strong>{session.user.name}</strong>
              <small>{session.user.email}</small>
            </div>
            <button className="button" type="button" onClick={onLogout}>
              Logout
            </button>
          </div>
        </header>

        <div className="backend-room-grid">
        <article className="backend-room-panel notes-panel">
          <div className="panel-title">
            <h3>{language === 'pt' ? 'Anotações privadas' : 'Private notes'}</h3>
            <span>POST /notes</span>
          </div>

          <form className="note-composer" onSubmit={createNote}>
            <textarea
              value={noteContent}
              onChange={(event) => setNoteContent(event.target.value)}
              placeholder={language === 'pt' ? 'Nova anotação privada...' : 'New private note...'}
              maxLength={1200}
            />
            <button className="button primary" type="submit" disabled={loading || !noteContent.trim()}>
              {language === 'pt' ? 'Salvar nota' : 'Save note'}
            </button>
          </form>

          <div className="note-list">
            {room?.notes.length ? (
              room.notes.map((note) => (
                <article className="note-card" key={note.id}>
                  <textarea
                    value={noteDrafts[note.id] ?? note.content}
                    onChange={(event) =>
                      setNoteDrafts((current) => ({ ...current, [note.id]: event.target.value }))
                    }
                  />
                  <div>
                    <span>{new Date(note.updatedAt).toLocaleString(locale)}</span>
                    <button type="button" disabled={loading} onClick={() => saveNote(note)}>
                      {language === 'pt' ? 'Atualizar' : 'Update'}
                    </button>
                    <button type="button" disabled={loading} onClick={() => deleteNote(note.id)}>
                      {language === 'pt' ? 'Apagar' : 'Delete'}
                    </button>
                  </div>
                </article>
              ))
            ) : (
              <p className="empty-state">
                {language === 'pt'
                  ? 'Sem notas por enquanto. Crie uma anotação e ela volta quando você fizer login de novo.'
                  : 'No notes yet. Create one and it comes back when you log in again.'}
              </p>
            )}
          </div>
        </article>

        <article className="backend-room-panel paint-panel">
          <div className="panel-title">
            <h3>{language === 'pt' ? 'Canvas privado' : 'Private canvas'}</h3>
            <span>PUT /drawing</span>
          </div>

          <div className="paint-toolbar">
            <div className="paint-swatches">
              {['#111111', '#ffffff', '#ef4444', '#2563eb', '#16a34a'].map((color) => (
                <button
                  className={brushColor === color ? 'active' : ''}
                  key={color}
                  type="button"
                  style={{ '--swatch': color } as CSSProperties}
                  aria-label={`Brush ${color}`}
                  onClick={() => setBrushColor(color)}
                />
              ))}
            </div>
            <label>
              {language === 'pt' ? 'Traço' : 'Stroke'}
              <input
                type="range"
                min="2"
                max="16"
                value={brushSize}
                onChange={(event) => setBrushSize(Number(event.target.value))}
              />
            </label>
          </div>

          <canvas
            ref={canvasRef}
            width="920"
            height="460"
            className="paint-canvas"
            onPointerDown={startDrawing}
            onPointerMove={draw}
            onPointerUp={stopDrawing}
            onPointerCancel={stopDrawing}
          />

          <div className="paint-actions">
            <span>{drawingStatus}</span>
            <button type="button" disabled={loading} onClick={resetCanvas}>
              {language === 'pt' ? 'Limpar' : 'Clear'}
            </button>
            <button className="button primary" type="button" disabled={loading} onClick={saveDrawing}>
              {language === 'pt' ? 'Salvar canvas' : 'Save canvas'}
            </button>
          </div>

          <form className="community-composer" onSubmit={shareDrawing}>
            <input
              value={communityCaption}
              onChange={(event) => setCommunityCaption(event.target.value)}
              maxLength={160}
              placeholder={language === 'pt' ? 'Legenda opcional para o feed...' : 'Optional feed caption...'}
            />
            <button className="button primary" type="submit" disabled={loading}>
              {language === 'pt' ? 'Publicar no feed' : 'Share to feed'}
            </button>
          </form>
        </article>
        </div>

        <article className="backend-room-panel community-panel">
          <div className="panel-title">
            <h3>{language === 'pt' ? 'Feed da comunidade' : 'Community feed'}</h3>
            <span>POST /community</span>
          </div>

          <div className="community-feed">
            {room?.communityPosts.length ? (
              room.communityPosts.map((post) => (
                <article className="community-post" key={post.id}>
                  <img src={post.dataUrl} alt={post.caption || `Canvas by ${post.authorName}`} />
                  <div className="community-post-meta">
                    <div>
                      <strong>{post.authorName}</strong>
                      <span>{new Date(post.createdAt).toLocaleString(locale)}</span>
                    </div>
                    <div className="community-post-actions">
                      <button
                        className={`heart-button ${post.likedByCurrentUser ? 'is-liked' : ''}`}
                        type="button"
                        disabled={communityActionPostId === post.id}
                        onClick={() => toggleCommunityLike(post)}
                        aria-label={
                          post.likedByCurrentUser
                            ? language === 'pt'
                              ? 'Remover curtida'
                              : 'Remove like'
                            : language === 'pt'
                              ? 'Curtir desenho'
                              : 'Like drawing'
                        }
                      >
                        <span aria-hidden="true">♥</span>
                        {post.likesCount}
                      </button>
                      {isOwnCommunityPost(post) && (
                        <button
                          className="community-delete-button"
                          type="button"
                          disabled={communityActionPostId === post.id}
                          onClick={() => deleteCommunityPost(post)}
                        >
                          {language === 'pt' ? 'Excluir' : 'Delete'}
                        </button>
                      )}
                    </div>
                  </div>
                  <span className="community-expiry">{getCommunityPostExpiry(post)}</span>
                  {post.caption && <p>{post.caption}</p>}
                </article>
              ))
            ) : (
              <p className="empty-state">
                {language === 'pt'
                  ? 'Nenhum desenho publicado ainda. Seja o primeiro a sujar esse feed.'
                  : 'No shared drawings yet. Be the first to mess up this feed.'}
              </p>
            )}
          </div>
        </article>

        <details className="backend-room-panel room-json-panel">
          <summary>
            <span>{language === 'pt' ? 'Ver trace da API' : 'View API trace'}</span>
            <code>GET /api/backend-room</code>
          </summary>
          <pre>{JSON.stringify({ token: tokenPreview, lastAction: roomResult }, null, 2)}</pre>
        </details>
      </div>
    </section>
  )
}

function AuthResetPage({ t, language }: { t: typeof copy.pt; language: Language }) {
  const searchParams = new URLSearchParams(window.location.search)
  const email = searchParams.get('authResetEmail') ?? ''
  const token = searchParams.get('authResetToken') ?? ''
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [session, setSession] = useState<AuthSession | null>(null)
  const [result, setResult] = useState<unknown>({ waiting: true })
  const [loading, setLoading] = useState(false)

  async function submitReset(event: FormEvent) {
    event.preventDefault()

    if (password !== confirmPassword) {
      setResult({ success: false, error: t.authPasswordMismatch })
      return
    }

    if (!email || !token) {
      setResult({
        success: false,
        error: language === 'pt'
          ? 'Link de recuperação inválido ou incompleto.'
          : 'Invalid or incomplete recovery link.',
      })
      return
    }

    setLoading(true)

    try {
      await postApi<PasswordResetConfirmation>('/api/auth/reset-password', {
        email,
        token,
        newPassword: password,
      })

      const login = await postApi<AuthSession>('/api/auth/login', { email, password })
      setSession(login)
      saveAuthSession(login)
      setResult({
        success: true,
        message: t.authResetSuccess,
        user: login.user,
        tokenType: login.tokenType,
        expiresAt: login.expiresAt,
        preview: `${login.accessToken.slice(0, 42)}...`,
      })
    } catch (error) {
      setResult({
        success: false,
        error: error instanceof Error ? error.message : 'Request failed',
      })
    } finally {
      setLoading(false)
    }
  }

  if (session) {
    return (
      <BackendRoom
        session={session}
        language={language}
        onLogout={() => {
          clearStoredAuthSession()
          setSession(null)
          setPassword('')
          setConfirmPassword('')
          setResult({ waiting: true })
        }}
      />
    )
  }

  return (
    <section className="lab-page auth-page auth-reset-page">
      <SectionHeading title={t.authResetPageTitle} text={t.authResetPageText} />

      <div className="auth-grid">
        <article className="auth-panel">
          <form className="auth-form" onSubmit={submitReset}>
            <PasswordField
              label={t.authNewPassword}
              value={password}
              onChange={setPassword}
              autoComplete="new-password"
              autoFocus
            />

            <PasswordField
              label={t.authConfirmPassword}
              value={confirmPassword}
              onChange={setConfirmPassword}
              autoComplete="new-password"
            />

            <button className="button primary" type="submit" disabled={loading}>
              {loading ? '...' : t.authReset}
            </button>
          </form>
        </article>

        <article className="auth-panel auth-token-panel">
          <div className="panel-title">
            <h3>{t.response}</h3>
          </div>
          <pre className="auth-token">{JSON.stringify(result, null, 2)}</pre>
        </article>
      </div>
    </section>
  )
}

function PasswordField({
  label,
  value,
  onChange,
  autoComplete,
  autoFocus = false,
}: {
  label: string
  value: string
  onChange: (value: string) => void
  autoComplete: string
  autoFocus?: boolean
}) {
  const [visible, setVisible] = useState(false)

  return (
    <label>
      {label}
      <span className="password-field">
        <input
          value={value}
          onChange={(event) => onChange(event.target.value)}
          type={visible ? 'text' : 'password'}
          autoComplete={autoComplete}
          autoFocus={autoFocus}
          placeholder="**************"
        />
        <button
          className={`password-toggle ${visible ? 'is-visible' : ''}`}
          type="button"
          aria-label={visible ? 'Ocultar senha' : 'Exibir senha'}
          onClick={() => setVisible((current) => !current)}
        >
          <img className="password-eye password-eye-normal" src={assetUrl('anime-eye-password.png')} alt="" aria-hidden="true" />
          <img className="password-eye password-eye-sharingan" src={assetUrl('sharingan-password.png')} alt="" aria-hidden="true" />
        </button>
      </span>
    </label>
  )
}

function SectionHeading({ eyebrow, title, text }: { eyebrow?: string; title: string; text?: string }) {
  return (
    <div className="section-heading">
      {eyebrow && <p className="eyebrow">{eyebrow}</p>}
      <h2>{title}</h2>
      {text && <p>{text}</p>}
    </div>
  )
}

function ExperienceList({ items, language }: { items: ExperienceItem[]; language: Language }) {
  const [openItemId, setOpenItemId] = useState<string | null>('control-id')

  return (
    <div className="experience-list">
      {items.map((item) => {
        const context = language === 'pt' ? item.contextPt : item.contextEn
        const role = language === 'pt' ? item.rolePt : item.roleEn
        const description = language === 'pt' ? item.descriptionPt : item.descriptionEn
        const details = language === 'pt' ? item.detailsPt : item.detailsEn
        const detailsLabel = language === 'pt' ? 'Em destaque' : 'Highlights'
        const isOpen = openItemId === item.id
        const detailsId = `experience-details-${item.id}`

        return (
          <article className={`experience-item ${isOpen ? 'is-open' : ''}`} key={item.id}>
            <button
              className="experience-summary"
              type="button"
              aria-expanded={isOpen}
              aria-controls={detailsId}
              onClick={() => setOpenItemId((current) => (current === item.id ? null : item.id))}
            >
              <span className="experience-indicator" aria-hidden="true"></span>
              <span className="experience-copy">
                <span className="experience-context">{context}</span>
                <strong>{role}</strong>
                <span>{description}</span>
              </span>
              <span className="experience-toggle" aria-hidden="true">
                {isOpen ? (language === 'pt' ? 'Fechar' : 'Close') : (language === 'pt' ? 'Detalhes' : 'Details')}
              </span>
            </button>
            <div className="experience-details" id={detailsId}>
              <div className="experience-details-inner">
                <div className="experience-details-content">
                  <span className="experience-details-label">{detailsLabel}</span>
                  <ul>
                    {details.map((detail) => (
                      <li key={detail}>{detail}</li>
                    ))}
                  </ul>
                </div>
              </div>
            </div>
          </article>
        )
      })}
    </div>
  )
}

function CertificationsPage({
  t,
  language,
  certifications,
}: {
  t: typeof copy.pt
  language: Language
  certifications: Certification[]
}) {
  const [lightbox, setLightbox] = useState<CertificationLightbox | null>(null)
  const closeTimeoutRef = useRef<number | null>(null)

  useEffect(() => {
    if (!lightbox || lightbox.closing) return

    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        closeCertificationImage()
      }
    }

    document.body.style.overflow = 'hidden'
    window.addEventListener('keydown', onKeyDown)

    return () => {
      document.body.style.overflow = ''
      window.removeEventListener('keydown', onKeyDown)
    }
  }, [lightbox])

  useEffect(() => {
    return () => {
      if (closeTimeoutRef.current) {
        window.clearTimeout(closeTimeoutRef.current)
      }
    }
  }, [])

  function openCertificationImage(event: MouseEvent<HTMLButtonElement>, certification: Certification, alt: string) {
    const image = event.currentTarget.querySelector('img')
    const rect = (image ?? event.currentTarget).getBoundingClientRect()
    const naturalWidth = image?.naturalWidth || rect.width
    const naturalHeight = image?.naturalHeight || rect.height
    const ratio = naturalWidth / naturalHeight
    const maxWidth = window.innerWidth - 48
    const maxHeight = window.innerHeight - 96
    let width = Math.min(maxWidth, maxHeight * ratio)
    let height = width / ratio

    if (height > maxHeight) {
      height = maxHeight
      width = height * ratio
    }

    if (closeTimeoutRef.current) {
      window.clearTimeout(closeTimeoutRef.current)
      closeTimeoutRef.current = null
    }

    setLightbox({
      src: certification.imageUrl,
      alt,
      origin: {
        top: rect.top,
        left: rect.left,
        width: rect.width,
        height: rect.height,
      },
      target: {
        top: (window.innerHeight - height) / 2,
        left: (window.innerWidth - width) / 2,
        width,
        height,
      },
      closing: false,
    })
  }

  function closeCertificationImage() {
    setLightbox((current) => {
      if (!current || current.closing) return current
      return { ...current, closing: true }
    })

    closeTimeoutRef.current = window.setTimeout(() => {
      setLightbox(null)
      closeTimeoutRef.current = null
    }, 280)
  }

  return (
    <section className="lab-page">
      <SectionHeading title={t.certificationTitle} text={t.certificationText} />

      <div className="certification-grid">
        {certifications.map((certification) => {
          const name = language === 'pt' ? certification.namePt : certification.nameEn
          const summary = language === 'pt' ? certification.summaryPt : certification.summaryEn

          return (
            <article className="certification-card" key={certification.id}>
              <button
                className="certification-preview"
                type="button"
                onClick={(event) => openCertificationImage(event, certification, name)}
                aria-label={`${language === 'pt' ? 'Ampliar certificação' : 'Expand certification'}: ${name}`}
              >
                <img src={certification.imageUrl} alt={name} loading="lazy" />
              </button>

              <div className="certification-content">
                <span>{certification.issuer}</span>
                <h3>{name}</h3>
                <p>{summary}</p>
                <a className="button primary" href={certification.credentialUrl} target="_blank" rel="noreferrer">
                  {t.openCredential}
                </a>
              </div>
            </article>
          )
        })}
      </div>

      {lightbox && (
        <div
          className={`certification-lightbox ${lightbox.closing ? 'is-closing' : 'is-open'}`}
          role="dialog"
          aria-modal="true"
          aria-label={lightbox.alt}
          onMouseDown={closeCertificationImage}
        >
          <button
            className="certification-lightbox-close"
            type="button"
            onMouseDown={(event) => event.stopPropagation()}
            onClick={closeCertificationImage}
          >
            {t.close}
          </button>

          <div
            className="certification-lightbox-stage"
            style={
              {
                '--origin-top': `${lightbox.origin.top}px`,
                '--origin-left': `${lightbox.origin.left}px`,
                '--origin-width': `${lightbox.origin.width}px`,
                '--origin-height': `${lightbox.origin.height}px`,
                '--target-top': `${lightbox.target.top}px`,
                '--target-left': `${lightbox.target.left}px`,
                '--target-width': `${lightbox.target.width}px`,
                '--target-height': `${lightbox.target.height}px`,
              } as CSSProperties
            }
            onMouseDown={(event) => event.stopPropagation()}
          >
            <img src={lightbox.src} alt={lightbox.alt} />
          </div>
        </div>
      )}
    </section>
  )
}

function InteractiveConsole({
  data,
  language,
  repos,
  setPage,
  showJson,
}: {
  data: PortfolioData
  language: Language
  repos: GitHubRepository[]
  setPage: (page: Page) => void
  showJson: (title: string, path: string) => void
}) {
  const [input, setInput] = useState('')
  const [entries, setEntries] = useState<ConsoleEntry[]>([])
  const inputRef = useRef<HTMLInputElement>(null)
  const typingRunRef = useRef(0)
  const prompt = 'C:\\Port\\Caio>'

  const help = useMemo(
    () => [
      'Digite "whoami"       -> mostra nome, foco e localização',
      'Digite "controlid"    -> mostra minha atuação atual',
      'Digite "impact"       -> mostra impacto real do iDSupport',
      'Digite "stack"        -> lista tecnologias principais',
      'Digite "projects"     -> lista projetos e links',
      'Digite "github"       -> mostra repositórios recentes',
      'Digite "certs"        -> mostra certificações',
      'Digite "english"      -> mostra meu nível de inglês',
      'Digite "api profile"  -> abre o JSON do perfil',
      'Digite "api projects" -> abre o JSON dos projetos',
      'Digite "lab"          -> abre o Integration Lab',
      'Digite "auth"         -> abre o Auth Lab com JWT',
      'Digite "contact"      -> mostra GitHub e LinkedIn',
      'Digite "clear"        -> reinicia este console',
    ],
    [],
  )

  useEffect(() => {
    void bootConsole()

    return () => {
      typingRunRef.current += 1
    }
  }, [])

  async function bootConsole() {
    const runId = typingRunRef.current + 1
    typingRunRef.current = runId
    setEntries([])

    for (const entry of bootEntries) {
      if (typingRunRef.current !== runId) return
      await typeEntry(entry, runId)
    }
  }

  async function typeEntry(entry: ConsoleEntry, runId: number) {
    const id = window.crypto?.randomUUID?.() ?? `${Date.now()}-${Math.random()}`
    if (typingRunRef.current !== runId) return
    setEntries((current) => [...current, { ...entry, id, text: '' }])

    for (let index = 1; index <= entry.text.length; index += 1) {
      if (typingRunRef.current !== runId) return
      await wait(index < 8 ? 22 : 9)
      if (typingRunRef.current !== runId) return
      setEntries((current) =>
        current.map((item) => (item.id === id ? { ...item, text: entry.text.slice(0, index) } : item)),
      )
    }

    await wait(80)
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    const command = input.trim()
    if (!command) return

    setInput('')

    if (['clear', 'limpar'].includes(command.toLowerCase())) {
      void bootConsole()
      return
    }

    const runId = typingRunRef.current + 1
    typingRunRef.current = runId
    const output = await resolveCommand(command)
    setEntries((current) => [...current, { type: 'command', text: command, id: `cmd-${Date.now()}` }])

    for (const entry of Array.isArray(output) ? output : [output]) {
      await typeEntry(entry, runId)
    }
  }

  async function resolveCommand(command: string): Promise<ConsoleEntry | ConsoleEntry[]> {
    const normalized = command.toLowerCase()

    if (normalized === 'help') {
      return help.map((text) => ({ type: 'output', text }))
    }

    if (['whoami', 'sobre'].includes(normalized)) {
      return {
        type: 'output',
        text: `${data.profile.name} - backend C#/.NET - ${data.profile.location}`,
      }
    }

    if (normalized === 'controlid') {
      return [
        {
          type: 'output',
          text: 'Control iD desde 06/2024: suporte técnico especializado + desenvolvimento de ferramentas internas.',
        },
        {
          type: 'output',
          text: 'Foco: automações, RMA, consulta de notas fiscais, bancos de dados e integração com APIs de ERP legado.',
        },
      ]
    }

    if (['az900', 'certs', 'certificacoes'].includes(normalized)) {
      return [
        {
          type: 'output',
          text: 'Microsoft Certified: Azure Fundamentals (AZ-900).',
        },
        {
          type: 'output',
          text: 'TIVIT - .NET com GitHub Copilot.',
        },
      ]
    }

    if (['english', 'ingles'].includes(normalized)) {
      return {
        type: 'output',
        text: 'Inglês avançado para leitura técnica, documentação e comunicação profissional.',
      }
    }

    if (normalized === 'impact') {
      return {
        type: 'output',
        text: 'iDSupport/RmaWorker: abertura de OS de ~10 min para ~1 min com automações e integrações internas.',
      }
    }

    if (normalized === 'stack') {
      return data.skills.map((group) => ({
        type: 'output',
        text: `${group.name}: ${group.items.join(', ')}`,
      }))
    }

    if (['projects', 'projetos'].includes(normalized)) {
      return data.projects.map((project) => ({
        type: 'output',
        text: `${project.name} -> ${project.repositoryUrl}`,
      }))
    }

    if (normalized === 'github') {
      return (repos.length ? repos : fallbackRepos()).slice(0, 5).map((repo) => ({
        type: 'output',
        text: `${repo.name} - ${repo.language ?? 'Mixed'} - ${repo.htmlUrl}`,
      }))
    }

    if (normalized === 'api profile') {
      showJson('GET /api/profile', '/api/profile')
      return { type: 'output', text: 'Abrindo GET /api/profile...' }
    }

    if (normalized === 'api projects') {
      showJson('GET /api/projects', '/api/projects')
      return { type: 'output', text: 'Abrindo GET /api/projects...' }
    }

    if (normalized === 'lab') {
      setPage('lab')
      return { type: 'output', text: 'Abrindo Integration Lab...' }
    }

    if (normalized === 'auth') {
      setPage('auth')
      return { type: 'output', text: 'Abrindo Auth Lab...' }
    }

    if (['contact', 'contato'].includes(normalized)) {
      return [
        { type: 'output', text: data.profile.gitHubUrl },
        { type: 'output', text: data.profile.linkedInUrl },
      ]
    }

    return { type: 'error', text: `Comando não encontrado: ${command}. Tente "help".` }
  }

  return (
    <article className="console-window" aria-label="Interactive console" onClick={() => inputRef.current?.focus()}>
      <div className="console-titlebar">
        <span className="terminal-mark"></span>
        <strong>Prompt - C:\Port\Caio</strong>
      </div>
      <div className="console-body" aria-live="polite">
        {entries.slice(-12).map((entry, index) => (
          <p className={`console-entry ${entry.type}`} key={`${entry.text}-${index}`}>
            {entry.type === 'command' ? (
              <>
                <span>{prompt}</span>
                {entry.text}
              </>
            ) : (
              <>
                <span>{entry.type === 'error' ? 'Erro:' : ''}</span>
                {entry.text}
              </>
            )}
          </p>
        ))}
        <form className="console-input" onSubmit={handleSubmit}>
          <span>{prompt}</span>
          <input
            ref={inputRef}
            value={input}
            onChange={(event) => setInput(event.target.value)}
            aria-label={language === 'pt' ? 'Comando do console' : 'Console command'}
            spellCheck={false}
            autoComplete="off"
          />
        </form>
      </div>
    </article>
  )
}

function LabPage({
  t,
  cep,
  githubUser,
  animeQuery,
  weatherState,
  labResult,
  setCep,
  setGithubUser,
  setAnimeQuery,
  setWeatherState,
  runLab,
  setPage,
}: {
  t: typeof copy.pt
  cep: string
  githubUser: string
  animeQuery: string
  weatherState: WeatherStateKey
  labResult: unknown
  setCep: (value: string) => void
  setGithubUser: (value: string) => void
  setAnimeQuery: (value: string) => void
  setWeatherState: (value: WeatherStateKey) => void
  runLab: (path: string) => void
  setPage: (page: Page) => void
}) {
  const selectedState = weatherStates[weatherState]

  return (
    <section className="lab-page">
      <SectionHeading title="Integration Lab" text={t.labText} />

      <div className="lab-grid">
        <LabCard
          index="01"
          title={t.cepTitle}
          hint={t.cepHint}
          value={cep}
          placeholder={t.cepPlaceholder}
          endpoint="/api/lab/cep/{cep}"
          onChange={setCep}
          onRun={() => runLab(`/api/lab/cep/${cep}`)}
          runLabel={t.run}
        />
        <LabCard
          index="02"
          title={t.gitHubUserTitle}
          hint={t.gitHubUserHint}
          value={githubUser}
          placeholder={t.githubPlaceholder}
          endpoint="/api/lab/github/{username}"
          onChange={setGithubUser}
          onRun={() => runLab(`/api/lab/github/${encodeURIComponent(githubUser)}`)}
          runLabel={t.run}
        />
        <LabCard
          index="03"
          title={t.animeTitle}
          hint={t.animeHint}
          value={animeQuery}
          placeholder={t.animePlaceholder}
          endpoint="/api/lab/anime?query={query}"
          onChange={setAnimeQuery}
          onRun={() => runLab(`/api/lab/anime?query=${encodeURIComponent(animeQuery)}`)}
          runLabel={t.run}
        />
        <article className="lab-card">
          <span>04</span>
          <h3>{t.weatherTitle}</h3>
          <p>{t.weatherHint}</p>
          <div className="lab-form">
            <label>
              {t.stateLabel}
              <select
                value={weatherState}
                onChange={(event) => setWeatherState(event.target.value as WeatherStateKey)}
              >
                {Object.entries(weatherStates).map(([key, state]) => (
                  <option key={key} value={key}>
                    {key} — {state.label}
                  </option>
                ))}
              </select>
            </label>
            <button
              type="button"
              onClick={() =>
                runLab(
                  `/api/lab/weather?city=${encodeURIComponent(`${selectedState.capital}, ${weatherState}`)}&latitude=${selectedState.latitude}&longitude=${selectedState.longitude}`,
                )
              }
            >
              {t.run}
            </button>
          </div>
          <code>GET /api/lab/weather</code>
        </article>
        <article className="lab-card auth-lab-card">
          <span>05</span>
          <h3>{t.authTitle}</h3>
          <p>{t.authHint}</p>
          <div className="lab-form single-action">
            <button
              className="button primary"
              type="button"
              onClick={() => {
                setPage('auth')
                window.setTimeout(() => window.scrollTo({ top: 0, behavior: 'smooth' }), 0)
              }}
            >
              {t.authOpen}
            </button>
          </div>
          <code>POST /api/auth/register · POST /api/auth/login · GET /api/auth/me</code>
        </article>
      </div>

      <article className="lab-response">
        <div className="panel-title">
          <h3>{t.response}</h3>
        </div>
        <pre>{JSON.stringify(labResult ?? { waiting: true }, null, 2)}</pre>
      </article>
    </section>
  )
}

function LabCard({
  index,
  title,
  hint,
  value,
  placeholder,
  endpoint,
  onChange,
  onRun,
  runLabel,
}: {
  index: string
  title: string
  hint: string
  value: string
  placeholder: string
  endpoint: string
  onChange: (value: string) => void
  onRun: () => void
  runLabel: string
}) {
  return (
    <article className="lab-card">
      <span>{index}</span>
      <h3>{title}</h3>
      <p>{hint}</p>
      <div className="lab-form">
        <label>
          Query
          <input value={value} onChange={(event) => onChange(event.target.value)} placeholder={placeholder} />
        </label>
        <button type="button" onClick={onRun}>{runLabel}</button>
      </div>
      <code>GET {endpoint}</code>
    </article>
  )
}

function fallbackRepos(): GitHubRepository[] {
  return [
    {
      name: 'caio-matheus-dev',
      htmlUrl: 'https://github.com/cmathxus/caio-matheus-dev',
      description: null,
      language: 'C# / TypeScript',
      stars: 0,
      forks: 0,
      updatedAt: new Date().toISOString(),
    },
  ]
}

export default App
