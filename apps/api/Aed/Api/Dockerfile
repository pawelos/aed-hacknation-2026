FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Api/Aed.csproj", "Api/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["SharedKernel/SharedKernel.csproj", "SharedKernel/"]
COPY ["ServiceDefaults/ServiceDefaults.csproj", "ServiceDefaults/"]
RUN dotnet restore "Api/Aed.csproj"
COPY . .
WORKDIR "/src/Api"
RUN dotnet publish "Aed.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Aed.dll"]
