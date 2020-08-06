FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine

RUN apk add --update tzdata
	
WORKDIR /app
COPY /WebAPI ./WebAPI

COPY /JWT.key ./WebAPI/JWT.key

WORKDIR /app/WebAPI
ENTRYPOINT ["dotnet", "PortfolioManager.Web.dll"]