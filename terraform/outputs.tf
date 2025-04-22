output "container_app_urls" {
  value = [for ingress in azurerm_container_app.talkliketv.ingress : ingress.fqdn]
}

output "azurerm_container_registry_name" {
  value = azurerm_container_registry.acr.name
}

output "resource_group_name" {
  value = azurerm_resource_group.rg.name
}

output "sql_server_name" {
  value = azurerm_mssql_server.server.name
}
