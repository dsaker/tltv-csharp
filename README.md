# tltv-csharp

## Table of Contents
- [tltv-csharp](#tltv-csharp)
  - [Table of Contents](#table-of-contents)
  - [Introduction](#introduction)
    - [Technologies Used](#technologies-used)
    - [Required Tools](#required-tools)
  - [Getting Started](#getting-started)
  - [Initialize the database](#initialize-the-database)
  - [Setup azure translate and text-to-speech](#setup-azure-translate-and-text-to-speech)
  - [Build and Run the Projects](#build-and-run-the-projects)
    - [Run the MVC Project](#run-the-mvc-project)
    - [Run the WebAPI Project](#run-the-webapi-project)
    - [Run the FastEndpoints Project](#run-the-fastendpoints-project)
  - [Contributing](#contributing)
  - [License](#license)

## Introduction

TalkLikeTv is a language learning application designed to address limitations found in other popular language learning apps, such as Pimsleur, Babbel, and Duolingo. While these tools serve as strong foundational resources, they often plateau at the intermediate level.

This application generates a Pimsleur-like audio course from any file selected by the user. Subtitles from current TV shows from any country can be used to create these courses. This approach offers several benefits: it familiarizes users with contemporary slang, improves understanding of spoken dialogue, and practices proper pronunciation and accent.

The book [Real-World Web Development with .NET 9](https://github.com/markjprice/web-dev-net9?tab=readme-ov-file#real-world-web-development-with-net-9-first-edition) was used heavily in creating this application. I would highly recommend it to anyone wanting to lear c#.

### Technologies Used
- **C#** for backend development
- **ASP.NET Core MVC** for the web application
- **Entity Framework Core** for database management
- **Azure Services** for translation and text-to-speech
- **Docker** for containerization

### Required Tools

- Install [Docker](https://docs.docker.com/engine/install/) for containerization.
- Install [.NET SDK](https://dotnet.microsoft.com/download/dotnet) (version 9.0 or later).
- Create an [Azure Account](https://portal.azure.com/) for using Azure services.
- Install the [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) for managing Azure resources.

## Getting Started

1. Clone the repository:
    ```bash
    git clone https://github.com/yourusername/tltv-csharp.git
    cd tltv-csharp
    ```

2. Restore dependencies:
    ```bash
    dotnet restore
    ```

3. Open the project in your favorite IDE

## Initialize the database

1. Install and run the Azure SQL Edge container image
    ```bash
    docker pull mcr.microsoft.com/azure-sql-edge:latest
    docker run --cap-add SYS_PTRACE -e 'ACCEPT_EULA=1' -e 'MSSQL_SA_PASSWORD=s3cret-Ninja' -p 1433:1433 --name azuresqledge -d mcr.microsoft.com/azure-sql-edge
    ```
2. Initialize the database using Entity Framework Core:
    ```bash
    cd TalkLikeTv.EntityModels
    dotnet ef database update
    ```

    > **Tip:** Ensure the `dotnet-ef` CLI tool is installed by running `dotnet tool install --global dotnet-ef` if needed.

## Setup azure translate and text-to-speech

1. create .env file
    ```bash
    cd TalkLikeTv.Mvc
    mv .env.default .env
    ```
2. Obtain your [translate](https://learn.microsoft.com/en-us/answers/questions/1192881/how-to-get-microsoft-translator-api-key) and [text-to-speech](https://learn.microsoft.com/en-us/azure/ai-services/speech-service/get-started-text-to-speech?source=recommendations&tabs=macos%2Cterminal&pivots=programming-language-csharp) api keys and add them to the .env file

## Build and Run the Projects

### Run the MVC Project
1. Navigate to the `TalkLikeTv.Mvc` directory:
    ```bash
    cd TalkLikeTv.Mvc
    ```

2. Build and run the project:
    ```bash
    dotnet build
    dotnet run
    ```

3. Access the application at `http://localhost:5001`.

### Run the WebAPI Project
1. Navigate to the `TalkLikeTv.WebApi` directory:
    ```bash
    cd TalkLikeTv.WebApi
    ```

2. Build and run the project:
    ```bash
    dotnet build
    dotnet run
    ```

3. Access the API at `http://localhost:5000`.

### Run the FastEndpoints Project
1. Navigate to the `TalkLikeTv.FastEndpoints` directory:
    ```bash
    cd TalkLikeTv.FastEndpoints
    ```

2. Build and run the project:
    ```bash
    dotnet build
    dotnet run
    ```

3. Access the application at `http://localhost:5002`.

## Contributing

Contributions are welcome! To contribute:

1. Fork the repository.
2. Create a new branch for your feature or bugfix:
    ```bash
    git checkout -b feature-name
    ```
3. Commit your changes and push them to your fork:
    ```bash
    git push origin feature-name
    ```
4. Submit a pull request to the main repository.

Please ensure your code follows the project's coding standards and includes appropriate tests.

## License

This project is licensed under the MIT License. See the LICENSE file for details.
