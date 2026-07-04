FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["CSharp_Interactive_Learning_App.API.csproj", "./"]
RUN dotnet restore "CSharp_Interactive_Learning_App.API.csproj"

COPY . .
RUN dotnet publish "CSharp_Interactive_Learning_App.API.csproj" -c Release -o /app/out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/out .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0

ENTRYPOINT ["dotnet", "CSharp_Interactive_Learning_App.API.dll"]
