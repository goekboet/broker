FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./http/http.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY ./http ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM ego/aspnet:2.2
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "http.dll"]