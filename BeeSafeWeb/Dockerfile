FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BeeSafeWeb/BeeSafeWeb.csproj", "BeeSafeWeb/"]
COPY ["BeeSafeWeb.Utility/BeeSafeWeb.Utility.csproj", "BeeSafeWeb.Utility/"]
COPY package.json .
COPY package-lock.json .
COPY tailwind.config.js .
RUN apt update
RUN apt install -y npm
RUN npm install
RUN dotnet restore "BeeSafeWeb/BeeSafeWeb.csproj"
COPY . .
WORKDIR "/src/BeeSafeWeb"
RUN dotnet build "BeeSafeWeb.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BeeSafeWeb.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER root
RUN mkdir uploads/
RUN chown app:app uploads/
USER app

ENTRYPOINT ["dotnet", "BeeSafeWeb.dll"]
