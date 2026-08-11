module App.Articles.Posts.PersonalInfrastructure

open App.Articles
open App.Articles.Shared
open App.Common.View
open FSharp.ViewEngine
open System
open type Datastar
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

let private paragraph value = p { text value }

let private inlineCode value = code {
    _class "language-none"
    text value
}

let private codeBlock language value = pre {
    _class $"language-{language}"

    code {
        _class $"language-{language}"
        span { text value }
    }
}

let private link href label = a {
    _href href
    text label
}

let private systemContextDiagram =
    """flowchart TB
    accTitle: Meier Made platform system context
    accDescr: The developer and product users interact with the Meier Made Platform through GitHub Actions, Pulumi Cloud, Google Cloud, Cloudflare, and identity providers.

    operator["Developer and operator<br/>(Person)<br/>Builds and operates the platform"]
    users["Product users and website visitors<br/>(People)<br/>Use hosted applications"]
    platform["Meier Made Platform<br/>(Software system)<br/>Runs applications, data services, analytics, and operational tooling"]:::system
    github["GitHub Actions<br/>(External system)<br/>Tests, previews, and deploys reviewed changes"]
    pulumi["Pulumi Cloud and ESC<br/>(External system)<br/>Stores state and composes credentials and configuration"]
    gcp["Google Cloud<br/>(External system)<br/>Runs compute, networking, databases, messaging, and storage"]
    cloudflare["Cloudflare<br/>(External system)<br/>Provides DNS, edge policy, Access, and tunnels"]
    identity["Auth0 and Google Workspace<br/>(External systems)<br/>Authenticate customers and administrators"]

    operator -->|Commits and reviews changes| github
    operator -->|Operates services| platform
    github -->|Runs Pulumi programs| pulumi
    pulumi -->|Applies desired state| platform
    users -->|Use applications through| cloudflare
    cloudflare -->|Routes private-origin traffic| platform
    platform -->|Runs on| gcp
    platform -->|Delegates identity to| identity

    classDef system fill:#059669,stroke:#047857,color:#ffffff,stroke-width:3px"""

let private systemContext = figure {
    _class
        "not-prose my-8 max-w-full overflow-x-auto rounded-xl border border-gray-200 bg-white p-4 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-emerald-600 dark:border-gray-700 dark:bg-gray-800 sm:p-6"

    _attr ("data-system-context", "true")
    _attr ("tabindex", "0")
    _dataInit "renderMermaid($el)"

    figcaption {
        _class "mb-3 text-xs text-gray-500 dark:text-gray-400 md:hidden"
        text "Scroll horizontally to see the complete diagram."
    }

    div {
        _class "mermaid article-mermaid"
        text systemContextDiagram
    }
}

let private sharedRepositoryTree =
    """meiermade/
├── platform-identity/
│   ├── index.ts
│   └── src/gcp/
│       ├── project.ts
│       ├── serviceAccount.ts
│       ├── grant.ts
│       └── workloadIdentityPool.ts
│
├── platform-infrastructure/
│   ├── index.ts
│   └── src/
│       ├── auth0/
│       ├── cloudflare/
│       ├── gcp/
│       └── k8s/
│
└── environments/
    ├── index.ts
    └── environments/
        ├── platform-identity/
        ├── platform-infrastructure/
        ├── shared/
        └── <application>/"""

let private applicationRepositoryTree =
    """application/
├── app/
│   ├── src/App/
│   ├── src/Build/
│   ├── src/Tests/
│   ├── Dockerfile
│   └── fake.sh
├── pulumi/
│   ├── index.ts
│   └── src/
│       ├── cloudflare/
│       ├── docker/
│       └── k8s/
├── e2e/
│   └── tests/
└── .github/workflows/
    ├── preview.yml
    └── deploy.yml"""

