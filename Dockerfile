# Stage 1: Build & Publish (.NET 10 SDK)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["PersonalPlanner.csproj", "./"]
RUN dotnet restore "PersonalPlanner.csproj"

COPY . .
RUN dotnet publish "PersonalPlanner.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime (.NET 10 ASP.NET Core Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PersonalPlanner.dll"]