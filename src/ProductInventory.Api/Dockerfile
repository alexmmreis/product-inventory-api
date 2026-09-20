# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/ProductInventory.Domain/ProductInventory.Domain.csproj", "src/ProductInventory.Domain/"]
COPY ["src/ProductInventory.Application/ProductInventory.Application.csproj", "src/ProductInventory.Application/"]
COPY ["src/ProductInventory.Infrastructure/ProductInventory.Infrastructure.csproj", "src/ProductInventory.Infrastructure/"]
COPY ["src/ProductInventory.Api/ProductInventory.Api.csproj", "src/ProductInventory.Api/"]
RUN dotnet restore "src/ProductInventory.Api/ProductInventory.Api.csproj"

COPY src/ src/
WORKDIR /src/src/ProductInventory.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ProductInventory.Api.dll"]
