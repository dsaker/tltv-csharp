variable "my_sql_pwd" {
    type        = string
    description = "password for mysql"
    default     = null
}

variable "my_sql_usr" {
    type        = string
    description = "username for mysql"
    default     = null
}

variable "azure_translate_key" {
    type        = string
    description = "azure translate key"
    default     = null
}

variable "azure_tts_key" {
    type        = string
    description = "azure tts key"
    default     = null
}

variable "azure_region" {
    type        = string
    description = "azure region"
    default     = null
}

variable "resource_group_location" {
  type        = string
  description = "Location for all resources."
  default     = "eastus"
}

variable "resource_group_name_prefix" {
  type        = string
  description = "Prefix of the resource group name that's combined with a random ID so name is unique in your Azure subscription."
  default     = "rg"
}

variable "sql_db_name" {
  type        = string
  description = "The name of the SQL Database."
  default     = "SampleDB"
}
