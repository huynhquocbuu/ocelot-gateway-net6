FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 as runtime
WORKDIR /ocelot-gateway
COPY --from=build-env /publish .
EXPOSE 5501
ENTRYPOINT ["dotnet", "OcelotGateway.dll"]