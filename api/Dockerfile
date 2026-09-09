
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src


COPY NvnStok.Domain/*.csproj NvnStok.Domain/
COPY NvnStok.Application/*.csproj NvnStok.Application/
COPY NvnStok.Infrastructure/*.csproj NvnStok.Infrastructure/
COPY NvnStok.Api/*.csproj NvnStok.Api/

RUN dotnet restore NvnStok.Api/NvnStok.Api.csproj


COPY NvnStok.Domain/ NvnStok.Domain/
COPY NvnStok.Application/ NvnStok.Application/
COPY NvnStok.Infrastructure/ NvnStok.Infrastructure/
COPY NvnStok.Api/ NvnStok.Api/

RUN dotnet publish NvnStok.Api/NvnStok.Api.csproj -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app


COPY --from=build /app/publish .


ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "NvnStok.Api.dll"]