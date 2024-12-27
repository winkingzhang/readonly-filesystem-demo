FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
ENV ASPNETCORE_URLS=https://+:15067;http://+:15066
EXPOSE 15066

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RoFsApp/RoFsApp.csproj", "RoFsApp/"]
RUN dotnet restore "RoFsApp/RoFsApp.csproj"
COPY . .
WORKDIR "/src/RoFsApp"
RUN dotnet build "RoFsApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RoFsApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RoFsApp.dll"]
