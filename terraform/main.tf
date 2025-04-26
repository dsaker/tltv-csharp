# Create an Azure Container Apps Environment
resource "azurerm_container_app_environment" "aca_env" {
  name                = "talkliketv-aca-env"
  resource_group_name = azurerm_resource_group.talkliketv.name
  location            = azurerm_resource_group.talkliketv.location
}

# 1. Create a User-Assigned Managed Identity
resource "azurerm_user_assigned_identity" "container_app_identity" {
  name                = "talkliketv-app-identity"
  resource_group_name = azurerm_resource_group.talkliketv.name
  location            = azurerm_resource_group.talkliketv.location
}

# 2. Assign AcrPull Role to the User-Assigned Identity
resource "azurerm_role_assignment" "acr_pull_user_identity" {
  principal_id         = azurerm_user_assigned_identity.container_app_identity.principal_id
  role_definition_name = "AcrPull"
  scope                = azurerm_container_registry.acr.id
  depends_on           = [
    azurerm_user_assigned_identity.container_app_identity,
    azurerm_container_registry.acr
  ]
}

# Optional: Add a small delay after role assignment for propagation
resource "time_sleep" "wait_for_user_rbac" {
  depends_on = [
    azurerm_role_assignment.acr_pull_user_identity,
  ]
  create_duration = "30s"
}

# 3. Create the Azure Container App using the User-Assigned Identity
resource "azurerm_container_app" "talkliketv" {
  name                         = "talkliketv-app"
  container_app_environment_id = azurerm_container_app_environment.aca_env.id
  resource_group_name          = azurerm_resource_group.talkliketv.name
  revision_mode                = "Single"

  # Depend on the RBAC delay for the user identity
  depends_on = [
    time_sleep.wait_for_user_rbac]

  # Assign the User-Assigned Identity
  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.container_app_identity.id]
  }

  # 4. Use the User-Assigned Identity for registry access
  registry {
    server   = azurerm_container_registry.acr.login_server
    identity = azurerm_user_assigned_identity.container_app_identity.id # Use the User-Assigned Identity ID
  }

  secret {
    name = "mysql-pwd"  # Changed from MySqlPwd
    value = var.my_sql_pwd
  }

  secret {
    name = "mysql-usr"  # Changed from MySqlUsr
    value = var.my_sql_usr
  }

  secret {
    name = "azure-translate-key"  # Changed from AzureTranslateKey 
    value = var.azure_translate_key
  }

  secret {
    name = "azure-tts-key"  # Changed from AzureTtsKey
    value = var.azure_tts_key
  }

  secret {
    name = "azure-region"  # Changed from AzureRegion
    value = var.azure_region
  }

  template {
    container {
      name   = "talkliketvcontainerapp"
      //image  = "mcr.microsoft.com/k8se/quickstart:latest"
      image  = "${azurerm_container_registry.acr.login_server}/talkliketv:helloworld"
      cpu    = 0.25
      memory = "0.5Gi"
           
      env {
        name  = "MY_SQL_PWD"
        secret_name = "mysql-pwd"  # Changed from MySqlPwd
      }
      env {
        name = "MY_SQL_USR"
        secret_name = "mysql-usr"  # Changed from MySqlUsr
      }
      env {
        name  = "AZURE_TRANSLATE_KEY"
        secret_name = "azure-translate-key"  # Changed from AzureTranslateKey
      }
      env {
        name = "AZURE_TTS_KEY"
        secret_name = "azure-tts-key"  # Changed from AzureTtsKey
      }
      env {
        name = "AZURE_REGION"
        secret_name = "azure-region"  # Changed from AzureRegion
      }
    }
  }

  ingress {
    external_enabled = true
    target_port      = 8080
    transport        = "auto"
    traffic_weight {
      percentage = 100
      latest_revision = true
    }
  }
}

