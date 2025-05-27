FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SecureEduIdInviteProxy/SecureEduIdInviteProxy.csproj", "SecureEduIdInviteProxy/"]
RUN dotnet restore "SecureEduIdInviteProxy/SecureEduIdInviteProxy.csproj"
COPY . .
WORKDIR "/src/SecureEduIdInviteProxy"
RUN dotnet build "./SecureEduIdInviteProxy.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SecureEduIdInviteProxy.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SecureEduIdInviteProxy.dll"]
