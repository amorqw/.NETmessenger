FROM mcr.microsoft.com/dotnet/sdk:8.0
EXPOSE 5431
COPY . /source
WORKDIR /source
RUN dotnet restore

ENTRYPOINT ["dotnet", "run", "--project", "src/NETmessenger.Web/NETmessenger.Web.csproj", "--urls", "http://*:5017"]