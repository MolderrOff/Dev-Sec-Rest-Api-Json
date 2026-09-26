FROM mcr.microsoft.com/dotnet/sdk:9.0 
WORKDIR /src

ENV ASPNETCORE_URLS=http://+:8090
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV ASPNETCORE_ENVIRONMENT=Development

EXPOSE 8090

ENTRYPOINT ["dotnet", "watch", "run", "--project", "DevSecRestApiJson/DevSecApi.Host.csproj", "--urls", "http://0.0.0.0:8090"]

