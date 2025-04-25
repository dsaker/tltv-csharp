# Create a Resource Group
resource "azurerm_resource_group" "talkliketv" {
  name     = "talkliketv-rg"
  location = var.resource_group_location # Choose your desired Azure region
}

# Create an Azure Container Registry
resource "azurerm_container_registry" "acr" {
  name                = "talkliketvacr"
  resource_group_name = azurerm_resource_group.talkliketv.name
  location            = azurerm_resource_group.talkliketv.location
  sku                = "Standard" # Or "Basic", "Premium", etc.
}