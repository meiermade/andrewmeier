module App.Articles.Posts.PersonalInfrastructure

open App.Articles
open App.Articles.Shared
open FSharp.ViewEngine
open System
open type Html

let private metadata =
    { permalink = "personal-infrastructure"
      title = "Personal Infrastructure"
      summary = "How I run a small GCP platform with GKE, Pulumi ESC, and Cloudflare"
      cover = "https://assets.meiermade.com/andymeier/articles/shared/gradient-purple-4776537cdf89.webp"
      tags = [| "DevOps"; "Pulumi"; "GCP"; "Kubernetes"; "Cloudflare"; "TypeScript" |]
      createdAt = DateTimeOffset(2026, 8, 11, 0, 0, 0, TimeSpan.Zero) }

let private heading id' label = h2 {
    _class "mt-10 scroll-mt-24"
    _id id'
    text label
}

let private subheading id' label = h3 {
    _class "mt-6 scroll-mt-24"
    _id id'
    text label
}

let private paragraph value = p { text value }

let private inlineCode value = code {
    _class "language-none"
    text value
}

let private link href label = a {
    _href href
    text label
}

let private architectureStep number title description = li {
    _class "relative border-l border-gray-200 pl-6 dark:border-gray-700 lg:border-l-0 lg:border-t lg:pl-0 lg:pt-6"

    span {
        _class
            "absolute -left-3 -top-1 inline-flex size-6 items-center justify-center rounded-full bg-emerald-600 text-xs font-semibold text-white ring-4 ring-white dark:bg-emerald-500 dark:ring-gray-900 lg:-top-3 lg:left-0"

        text number
    }

    h3 {
        _class "m-0 text-base font-semibold text-gray-900 dark:text-gray-100"
        text title
    }

    p {
        _class "mt-2 text-sm leading-6 text-gray-600 dark:text-gray-400"
        text description
    }
}

let private architectureOverview = section {
    _class
        "not-prose my-10 rounded-2xl border border-gray-200 bg-gray-50/70 p-6 dark:border-gray-700 dark:bg-gray-800/40 sm:p-8"

    _ariaLabel "Infrastructure layers"

    div {
        _class "max-w-2xl"

        p {
            _class "text-sm font-semibold uppercase tracking-wider text-emerald-700 dark:text-emerald-400"
            text "Dependency order"
        }

        p {
            _class "mt-2 text-lg font-semibold text-gray-900 dark:text-gray-100"
            text "Identity establishes trust; each later layer consumes the outputs before it."
        }
    }

    ol {
        _class "mt-8 grid grid-cols-1 gap-7 lg:grid-cols-4 lg:gap-6"

        architectureStep
            "1"
            "Platform identity"
            "Projects, service accounts, IAM grants, groups, and workload-identity bindings."

        architectureStep
            "2"
            "Shared infrastructure"
            "Networking, GKE, databases, storage, secrets, edge services, and shared workloads."

        architectureStep
            "3"
            "Environments"
            "Pulumi ESC composes stack outputs, short-lived credentials, secrets, and configuration."

        architectureStep
            "4"
            "Applications"
            "Product repositories build images and deploy workloads into their assigned boundaries."
    }
}

