FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["CaioMatheusDev.Api.csproj", "."]
RUN dotnet restore "CaioMatheusDev.Api.csproj"
COPY . .
RUN dotnet publish "CaioMatheusDev.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "CaioMatheusDev.Api.dll"]
