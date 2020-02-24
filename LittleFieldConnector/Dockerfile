FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["LittleFieldConnector/LittleFieldConnector.csproj", "LittleFieldConnector/"]
RUN dotnet restore "LittleFieldConnector/LittleFieldConnector.csproj"
COPY . .
WORKDIR "/src/LittleFieldConnector"
RUN dotnet build "LittleFieldConnector.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "LittleFieldConnector.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "LittleFieldConnector.dll"]