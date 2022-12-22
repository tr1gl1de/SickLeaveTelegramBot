FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["SickLeaveTelegramBot.App/SickLeaveTelegramBot.App.csproj", "SickLeaveTelegramBot.App/"]
RUN dotnet restore "SickLeaveTelegramBot.App/SickLeaveTelegramBot.App.csproj"
COPY . .
WORKDIR "/src/SickLeaveTelegramBot.App"
RUN dotnet build "SickLeaveTelegramBot.App.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SickLeaveTelegramBot.App.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SickLeaveTelegramBot.App.dll"]
