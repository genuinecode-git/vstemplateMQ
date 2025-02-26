# Microservice App with MQ Template
[![tech](https://img.shields.io/badge/powredby-.netcore-purple.svg)](https://dotnet.microsoft.com/en-us/download) [![NUnit License](https://img.shields.io/badge/powredby-NUnit-green.svg)](https://nunit.org/) [![Mediator](https://img.shields.io/badge/powredby-MediatR-blue.svg)](https://www.nuget.org/packages/mediatr/) [![DomainDrivenDesign](https://img.shields.io/badge/powredby-DDD-red.svg)](https://en.wikipedia.org/wiki/Domain-driven_design) [![Swagger](https://img.shields.io/badge/powredby-swagger-gree.svg)](https://swagger.io/) [![SeriLog](https://img.shields.io/badge/powredby-serilog-orange.svg)](https://serilog.net/)[![RabbitMQ](https://img.shields.io/badge/powredby-serilog-yellow.svg)](https://www.rabbitmq.com/)

This is a .NET Core solution template for a Microservice API using CQRS architecture with MediatR, Dapper, FluentValidation, RabitMQ and Entity Framework Core.

## 📌 Features
- **CQRS Architecture** (MediatR)
- **Dapper** for efficient queries
- **Entity Framework Core** for persistence
- **NUnit** for unit testing
- **Web API** interface
- **RabbitMQ** messaging

## Project Structure

📂 ApplicationTemplate <br>
 ├── 📂 src <br>
    &nbsp;&nbsp;&nbsp;├── 📂 ApplicationTemplate.API            # ASP.NET Core Web API <br>
    &nbsp;&nbsp;&nbsp;├── 📂 ApplicationTemplate.Domain         # Domain models <br>
    &nbsp;&nbsp;&nbsp;├── 📂 ApplicationTemplate.Infrastructure # Data access (EF Core) <br>
 ├── 📂 ApplicationTemplate.Tests          # NUnit test project <br>
 ├── 🗂️ ApplicationTemplate.sln <br>


---

## 🚀 Installation

### **1️⃣ Install the Template**

1. Clone the repository
2. Open Application foler (cd application)
3. Run
```powershell
dotnet new --install .
```

### After Install use

```powershell
dotnet new MicroserviceAPIMq -n <YourSolutionName>
```
#### In Visual Studio

![Alt Text](https://github.com/genuinecode-git/vstemplateMQ/blob/main/TemplateMQ/VisualStudio.PNG)

