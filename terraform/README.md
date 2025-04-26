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

## Setup .env file

```bash
cd terraform
terraform init
cp terraform.tfvars.tmpl terraform.tfvars
```

1. Edit terraform.tfvars and fill in the required values
2. Obtain your [translate](https://learn.microsoft.com/en-us/answers/questions/1192881/how-to-get-microsoft-translator-api-key) and [text-to-speech](https://learn.microsoft.com/en-us/azure/ai-services/speech-service/get-started-text-to-speech?source=recommendations&tabs=macos%2Cterminal&pivots=programming-language-csharp) api keys and add them to the tfvars file
3. Uncomment image line in main.tf for mcr.microsoft.com/k8se/quickstart:latest
4. Comment out image that will be pushed to azure container registry in following steps

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

## Initialize the database

1. Change the username and password in the following commands to the same values in your tfvars file
   ```bash
    export ASPNETCORE_ENVIRONMENT=Production
    export MY_SQL_USR={username}
    export MY_SQL_PWD={password}
   ```

2. Initialize the database using Entity Framework Core:
    ```bash
    cd TalkLikeTv.EntityModels
    dotnet tool install --global dotnet-ef
    dotnet ef database update InitialBaseline
    dotnet ef database update AddPopularityColumn
    ```

3. Run scripts to fill the data in the tables
    ```bash
    cd ../TalkLikeTv/TalkLikeTv.Scripts
    dotnet run -- all    # Run all data population scripts
    dotnet run -- tokens # Generate authentication tokens
    ```


