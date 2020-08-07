FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine

RUN apk add --update tzdata
	
WORKDIR /app
COPY /deploy ./WebAPI

RUN mkdir ./WebAPI/config

WORKDIR /app/WebAPI
ENTRYPOINT ["dotnet", "Booth.PortfolioManager.Web.dll"]