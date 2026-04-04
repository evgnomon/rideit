@description('Base name for all resources')
param appName string = 'rideit'

@description('Azure region')
param location string = resourceGroup().location

@description('Deployment mode: "functions" or "container"')
@allowed(['functions', 'container'])
param deployMode string = 'functions'

@description('Container image tag (only used for container mode)')
param imageTag string = 'latest'

// --- Azure Functions deployment ---

module func 'functions.bicep' = if (deployMode == 'functions') {
  name: 'functions-deployment'
  params: {
    appName: appName
    location: location
  }
}

// --- Container App deployment ---

module acr 'acr.bicep' = if (deployMode == 'container') {
  name: 'acr-deployment'
  params: {
    appName: appName
    location: location
  }
}

module app 'container.bicep' = if (deployMode == 'container') {
  name: 'container-deployment'
  params: {
    appName: appName
    location: location
    containerImage: '${acr!.outputs.loginServer}/${appName}:${imageTag}'
    registryServer: acr!.outputs.loginServer
  }
}

output appUrl string = deployMode == 'functions' ? func!.outputs.functionAppUrl : app!.outputs.appUrl
