FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine

RUN apk add --update tzdata

RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
	
WORKDIR /app
COPY /deploy ./WebAPI

RUN mkdir ./WebAPI/config

WORKDIR /app/WebAPI
ENTRYPOINT ["dotnet", "Booth.PortfolioManager.Web.dll"]