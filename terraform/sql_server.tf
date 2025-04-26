resource "random_pet" "rg_name" {
  prefix = var.resource_group_name_prefix
}

resource "azurerm_resource_group" "rg" {
  name     = random_pet.rg_name.id
  location = var.resource_group_location
}

resource "random_pet" "azurerm_mssql_server_name" {
  prefix = "sql"
}

resource "azurerm_mssql_server" "server" {
    name                         = random_pet.azurerm_mssql_server_name.id
    resource_group_name          = azurerm_resource_group.rg.name
    location                     = azurerm_resource_group.rg.location
    administrator_login          = var.my_sql_usr
    administrator_login_password = var.my_sql_pwd
    version                      = "12.0"
}

/// This allows access from all azure services to the SQL server
/// This is needed for the container app to access the SQL server
resource "azurerm_mssql_firewall_rule" "allow_container_app" {
    server_id = azurerm_mssql_server.server.id
    name                = "AllowContainerApp"
    start_ip_address    = "0.0.0.0"
    end_ip_address      = "0.0.0.0"
}

resource "azurerm_mssql_database" "db" {
  name      = var.sql_db_name
  server_id = azurerm_mssql_server.server.id
}