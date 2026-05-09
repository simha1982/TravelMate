@description('Azure region for all TravelMate resources.')
param location string = resourceGroup().location

@description('Short environment name, for example dev, test, uat, or prod.')
@allowed([
  'dev'
  'test'
  'uat'
  'prod'
])
param environmentName string = 'dev'

@description('Unique suffix used in globally unique resource names.')
param uniqueSuffix string = uniqueString(resourceGroup().id)

@description('Administrator login for Azure SQL.')
param sqlAdministratorLogin string

@description('Administrator password for Azure SQL.')
@secure()
param sqlAdministratorPassword string

var namePrefix = 'travelmate-${environmentName}'
var storageName = 'tm${environmentName}${uniqueSuffix}'
var sqlServerName = '${namePrefix}-sql-${uniqueSuffix}'
var appServicePlanName = '${namePrefix}-plan'
var apiAppName = '${namePrefix}-api-${uniqueSuffix}'
var searchName = '${namePrefix}-search-${uniqueSuffix}'
var apiManagementName = '${namePrefix}-apim-${uniqueSuffix}'
var keyVaultName = '${namePrefix}-kv-${uniqueSuffix}'
var appInsightsName = '${namePrefix}-appi'
var logAnalyticsName = '${namePrefix}-log'
var speechName = '${namePrefix}-speech-${uniqueSuffix}'
var openAiName = '${namePrefix}-openai-${uniqueSuffix}'

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

resource audioContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  name: '${storage.name}/default/story-audio'
  properties: {
    publicAccess: 'None'
  }
}

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdministratorLogin
    administratorLoginPassword: sqlAdministratorPassword
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  name: '${sqlServer.name}/TravelMate'
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
    capacity: 1
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource apiApp 'Microsoft.Web/sites@2023-12-01' = {
  name: apiAppName
  location: location
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      minimumTlsVersion: '1.2'
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'TravelMate__StorageAccountName'
          value: storage.name
        }
        {
          name: 'TravelMate__SqlServer'
          value: sqlServer.properties.fullyQualifiedDomainName
        }
        {
          name: 'TravelMate__SqlDatabase'
          value: 'TravelMate'
        }
        {
          name: 'AudioStorage__ContainerName'
          value: 'story-audio'
        }
        {
          name: 'AzureSearch__Endpoint'
          value: 'https://${search.name}.search.windows.net'
        }
        {
          name: 'AzureSearch__IndexName'
          value: 'travelmate-stories'
        }
        {
          name: 'AzureSpeech__Region'
          value: location
        }
        {
          name: 'AzureOpenAI__Endpoint'
          value: openAi.properties.endpoint
        }
      ]
    }
  }
}

resource search 'Microsoft.Search/searchServices@2024-06-01-preview' = {
  name: searchName
  location: location
  sku: {
    name: 'basic'
  }
  properties: {
    replicaCount: 1
    partitionCount: 1
    hostingMode: 'default'
    publicNetworkAccess: 'enabled'
  }
}

resource storySearchIndex 'Microsoft.Search/searchServices/indexes@2024-06-01-preview' = {
  parent: search
  name: 'travelmate-stories'
  properties: {
    fields: [
      {
        name: 'id'
        type: 'Edm.String'
        key: true
        searchable: false
        filterable: true
        sortable: false
        facetable: false
      }
      {
        name: 'placeName'
        type: 'Edm.String'
        searchable: true
        filterable: true
        sortable: true
        facetable: false
      }
      {
        name: 'title'
        type: 'Edm.String'
        searchable: true
        filterable: false
        sortable: true
        facetable: false
      }
      {
        name: 'summary'
        type: 'Edm.String'
        searchable: true
        filterable: false
        sortable: false
        facetable: false
      }
      {
        name: 'languageCode'
        type: 'Edm.String'
        searchable: false
        filterable: true
        sortable: false
        facetable: true
      }
      {
        name: 'categories'
        type: 'Collection(Edm.String)'
        searchable: true
        filterable: true
        sortable: false
        facetable: true
      }
      {
        name: 'sourceName'
        type: 'Edm.String'
        searchable: true
        filterable: true
        sortable: false
        facetable: false
      }
      {
        name: 'sourceUrl'
        type: 'Edm.String'
        searchable: false
        filterable: false
        sortable: false
        facetable: false
      }
    ]
  }
}

resource speech 'Microsoft.CognitiveServices/accounts@2024-10-01' = {
  name: speechName
  location: location
  sku: {
    name: 'S0'
  }
  kind: 'SpeechServices'
  properties: {
    publicNetworkAccess: 'Enabled'
  }
}

resource openAi 'Microsoft.CognitiveServices/accounts@2024-10-01' = {
  name: openAiName
  location: location
  sku: {
    name: 'S0'
  }
  kind: 'OpenAI'
  properties: {
    customSubDomainName: openAiName
    publicNetworkAccess: 'Enabled'
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2024-04-01-preview' = {
  name: keyVaultName
  location: location
  properties: {
    tenantId: tenant().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    enableRbacAuthorization: true
    enabledForTemplateDeployment: true
    publicNetworkAccess: 'Enabled'
  }
}

resource apiManagement 'Microsoft.ApiManagement/service@2023-09-01-preview' = {
  name: apiManagementName
  location: location
  sku: {
    name: 'Developer'
    capacity: 1
  }
  properties: {
    publisherEmail: 'admin@travelmate.local'
    publisherName: 'TravelMate'
  }
}

output apiUrl string = 'https://${apiApp.properties.defaultHostName}'
output apiManagementGatewayUrl string = apiManagement.properties.gatewayUrl
output storageAccountName string = storage.name
output sqlServerFullyQualifiedDomainName string = sqlServer.properties.fullyQualifiedDomainName
output searchServiceName string = search.name
output searchIndexName string = storySearchIndex.name
output speechServiceName string = speech.name
output openAiAccountName string = openAi.name
output keyVaultName string = keyVault.name
