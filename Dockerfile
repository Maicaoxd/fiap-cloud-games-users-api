FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/UsersAPI/UsersAPI.csproj", "src/UsersAPI/"]
RUN dotnet restore "src/UsersAPI/UsersAPI.csproj"
COPY . .
RUN dotnet publish "src/UsersAPI/UsersAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

FROM runtime AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UsersAPI.dll"]
