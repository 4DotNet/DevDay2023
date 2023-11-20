targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment that can be used as part of naming resource convention')
param environmentName string

@description('Primary location for all resources')
@allowed([ 'australiaeast', 'canadaeast', 'eastus', 'eastus2', 'francecentral', 'japaneast', 'northcentralus', 'swedencentral', 'switzerlandnorth', 'uksouth' ])
param location string

@allowed([ 'azure', 'openai' ])
param openAiHost string // Set in main.parameters.json
@secure()
param openAiApiKey string = ''
param openAiApiOrganization string = ''

param openAiSkuName string = 'S0'

param chatGptDeploymentName string // Set in main.parameters.json
param chatGptDeploymentCapacity int = 30
param chatGptModelName string = (openAiHost == 'azure') ? 'gpt-35-turbo' : 'gpt-3.5-turbo'
param chatGptModelVersion string = '0613'
param embeddingDeploymentName string // Set in main.parameters.json
param embeddingDeploymentCapacity int = 30
param embeddingModelName string = 'text-embedding-ada-002'

@allowed([ 'basic', 'standard', 'standard2', 'standard3', 'storage_optimized_l1', 'storage_optimized_l2' ])
param searchServiceSkuName string // Set in main.parameters.json
param searchIndexName string // Set in main.parameters.json

// param apiExists bool
// @secure()
// param apiDefinition object

// param webExists bool
// @secure()
// param webDefinition object

@description('Id of the user or app to assign application roles')
param principalId string

var tags = {
  'azd-env-name': environmentName
}

var abbrs = loadJsonContent('./abbreviations.json')
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))

resource rg 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: 'rg-${environmentName}'
  location: location
  tags: tags
}

module monitoring './shared/monitoring.bicep' = {
  name: 'monitoring'
  params: {
    location: location
    tags: tags
    logAnalyticsName: '${abbrs.operationalInsightsWorkspaces}${resourceToken}'
    applicationInsightsName: '${abbrs.insightsComponents}${resourceToken}'
  }
  scope: rg
}

module dashboard './shared/dashboard-web.bicep' = {
  name: 'dashboard'
  params: {
    name: '${abbrs.portalDashboards}${resourceToken}'
    applicationInsightsName: monitoring.outputs.applicationInsightsName
    location: location
    tags: tags
  }
  scope: rg
}

// module registry './shared/registry.bicep' = {
//   name: 'registry'
//   params: {
//     location: location
//     tags: tags
//     name: '${abbrs.containerRegistryRegistries}${resourceToken}'
//   }
//   scope: rg
// }

module keyVault './shared/keyvault.bicep' = {
  name: 'keyvault'
  params: {
    location: location
    tags: tags
    name: '${abbrs.keyVaultVaults}${resourceToken}'
    principalId: principalId
    secrets: [ {
        name: 'openAiApiKey'
        value: openAiApiKey
      }
      {
        name: 'openAiApiOrganization'
        value: openAiApiOrganization
      } ]
  }
  scope: rg
}

// Cognitive services

module openAi 'core/ai/cognitive-services.bicep' = if (openAiHost == 'azure') {
  name: 'openai'
  scope: rg
  params: {
    name: '${abbrs.cognitiveServicesAccounts}${resourceToken}'
    location: location
    tags: tags
    sku: {
      name: openAiSkuName
    }
    deployments: [
      {
        name: chatGptDeploymentName
        model: {
          format: 'OpenAI'
          name: chatGptModelName
          version: chatGptModelVersion
        }
        sku: {
          name: 'Standard'
          capacity: chatGptDeploymentCapacity
        }
      }
      {
        name: embeddingDeploymentName
        model: {
          format: 'OpenAI'
          name: embeddingModelName
          version: '2'
        }
        capacity: embeddingDeploymentCapacity
      }
    ]
  }
}

module searchService 'core/search/search-services.bicep' = {
  name: 'search-service'
  scope: rg
  params: {
    name: 'gptkb-${resourceToken}'
    location: location
    tags: tags
    authOptions: {
      aadOrApiKey: {
        aadAuthFailureMode: 'http401WithBearerChallenge'
      }
    }
    sku: {
      name: searchServiceSkuName
    }
    semanticSearch: 'free'
  }
}

// Application services

module appServicePlan './app/appserviceplan.bicep' = {
  name: 'appserviceplan'
  scope: rg
  params: {
    name: '${abbrs.webServerFarms}${resourceToken}'
    location: location
    tags: tags
    sku: {
      name: 'B1'
    }
  }
}

module api './app/appservice.bicep' = {
  name: 'api'
  scope: rg
  params: {
    name: '${abbrs.appContainerApps}api-${resourceToken}'
    location: location
    appServicePlanId: appServicePlan.outputs.id
    keyVaultName: keyVault.outputs.name
    applicationInsightsName: monitoring.outputs.applicationInsightsName
    runtimeName: 'dotnetcore'
    runtimeVersion: '8.0'
    tags: union(tags, { 'azd-service-name': 'api' })
    appSettings: {
      // Shared by all OpenAI deployments
      OPENAI_HOST: openAiHost
      AZURE_OPENAI_EMB_MODEL_NAME: embeddingModelName
      AZURE_OPENAI_CHATGPT_MODEL: chatGptModelName
      // Specific to Azure OpenAI
      AZURE_OPENAI_SERVICE: openAiHost == 'azure' ? openAi.outputs.endpoint : ''
      AZURE_OPENAI_CHATGPT_DEPLOYMENT: chatGptDeploymentName
      AZURE_OPENAI_EMB_DEPLOYMENT: embeddingDeploymentName
      // Used only with non-Azure OpenAI deployments
      OPENAI_API_KEY: openAiApiKey
      OPENAI_ORGANIZATION: openAiApiOrganization

      AZURE_SEARCH_INDEX: searchIndexName
      AZURE_SEARCH_SERVICE: searchService.outputs.endpoint
    }
  }
}

