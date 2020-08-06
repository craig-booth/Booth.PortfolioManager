FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine

RUN apk add --update tzdata
	
WORKDIR /app
COPY /deploy ./WebAPI

WORKDIR /app/WebAPI
ENTRYPOINT ["dotnet", "PortfolioManager.Web.dll"]