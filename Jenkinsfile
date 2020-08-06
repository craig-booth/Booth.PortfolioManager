pipeline {
    agent { docker 'mcr.microsoft.com/dotnet/core/sdk:3.1' }
	
	environment {
		PROJECT      = './Booth.PortfolioManager.RestApi/Booth.PortfolioManager.Web.csproj'
		TEST_PROJECT1 = './Booth.PortfolioManager.RestApi.Test/Booth.PortfolioManager.Domain.Test.csproj'
        TEST_PROJECT2 = './Booth.PortfolioManager.RestApi.Test/Booth.PortfolioManager.DataServices.Test.csproj'
		TEST_PROJECT3 = './Booth.PortfolioManager.RestApi.Test/Booth.PortfolioManager.Web.Test.csproj'

		INTTEST_PROJECT = './Booth.PortfolioManager.RestApi.Test/Booth.PortfolioManager.IntegrationTest.csproj'

		NUGET_KEY = credentials('nuget')
    }

    stages {
		stage('Build') {
			steps {
				sh "dotnet build ${PROJECT} --configuration Release"
            }
		}
	    stage('Test') {
			steps {
				sh "dotnet test ${TEST_PROJECT1} --configuration Release --logger trx --results-directory ./testresults"
				sh "dotnet test ${TEST_PROJECT2} --configuration Release --logger trx --results-directory ./testresults"
				sh "dotnet test ${TEST_PROJECT3} --configuration Release --logger trx --results-directory ./testresults"

				sh "dotnet test ${INTTEST_PROJECT} --configuration Release --logger trx --results-directory ./testresults"
            }
			post {
				always {
					xunit (
						thresholds: [ skipped(failureThreshold: '0'), failed(failureThreshold: '0') ],
						tools: [ MSTest(pattern: 'testresults/*.trx') ]
						)
				}
			}
        }
		stage('Deploy') {
			steps {
				sh "dotnet publish ${PROJECT} --configuration Release --output ./deploy"

				def dockerImage = docker.build("craigbooth/portfoliomanager")
				dockerImage.push();
            }
		}
    }
	
	post {
		success {
			cleanWs()
		}
	}
}