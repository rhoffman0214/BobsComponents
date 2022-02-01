#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["BobsComponent.Client/BobsComponent.Client.csproj", "BobsComponent.Client/"]
COPY ["BobsComponent.Library/BobsComponent.Library.csproj", "BobsComponent.Library/"]
RUN dotnet restore "BobsComponent.Client/BobsComponent.Client.csproj"
COPY . .
WORKDIR "/src/BobsComponent.Client"
RUN dotnet build "BobsComponent.Client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BobsComponent.Client.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY "BobsComponent.Client/nginx.conf" /etc/nginx/nginx.conf

#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "BobsComponent.Client.dll"]