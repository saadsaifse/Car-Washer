FROM microsoft/dotnet:2.0-runtime-nanoserver-sac2016 AS base
WORKDIR /app

FROM microsoft/dotnet:2.0-sdk-nanoserver-sac2016 AS build
WORKDIR /src
COPY *.sln ./
COPY FirstScreen.CarWasher/FirstScreen.CarWasher.csproj FirstScreen.CarWasher/
RUN dotnet restore
COPY . .
WORKDIR /src/FirstScreen.CarWasher
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "FirstScreen.CarWasher.dll"]
