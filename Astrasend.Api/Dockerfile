FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Astrasend.Api/Astrasend.Api.csproj", "Astrasend.Api/"]
RUN dotnet restore "Astrasend.Api/Astrasend.Api.csproj"
COPY . .
WORKDIR "/src/Astrasend.Api"
RUN dotnet build "Astrasend.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Astrasend.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Astrasend.Api.dll"]
