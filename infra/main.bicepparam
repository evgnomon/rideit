using './main.bicep'

param appName = 'rideit'
param containerImage = 'rideitacr.azurecr.io/rideit:latest'
param registryServer = 'rideitacr.azurecr.io'
// Set these via --parameters or environment:
// param registryUsername = ''
// param registryPassword = ''
