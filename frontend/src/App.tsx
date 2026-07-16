import { useEffect, useMemo, useRef, useState } from 'react'
import type { CSSProperties, FormEvent, MouseEvent } from 'react'
import './App.css'

type Language = 'pt' | 'en'
type Page = 'home' | 'lab' | 'certifications'
type NavKey = 'home' | 'projects' | 'api' | 'lab' | 'certifications'
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

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5089'

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
    imageUrl: '/certifications/microsoft-az-900.png',
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
    imageUrl: '/certifications/tivit-dotnet-copilot.png',
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
  const [page, setPage] = useState<Page>('home')
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
    document.documentElement.dataset.theme = theme
    document.documentElement.style.colorScheme = theme
    localStorage.setItem('theme', theme)
  }, [theme])

  useEffect(() => {
    if (page !== 'home') {
      pendingScrollTarget.current = null
      setActiveNav(page)
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
          setCertifications(certificationsData)
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

  function navigatePage(nextPage: Extract<Page, 'lab' | 'certifications'>) {
    pendingScrollTarget.current = null
    setActiveNav(nextPage)
    setPage(nextPage)
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
          <div className="segmented-control" aria-label="Language selector">
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
        />
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
