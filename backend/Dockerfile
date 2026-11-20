FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["MerxoSell.API.csproj", "./"]

RUN dotnet restore "MerxoSell.API.csproj"

COPY . .

RUN dotnet build "MerxoSell.API.csproj" -c Release -o /app/build


FROM build AS publish

RUN dotnet publish "MerxoSell.API.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "MerxoSell.API.dll"]
