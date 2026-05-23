FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["QuanlyDiemAPI/QuanlyDiemAPI.csproj", "QuanlyDiemAPI/"]
RUN dotnet restore "QuanlyDiemAPI/QuanlyDiemAPI.csproj"
COPY . .
WORKDIR "/src/QuanlyDiemAPI"
RUN dotnet publish "QuanlyDiemAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QuanlyDiemAPI.dll"]
