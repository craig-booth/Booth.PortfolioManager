pipeline {
    	
	agent any

	environment {
		PROJECT      = './Booth.PortfolioManager.Web/Booth.PortfolioManager.Web.csproj'
		TEST_PROJECT1 = './Booth.PortfolioManager.Domain.Test/Booth.PortfolioManager.Domain.Test.csproj'
        TEST_PROJECT2 = './Booth.PortfolioManager.DataServices.Test/Booth.PortfolioManager.DataServices.Test.csproj'
		TEST_PROJECT3 = './Booth.PortfolioManager.Web.Test/Booth.PortfolioManager.Web.Test.csproj'

		INTTEST_PROJECT = './Booth.PortfolioManager.IntegrationTest/Booth.PortfolioManager.IntegrationTest.csproj'
    }

    stages {
		stage('Build') {
			agent { 
				docker { 
					image 'mcr.microsoft.com/dotnet/core/sdk:3.1' 
					reuseNode true
				}
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

				stage('Publish') {
					steps {
						sh "dotnet publish ${PROJECT} --configuration Release --output ./deploy"
					}
				}
			}

		}
		
		stage('Deploy') {
			steps {
				script {
					def dockerImage = docker.build("craigbooth/portfoliomanager")
					httpRequest httpMode: 'POST', responseHandle: 'NONE', url: 'https://portainer.boothfamily.id.au/api/webhooks/f70bd8fe-e97a-4b36-ab0d-86257c4b33dc', wrapAsMultipart: false
				}
            }
		}
    }
	
	post {
		success {
			cleanWs()
		}
	}
}