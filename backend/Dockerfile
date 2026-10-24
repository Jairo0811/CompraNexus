FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ./backend/CompraNexus.Api/CompraNexus.Api.csproj backend/CompraNexus.Api/
RUN dotnet restore backend/CompraNexus.Api/CompraNexus.Api.csproj
COPY ./backend/CompraNexus.Api/ backend/CompraNexus.Api/
RUN dotnet publish backend/CompraNexus.Api/CompraNexus.Api.csproj -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "CompraNexus.Api.dll"]