let private gkeExample =
    """const cluster = new gcp.container.Cluster('platform', {
    location: `${region}-b`,
    network: network.id,
    subnetwork: subnet.id,
    removeDefaultNodePool: true,
    ipAllocationPolicy: {
        clusterSecondaryRangeName: 'pods',
        servicesSecondaryRangeName: 'services',
    },
    privateClusterConfig: {
        enablePrivateNodes: true,
        enablePrivateEndpoint: false,
    },
    releaseChannel: { channel: 'REGULAR' },
    workloadIdentityConfig: {
        workloadPool: `${projectId}.svc.id.goog`,
    },
})

new gcp.container.NodePool('platform-primary', {
    cluster: cluster.name,
    autoscaling: { minNodeCount: 1, maxNodeCount: 4 },
    management: { autoRepair: true, autoUpgrade: true },
})"""

let private applicationDeploymentExample =
    """const deployment = new k8s.apps.v1.Deployment('app', {
    metadata: { namespace: config.k8s.namespace },
    spec: {
        replicas: 1,
        selector: { matchLabels: labels },
        template: {
            metadata: { labels },
            spec: {
                securityContext: { runAsNonRoot: true },
                containers: [{
                    name: 'app',
                    image: image.imageRef,
                    resources: {
                        requests: { cpu: '25m', memory: '64Mi' },
                        limits: { cpu: '250m', memory: '256Mi' },
                    },
                    livenessProbe: {
                        httpGet: { path: '/health', port: 5000 },
                    },
                    readinessProbe: {
                        httpGet: { path: '/health', port: 5000 },
                    },
                }],
            },
        },
    },
})"""

let private escExample =
    """values:
  stacks:
    fn::open::pulumi-stacks:
      stacks:
        identity:
          stack: platform-identity/prod
        infrastructure:
          stack: platform-infrastructure/prod

  gcpLogin:
    fn::open::gcp-login:
      project: ${stacks.identity.platformProjectNumber}
      oidc:
        workloadPoolId: ${stacks.identity.workloadIdentityPoolId}
        providerId: pulumi
        serviceAccount: ${stacks.identity.applicationDeployerEmail}

  environmentVariables:
    GOOGLE_OAUTH_ACCESS_TOKEN: ${gcpLogin.accessToken}

  files:
    KUBECONFIG: ${stacks.infrastructure.kubeconfig}

  pulumiConfig:
    docker:registryUri: ${stacks.infrastructure.registryUri}
    k8s:namespace: ${stacks.infrastructure.applicationNamespace}
    seq:endpoint: ${stacks.infrastructure.seqIngestEndpoint}"""