let private content =
    [ p {
          text
              "My personal infrastructure is a small cloud platform for the applications, data systems, automation, and internal tools I operate through Meier Made. It uses managed Google Cloud services for the parts that are expensive to operate well, Kubernetes as the common workload layer, Cloudflare as the edge, and Pulumi to keep the entire system reviewable as code."
      }
      p {
          text
              "The goal is not to imitate a large enterprise platform. It is to create one dependable foundation that I can understand, change safely, and reuse across products without rebuilding identity, networking, databases, observability, and deployment automation every time. The result is deliberately modest in scale but complete enough to support real production workloads."
      }
      nav {
          _ariaLabel "Table of contents"

          h2 {
              _class "mt-8"
              text "Contents"
          }

          ul {
              _class "list-disc"

              for id', label in
                  [ "principles", "Design principles"
                    "platform", "The platform at a glance"
                    "identity", "Identity before infrastructure"
                    "gcp", "The shared GCP foundation"
                    "kubernetes", "Kubernetes as the workload boundary"
                    "edge", "Cloudflare at the edge"
                    "data", "Data, storage, and secrets"
                    "observability", "Observability and analytics"
                    "environments", "Pulumi ESC as the configuration layer"
                    "delivery", "Application delivery"
                    "operations", "Operating the platform"
                    "tradeoffs", "Intentional tradeoffs" ] do
                  li {
                      a {
                          _href $"#{id'}"
                          text label
                      }
                  }
          }
      }
      heading "principles" "Design principles"
      paragraph
          "I use a few principles to decide whether a resource belongs in the platform and how it should be managed. They keep the architecture from becoming either a collection of one-off application stacks or an oversized internal platform."
      ul {
          _class "list-disc"

          li {
              strong { text "Managed where operations matter." }

              text
                  " Google manages the Kubernetes control plane, PostgreSQL, Redis, object storage, secret storage, and the underlying availability and patching concerns for those services."
          }

          li {
              strong { text "One owner for every resource." }

              text
                  " Identity, shared infrastructure, environment composition, and application deployment have distinct Pulumi projects. A resource lives at the lowest layer that can own it without creating duplication."
          }

          li {
              strong { text "Short-lived identity by default." }

              text
                  " CI and supported workloads exchange identity for temporary credentials instead of carrying static cloud keys. Permissions are granted to purpose-specific service accounts rather than to one universal deployer."
          }

          li {
              strong { text "Private origins." }

              text
                  " Worker nodes and data services live on private networking. Cloudflare Tunnel carries application traffic to the cluster without requiring a public Kubernetes load balancer."
          }

          li {
              strong { text "Preview before update." }

              text
                  " Infrastructure changes arrive through pull requests with tests and Pulumi previews. The reviewed main branch, rather than a laptop, is the normal path to production."
          }
      }
      heading "platform" "The platform at a glance"
      paragraph
          "The platform is split into four layers. Three shared repositories define identity, common infrastructure, and environments. Product repositories own the final application-specific deployment. The separation is more important than the repository names: each layer has a narrow responsibility and exports only what the next layer needs."
      architectureOverview
      p {
          text "The dependency direction is intentional. "
          inlineCode "platform-identity"
          text " establishes projects and principals. "
          inlineCode "platform-infrastructure"
          text " uses those principals to create shared resources and access boundaries. "
          inlineCode "environments"

          text
              " turns outputs and secrets into usable configuration. Application stacks receive that configuration without needing to understand how the platform was assembled."
      }
      paragraph
          "Pulumi TypeScript is the common language across these layers. Stack outputs are contracts between projects, while Pulumi ESC is the composition point. This avoids copying values between repositories and makes a dependency visible where it is consumed."
      heading "identity" "Identity before infrastructure"
      p {
          text "The identity layer starts with the "

          link
              "https://cloud.google.com/resource-manager/docs/cloud-platform-resource-hierarchy"
              "Google Cloud resource hierarchy"

          text
              ". It creates the projects that separate platform, product, and data responsibilities, enables the APIs each project needs, and defines the service accounts used by automation and workloads. Project separation provides a useful IAM and billing boundary without requiring each application to recreate the entire platform."
      }
      paragraph
          "I distinguish deployment identities from runtime identities. A deployer can change a specific Pulumi stack and its assigned Kubernetes namespace. A runtime account receives only the Google Cloud permissions the application needs after it starts. The GKE node account has its own infrastructure role and is not reused as an application identity."
      p {
          text "For CI, Pulumi ESC uses OpenID Connect to obtain short-lived Google credentials. "

          link
              "https://www.pulumi.com/docs/esc/integrations/dynamic-login-credentials/gcp-login/"
              "ESC's GCP login integration"

          text
              " exchanges the environment's OIDC assertion for access to a designated deployer account. GitHub Actions needs permission to request an identity token, but it does not need a long-lived Google service-account key in the repository."
      }
      p {
          text "Inside the cluster, "

          link
              "https://cloud.google.com/kubernetes-engine/docs/concepts/workload-identity"
              "Workload Identity Federation for GKE"

          text
              " maps Kubernetes service accounts to narrowly scoped Google service accounts. Snowplow components, data workloads, and product services can therefore authenticate to Google APIs without mounting cloud keys into their pods. Some external integrations still impose their own credential constraints, but the normal platform path is keyless."
      }
      paragraph
          "There is one unavoidable bootstrap boundary: the identity stack cannot create the identity required to deploy itself. A pre-existing workload-identity pool and initial deployer establish that root of trust. After that bootstrap, the identity project manages the normal grants and exports consumed by the rest of the platform."
      heading "gcp" "The shared GCP foundation"
      p {
          text
              "The shared infrastructure runs in a custom Google Cloud VPC. Google Kubernetes Engine worker nodes use private addresses, and the application and service address ranges are allocated separately from the node subnet. A Cloud NAT gateway provides controlled outbound internet access with a stable egress address, while the cluster does not depend on public addresses for individual nodes."
      }
      p {
          text "The cluster uses "

          link
              "https://cloud.google.com/kubernetes-engine/docs/concepts/autopilot-overview#standard"
              "GKE Standard mode"

          text
              " because I want explicit control over node pools, Kubernetes resources, and workload placement. A primary node pool handles the regular baseline and can scale with demand. A secondary pool can scale down to zero and provides additional capacity without keeping every possible node running all the time. Google manages the control plane, upgrades follow the regular release channel, and Workload Identity is enabled at the cluster level."
      }
      paragraph
          "Artifact Registry stores application container images close to the cluster. Application pipelines build immutable images, push them to the shared registry, and give Kubernetes the resulting image reference. This makes the deployed artifact explicit and keeps image production in the same reviewed workflow as the deployment that consumes it."
      subheading "availability" "Availability profile"
      paragraph
          "The platform is intentionally zonal. The GKE cluster, Cloud SQL instance, and primary workload placement favor one zone instead of paying for regional redundancy. That is an acceptable trade for my current applications: a zonal outage can interrupt service, but the lower steady-state cost lets me run a proper managed platform continuously. Workload replicas, health probes, backups, and reproducible infrastructure reduce other failure modes without pretending that the system is multi-region."
      heading "kubernetes" "Kubernetes as the workload boundary"
      paragraph
          "Kubernetes is the common execution layer, not the place where every concern is implemented. Stateless web applications, agents, data workloads, tunnel connectors, Seq, and the Snowplow processing components run there. PostgreSQL, Redis, object storage, secrets, Pub/Sub, and BigQuery remain managed Google Cloud services outside the cluster."
      paragraph
          "Each product or data workload receives its own namespace. The platform applies default CPU and memory requests, namespace-level limits, and role bindings that grant a product deployer administrative access only inside its namespace. A deployment for one product should not need cluster-wide credentials or permission to change another product's workloads."
      paragraph
          "Pods use non-root security contexts where the image supports them, drop unnecessary Linux capabilities, declare resource requests and limits, and expose liveness and readiness probes. These are small controls, but they make scheduling and failure recovery predictable on a compact cluster. They also give a deployment pipeline an objective signal that a rollout is ready before post-deployment checks begin."
      paragraph
          "The shared infrastructure project owns cluster-wide boundaries such as namespaces and RBAC. Application projects own their Deployments, Services, configuration, and product-specific service accounts. That division keeps the platform reusable while allowing each application to evolve independently."
      heading "edge" "Cloudflare at the edge"
      p {
          text
              "Cloudflare provides DNS, traffic filtering, access control, and the connection from the public edge to private services. A "

          link "https://developers.cloudflare.com/cloudflare-one/connections/connect-networks/" "Cloudflare Tunnel"

          text
              " is initiated outbound by cloudflared, so the origin does not require a public IP address or an inbound firewall opening. Public DNS records point to tunnel hostnames rather than directly to the cluster."
      }
      paragraph
          "The platform runs a shared tunnel connector for platform services. Applications can also own a dedicated connector when that creates a cleaner lifecycle and access boundary. The high-level pattern remains the same: Cloudflare accepts the request, applies edge policy, and sends it through an authenticated tunnel to a ClusterIP service. There is no public Kubernetes ingress load balancer in the request path."
      p {
          text "Administrative applications are protected with "
          link "https://developers.cloudflare.com/cloudflare-one/access-controls/" "Cloudflare Access"

          text
              ". Interactive access uses Google Workspace as the identity provider, while service tokens cover narrowly defined machine-to-machine routes. Public endpoints, such as websites and analytics collection, use explicit bypass policies rather than being accidentally exposed by a broad rule."
      }
      paragraph
          "Customer identity is a separate concern. Auth0 provides the shared customer-facing tenant, branded login experience, and custom authentication domain. Cloudflare Access answers “who can reach this administrative service?”; Auth0 answers “who is the customer using this product?” Keeping those roles distinct prevents edge administration policy from leaking into product authentication."
      heading "data" "Data, storage, and secrets"
      p {
          text "The transactional data layer is a private "
          link "https://cloud.google.com/sql/docs/postgres" "Cloud SQL for PostgreSQL"

          text
              " instance. It has no public IPv4 address, requires encrypted connections, and exposes its private address only inside the VPC. A platform bootstrap job creates logical databases and restricted application roles so products do not share one superuser credential."
      }
      p {
          text
              "Automated backups, point-in-time recovery, and deletion protection cover the most important failure paths. "

          link "https://cloud.google.com/sql/docs/postgres/backup-recovery/pitr" "Point-in-time recovery"

          text
              " is particularly useful because many database incidents are logical mistakes rather than disk failures. Protection also means destructive infrastructure changes must be staged deliberately instead of disappearing in the same update that removes a resource from code."
      }
      p {
          text "A private "
          link "https://cloud.google.com/memorystore/docs/redis/redis-overview" "Memorystore for Redis"

          text
              " instance supports caching and short-lived application state. It uses the BASIC tier rather than a replicated high-availability tier. Redis is treated as recoverable infrastructure; PostgreSQL and object storage remain the durable systems of record."
      }
      paragraph
          "Cloud Storage serves two different purposes. Private buckets hold platform data that should be reachable only by designated service accounts. The public assets bucket serves immutable website assets through a custom domain. Prefix-scoped IAM lets a publisher manage one site's files without gaining write access to every object in the bucket."
      p {
          text
              "Secret Manager holds runtime credentials and integration secrets. Pulumi creates the secret containers and IAM policies, while values are populated separately so plaintext never appears in the infrastructure source. "

          link "https://www.pulumi.com/docs/esc/" "Pulumi ESC"

          text
              " retrieves the versions at environment-open time and passes them to the stack as secret Pulumi configuration or process environment values."
      }
      heading "observability" "Observability and analytics"
      p {
          strong { text "Seq" }

          text
              " is the shared destination for structured application logs and traces. Applications receive the internal ingestion endpoint through their environment and send machine-readable events rather than unstructured log lines. The Seq interface is routed through the platform tunnel and protected with Cloudflare Access, while ingestion uses a scoped API key."
      }
      p {
          text
              "Centralized events make it possible to follow a request across services, search by structured properties, and inspect production errors without opening a shell in a pod. Kubernetes logs are still useful for low-level diagnosis, but "

          link "https://docs.datalust.co/docs" "Seq"
          text " is the normal starting point because it preserves the application context attached to each event."
      }
      p {
          strong { text "Snowplow" }

          text
              " provides the behavioral analytics pipeline. Collectors in GKE receive website events and publish good and bad records to Pub/Sub. An enrichment workload adds campaign attribution and referrer context, then a loader writes validated events to BigQuery in the data project. Failed records follow separate topics instead of silently disappearing."
      }
      p {
          text "This follows Snowplow's "

          link
              "https://docs.snowplow.io/docs/api-reference/loaders-storage-targets/bigquery-loader/"
              "BigQuery loading architecture"

          text
              ": collection and validation happen as a stream, while BigQuery becomes the durable analytical store. Separate Kubernetes service accounts give the collector, enricher, and loader only the Pub/Sub or BigQuery access each component requires."
      }
      heading "environments" "Pulumi ESC as the configuration layer"
      paragraph
          "Infrastructure outputs are useful only when downstream stacks can consume them safely. The environments repository defines Pulumi ESC environments as infrastructure code and composes the platform into product-specific views. Shared fragments provide Cloudflare and platform values; product environments add their own identity, secrets, namespace, and application configuration."
      paragraph
          "An environment can open outputs from the identity and infrastructure stacks, exchange its OIDC identity for a short-lived Google token, retrieve selected Secret Manager values, and construct files such as a kubeconfig. It then exposes only the Pulumi configuration and environment variables needed by the target stack."
      paragraph
          "This removes a large class of repository secrets and copied configuration. A product pipeline does not need to know the cluster endpoint, registry location, database address, Cloudflare account identifiers, or runtime secret values in advance. It opens its environment and receives a coherent snapshot assembled from the stacks that own those values. Secret values remain marked secret as they move through ESC and Pulumi."
      paragraph
          "The environment graph also documents dependency order. Identity outputs feed the platform environment; platform outputs feed product environments; product stacks consume those environments. When a shared output changes, previews show the effect where it will be used rather than relying on a person to update several independent configuration files."
      heading "delivery" "Application delivery"
      paragraph
          "Application repositories own their final deployment layer at a high level. A typical stack builds a container, pushes it to Artifact Registry, deploys it to the assigned namespace, creates a ClusterIP service, and connects the appropriate Cloudflare hostname. Applications can add product-specific storage, customer identity clients, WAF rules, or scheduled workloads without placing those resources in the shared platform project."
      paragraph
          "The platform supplies boundaries and capabilities rather than one universal application chart. That keeps deployment code close to the application version it runs. It also means a product change can update its image and Kubernetes resources without previewing unrelated shared databases, IAM grants, or cluster services."
      paragraph
          "Pull requests run application tests and a Pulumi preview. After merge, GitHub Actions authenticates to Pulumi, opens the stack's ESC environment, builds the reviewed source, and performs the update. Health probes gate the Kubernetes rollout, and applications can run browser tests or API checks against the deployed service before the workflow is considered complete."
      p {
          text "I normally do not run "
          inlineCode "pulumi up"

          text
              " from my workstation. Local checks and previews are useful, but the main-branch workflow is the consistent deployment path. It creates an audit trail linking source, preview, review, update, and post-deployment evidence."
      }
      heading "operations" "Operating the platform"
      paragraph
          "Most routine operation is deliberately uneventful. GKE and the managed data services handle infrastructure health, Kubernetes restarts unhealthy containers, Cloudflare maintains edge connectivity, and deployment workflows reconcile code with the live resources. My work is usually reviewing a preview, inspecting Seq, checking a rollout, or tracing a failed event through Pub/Sub and BigQuery."
      paragraph
          "Changes to shared resources move in dependency order. Identity changes land before infrastructure that consumes a new principal. Infrastructure outputs land before environments reference them. Environment changes land before an application expects the new configuration. Pull request previews make that sequence visible and keep a single large update from hiding several independent assumptions."
      paragraph
          "Protected resources require extra care. For example, removing a protected database or secret is intentionally a multi-step process: first change the protection setting and deploy that state, then remove the resource in a later reviewed update. The inconvenience is useful because it prevents a routine refactor from becoming an immediate destructive operation."
      paragraph
          "The source of truth is the deployed TypeScript and ESC configuration plus the CI workflows that apply it. Architecture articles and READMEs are explanations, not executable truth, so they need to be kept current as the platform changes."
      heading "tradeoffs" "Intentional tradeoffs"
      paragraph
          "This platform optimizes for a solo operator running several real services, not for zero downtime under every regional failure. A zonal GKE cluster and Cloud SQL instance can both be unavailable during a zone-level incident. BASIC Redis has no replica to promote. A shared cluster and database instance also create more shared blast radius than fully independent product platforms."
      paragraph
          "I accept those risks because regional clusters, highly available Redis, and separate database instances for every small product would materially increase the monthly baseline and operational surface. Durable data receives backups, point-in-time recovery, private networking, and deletion protection. Recoverable services are recreated from Pulumi. Applications declare health checks and resource bounds. That is the reliability level the current workloads justify."
      paragraph
          "The architecture also depends heavily on Google Cloud, Cloudflare, Pulumi, and Kubernetes. That is intentional. Portability is less valuable to me than having one coherent system with managed operations, explicit identity, private networking, and repeatable delivery. The source code describes how the pieces fit together, but I am not trying to make every service interchangeable."
      paragraph
          "The measure of success is straightforward: I can add an application by defining its identity and namespace, composing an environment, and deploying a product-owned stack. It receives secure configuration, private data access, logs, analytics, and an edge route without rebuilding the platform. That is enough infrastructure to support the work while remaining small enough for one person to understand." ]

let article = Article.create metadata (ArticlePage.primary metadata content)
