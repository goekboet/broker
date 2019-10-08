FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./http/http.csproj ./http/
COPY ./scheduling/scheduling.csproj ./scheduling/
COPY ./postgres/postgres.csproj ./postgres/
RUN dotnet restore ./postgres/postgres.csproj
RUN dotnet restore ./http/http.csproj

# Copy everything else and build
COPY ./http ./http
COPY ./scheduling ./scheduling
COPY ./postgres ./postgres
RUN dotnet publish -c Release -o out ./http/http.csproj

# Build runtime image
FROM ego/aspnet:2.2
WORKDIR /app
COPY --from=build-env /app/http/out .
ENTRYPOINT ["dotnet", "http.dll"]