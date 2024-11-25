# OverEngineering Detector

Analyze project structures for architecture patterns, anti-patterns, and adherence to best practices. This tool combines a **React** frontend and a **.NET Core** backend deployed on **GitHub Pages** and **Azure App Service**.

## Project Overview

- **Frontend**: Built with React, deployed to GitHub Pages.
- **Backend**: .NET Core API leveraging Azure OpenAI, deployed on Azure App Service.

### Live Demo

- **Frontend**: [OverEngineering Detector GitHub Pages](https://glglak.github.io/over-engineering-detector/)
- **Backend API**: Hosted on Azure App Service.

---

## Getting Started

### Frontend

1. Navigate to the [frontend repository](https://glglak.github.io/over-engineering-detector/).
2. Interact with the application by uploading project structures for analysis.

### Backend

1. API is deployed on Azure App Service and processes requests from the frontend.
2. Example Endpoint:
   - `POST /api/analyzer/analyze`: Accepts project structures and returns metrics.

---

## Deployment Instructions

### Frontend

1. **Deploy to GitHub Pages**:
   - Run `npm run build` to build the React app.
   - Deploy using GitHub Actions or `gh-pages`.

2. **Live URL**:
   - Update `NEXT_PUBLIC_API_URL` to point to the backend API.

### Backend

1. **Deploy to Azure App Service**:
   - Publish the .NET project using:
     ```bash
     dotnet publish -c Release
     ```
   - Deploy the published files to Azure App Service.

2. **Configuration**:
   - Add the following App Settings in Azure:
     - `OpenAI_ApiKey`
     - `OpenAI_DeploymentName`
     - `OpenAI_ApiVersion`
     - `OpenAI_Endpoint`

---

## Features

- **Architecture Analysis**:
  - Detects patterns like Clean, Onion, or Microservices.
- **Anti-Patterns**:
  - Identifies overengineering and SOLID violations.
- **Metrics**:
  - Comprehensive insights into directory structure and file types.

---

## Technologies

- **Frontend**: React, TailwindCSS.
- **Backend**: .NET Core, Azure OpenAI, Swagger.

## Future Enhancements

1. Improved complexity scoring for neat projects.
2. Expanded support for additional architecture patterns.

---
