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
      summary = "How I run personal websites and agents with GCP, Kubernetes, Cloudflare, Pulumi, and GitHub"
      cover = "https://assets.meiermade.com/andymeier/articles/personal-infrastructure/system-context-dec390cb7efb.webp"
      tags = [| "DevOps"; "Pulumi"; "GCP"; "Kubernetes"; "Cloudflare"; "TypeScript" |]
      createdAt = DateTimeOffset(2026, 8, 11, 0, 0, 0, TimeSpan.Zero) }

let private heading id' label = h2 {
    _class "mt-10 scroll-mt-24"
    _id id'
    text label
}

let private subheading label = h3 {
    _class "mt-8"
    text label
}

let private paragraph value = p { text value }

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
    accTitle: Personal infrastructure system context
    accDescr: The operator and visitors use personal applications supported by GitHub Actions, Pulumi Cloud, Google Cloud, Cloudflare, and Google Workspace.

    operator["Operator<br/>(Person)<br/>Builds and operates personal applications"]
    visitors["Visitors<br/>(People)<br/>Use public personal applications"]
    platform["Personal infrastructure<br/>(Software system)<br/>Runs public and private applications and behavioral analytics"]:::system
    github["GitHub Actions<br/>(External system)<br/>Tests, previews, and deploys reviewed changes"]
    pulumi["Pulumi Cloud and ESC<br/>(External system)<br/>Stores state and composes environments"]
    gcp["Google Cloud<br/>(External system)<br/>Runs compute, storage, messaging, and analytics"]
    cloudflare["Cloudflare<br/>(External system)<br/>Provides DNS, edge policy, Access, and tunnels"]
    workspace["Google Workspace<br/>(External system)<br/>Provides identity for protected applications"]

    operator -->|Commits and reviews changes| github
    operator -->|Operates applications through| cloudflare
    visitors -->|Use public applications through| cloudflare
    github -->|Runs Pulumi programs| pulumi
    pulumi -->|Applies desired state| platform
    cloudflare -->|Routes accepted requests| platform
    cloudflare -->|Checks protected access with| workspace
    platform -->|Runs on| gcp

    classDef system fill:#059669,stroke:#047857,color:#ffffff,stroke-width:3px"""

let private runtimeDiagram =
    """flowchart TB
    accTitle: Personal infrastructure runtime
    accDescr: Cloudflare routes traffic through outbound cloudflared tunnels directly to andymeier.dev, Benji and Minnie, Seq, and Snowplow. Applications send OpenTelemetry logs and traces to Seq, while Snowplow processes behavioral events with Pub/Sub and BigQuery.

    visitors["Visitors<br/>(People)"]
    operator["Operator<br/>(Person)"]
    cloudflare["Cloudflare<br/>(External system)<br/>Applies DNS, edge, and Access policy"]
    workspace["Google Workspace<br/>(External system)<br/>Confirms identity for protected applications"]
    tunnel["Cloudflare connectors<br/>(Container: cloudflared)<br/>Create outbound-only origin tunnels"]
    website["andymeier.dev<br/>(Container: Kubernetes Deployment)<br/>Serves the public personal website"]:::primary
    agents["Benji and Minnie<br/>(Containers: Kubernetes StatefulSets)<br/>Run long-lived personal agents"]:::primary
    seq[("Seq<br/>(Container: Kubernetes StatefulSet)<br/>Stores and queries logs and traces")]
    assets[("Application assets<br/>(Container: Cloud Storage bucket)<br/>Stores immutable public files")]
    snowplow["Snowplow analytics<br/>(Containers: Collector, Enricher, and Loader)<br/>Processes behavioral events"]
    eventStreams[("Event streams<br/>(Container: Pub/Sub topics)<br/>Buffer good and bad events")]
    warehouse[("Analytics warehouse<br/>(Container: BigQuery dataset)<br/>Stores validated events")]

    visitors -->|Use public applications| cloudflare
    operator -->|Uses protected applications| cloudflare
    cloudflare -->|Checks protected requests with| workspace
    cloudflare -->|Routes application traffic| tunnel
    cloudflare -->|Serves public assets from| assets
    cloudflare -->|Routes event collection| snowplow
    tunnel -->|Routes website traffic| website
    tunnel -->|Routes agent traffic| agents
    tunnel -->|Opens protected interface| seq
    website -->|Exports OpenTelemetry| seq
    agents -->|Exports OpenTelemetry| seq
    website -->|References immutable files| assets
    snowplow <-->|Consumes and publishes| eventStreams
    snowplow -->|Loads validated events| warehouse

    classDef primary fill:#059669,stroke:#047857,color:#ffffff,stroke-width:3px"""

