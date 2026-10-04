# Podman and Kubernetes

## Build images

Run these commands from the repository root. The build context must be the root because each Containerfile copies its service project directory.

```powershell
podman build -f ProductService/Containerfile -t localhost/product-service:1.0 .
podman build -f OrderService/Containerfile -t localhost/order-service:1.0 .
podman build -f UserService/Containerfile -t localhost/user-service:1.0 .
podman build -f PaymentService/Containerfile -t localhost/payment-service:1.0 .
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

## Open Swagger

Port-forward each ClusterIP Service from a separate terminal:

```powershell
kubectl port-forward -n ecommerce svc/product-service 5012:80
kubectl port-forward -n ecommerce svc/order-service 5080:80
kubectl port-forward -n ecommerce svc/user-service 5078:80
kubectl port-forward -n ecommerce svc/payment-service 5022:80
```

Then browse to `/swagger` on the corresponding localhost port. OrderService reaches ProductService at `http://product-service` inside the cluster.
