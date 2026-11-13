# E-Commerce Microservices Architecture

## Service boundaries

Each backend owns its API and data model. Services do not read or write another
service's database directly.

| Service | Responsibility and owned data | Main API |
| --- | --- | --- |
| ProductService | Product catalog, descriptions, prices, and stock. Owns `ProductDb` and the product read cache. | `/api/Products` |
| OrderService | Order lifecycle, order snapshots, checkout orchestration, and the status-change outbox. Owns `OrderDb`. | `/api/Orders` and `/hubs/orders` |
| UserService | Registration, password hashing, login, JWT issuance, and user roles. Owns `UserDb`. | `/api/Users` |
| PaymentService | Payment records and payment status transitions. Owns `PaymentDb`. | `/api/Payments` |

The repositories and business services inside ProductService and OrderService
support their owning service; they are not separately deployed services.

## Communication

The Gateway provides the browser-facing API and routes `/api/product`,
`/api/order`, `/api/user`, and `/api/payment` to their corresponding services.
The WebUI calls the Gateway rather than connecting directly to backend services.

During checkout, OrderService validates the customer with UserService, checks
catalog details with ProductService, and requests payment processing from
PaymentService. The services exchange HTTP requests; each service persists
only its own data. Order status changes are recorded with an outbox message in
OrderDb and published to clients over the authenticated SignalR hub.

## Deployment

The solution contains separate projects and container build files for the four
domain services, the Gateway, and the WebUI. The Kubernetes resources are in
[`k8s/microservices.yaml`](./k8s/microservices.yaml); each workload has its own
Deployment and ClusterIP Service. Kubernetes configuration supplies internal
service URLs. Database connection strings and authentication/bootstrap-admin
values are provided as Kubernetes Secrets, not committed in the manifest.

The GitHub Actions workflow builds each backend project and the WebUI, runs
backend tests, and on pushes to `main` or `master` publishes the container
images to GHCR. Kubernetes deployment is enabled only when `KUBE_CONFIG` is
configured and the cluster's namespace and application Secrets are already
provisioned. See [`k8s/README.md`](./k8s/README.md) for local build and
deployment instructions.
