using CaioMatheusDev.Api.Domain.Integrations;
using CaioMatheusDev.Api.Domain.Portfolio;

namespace CaioMatheusDev.Api.Infrastructure.Data;

public static class PortfolioData
{
    public static Profile Profile { get; } = new(
        Name: "Caio Matheus",
        Role: "Backend Developer C#/.NET",
        Location: "Guarulhos, SP",
        SummaryPt: "Desenvolvedor back-end com foco em C#, .NET, ASP.NET Core e SQL, atuando na construção de APIs REST, aplicações internas e integrações entre sistemas corporativos.",
        SummaryEn: "Back-end developer focused on C#, .NET, ASP.NET Core and SQL, building REST APIs, internal applications and integrations between corporate systems.",
        GitHubUrl: "https://github.com/cmathxus",
        LinkedInUrl: "https://www.linkedin.com/in/caio-matheus-b68977236",
        Highlights:
        [
            "C#",
            ".NET 8",
            "ASP.NET Core",
            "SQL",
            "Azure",
            "GitHub Actions"
        ]);

    public static Project[] Projects { get; } =
    [
        new(
            "idsupport",
            "iDSupport / RmaWorker",
            "Aplicação interna em produção na Control iD para apoiar fluxos de suporte, RMA e ordens de serviço.",
            "Internal production application at Control iD supporting support, RMA and service order workflows.",
            "C#, .NET 8, TypeScript, Playwright, GitHub Actions",
            "Reduziu a abertura de ordens de serviço de aproximadamente 10 minutos para cerca de 1 minuto.",
            "Reduced service order opening time from around 10 minutes to about 1 minute.",
            "https://github.com/ya-labs/RmaWorker",
            true),
        new(
            "rental-manager",
            "Rental Manager",
            "API solo para organizar imóveis, contratos, locatários e pagamentos.",
            "Solo API for organizing properties, contracts, tenants, and payments.",
            "ASP.NET Core, .NET 9, PostgreSQL, DDD, Clean Architecture",
            "Projeto focado em modelagem de domínio, persistência e organização de camadas.",
            "Project focused on domain modeling, persistence, and layered architecture.",
            "https://github.com/cmathxus/rental-manager",
            false),
        new(
            "cade-o-dano",
            "Cade o Dano",
            "Aplicação com dados de League of Legends e integração com API externa.",
            "Application with League of Legends data and external API integration.",
            "TypeScript, CSS, C#",
            "Backend usado para intermediar chamadas, proteger detalhes de integração e normalizar respostas.",
            "Backend used to intermediate calls, protect integration details, and normalize responses.",
            "https://github.com/ya-labs/CADE-O-DANO",
            false),
        new(
            "yahub",
            "YAHub",
            "Site oficial da YA LABS, usado como presença pública dos projetos.",
            "Official YA LABS website, used as the public presence for projects.",
            "JavaScript, TypeScript",
            "Organização de produto, documentação visual e publicação em produção.",
            "Product organization, visual documentation, and production publishing.",
            "https://github.com/ya-labs/YAHub",
            true)
    ];

    public static SkillGroup[] Skills { get; } =
    [
        new("Backend", ["C#", ".NET 8", "ASP.NET Core", "APIs REST"]),
        new("Arquitetura", ["Clean Architecture", "DDD", "DTOs", "REST", "Error handling"]),
        new("Dados", ["PostgreSQL", "MySQL", "SQL", "Modelagem", "Cache"]),
        new("Entrega", ["GitHub", "GitHub Actions", "Docker", "Azure", "Linux"])
    ];

