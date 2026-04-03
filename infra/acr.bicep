@description('Base name for the registry')
param appName string = 'rideit'

@description('Azure region')
param location string = resourceGroup().location

@description('ACR SKU')
@allowed(['Basic', 'Standard', 'Premium'])
param sku string = 'Basic'

// Azure Container Registry
resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: replace(appName, '-', '')
  location: location
  sku: {
    name: sku
  }
  properties: {
    adminUserEnabled: true
  }
}

output loginServer string = acr.properties.loginServer
output registryName string = acr.name
