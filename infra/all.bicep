@description('Base name for all resources')
param appName string = 'rideit'

@description('Azure region')
param location string = resourceGroup().location

@description('Container image tag')
param imageTag string = 'latest'

module acr 'acr.bicep' = {
  name: 'acr-deployment'
  params: {
    appName: appName
    location: location
  }
}

module app 'main.bicep' = {
  name: 'app-deployment'
  params: {
    appName: appName
    location: location
    containerImage: '${acr.outputs.loginServer}/${appName}:${imageTag}'
    registryServer: acr.outputs.loginServer
  }
}

output acrLoginServer string = acr.outputs.loginServer
output appUrl string = app.outputs.appUrl
