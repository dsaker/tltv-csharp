# TalkLikeTv Terraform Deployment

This README provides instructions for deploying the TalkLikeTv application infrastructure using Terraform and Azure CLI.

## Prerequisites

- Azure CLI installed
- Terraform CLI installed
- Docker installed
- .NET SDK installed

## Setup Azure Infrastructure

### Login and Register Provider

```bash
az login
az provider register --namespace 'Microsoft.App'
```

### Deploy Infrastructure with Terraform

```bash
cd terraform
terraform init
cp terraform.tfvars.tmpl terraform.tfvars
```

1. Edit terraform.tfvars and fill in the required values, then apply the configuration
2. Uncomment image line in main.tf mcr.microsoft.com/k8se/quickstart:latest
3. Comment out image that will be pushed to azure container registry

```bash
terraform apply 
```

## Build and Deploy Application

### Add Azure Container App Extension

```bash
az extension add --name containerapp --upgrade
az acr login -n talkliketvacr
```

## Configure Database Access

1. Add your IP address to the SQL Server firewall allowed list in the Azure Portal
2. Use the connection string format below (replace with your actual server name) and add to appsettings.json in TalkLikeTv.Mvc (you can also look up this string in the Azure Portal)

```
jdbc:sqlserver://{server_name}.database.windows.net:1433;database=SampleDB;user=azureadmin@{server_name};password={your_password_here};encrypt=true;trustServerCertificate=false;hostNameInCertificate=*.database.windows.net;loginTimeout=30;
```

### Build and Push Docker Image

```bash
cd ../code/TalkLikeTv
export CONTAINER_APP_NAME=talkliketv
export REGISTRY_NAME=talkliketvacr
az acr build \
    -t $REGISTRY_NAME".azurecr.io/"$CONTAINER_APP_NAME":helloworld" \
    -r $REGISTRY_NAME .
```

1. Change image to name to the one you just pushed in main.tf and reapply terraform

## Generate Database Scripts

```bash
cd TalkLikeTv/TalkLikeTv.EntityModels
dotnet ef migrations script --context TalkliketvContext -o Migrations/InitialCreate.sql
```

- you can apply the InitialCreate.sql scirpt using you favorite IDE or in the [Azure Portal](https://learn.microsoft.com/en-us/azure/azure-sql/database/connect-query-portal?view=azuresql)