let private deploymentDiagram =
    """flowchart TB
    accTitle: Personal infrastructure deployment
    accDescr: GitHub Actions uses Pulumi Cloud and ESC to deploy andymeier.dev, Benji, and Minnie into Google Cloud. Cloudflare connectors, personal applications, Seq, and Snowplow run in a zonal GKE cluster supported by Artifact Registry, Secret Manager, Cloud Storage, Pub/Sub, BigQuery, Cloud Logging, and Cloud Monitoring.

    github["GitHub Actions<br/>(Deployment environment)"]
    pulumi["Pulumi Cloud and ESC<br/>(Configuration and state)"]
    cloudflare["Cloudflare edge<br/>(External network)"]

    subgraph gcp["Google Cloud"]
        direction TB
        registry[("Artifact Registry<br/>Immutable application images")]

        subgraph gke["Zonal GKE cluster"]
            direction TB
            runtime["GKE runtime<br/>Nodes and system components"]
            connectors["Cloudflare connectors<br/>Kubernetes Deployments"]
            website["andymeier.dev<br/>Kubernetes Deployment and Service"]:::primary
            agents["Benji and Minnie<br/>Kubernetes StatefulSets and Services"]:::primary
            seq["Seq<br/>Kubernetes StatefulSet and persistent disk"]
            snowplow["Snowplow pipeline<br/>Collector, Enricher, and Loader Deployments"]
        end

        secrets[("Secret Manager<br/>Long-lived application secrets")]
        storage[("Cloud Storage<br/>Immutable application assets")]
        pubsub[("Pub/Sub<br/>Analytics topics and subscriptions")]
        bigquery[("BigQuery<br/>Analytics dataset")]
        operations[("Cloud Logging and Monitoring<br/>System logs, metrics, dashboards, and alerts")]

        registry -->|Supplies website image| website
        registry -->|Supplies agent image| agents
        connectors -->|Routes website traffic| website
        connectors -->|Routes agent traffic| agents
        connectors -->|Opens protected interface| seq
        connectors -->|Routes event collection| snowplow
        website -->|References assets| storage
        website -->|Exports logs and traces| seq
        agents -->|Reads authorized values| secrets
        agents -->|Exports logs and traces| seq
        runtime -->|Exports system telemetry| operations
        snowplow -->|Streams events through| pubsub
        snowplow -->|Loads validated events| bigquery
    end

    github -->|Exchanges OIDC identity and runs updates| pulumi
    github -->|Publishes images| registry
    pulumi -->|Applies desired state| gcp
    pulumi -->|Configures routes and policies| cloudflare
    cloudflare <-->|Private-origin tunnels| connectors

    style gcp fill:transparent,stroke:#047857,stroke-width:3px
    style gke fill:transparent,stroke:#059669,stroke-width:2px
    classDef primary fill:#059669,stroke:#047857,color:#ffffff,stroke-width:3px"""

let private architectureDiagram dataAttribute diagram = figure {
    _class
        "not-prose my-8 max-w-full overflow-x-auto rounded-xl border border-gray-200 bg-white p-4 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-emerald-600 dark:border-gray-700 dark:bg-gray-800 sm:p-6"

    _attr (dataAttribute, "true")
    _attr ("tabindex", "0")
    _dataInit "renderMermaid($el)"

    figcaption {
        _class "sticky left-0 mb-3 w-fit text-xs text-gray-500 dark:text-gray-400 md:hidden"
        text "Scroll horizontally to see the complete diagram."
    }

    div {
        _class "mermaid article-mermaid"
        text diagram
    }
}

