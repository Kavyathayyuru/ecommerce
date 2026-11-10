# Podman and Kubernetes

## Build images

Run these commands from the repository root. The build context must be the root because each Containerfile copies its service project directory.

```powershell
podman build -f ProductService/Containerfile -t localhost/product-service:1.0 .
podman build -f OrderService/Containerfile -t localhost/order-service:1.0 .
podman build -f UserService/Containerfile -t localhost/user-service:1.0 .
podman build -f PaymentService/Containerfile -t localhost/payment-service:1.0 .
podman build -f Gateway/Containerfile -t localhost/gateway:1.0 .
```

## Deploy to Kubernetes

The services use separate SQL Server databases. LocalDB is Windows-only and is not reachable from a Pod. Provide a SQL Server host that is reachable inside the cluster, provision the four databases, and create this Secret before applying the workloads. Replace the server, username, and password with your environment's values:

```powershell
kubectl apply -f k8s/microservices.yaml
kubectl create secret generic ecommerce-db-connection-strings -n ecommerce `
  --from-literal=ProductService='Server=sqlserver,1433;Database=ProductDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;' `
  --from-literal=OrderService='Server=sqlserver,1433;Database=OrderDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;' `
  --from-literal=UserService='Server=sqlserver,1433;Database=UserDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;' `
  --from-literal=PaymentService='Server=sqlserver,1433;Database=PaymentDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;'
kubectl apply -f k8s/microservices.yaml
kubectl get pods,services -n ecommerce
```

Apply each service's EF Core migrations to the target SQL Server before testing its API. The rows previously stored in Windows LocalDB are not copied into the cluster database; seed ProductDb there separately if you want the catalog items available.

The first apply creates the namespace. The second apply starts the Deployments after the Secret exists. For production, keep database credentials in a managed secret store and change `ASPNETCORE_ENVIRONMENT` to `Production` to disable Swagger.

The manifest uses `localhost/...` image names. Make those images available to the Kubernetes node (for example, load them into a local kind/minikube cluster) or retag and push them to a registry, then update each Deployment's `image` field.

The GitHub Actions workflow builds and publishes images for Product, Order, User, Payment, and Gateway to GHCR on pushes to `main` or `master`. It deploys only when the repository has a `KUBE_CONFIG` Actions secret. Ensure the cluster can pull the published images (for example, make the GHCR packages public or configure a Kubernetes image pull secret with read access to them) before enabling deployment. The database connection-string Secret described above must also exist in the cluster.

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