module web './app/appservice.bicep' = {
  name: 'web'
  scope: rg
  params: {
    name: '${abbrs.appContainerApps}web-${resourceToken}'
    location: location
    appServicePlanId: appServicePlan.outputs.id
    keyVaultName: keyVault.outputs.name
    applicationInsightsName: monitoring.outputs.applicationInsightsName
    runtimeName: 'dotnetcore'
    runtimeVersion: '8.0'
    tags: union(tags, { 'azd-service-name': 'web' })
    appSettings: {
      Services__apiservice__0: api.outputs.endpoint
    }
  }
}

// module appsEnv './shared/apps-env.bicep' = {
//   name: 'apps-env'
//   params: {
//     name: '${abbrs.appManagedEnvironments}${resourceToken}'
//     location: location
//     tags: tags
//     applicationInsightsName: monitoring.outputs.applicationInsightsName
//     logAnalyticsWorkspaceName: monitoring.outputs.logAnalyticsWorkspaceName
//   }
//   scope: rg
// }

// module api './app/containerapp.bicep' = {
//   name: 'api'
//   params: {
//     name: '${abbrs.appContainerApps}api-${resourceToken}'
//     location: location
//     tags: union(tags, {'azd-service-name':  'api' })
//     identityName: '${abbrs.managedIdentityUserAssignedIdentities}api-${resourceToken}'
//     applicationInsightsName: monitoring.outputs.applicationInsightsName
//     containerAppsEnvironmentName: appsEnv.outputs.name
//     containerRegistryName: registry.outputs.name
//     exists: apiExists
//     appDefinition: apiDefinition
//   }
//   scope: rg
// }

// module web './app/containerapp.bicep' = {
//   name: 'web'
//   params: {
//     name: '${abbrs.appContainerApps}web-${resourceToken}'
//     location: location
//     tags: union(tags, {'azd-service-name':  'web' })
//     identityName: '${abbrs.managedIdentityUserAssignedIdentities}web-${resourceToken}'
//     applicationInsightsName: monitoring.outputs.applicationInsightsName
//     containerAppsEnvironmentName: appsEnv.outputs.name
//     containerRegistryName: registry.outputs.name
//     exists: webExists
//     appDefinition: webDefinition
//   }
//   scope: rg
// }

// User roles

module openAiRoleUser 'core/security/role.bicep' = if (openAiHost == 'azure') {
  scope: rg
  name: 'openai-role-user'
  params: {
    principalId: principalId
    roleDefinitionId: '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd'
    principalType: 'User'
  }
}

module searchRoleUser 'core/security/role.bicep' = {
  scope: rg
  name: 'search-role-user'
  params: {
    principalId: principalId
    roleDefinitionId: '1407120a-92aa-4202-b7e9-c0e197c71c8f'
    principalType: 'User'
  }
}

module searchContribRoleUser 'core/security/role.bicep' = {
  scope: rg
  name: 'search-contrib-role-user'
  params: {
    principalId: principalId
    roleDefinitionId: '8ebe5a00-799e-43f5-93ac-243d3dce84a7'
    principalType: 'User'
  }
}

module searchServiceContribRoleUser 'core/security/role.bicep' = {
  scope: rg
  name: 'search-svccontrib-role-user'
  params: {
    principalId: principalId
    roleDefinitionId: '7ca78c08-252a-4471-8644-bb5ff32d4ba0'
    principalType: 'User'
  }
}

// System identities

module openAiRoleBackend 'core/security/role.bicep' = if (openAiHost == 'azure') {
  scope: rg
  name: 'openai-role-backend'
  params: {
    principalId: api.outputs.identityPrincipalId
    roleDefinitionId: '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd'
    principalType: 'ServicePrincipal'
  }
}

module searchRoleBackend 'core/security/role.bicep' = {
  scope: rg
  name: 'search-role-backend'
  params: {
    principalId: api.outputs.identityPrincipalId
    roleDefinitionId: '1407120a-92aa-4202-b7e9-c0e197c71c8f'
    principalType: 'ServicePrincipal'
  }
}

output SERVICE_API_URI string = api.outputs.uri
// output AZURE_CONTAINER_REGISTRY_ENDPOINT string = registry.outputs.loginServer
output AZURE_KEY_VAULT_NAME string = keyVault.outputs.name
output AZURE_KEY_VAULT_ENDPOINT string = keyVault.outputs.endpoint

output OPENAI_HOST string = openAiHost
output AZURE_OPENAI_EMB_MODEL_NAME string = embeddingModelName
output AZURE_OPENAI_CHATGPT_MODEL string = chatGptModelName

// Specific to Azure OpenAI
output AZURE_OPENAI_SERVICE string = (openAiHost == 'azure') ? openAi.outputs.endpoint : ''
output AZURE_OPENAI_RESOURCE_GROUP string = (openAiHost == 'azure') ? rg.name : ''
output AZURE_OPENAI_CHATGPT_DEPLOYMENT string = (openAiHost == 'azure') ? chatGptDeploymentName : ''
output AZURE_OPENAI_EMB_DEPLOYMENT string = (openAiHost == 'azure') ? embeddingDeploymentName : ''

// Azure Search
output AZURE_SEARCH_INDEX string = searchIndexName
output AZURE_SEARCH_SERVICE string = searchService.outputs.endpoint