let private systemContext =
    architectureDiagram "data-system-context" systemContextDiagram

let private runtimeView = architectureDiagram "data-container-view" runtimeDiagram

let private deploymentView =
    architectureDiagram "data-deployment-view" deploymentDiagram

let private organizationTree =
    """personal-cloud/
├── identity/
│   ├── project.ts
│   ├── serviceAccounts.ts
│   └── oidc.ts
├── infrastructure/
│   ├── gcp/
│   ├── cloudflare/
│   └── kubernetes/
├── environments/
│   ├── shared.yaml
│   └── <application>.yaml
└── applications/
    ├── andymeier/
    └── <personal-application>/"""

let private gkeExample =
    """const cluster = new gcp.container.Cluster('personal', {
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

new gcp.container.NodePool('personal-primary', {
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
})

new k8s.core.v1.Service('app', {
    metadata: { namespace: config.k8s.namespace },
    spec: { type: 'ClusterIP', selector: labels, ports: [{ port: 80 }] },
})"""

let private escExample =
    """values:
  gcpLogin:
    fn::open::gcp-login:
      project: <project-number>
      oidc:
        workloadPoolId: <workload-pool>
        providerId: pulumi
        serviceAccount: <environment-service-account>
        subjectAttributes:
          - currentEnvironment.name

  secrets:
    fn::open::gcp-secrets:
      login: ${gcpLogin}
      access:
        applicationApiKey:
          name: <secret-name>

  environmentVariables:
    GOOGLE_OAUTH_ACCESS_TOKEN: ${gcpLogin.accessToken}

  pulumiConfig:
    application:apiKey: ${secrets.applicationApiKey}"""

let private githubWorkflowExample =
    """permissions:
  contents: read
  id-token: write

steps:
  - uses: actions/checkout@v7
  - name: Authenticate with Pulumi
    uses: pulumi/auth-actions@v2
    with:
      organization: <organization>
      requested-token-type: <scoped-token-type>
  - name: Preview or update
    uses: pulumi/actions@v7
    with:
      work-dir: ./pulumi
      stack-name: prod
      command: <preview-or-up>"""

