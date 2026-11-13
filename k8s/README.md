# Podman and Kubernetes

For the service responsibilities and communication boundaries, see the
[architecture overview](../ARCHITECTURE.md).

## Local administrator login

User registration creates customer accounts only. The local development administrator is provisioned from User Secrets (not source-controlled configuration):

```powershell
dotnet user-secrets set "BootstrapAdmin:Email" "admin@localhost" --project UserService
dotnet user-secrets set "BootstrapAdmin:Password" "REPLACE_WITH_A_STRONG_PASSWORD" --project UserService
```

Restart UserService after changing these values. The configured email is assigned the Admin role and the configured password is set at startup. Local services share a development-only JWT signing key; never use it outside local development.

## Build images

Run these commands from the repository root. The build context must be the root because each Containerfile copies its service project directory. The Containerfiles use standard Dockerfile syntax and work with Docker as well as Podman when passed with `-f`.

```powershell
podman build -f ProductService/Containerfile -t localhost/product-service:1.0 .
podman build -f OrderService/Containerfile -t localhost/order-service:1.0 .
podman build -f UserService/Containerfile -t localhost/user-service:1.0 .
podman build -f PaymentService/Containerfile -t localhost/payment-service:1.0 .
podman build -f Gateway/Containerfile -t localhost/gateway:1.0 .
podman build -f WebUI/Containerfile --build-arg VITE_API_BASE_URL=http://localhost:5005 -t localhost/ecommerce-web-ui:1.0 .
```

## Deploy to Kubernetes

The services use separate SQL Server databases. LocalDB is Windows-only and is not reachable from a Pod. Provide a SQL Server host reachable inside the cluster and provision the four databases. Create the namespace and secrets first:

```powershell
kubectl create namespace ecommerce
kubectl create secret generic ecommerce-db-connection-strings -n ecommerce `
  --from-literal=ProductService='Server=sqlserver,1433;Database=ProductDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;' `
  --from-literal=OrderService='Server=sqlserver,1433;Database=OrderDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;' `
  --from-literal=UserService='Server=sqlserver,1433;Database=UserDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;' `
  --from-literal=PaymentService='Server=sqlserver,1433;Database=PaymentDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;'
kubectl create secret generic ecommerce-auth -n ecommerce `
  --from-literal=SigningKey='GENERATE_A_RANDOM_SECRET_AT_LEAST_32_CHARACTERS_LONG' `
  --from-literal=BootstrapAdminEmail='admin@example.com' `
  --from-literal=BootstrapAdminPassword='USE_A_STRONG_SECRET_PASSWORD'
```

Apply each service's EF Core migrations to the target SQL Server before applying the workloads, including the UserService `AddUserRole` and OrderService `AddOrderStatusOutbox` migrations. Configure the target SQL Server connection string in your local environment before invoking `dotnet ef database update --project OrderService` and the corresponding commands for the other services. The rows previously stored in Windows LocalDB are not copied into the cluster database; seed ProductDb there separately if you want catalog items available.

OrderService stores status-change events in its SQL outbox and a hosted worker publishes them through the authenticated `/hubs/orders` SignalR endpoint. The Gateway proxies this endpoint; the UI receives live updates while signed in. Run `dotnet test ECommerceMicroservices.slnx --configuration Release` from the repository root to execute the automated backend unit tests before packaging.

Then apply the workloads and check them:

```powershell
kubectl apply -f k8s/microservices.yaml
kubectl get pods,services -n ecommerce
```

For production, keep database credentials and the JWT signing key in a managed secret store and change `ASPNETCORE_ENVIRONMENT` to `Production` to disable Swagger.

The manifest uses `localhost/...` image names. Make those images available to the Kubernetes node (for example, load them into a local kind/minikube cluster) or retag and push them to a registry, then update each Deployment's `image` field.

The GitHub Actions workflow builds and publishes images for Product, Order, User, Payment, Gateway, and WebUI to GHCR on pushes to `main` or `master`. It deploys only when the repository has a `KUBE_CONFIG` Actions secret. Ensure the cluster can pull the published images (for example, make the GHCR packages public or configure a Kubernetes image pull secret with read access to them) before enabling deployment. The database connection-string Secret described above must also exist in the cluster.

## Open Swagger

Port-forward each ClusterIP Service from a separate terminal:

```powershell
kubectl port-forward -n ecommerce svc/product-service 5012:80
kubectl port-forward -n ecommerce svc/order-service 5080:80
kubectl port-forward -n ecommerce svc/user-service 5078:80
kubectl port-forward -n ecommerce svc/payment-service 5022:80
```

Then browse to `/swagger` on the corresponding localhost port. OrderService reaches ProductService at `http://product-service` inside the cluster. The Gateway is available at `http://localhost:5005` when port-forwarded from a separate terminal:

```powershell
kubectl port-forward -n ecommerce svc/gateway 5005:80
```

The Gateway's Kubernetes configuration routes its requests to the in-cluster service names. The local `appsettings.json` destinations remain configured for local development.

The React UI is available through the `web-ui` Service. Port-forward it separately and open `http://localhost:5173`:

```powershell
kubectl port-forward -n ecommerce svc/web-ui 5173:80
```
