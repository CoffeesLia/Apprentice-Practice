# 📌 Portal API

## 📝 Descrição

API REST desenvolvida em **.NET 8** para o sistema de gerenciamento de serviços, responsável pela **lógica de negócio**, **persistência de dados** e **integração com o frontend**.  

A arquitetura segue os princípios **SOLID** e **Clean Architecture**, com **JWT para autenticação**, **documentação Swagger** e suporte a **operações em tempo real** via SignalR.

---

## 🚀 Tecnologias Utilizadas

- **Backend:** .NET 8, ASP.NET Core Web API  
- **Persistência:** Entity Framework Core (SQLite/InMemory)  
- **Validação:** FluentValidation  
- **Mapeamento:** AutoMapper  
- **Autenticação:** JWT Bearer  
- **Real-time:** SignalR  
- **Documentação:** Swagger UI  
- **Testes:** xUnit, Moq  

---

## 🏅 Badges

![Build](https://img.shields.io/badge/build-passing-brightgreen)  
![Coverage](https://img.shields.io/badge/coverage-85%25-green)  
![License](https://img.shields.io/badge/license-MIT-blue)

---

## 🖼 Visual

<p align="center">
  <img src="docs/swagger-ui.png" alt="Swagger UI" width="600"/>
</p>

---

## 💻 Como Rodar o Projeto Localmente

### 🔹 Pré-requisitos

- **.NET 8 SDK**  
- **Visual Studio 2022** ou **VS Code** (recomendado)  

### 🔹 Passos

```bash
# Clonar o repositório
git clone https://gitlab.fcalatam.com/fca/ams/portal/portal-api.git

# Acessar a pasta do projeto
cd portal-api

# Restaurar dependências
dotnet restore

# Executar a aplicação
dotnet run

 📂 Estrutura de Pastas (Resumo)
plaintext
src/
 ├─ Application/         # Casos de uso e serviços
 ├─ Domain/              # Entidades e interfaces
 ├─ Infrastructure/      # Implementações de persistência
 ├─ WebApi/              # Controllers e configuração
 └─ Tests/               # Testes unitários e 
 
 🤝 Como Contribuir
Padrão de branches:

feature/nome-da-feature

fix/descricao-do-bug

hotfix/descricao

Formato de commits:

feat: adiciona autenticação JWT

fix: corrige validação de campos

docs: atualiza documentação Swagger

Requisitos:

Sempre atualize a documentação Swagger

Mantenha cobertura de testes ≥ 80%

Siga as convenções de código do projeto

🔗 Links Úteis
Swagger: http://localhost:5000/swagger

Frontend: portal

Documentação .NET: Microsoft Docs

📜 Licença
Este projeto está licenciado sob a licença MIT.

📊 Status do Projeto
🚧 Em desenvolvimento 