let private content =
    [ p {
          text
              "I run my public websites, private utilities, and long-lived AI agents on one small cloud platform. Google Cloud provides the foundation, Kubernetes gives each workload a common deployment API, Cloudflare handles traffic and access, Pulumi defines the infrastructure, and GitHub Actions delivers changes. andymeier.dev, Benji, and Minnie are the current workloads."
      }
      p {
          text
              "I operate it with the same discipline I bring to client infrastructure: changes are defined in code, previewed, and reviewed; identities are narrowly scoped; secrets stay out of repositories; and deployments are observable and reproducible. Because I own the cost and blast radius, it also serves as a proving ground for new tools and architectural patterns before I apply what I learn to client work."
      }
      p {
          text
              "I favor a small, coherent set of tools with strong APIs. In an agent-driven workflow, that lets me and my agents build, inspect, and diagnose the same systems programmatically. Every client environment has its own scale, risk, compliance, and operational requirements, so experience here informs those decisions rather than becoming a default blueprint."
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
                  [ "architecture", "Architecture at a glance"
                    "organization", "How it is organized"
                    "applications", "Personal applications and agents"
                    "gcp", "Why Google Cloud"
                    "kubernetes", "Kubernetes without platform engineering"
                    "cloudflare", "Cloudflare for networking and access"
                    "observability", "Observability with Seq"
                    "pulumi", "Pulumi and environments"
                    "github", "GitHub for delivery"
                    "tradeoffs", "Intentional tradeoffs" ] do
                  li {
                      a {
                          _href $"#{id'}"
                          text label
                      }
                  }
          }
      }
      heading "architecture" "Architecture at a glance"
      subheading "System context"
      paragraph
          "Cloudflare fronts every public and protected application, while GitHub Actions and Pulumi deliver reviewed changes to Google Cloud. Google Workspace provides my identity for protected applications."
      systemContext
      subheading "Runtime"
      paragraph
          "andymeier.dev serves the public website, while Benji and Minnie run as long-lived AI agents. Cloudflare connectors route accepted requests to those workloads, Seq, or Snowplow. Applications send telemetry to Seq; managed storage and analytics stay outside the request path."
      runtimeView
      subheading "Deployment"
      paragraph
          "Application images live in Artifact Registry. andymeier.dev, Benji, Minnie, Cloudflare connectors, Seq, and Snowplow run in one zonal GKE cluster. Secret Manager, Cloud Storage, Pub/Sub, BigQuery, Cloud Logging, and Cloud Monitoring remain managed Google Cloud services."
      deploymentView
      heading "organization" "How it is organized"
      paragraph
          "I organize the infrastructure in dependency order. Identity comes first, followed by shared infrastructure, application environments, and application-owned deployments."
      codeBlock "none" organizationTree
      paragraph
          "Each resource has one owner. The infrastructure layer owns cluster-wide capabilities; application repositories own their workloads, routes, and access policies. Environments connect the two without copying identifiers or credentials between repositories."
      paragraph
          "Namespaces are the runtime boundary. Each application gets its own namespace and deployment identity, and Kubernetes RBAC limits the deployer to that namespace. Workload Identity gives a Pod a narrowly scoped Google identity only when it needs a managed service."
      heading "applications" "Personal applications and agents"
      paragraph
          "andymeier.dev is a stateless web application behind a private Service. Benji and Minnie are my two long-running AI agents. I use them for coding, scheduled routines, email and messaging, task follow-up, and other recurring work. All three applications share deployment, identity, networking, and observability conventions."
      p {
          text
              "Benji and Minnie share one runtime but deploy as separate single-replica StatefulSets. Each has independent configuration, identity, persistent storage, hostname, Cloudflare policy, and "

          link "https://pi.dev/" "Pi coding agent"

          text
              " sessions. Their runtime handles webhooks, scheduled work, and longer task sessions. Each agent can evolve or restart without sharing credentials or session state."
      }
      paragraph
          "The agents need persistent processes, private endpoints, background work, workload identity, durable volumes, and traceable task execution. Kubernetes gives them and the stateless website one operational model without forcing them into the same architecture."
      heading "gcp" "Why Google Cloud"
      paragraph
          "Google Cloud fits this environment because Google Workspace, IAM, GKE, Artifact Registry, Secret Manager, Cloud Logging, and Cloud Monitoring share a coherent identity and operations model. Its APIs and command-line tools are consistent, and Google Kubernetes Engine (GKE) is the managed Kubernetes service I know best and prefer."
      p {
          text
              "The economics of a small zonal cluster are unusually good. Google charges a cluster management fee, but the "

          link "https://cloud.google.com/kubernetes-engine/pricing" "GKE free tier"

          text
              " provides $74.40 in monthly credits per billing account, offsetting the management fee for one zonal Standard or Autopilot cluster. Worker nodes, disks, networking, and usage-based services remain billable, but the managed control plane adds no incremental fee within that allowance."
      }
      paragraph
          "I use a zonal Standard cluster to control node pools, pack small workloads efficiently, and avoid paying for multi-zone availability I do not need. Auto-repair, auto-upgrade, a regular release channel, and reproducible configuration handle much of the maintenance."
      paragraph "This TypeScript is an abridged version of my zonal GKE configuration:"
      codeBlock "typescript" gkeExample
      heading "kubernetes" "Kubernetes without platform engineering"
      paragraph
          "I deliberately use a small Kubernetes vocabulary: Namespaces, Deployments, and Services, with Jobs and ConfigMaps only when a workload requires them. Deployments create and replace Pods, Services give them a stable private address, and Namespaces provide ownership and authorization boundaries."
      p {
          text "That small subset still provides a useful "
          link "https://kubernetes.io/docs/concepts/overview/kubernetes-api/" "programmatic API"

          text
              ". A Deployment declares its image, replicas, probes, resources, and security context. Kubernetes reconciles the workload, restarts failed containers, and waits for readiness during a rollout. That gives me repeatable deployments without custom process managers or remote-shell scripts."
      }
      paragraph
          "The same tools list workloads, inspect events, stream logs, restart rollouts, and forward private ports for every application. GKE sends logs to Cloud Logging and system metrics to Cloud Monitoring by default. Keeping the Kubernetes vocabulary small keeps its complexity bounded."
      paragraph "This is an abridged application Deployment and Service:"
      codeBlock "typescript" applicationDeploymentExample
      heading "cloudflare" "Cloudflare for networking and access"
      p {
          text "All application origins stay on the private network. A "

          link
              "https://developers.cloudflare.com/cloudflare-one/networks/connectors/cloudflare-tunnel/"
              "Cloudflare Tunnel"

          text
              " connector runs in Kubernetes and creates an outbound-only connection to Cloudflare. The cluster does not need a public application load balancer, and its Services can remain private ClusterIP addresses. Public traffic reaches an accepted hostname at Cloudflare and is forwarded through the tunnel to the appropriate Service."
      }
      p {
          text "For a private application, "

          link
              "https://developers.cloudflare.com/cloudflare-one/setup/secure-private-apps/private-web-app/"
              "Cloudflare Access"

          text
              " applies an identity policy before the request ever reaches the origin. I can define a policy for each hostname and use Google Workspace as the identity provider. A public website can remain public, while an administration page or experimental utility can require my account or an approved group. Both use the same tunnel and private network. Cloudflare exposes APIs for DNS, tunnel routes, and Access policies, so I manage those controls through Pulumi rather than configuring them only in the dashboard."
      }
      paragraph
          "For browser-based applications, this gives me per-application access without exposing origins or granting a device network-wide access."
      heading "observability" "Observability with Seq"
      p {
          text "I use "
          link "https://datalust.co/docs/getting-started-with-docker" "Seq"

          text
              " for application observability because it is equally easy to run locally in Docker or deploy as a StatefulSet in Kubernetes. Applications export structured logs and traces through OpenTelemetry, so the instrumentation does not depend on a proprietary logging client. The same configuration sends local development telemetry to a local Seq instance and production telemetry to the private service in the cluster."
      }
      p {
          text
              "Seq puts correlated logs and traces in one event stream. I can move from an error to its surrounding events and trace, then use the "

          link "https://datalust.co/docs/the-seq-query-language" "query language"

          text " in the UI or the "
          link "https://datalust.co/docs/using-the-http-api" "HTTP API"

          text
              " from an agent. Benji, Minnie, and coding agents can retrieve recent errors, filter telemetry, and follow traces without reproducing a browser workflow. Cloudflare Access protects the interface."
      }
      p {
          text
              "Benji and Minnie each have a separate Google Workspace account and Kubernetes identity. Their Kubernetes identities can inspect workloads, events, and logs without changing them. Because a Workspace account is also a "

          link "https://cloud.google.com/iam/docs/principal-identifiers" "Google Cloud IAM principal"

          text
              ", I can grant narrowly scoped viewer roles for relevant Google Cloud APIs when an agent needs cloud-level context. Seq covers application behavior and correlated traces; Cloud Logging and Cloud Monitoring provide Kubernetes events, system logs, infrastructure metrics, and cluster context. Together, those APIs let an agent gather evidence without receiving deployment permissions."
      }
      heading "pulumi" "Pulumi and environments"
      p {
          text "I define infrastructure with "
          link "https://www.pulumi.com/docs/iac/languages-sdks/javascript/" "Pulumi and TypeScript"

          text
              " to reuse policies and resource shapes with normal language and refactoring tools. Provider types and IntelliSense expose available properties while I write and review changes, and strong types provide useful evidence even when AI helps with discovery."
      }
      paragraph
          "I keep resource modules small, ownership explicit, and abstractions limited. Pulumi calculates previews, records state, and applies only the reviewed difference."
      p {
          text
              "Pulumi ESC is the environment boundary. An environment composes stack outputs, non-secret configuration, short-lived cloud credentials, and selected secrets into the exact values a deployment needs. The "

          link "https://www.pulumi.com/docs/esc/guides/pulumi-iac/" "GCP login provider"

          text
              " exchanges OpenID Connect identity for a temporary Google Cloud token. Long-lived secrets stay in GCP Secret Manager, where I can rotate them independently and audit access."
      }
      paragraph
          "For secret access, I create the secret container in GCP, grant a dedicated service account permission to read only the required secret, and allow a specific Pulumi environment to impersonate that account through OIDC. ESC then reads the current secret value when the environment opens. The application repository does not receive a service-account key, and one environment cannot automatically read another environment's secrets. An abridged ESC environment captures that trust chain:"
      codeBlock "yaml" escExample
      heading "github" "GitHub for delivery"
      paragraph
          "Each application has a GitHub repository. Pull requests collect code and infrastructure changes, run tests, and produce a Pulumi preview. Merging publishes an immutable image, applies the reviewed update, waits for Kubernetes readiness, and runs browser or API checks."
      p {
          text "GitHub Actions requests an OIDC token, and "

          link "https://www.pulumi.com/docs/iac/guides/continuous-delivery/github-actions/" "Pulumi exchanges it"

          text
              " for short-lived, scoped access. ESC obtains a separate temporary Google Cloud identity, so trust follows the repository and environment rather than stored access tokens or cloud keys."
      }
      paragraph "The core deployment workflow is small:"
      codeBlock "yaml" githubWorkflowExample
      p {
          text "Locally, the "
          link "https://cli.github.com/manual/" "GitHub CLI"

          text
              " makes the same workflow practical from the terminal. I use it to create and inspect pull requests, read check results, review diffs, and manage branches. The GitHub API also gives agents the same repository, pull-request, and check data without requiring a browser. The pull request—not a workstation running an unreviewed production update—is the normal unit of change."
      }
      heading "tradeoffs" "Intentional tradeoffs"
      paragraph
          "A zonal GKE cluster is not the smallest possible way to host one website. A managed static host or one virtual machine would use fewer concepts. The calculation changes when several personal applications reuse the same cluster, private ingress, deployment workflow, observability, and identity model. Adding another application becomes a namespace, a Deployment, a Service, a route, and a workflow rather than another hand-configured server."
      paragraph
          "The zonal design also accepts that a zone-level incident can interrupt every application. Regional control planes and multi-zone node pools would improve availability but increase the baseline compute cost and operational surface. For personal applications, I prefer health probes, reproducible deployments, immutable images, and managed data services over paying continuously for regional redundancy."
      paragraph
          "Cloudflare, Google Cloud, Pulumi, Kubernetes, and GitHub are deliberate dependencies. Replacing any one of them would require real work. I accept that coupling because each product removes more operational burden than it adds, and because the boundaries between them are still visible: containers, Kubernetes resources, DNS routes, OIDC identities, and TypeScript programs."
      paragraph
          "The result is a small, coherent platform that keeps personal applications inexpensive, private by default, observable, and programmatically deployable. A narrow Kubernetes vocabulary and short-lived identities keep it manageable for one operator."
      script {
          _src (Asset.fingerprinted "/scripts/mermaid.11.16.0.min.js")
          _onload "window.renderMermaid?.(document)"
      }
      script {
          js
              "window.renderMermaid=async function(el){const nodes=el?.matches?.('.mermaid')?[el]:Array.from(el?.querySelectorAll?.('.mermaid')??[]);if(!window.mermaid||nodes.length===0)return;for(const node of nodes){node.dataset.mermaidSource=node.dataset.mermaidSource||node.textContent.trim();node.textContent=node.dataset.mermaidSource;node.removeAttribute('data-processed')}window.mermaid.initialize({startOnLoad:false,theme:document.documentElement.classList.contains('dark')?'dark':'neutral',securityLevel:'strict'});await window.mermaid.run({nodes});for(const node of nodes){const scroller=node.parentElement;if(scroller&&scroller.scrollWidth>scroller.clientWidth&&scroller.scrollLeft===0)scroller.scrollLeft=(scroller.scrollWidth-scroller.clientWidth)/2}};void window.renderMermaid(document)"
      } ]

let article = Article.create metadata (ArticlePage.primary metadata content)
