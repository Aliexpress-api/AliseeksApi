FROM microsoft/dotnet:latest

COPY . /app

WORKDIR /app

#RUN ["dotnet", "restore"]

#RUN ["dotnet", "build"]

EXPOSE 1460/tcp

ENTRYPOINT ["dotnet", "AliseeksApi.dll", "--server.urls", "http://*:1460"]
