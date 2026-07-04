FROM ://microsoft.com AS build
WORKDIR /src

COPY . .

RUN dotnet restore "CSharp_Interactive_Learning_App.API/CSharp_Interactive_Learning_App.API.csproj"

WORKDIR "/src/CSharp_Interactive_Learning_App.API"
RUN dotnet publish "CSharp_Interactive_Learning_App.API.csproj" -c Release -o /app/out --no-restore

FROM ://microsoft.com AS final
WORKDIR /app

COPY --from=build /app/out .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0

ENTRYPOINT ["dotnet", "CSharp_Interactive_Learning_App.API.dll"]
