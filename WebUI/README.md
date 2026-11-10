# E-Commerce Web UI

React + Vite frontend for the microservices application.

## Run locally

```powershell
npm install
$env:VITE_API_BASE_URL = "http://localhost:5005"
npm run dev
```

## Build the production image

Run from the repository root:

```powershell
podman build -f WebUI/Containerfile --build-arg VITE_API_BASE_URL=http://localhost:5005 -t localhost/ecommerce-web-ui:1.0 .
```