    public static ExperienceItem[] Experience { get; } =
    [
        new(
            "control-id",
            "Control iD",
            "Control iD",
            "Suporte técnico de sistemas + desenvolvimento interno",
            "Technical systems support + internal development",
            "Desde 06/2024, atuando em suporte técnico especializado, diagnóstico de falhas e desenvolvimento de aplicações internas em produção.",
            "Since 06/2024, working with specialized technical support, failure diagnosis, and production internal applications.",
            [
                "Automação de processos internos e redução de trabalho manual.",
                "Apoio técnico em IDSecure, IDSecure Cloud, bancos de dados e ambientes Linux.",
                "Integrações com APIs de ERP legado e fluxos corporativos reais."
            ],
            [
                "Internal process automation and manual-work reduction.",
                "Technical support for IDSecure, IDSecure Cloud, databases and Linux environments.",
                "Integrations with legacy ERP APIs and real corporate workflows."
            ]),
        new(
            "automation",
            "Operação e automação",
            "Operations and automation",
            "Integrações corporativas",
            "Corporate integrations",
            "Automação de processos, consulta de notas fiscais, fluxos de RMA, integrações com ERP legado, VPN e Playwright.",
            "Process automation, invoice lookup, RMA flows, legacy ERP integrations, VPN and Playwright.",
            [
                "iDSupport/RmaWorker em produção para apoiar fluxos de suporte e RMA.",
                "Abertura de ordem de serviço reduzida de aproximadamente 10 minutos para cerca de 1 minuto.",
                "Uso de C#, .NET 8, TypeScript, Playwright e GitHub Actions."
            ],
            [
                "iDSupport/RmaWorker in production supporting support and RMA workflows.",
                "Service-order opening time reduced from around 10 minutes to about 1 minute.",
                "Use of C#, .NET 8, TypeScript, Playwright and GitHub Actions."
            ]),
        new(
            "education",
            "Formação",
            "Education",
            "Análise e desenvolvimento de sistemas",
            "Systems analysis and development",
            "Centro Universitário ENIAC, conclusão em 12/2024. Certificação Microsoft Azure Fundamentals AZ-900.",
            "Centro Universitário ENIAC, completed in 12/2024. Microsoft Azure Fundamentals AZ-900 certification.",
            [
                "Base em desenvolvimento back-end, bancos de dados e arquitetura de aplicações.",
                "Inglês avançado para leitura técnica, documentação e comunicação profissional.",
                "Interesse atual em C#, ASP.NET Core, APIs REST, cloud e automações."
            ],
            [
                "Foundation in back-end development, databases and application architecture.",
                "Advanced English for technical reading, documentation and professional communication.",
                "Current focus on C#, ASP.NET Core, REST APIs, cloud and automation."
            ])
    ];

    public static Certification[] Certifications { get; } =
    [
        new(
            "az-900",
            "Microsoft Certified: Azure Fundamentals (AZ-900)",
            "Microsoft Certified: Azure Fundamentals (AZ-900)",
            "Microsoft",
            "Certificação de fundamentos de cloud, serviços Azure, segurança, governança e modelos de cobrança.",
            "Certification covering cloud fundamentals, Azure services, security, governance and pricing models.",
            "/certifications/microsoft-az-900.png",
            "https://learn.microsoft.com/pt-br/users/caiomatheusqueiroz-7788/credentials/94eaa0e05ee099e?ref=https%3A%2F%2Fwww.linkedin.com%2F"),
        new(
            "tivit-dotnet-copilot",
            "TIVIT - .NET com GitHub Copilot",
            "TIVIT - .NET with GitHub Copilot",
            "TIVIT",
            "Bootcamp focado em desenvolvimento .NET com uso de GitHub Copilot no fluxo de construção de aplicações.",
            "Bootcamp focused on .NET development using GitHub Copilot in the application-building workflow.",
            "/certifications/tivit-dotnet-copilot.png",
            "https://www.linkedin.com/posts/caio-matheus-b68977236_finalizei-hoje-o-bootcamp-da-dio-tivit-activity-7399581791495512064-x5M4?utm_source=social_share_send&utm_medium=member_desktop_web&rcm=ACoAADrgOMABjnHamE70unLi5JMtxUjA3G1Ooyo")
    ];

    public static StatusTarget[] StatusTargets { get; } =
    [
        new("YAHub", "https://github.com/ya-labs/YAHub"),
        new("Cade o Dano", "https://ya-labs.github.io/CADE-O-DANO/"),
        new("GitHub Profile", "https://github.com/cmathxus")
    ];
}
