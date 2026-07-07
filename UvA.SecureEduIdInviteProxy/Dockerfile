# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

COPY . .
RUN dotnet restore
RUN dotnet test
RUN dotnet publish -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
RUN apt install tzdata
ENV TZ=Europe/Amsterdam

WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "UvA.SecureEduIdInviteProxy.dll"]