let private content =
    [ p {
          text
              "My personal infrastructure is a small cloud platform for the applications, data systems, automation, and internal tools I operate through Meier Made. Google Cloud provides managed compute and data services, Kubernetes is the common workload layer, Cloudflare is the edge, and Pulumi keeps the system reviewable as code."
      }
      p {
          text
              "The goal is one foundation that I can understand and reuse without rebuilding identity, networking, databases, observability, and deployment automation for every product. It is intentionally modest in scale, but it supports real production workloads and a repeatable path from a pull request to a running application."
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
                  [ "context", "System context"
                    "repositories", "Repository layout"
                    "foundation", "Identity and GCP foundation"
                    "applications", "Application repositories"
                    "services", "Edge and data services"
                    "observability", "Observability and analytics"
                    "environments", "Environments and delivery"
                    "tradeoffs", "Intentional tradeoffs" ] do
                  li {
                      a {
                          _href $"#{id'}"
                          text label
                      }
                  }
          }
      }
      heading "context" "System context"
      paragraph
          "At the widest level, the Meier Made Platform is one software system. People reach its applications through Cloudflare, reviewed changes arrive through GitHub and Pulumi, its workloads run on Google Cloud, and identity is delegated to Google Workspace or Auth0. The diagram deliberately omits internal repositories and services; those belong at the next level of detail."
      systemContext
      paragraph
          "Cloudflare Access protects administrative services with Google Workspace identity, while Auth0 handles customer-facing authentication. Pulumi Cloud stores stack state and Pulumi ESC composes deployment configuration. GitHub Actions is the normal execution environment for previews and updates, using short-lived credentials rather than a permanent Google Cloud key."
      heading "repositories" "Repository layout"
      paragraph
          "The shared platform is split by ownership and deployment order. Identity must exist before shared infrastructure can use it, and both must exist before environments can expose their outputs to applications. Pulumi recommends this kind of layered structure when ownership and blast radius justify separating stacks."
      codeBlock "none" sharedRepositoryTree
      p {
          inlineCode "platform-identity"
          text " creates projects, service accounts, IAM grants, Google groups, and workload-identity bindings. "
          inlineCode "platform-infrastructure"
          text " owns the VPC, GKE, data services, Cloudflare, Auth0, namespaces, Seq, and Snowplow. "
          inlineCode "environments"

          text
              " defines the Pulumi ESC environments that join their outputs and secrets into configuration applications can consume."
      }
      paragraph
          "Each resource has one owner. Shared capabilities stay in the platform repositories; a product-specific route, image, deployment, or identity client stays with that product. This keeps routine application changes out of the larger shared-infrastructure preview."
      heading "foundation" "Identity and GCP foundation"
      p {
          text "The identity layer follows the "

          link
              "https://cloud.google.com/resource-manager/docs/cloud-platform-resource-hierarchy"
              "Google Cloud resource hierarchy"

          text
              ". Projects separate platform, product, and data permissions. Deployment service accounts are distinct from runtime accounts, and the GKE node identity is not reused by applications. Pulumi ESC obtains short-lived deployment credentials through OpenID Connect, while "

          link
              "https://cloud.google.com/kubernetes-engine/docs/concepts/workload-identity"
              "Workload Identity Federation for GKE"

          text " gives pods narrowly scoped Google identities without mounted keys."
      }
      paragraph
          "The shared infrastructure uses a custom VPC, private worker nodes, separate pod and service address ranges, and Cloud NAT for controlled outbound access. The important GKE settings are visible in this abridged version of the current Pulumi program:"
      codeBlock "typescript" gkeExample
      paragraph
          "Google Kubernetes Engine (GKE) Standard mode provides explicit node-pool and workload control. The primary pool scales with demand, while a secondary pool can scale down to zero. Artifact Registry holds immutable application images close to the cluster, and namespaces plus Kubernetes RBAC limit each deployer to its assigned product or data boundary."
      paragraph
          "The platform is intentionally zonal. GKE and Cloud SQL favor one zone, and Redis uses the BASIC tier. A zone-level incident can interrupt service, but this keeps the continuous cost appropriate for the workloads. Backups, point-in-time recovery, health probes, deletion protection, and reproducible Pulumi programs address the failure modes that justify their cost today."
      heading "applications" "Application repositories"
      p {
          text
              "Application code and its final infrastructure live together because the same owner changes both. A typical repository has this shape:"
      }
      codeBlock "none" applicationRepositoryTree
      paragraph
          "The application directory contains source, tests, the build, and its container definition. The Pulumi project builds and publishes that container, deploys it into the namespace supplied by the platform, and owns its Cloudflare hostname and product-specific resources. Playwright tests and GitHub workflows provide pre- and post-deployment evidence."
      paragraph
          "There is no universal application chart. Each stack declares the resources its workload actually needs, while following shared security and operational conventions. A representative deployment looks like this:"
      codeBlock "typescript" applicationDeploymentExample
      paragraph
          "Production deployments also drop unnecessary Linux capabilities and may run an application-specific cloudflared connector beside the app. The essential contract is smaller: a reviewed image, a namespace boundary, realistic resource limits, and health endpoints Kubernetes can use during rollout."
      heading "services" "Edge and data services"
      p {
          text "A "
          link "https://developers.cloudflare.com/cloudflare-one/connections/connect-networks/" "Cloudflare Tunnel"

          text
              " is initiated outbound from the cluster, so application origins do not require public IP addresses or inbound firewall openings. Public DNS points to tunnel hostnames, and Cloudflare applies edge and Access policy before sending traffic to ClusterIP services. Platform services can share a connector; an application can own one when that gives it a cleaner lifecycle."
      }
      p {
          text "The durable transactional store is private "
          link "https://cloud.google.com/sql/docs/postgres" "Cloud SQL for PostgreSQL"

          text
              ". A bootstrap job creates logical databases and restricted roles rather than distributing one superuser credential. Automated backups, point-in-time recovery, encrypted connections, and deletion protection cover durable state. Private Memorystore provides recoverable cache and short-lived state."
      }
      paragraph
          "Secret Manager holds runtime and integration secrets. Pulumi owns the secret containers and IAM policy, while values are populated separately. Cloud Storage provides private platform buckets and a public asset bucket whose prefix-scoped IAM lets each publisher manage only its own immutable files."
      heading "observability" "Observability and analytics"
      p {
          strong { text "Seq" }

          text
              " is the shared destination for structured logs and traces. Applications receive its internal ingestion endpoint through their environment, while the interface is routed through Cloudflare Access. Structured properties make Seq the normal starting point for production diagnosis; Kubernetes pod logs remain available for lower-level failures."
      }
      p {
          strong { text "Snowplow" }

          text
              " provides behavioral analytics. Collectors in GKE publish good and bad events to Pub/Sub, an enrichment workload adds campaign and referrer context, and the BigQuery loader writes validated events to the data project. Separate workload identities limit each component to the Pub/Sub or BigQuery operations it needs, and failed events follow explicit topics instead of disappearing."
      }
      heading "environments" "Environments and delivery"
      p {
          text "Pulumi ESC is the configuration boundary between shared infrastructure and an application. The "
          link "https://www.pulumi.com/docs/esc/providers/pulumi-stacks/" "pulumi-stacks provider"

          text
              " opens outputs at runtime, the GCP login provider exchanges OIDC identity for a short-lived token, and the environment maps only the required values into files, environment variables, and Pulumi configuration. An abridged application environment shows the complete flow:"
      }
      codeBlock "yaml" escExample
      paragraph
          "The application pipeline does not need a copied registry address, cluster endpoint, kubeconfig, namespace, or Seq endpoint. It opens the environment and receives a coherent snapshot from the stacks that own those values. Secret Manager values use the same pattern and remain secret as they pass through ESC and Pulumi."
      paragraph
          "Pull requests run tests and a Pulumi preview. After merge, GitHub Actions authenticates to Pulumi, opens the ESC environment, builds the reviewed image, and performs the update. Kubernetes readiness gates the rollout, and product repositories can run browser or API checks against the deployed service. I use local previews for review, but the main-branch workflow—not a workstation running pulumi up—is the normal production path."
      heading "tradeoffs" "Intentional tradeoffs"
      paragraph
          "This platform optimizes for one operator running several real services, not for zero downtime through every regional failure. A shared zonal cluster and database instance have more common blast radius than independent regional platforms. Cloudflare, Google Cloud, Pulumi, and Kubernetes are also deliberate dependencies rather than interchangeable abstractions."
      paragraph
          "I accept those constraints because the alternative would materially increase baseline cost and operational work. Durable data receives stronger protection; recoverable services are recreated from code. The result is enough infrastructure to add an application with identity, private data access, logs, analytics, and an edge route, while remaining small enough for one person to understand."
      script { _src (Asset.fingerprinted "/scripts/mermaid.11.16.0.min.js") }
      script {
          js
              "window.renderMermaid=async function(el){const nodes=el?.matches?.('.mermaid')?[el]:Array.from(el?.querySelectorAll?.('.mermaid')??[]);if(!window.mermaid||nodes.length===0)return;for(const node of nodes){node.dataset.mermaidSource=node.dataset.mermaidSource||node.textContent.trim();node.textContent=node.dataset.mermaidSource;node.removeAttribute('data-processed')}window.mermaid.initialize({startOnLoad:false,theme:document.documentElement.classList.contains('dark')?'dark':'neutral',securityLevel:'strict'});await window.mermaid.run({nodes})};void window.renderMermaid(document)"
      } ]

let article = Article.create metadata (ArticlePage.primary metadata content)
