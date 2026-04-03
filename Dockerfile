FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY rideit.Core/rideit.Core.csproj rideit.Core/
COPY rideit/rideit.csproj rideit/
RUN dotnet restore rideit/rideit.csproj
COPY rideit.Core/ rideit.Core/
COPY rideit/ rideit/
RUN dotnet publish rideit/rideit.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "rideit.dll"]
