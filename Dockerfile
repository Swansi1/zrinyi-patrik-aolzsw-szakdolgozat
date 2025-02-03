FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["redmineGUI/redmineGUI/redmineGUI.csproj", "redmineGUI/"]
RUN dotnet restore "redmineGUI/redmineGUI.csproj"

COPY redmineGUI/ ./redmineGUI/
WORKDIR "/src/redmineGUI"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "redmineGUI.dll"]