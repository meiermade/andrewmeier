module App.Articles.Posts.PersonalInfrastructure

open App.Articles
open App.Articles.Shared
open FSharp.ViewEngine
open System
open type Html

let private metadata =
    { permalink = "personal-infrastructure"
      title = "Personal Infrastructure"
      summary = "How I set up the infrastructure for hosting my apps and services"
      cover = "https://assets.meiermade.com/andymeier/articles/personal-infrastructure/cover-735b86220f20.webp"
      tags = [| "DevOps"; "Pulumi"; "Programming"; "TypeScript" |]
      createdAt = DateTimeOffset(2022, 8, 26, 0, 0, 0, TimeSpan.Zero) }

let private content =
    [ h2 {
          _class "mt-8"
          _id "758baa07a9c043c98fe5e89f68158a4f"
          span { text "Summary" }
      }
      div {
          span {
              text
                  "In this post I will walk through how I set up my network and servers in order to run this blog and other services. "
          }
      }
      div { span { text "I chose to build my own ‘cloud’ because" } }
      ol {
          _class "list-decimal"
          li { span { text "I wanted to learn about networking and DevOps" } }

          li {
              span {
                  text "The costs of running servers on DigitalOcean or AWS was more than I wanted to spend each month "
              }
          }

          li {
              span { text "It was fun. I like building things and interacting with the physical world brings me joy" }
          }
      }
      div {
          span { text "My current infrastructure runs on 3 Raspberry Pis in my apartment. I followed " }

          a {
              _href "https://www.pulumi.com/docs/guides/crosswalk/kubernetes/playbooks/"
              span { text "Pulumi’s Playbook for Kubernetes" }
          }

          span {
              text
                  " for organizing the resources into different ‘stacks’ which I will go through in detail. You can see all the code in "
          }

          a {
              _href "https://github.com/ameier38/infrastructure"
              span { text "my GitHub repo" }
          }

          span { text "." }
      }
      div { br }
      img {
          _class "drop-shadow-xl rounded"

          _src
              "https://assets.meiermade.com/andymeier/articles/personal-infrastructure/raspberry-pi-cluster-507654f64fad.webp"

          _alt "Three Raspberry Pi computers mounted in a home server rack"
          _attr ("loading", "lazy")
          _attr ("width", "1600")
          _attr ("height", "1200")
      }
      h2 {
          _class "mt-8"
          _id "9876847e07a74d3da0e8f4ebad6b8888"
          span { text "Table of Contents" }
      }
      ul {
          _class "list-disc"

          li {
              a {
                  _href "#fb580b4b8c014b3dbe6f77f1bde90f37"
                  span { text "Hardware" }
              }

              span { text ": Everything I bought" }
          }

          li {
              a {
                  _href "#27c23f067a954b6eab3ece985ac07c02"
                  span { text "Identity Stack" }
              }

              span { text ": Users, roles, keys, and permissions" }
          }

          li {
              a {
                  _href "#573dfd6c0afe47d3a8d484c944e24842"
                  span { text "Managed Infrastructure Stack" }
              }

              span { text ": Resources needed to run the cluster" }
          }

          li {
              a {
                  _href "#7a1f1ed7f7da4059a63db2d46a4ad7f9"
                  span { text "Cluster Stack" }
              }

              span { text ": Cluster deployment " }
          }

          li {
              a {
                  _href "#f1d5b2ab35694d7cac0fdc80862ef03d"
                  span { text "Cluster Services Stack" }
              }

              span { text ": Cluster wide resources" }
          }

          li {
              a {
                  _href "#23ec3e2370f34b5eb33d4dd87df8d901"
                  span { text "App Services Stack" }
              }

              span { text ": App specific resources" }
          }

          li {
              a {
                  _href "#246a7816de444b889fd4e169e6017b31"
                  span { text "Managed Apps Stack" }
              }

              span { text ": Apps developed by someone else (e.g. Grafana) " }
          }

          li {
              a {
                  _href "#2ac83008618d4529be74301b94f49b13"
                  span { text "Apps Stacks" }
              }

              span { text ": Apps developed by me (e.g., this blog)" }
          }

          li {
              a {
                  _href "#96bd7ae28beb485bb4aa482490ff5536"
                  span { text "CI/CD" }
              }

              span { text ": Automated deployment using GitHub actions" }
          }
      }
      h2 {
          _class "mt-8"
          _id "fb580b4b8c014b3dbe6f77f1bde90f37"
          span { text "Hardware" }
      }
      ul {
          _class "list-disc"
          li { span { text "3 x Raspberry Pi 4 8GB" } }
          li { span { text "3 x Raspberry Pi POE Hat" } }
          li { span { text "3 x 32GB flash drive" } }
          li { span { text "3 x CAT 6 1 foot ethernet cables" } }

          li {
              span { text "1 x " }

              a {
                  _href "https://smile.amazon.com/gp/product/B082G2G2F8"
                  span { text "8 port POE Network Switch" }
              }
          }

          li {
              span { text "1 x " }

              a {
                  _href "https://smile.amazon.com/gp/product/B07R5Q8MTJ"
                  span { text "6U Server Cabinet" }
              }
          }

          li {
              span { text "1 x " }

              a {
                  _href "https://www.etsy.com/listing/978719017/1u-rack-for-raspberry-pi-19-rackmount"
                  span { text "1U Raspberry Pi Rack Mount" }
              }
          }

          li {
              span { text "1 x " }

              a {
                  _href "https://smile.amazon.com/gp/product/B00006B834"
                  span { text "1U Rack Mount Power Strip" }
              }
          }
      }
      h2 {
          _class "mt-8"
          _id "fb96de2e2abc4e869b209eb9f7b568ef"
          span { text "Stacks" }
      }
      div {
          span {
              text
                  "The reason for splitting up the infrastructure into different stacks is to limit the potential blast radius of errors. Typically the lower stacks (where the network, cluster, etc. are deployed) will change the least and the higher stacks (where applications are deployed) will change the most. This helps isolate potential errors when making changes. "
          }
      }
      h3 {
          _class "mt-6"
          _id "27c23f067a954b6eab3ece985ac07c02"
          span { text "Identity Stack" }
      }
      div {
          span {
              text
                  "This stack defines the users, roles, permissions, and keys that will be used by the other stacks in order to make changes to managed resources. In my case I am using AWS for key management, S3 buckets, container registries, and sending email notifications, so I have defined roles and permissions in order to deploy those resources."
          }
      }
      div {
          span {
              text
                  "In a fresh AWS account I first manually created a GitHub identity provider which will allow GitHub actions to assume AWS roles in my account. You can read more about how this is configured in the "
          }

          a {
              _href
                  "https://docs.github.com/en/actions/deployment/security-hardening-your-deployments/configuring-openid-connect-in-amazon-web-services"

              span { text "GitHub documentation" }
          }

          span { text ". Next I manually created an " }

          code {
              _class "language-none"
              text "identity-deployer"
          }

          span { text " role with the following managed permissions:" }
      }
      ul {
          _class "list-disc"

          li {
              code {
                  _class "language-none"
                  text "IAMFullAccess"
              }
          }

          li {
              code {
                  _class "language-none"
                  text "AWSKeyManagementServicePowerUser"
              }
          }
      }
      div { span { text "and the following trust relationship:" } }
      pre {
          _class "language-json"

          code {
              _class "language-json"

              span {
                  text
                      "{\n    \"Version\": \"2012-10-17\",\n    \"Statement\": [\n        {\n            \"Effect\": \"Allow\",\n            \"Principal\": {\n                \"AWS\": \"400689721046\"\n            },\n            \"Action\": \"sts:AssumeRole\"\n        },\n        {\n            \"Effect\": \"Allow\",\n            \"Principal\": {\n                \"Federated\": \"arn:aws:iam::400689721046:oidc-provider/token.actions.githubusercontent.com\"\n            },\n            \"Action\": \"sts:AssumeRoleWithWebIdentity\",\n            \"Condition\": {\n                \"StringEquals\": {\n                    \"token.actions.githubusercontent.com:aud\": \"sts.amazonaws.com\"\n                },\n                \"StringLike\": {\n                    \"token.actions.githubusercontent.com:sub\": \"repo:ameier38/infrastructure:*\"\n                }\n            }\n        }\n    ]\n}"
              }
          }
      }
      div {
          span {
              text
                  "This role has permissions to manage IAM resources and KMS keys, and the trust policy allows the role to be assumed by users in my account and by GitHub actions running in the "
          }

          a {
              _href "https://github.com/ameier38/infrastructure"
              span { text "ameier38/infrastructure repo" }
          }

          span { text "." }
      }
      div {
          span { text "Next I created an " }

          code {
              _class "language-none"
              text "admin"
          }

          span { text " user with the following policy attached:" }
      }
      pre {
          _class "language-json"

          code {
              _class "language-json"

              span {
                  text
                      "{\n    \"Version\": \"2012-10-17\",\n    \"Statement\": [\n        {\n            \"Effect\": \"Allow\",\n            \"Action\": \"sts:AssumeRole\",\n            \"Resource\": \"arn:aws:iam::400689721046:role/identity-deployer\"\n        }\n    ]\n}"
              }
          }
      }
      div {
          span { text "This allows the " }

          code {
              _class "language-none"
              text "admin"
          }

          span { text " user to assume the " }

          code {
              _class "language-none"
              text "identity-deployer"
          }

          span { text " role. I added the " }

          code {
              _class "language-none"
              text "admin"
          }

          span { text " user keys to the " }

          code {
              _class "language-none"
              text "~/.aws/credentials"
          }

          span { text " file on my computer." }
      }
      pre {
          _class "language-toml"

          code {
              _class "language-toml"
              span { text "[admin]\naws_access_key_id={ACCESS_KEY}\naws_secret_access_key={SECRET_KEY}" }
          }
      }
      div {
          span { text "I then added the " }

          code {
              _class "language-none"
              text "identity-deployer"
          }

          span { text " role to the " }

          code {
              _class "language-none"
              text "~/.aws/config"
          }

          span { text " file so I could assume this role in order to deploy the stack." }
      }
      pre {
          _class "language-toml"

          code {
              _class "language-toml"

              span {
                  text
                      "[profile admin]\nregion=us-east-1\noutput=json\n\n[profile identity-deployer]\nrole_arn=arn:aws:iam::400689721046:role/identity-deployer\nsource_profile=admin"
              }
          }
      }
      div {
          span { text "With the " }

          code {
              _class "language-none"
              text "identity-deployer"
          }

          span { text " role and " }

          code {
              _class "language-none"
              text "admin"
          }

          span { text " user created, I can then create the identity stack using the Pulumi CLI. " }
      }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"
              span { text "pulumi new aws-typescript" }
          }
      }
      div {
          span {
              text
                  "In the new Pulumi project I created the encryption key which will be used by the other stacks to encrypt configuration secrets."
          }
      }
      pre {
          _class "language-typescript"

          code {
              _class "language-typescript"
              span { text "import * as aws from '@pulumi/aws'\n\nconst pulumiKey = new aws.kms.Key('pulumi')" }
          }
      }
      div {
          span { text "Next I created the " }

          code {
              _class "language-none"
              text "infrastructure-deployer"
          }

          span { text " role." }
      }
      pre {
          _class "language-typescript"

          code {
              _class "language-typescript"

              span {
                  text
                      "import * as aws from '@pulumi/aws'\nimport * as identityProvider from './identityProvider'\nimport * as config from '../config'\n\nconst infrastructureDeployer = new aws.iam.Role('infrastructure-deployer', {\n    name: 'infrastructure-deployer',\n    assumeRolePolicy: {\n        Version: '2012-10-17',\n        Statement: [\n            // Trust principals in this account to assume role\n            {\n                Effect: 'Allow',\n                Action: 'sts:AssumeRole',\n                Principal: {\n                    AWS: config.accountId\n                }\n            },\n            // Allow `infrastructure` repo actions to assume role\n            {\n                Effect: 'Allow',\n                Action: 'sts:AssumeRoleWithWebIdentity',\n                Principal: {\n                    Federated: identityProvider.githubIdentityProviderArn\n                },\n                Condition: {\n                    StringEquals: {\n                        'token.actions.githubusercontent.com:aud': 'sts.amazonaws.com'\n                    },\n                    StringLike: {\n                        'token.actions.githubusercontent.com:sub': 'repo:ameier38/infrastructure:*'\n                    }\n                }\n            }\n        ]\n    }\n})"
              }
          }
      }
      div {
          span { text "Then I attached a policy to the " }

          code {
              _class "language-none"
              text "infrastructure-deployer"
          }

          span { text " role." }
      }
      pre {
          _class "language-typescript"

          code {
              _class "language-typescript"

              span {
                  text
                      "new aws.iam.RolePolicy('infrastructure-deployer', {\n    name: 'infrastructure-deployer',\n    role: role.infrastructureDeployerName,\n    policy: {\n        Version: '2012-10-17',\n        Statement: [\n            // Allow usage of `pulumi` key\n            {\n                Effect: 'Allow',\n                Action: [\n                    'kms:Decrypt',\n                    'kms:Encrypt'\n                ],\n                Resource: key.pulumiKey.arn\n            },\n            // Allow management of cloudflared ecr repository\n            {\n                Effect: 'Allow',\n                Action: 'ecr:GetAuthorizationToken',\n                Resource: '*'\n            },\n            {\n                Effect: 'Allow',\n                Action: [\n                    'ecr:*'\n                ],\n                Resource: pulumi.interpolate `arn:aws:ecr:${config.region}:${config.accountId}:repository/cloudflared-*`\n            },\n            // Allow management of ameier38-public bucket\n            {\n                Effect: 'Allow',\n                Action: [\n                    's3:*',\n                ],\n                Resource: [\n                    'arn:aws:s3:::ameier38-public',\n                    'arn:aws:s3:::ameier38-public/*'\n                ]\n            },\n            // Allow `infrastructure-deployer` role to manage email service\n            {\n                Effect: 'Allow',\n                Action: [\n                    'ses:*',\n                ],\n                Resource: '*'\n            },\n        ]\n    }\n})"
              }
          }
      }
      div {
          span { text "You can see the rest of the code in the " }

          a {
              _href "https://github.com/ameier38/infrastructure/tree/main/1-identity"
              span { text "GitHub repo" }
          }

          span { text "." }
      }
      div {
          span { text "I can then deploy the stack by first assuming the " }

          code {
              _class "language-none"
              text "identity-deployer"
          }

          span { text " role and using the Pulumi CLI." }
      }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"
              span { text "$env:AWS_PROFILE=\"identity-deployer\"\npulumi up" }
          }
      }
      h3 {
          _class "mt-6"
          _id "573dfd6c0afe47d3a8d484c944e24842"
          span { text "Managed Infrastructure Stack" }
      }
      div {
          span {
              text
                  "This stack includes resources needed to run and connect to the Kubernetes cluster. In order to create the stack I first added the "
          }

          code {
              _class "language-none"
              text "infrastructure-deployer"
          }

          span { text " role to the " }

          code {
              _class "language-none"
              text "~/.aws/config"
          }

          span { text " file." }
      }
      pre {
          _class "language-toml"

          code {
              _class "language-toml"

              span {
                  text
                      "[profile infrastructure-deployer]\nrole_arn=arn:aws:iam::400689721046:role/infrastructure-deployer\nsource_profile=admin"
              }
          }
      }
      div {
          span { text "Then I assumed the " }

          code {
              _class "language-none"
              text "infrastructure-deployer"
          }

          span { text " role and created the stack using the " }

          code {
              _class "language-none"
              text "pulumi"
          }

          span { text " encryption key created in the identity stack." }
      }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"

              span {
                  text
                      "$env:AWS_PROFILE=\"infrastructure-deployer\"\npulumi new aws-typescript --secrets-provider \"awskms://{pulumi key id}?region=us-east-1\""
              }
          }
      }
      h4 {
          _class "mt-4"
          _id "bb0431f031fd4e6e96b44f0af7ebfe20"
          span { text "Kubernetes API Tunnel" }
      }
      div {
          span {
              text
                  "Since I am running Kubernetes on Raspberry Pis in my apartment I need some way to connect to the Kubernetes API from outside my home network (such as GitHub Actions for deploying applications). This is made possible using "
          }

          a {
              _href "https://developers.cloudflare.com/cloudflare-one/connections/connect-apps/"
              span { text "Cloudflare Tunnels" }
          }

          span {
              text
                  ". A tunnel is a persistent connection between a daemon application (called cloudflared) and the nearest Cloudflare datacenter. Cloudflare creates a public IP address for each tunnel and then proxies requests to that IP address to the cloudflared daemon. The cloudflared daemon then forwards the request to a service based on rules you specify. The below diagram from "
          }

          a {
              _href "https://developers.cloudflare.com/cloudflare-one/connections/connect-apps/#how-it-works"
              span { text "Cloudflare’s documentation" }
          }

          span { text " illustrates this well." }
      }
      img {
          _class "drop-shadow-xl rounded"

          _src
              "https://assets.meiermade.com/andymeier/articles/personal-infrastructure/cloudflare-tunnel-diagram-ddaa9b198f33.webp"

          _alt "Cloudflare Tunnel routing a browser request through Cloudflare to a service on a local server"
          _attr ("loading", "lazy")
          _attr ("width", "1768")
          _attr ("height", "1102")
      }
      div {
          span {
              text
                  "I set up the cloudflared daemon on the Raspberry Pi running the master Kubernetes node. You can see this later in the "
          }

          a {
              _href "#ccd359bcf0b448efae3cefbf6ac48284"
              span { text "Cluster Stack" }
          }

          span { text "." }
      }
      div { span { text "Using Pulumi I can create the tunnel and configure the credentials. " } }
      pre {
          _class "language-typescript"

          code {
              _class "language-typescript"

              span {
                  text
                      "import * as cloudflare from '@pulumi/cloudflare'\nimport * as pulumi from '@pulumi/pulumi'\nimport * as random from '@pulumi/random'\n\nconst tunnelSecret = new random.RandomPassword('k8s-api-tunnel', {\n    length: 32\n})\n\nexport const k8sApiTunnel = new cloudflare.ArgoTunnel('k8s-api', {\n    accountId: cloudflare.config.accountId!,\n    name: 'k8s-api',\n    secret: tunnelSecret.result.apply(s => Buffer.from(s).toString('base64'))\n})\n\nexport const k8sApiTunnelCredentials = pulumi.all([\n    k8sApiTunnel.accountId,\n    k8sApiTunnel.id,\n    k8sApiTunnel.name,\n    k8sApiTunnel.secret\n]).apply(([accountId, tunnelId, tunnelName, tunnelSecret]) => {\n    return JSON.stringify({\n        AccountTag: accountId,\n        TunnelID: tunnelId,\n        TunnelName: tunnelName,\n        TunnelSecret: tunnelSecret\n    })\n})"
              }
          }
      }
      div {
          _class "bg-gray-200 dark:bg-gray-800 rounded p-2"
          span { text "The tunnel credentials format is not well documented but to get an example run " }

          code {
              _class "language-none"
              text "cloudflared tunnel create"
          }
      }
      h4 {
          _class "mt-4"
          _id "15f01fbec7194949a53899b8a59073a9"
          span { text "DNS Records" }
      }
      div {
          span {
              text
                  "With the tunnel defined, I then created a friendly DNS record to point to the tunnel. I use this domain in my kubeconfig file in order to connect to the Kubernetes API using kubectl."
          }
      }
      div {
          span { text "First I created the " }

          a {
              _href "https://www.cloudflare.com/learning/dns/glossary/dns-zone/"
              span { text "Cloudflare zone" }
          }

          span { text "." }
      }
      pre {
          _class "language-typescript"

          code {
              _class "language-typescript"

              span {
                  text
                      "import * as cloudflare from '@pulumi/cloudflare'\n\nconst andrewmeierDotDev = new cloudflare.Zone('andrewmeier.dev', {\n    zone: 'andrewmeier.dev'\n})\n\nnew cloudflare.ZoneSettingsOverride('andrewmeier.dev', {\n    zoneId: andrewmeierDotDev.id,\n    settings: {\n        ssl: 'strict'\n    }\n})\n\nexport const andrewmeierDotDevZoneId = andrewmeierDotDev.id\nexport const andrewmeierDotDevDomain = andrewmeierDotDev.zone"
              }
          }
      }
      div {
          span { text "Next I created the DNS record " }

          code {
              _class "language-none"
              text "k8s.andrewmeier.dev"
          }

          span { text " to point to the tunnel." }
      }
      pre {
          _class "language-typescript"

          code {
              _class "language-typescript"

              span {
                  text
                      "import * as cloudflare from '@pulumi/cloudflare'\nimport * as pulumi from '@pulumi/pulumi'\nimport * as tunnel from './tunnel'\nimport * as zone from './zone'\n\nexport const k8sApiRecord = new cloudflare.Record('k8s.andrewmeier.dev', {\n    zoneId: zone.andrewmeierDotDevZoneId,\n    name: 'k8s',\n    type: 'CNAME',\n    value: tunnel.k8sApiTunnel.cname,\n    proxied: true\n})"
              }
          }
      }
      h4 {
          _class "mt-4"
          _id "fbcd4ab34cd243b794de5698f11b5573"
          span { text "Identity Provider" }
      }
      div {
          span {
              text
                  "For applications that I don’t want to expose to the public I can also use Cloudflare to authenticate requests with an identity provider. I chose to use GitHub as the identity provider but there are other options as well (such as Google)."
          }
      }
      div {
          span { text "I first created a " }

          a {
              _href "https://docs.github.com/en/developers/apps/building-oauth-apps/creating-an-oauth-app"
              span { text "GitHub OAuth application" }
          }

          span { text " and then created the " }

          a {
              _href "https://developers.cloudflare.com/cloudflare-one/identity/"
              span { text "Cloudflare Access Identity Provider" }
          }

          span { text " in Pulumi using the client ID and client secret provided by the GitHub OAuth application." }
      }
      pre {
          _class "language-typescript"

          code {
              _class "language-typescript"

              span {
                  text
                      "const githubIdentityProvider = new cloudflare.AccessIdentityProvider('github', {\n    name: 'github',\n    type: 'github',\n    accountId: cloudflare.config.accountId!,\n    configs: [{\n        clientId: config.githubConfig.clientId,\n        clientSecret: config.githubConfig.clientSecret\n    }]\n})"
              }
          }
      }
      h4 {
          _class "mt-4"
          _id "86529189d6b9472f86b11f303c74c3eb"
          span { text "Access Application" }
      }
      div {
          span { text "In order to authenticate access to the Kubernetes API, I must create a " }

          a {
              _href "https://developers.cloudflare.com/cloudflare-one/tutorials/kubectl/"
              span { text "Cloudflare Access Application" }
          }

          span { text " associated with the DNS record that I created above." }
      }
      pre {
          _class "language-typescript"

          code {
              _class "language-typescript"

              span {
                  text
                      "import * as cloudflare from '@pulumi/cloudflare'\nimport { githubIdentityProvider } from './accessIdentityProvider'\nimport { k8sApiRecord } from './record'\n\nexport const k8sApi = new cloudflare.AccessApplication('k8s-api', {\n    name: 'Kubernetes API',\n    domain: k8sApiRecord.hostname,\n    accountId: cloudflare.config.accountId,\n    allowedIdps: [ githubIdentityProvider.id ],\n    autoRedirectToIdentity: true,\n    type: 'self_hosted'\n}) "
              }
          }
      }
      div {
          span { text "This way any request to " }

          code {
              _class "language-none"
              text "k8s.andrewmeier.dev"
          }

          span { text " will first need to authenticate with GitHub." }
      }
      h4 {
          _class "mt-4"
          _id "e8047634165f4a8eb61fbbfc2687baaf"
          span { text "Access Policy" }
      }
      div {
          span {
              text
                  "With the authentication configured I now need to configure authorization. This is accomplished using a "
          }

          a {
              _href "https://developers.cloudflare.com/cloudflare-one/policies/access/"
              span { text "Cloudflare Access Policy" }
          }

          span {
              text
                  ". Because I also want to access the Kubernetes API when running GitHub actions, I first need to create an "
          }

          a {
              _href "https://developers.cloudflare.com/cloudflare-one/identity/service-tokens/"
              span { text "Access Service Token" }
          }

          span { text "." }
      }
      pre {
          _class "language-typescript"

          code {
              _class "language-typescript"

              span {
                  text
                      "import * as cloudflare from '@pulumi/cloudflare'\n\nexport const githubServiceToken = new cloudflare.AccessServiceToken('github', {\n    name: 'GitHub',\n    accountId: cloudflare.config.accountId!\n})"
              }
          }
      }
      div {
          span {
              text
                  "Then I can create the access policies to authorize my email address (authenticated with GitHub) and systems using the service token."
          }
      }
      pre {
          _class "language-typescript"

          code {
              _class "language-typescript"

              span {
                  text
                      "import * as cloudflare from '@pulumi/cloudflare'\nimport { k8sApi } from './accessApplication'\nimport { githubServiceToken } from './serviceToken'\nimport { email } from '../config'\n\nnew cloudflare.AccessPolicy('k8s-api-user-access', {\n    name: 'Kubernetes API User Access',\n    precedence: 1,\n    accountId: cloudflare.config.accountId,\n    applicationId: k8sApi.id,\n    decision: 'allow',\n    includes: [{\n        emails: [ email ]\n    }]\n})\n\nnew cloudflare.AccessPolicy('k8s-api-github-access', {\n    name: 'Kubernetes API GitHub Access',\n    precedence: 2,\n    accountId: cloudflare.config.accountId,\n    applicationId: k8sApi.id,\n    decision: 'non_identity',\n    includes: [{\n        serviceTokens: [ githubServiceToken.id ]\n    }]\n})"
              }
          }
      }
      div {
          span { text "I can now deploy the stack by assuming the " }

          code {
              _class "language-none"
              text "infrastructure-deployer"
          }

          span { text " role then using the Pulumi CLI." }
      }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"
              span { text "$env:AWS_PROFILE=\"infrastructure-deployer\"\npulumi up" }
          }
      }
      h3 {
          _class "mt-6"
          _id "7a1f1ed7f7da4059a63db2d46a4ad7f9"
          span { text "Cluster Stack" }
      }
      div {
          span {
              text
                  "This stack defines the Kubernetes cluster. I decided to host the cluster myself instead of using a managed cluster for a few reasons:"
          }
      }
      ol {
          _class "list-decimal"
          li { span { text "I wanted to learn more about networking " } }
          li { span { text "It is cheaper" } }
      }
      div {
          span {
              text
                  "There are three nodes in the cluster, each of which is a Raspberry Pi. In order to run Kubernetes I am using "
          }

          a {
              _href "https://k3s.io/"
              span { text "k3s" }
          }

          span {
              text
                  ", which is a lightweight version of Kubernetes. Running k3s on Raspberry Pis is great because they don’t take up much space (the entire rack fits in my coat closet), they don’t use much power (can run on PoE), and they are cheap (I bought them before chip crisis). "
          }
      }
      div {
          span { text "There is a bit of manual setup for each Pi which is provided in the " }

          a {
              _href "https://github.com/ameier38/infrastructure/blob/main/2-cluster/README.md"
              span { text "stack README" }
          }

          span { text "." }
      }
      div {
          span { text "Once the Pis are setup and running, I can install k3s and cloudflared using the " }

          a {
              _href "https://www.pulumi.com/registry/packages/command/"
              span { text "Pulumi Command" }
          }

          span { text " package. I first created the stack using the key created in the identity stack." }
      }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"

              span {
                  text
                      "$env:AWS_PROFILE=\"infrastructure-deployer\"\npulumi new aws-typescript --secrets-provider \"awskms://{pulumi key id}?region=us-east-1\""
              }
          }
      }
      div {
          span {
              text
                  "Then I configured the connections to each of the Raspberry Pis and retrieved the Kubernetes API tunnel information exported from the Managed Infrastructure stack."
          }
      }
      pre {
          _class "language-typescript"

          code {
              _class "language-typescript"

              span {
                  text
                      "import * as pulumi from '@pulumi/pulumi'\nimport * as command from '@pulumi/command'\n\nexport const env = pulumi.getStack()\n\nconst managedInfrastructureStack = new pulumi.StackReference('ameier38/managed-infrastructure/prod')\n\nexport const k8sApiTunnelId = managedInfrastructureStack.requireOutput('k8sApiTunnelId')\nexport const k8sApiTunnelCredentials = managedInfrastructureStack.requireOutput('k8sApiTunnelCredentials')\nexport const k8sApiTunnelHost = managedInfrastructureStack.requireOutput('k8sApiTunnelHost')\n\nconst rawConfig = new pulumi.Config()\nexport const privateKey = rawConfig.requireSecret('privateKey')\n\nexport const masterConn: command.types.input.remote.ConnectionArgs = {\n    host: 'ameier-1',\n    port: 22,\n    user: 'root',\n    privateKey: privateKey\n}\n\nexport const agent1Conn: command.types.input.remote.ConnectionArgs = {\n    host: 'ameier-2',\n    port: 22,\n    user: 'root',\n    privateKey: privateKey\n}\n\nexport const agent2Conn: command.types.input.remote.ConnectionArgs = {\n    host: 'ameier-3',\n    port: 22,\n    user: 'root',\n    privateKey: privateKey\n} "
              }
          }
      }
      div { span { text "Then I created the scripts required to install cloudflared and k3s." } }
      pre {
          _class "language-typescript"

          code {
              _class "language-typescript"

              span {
                  text
                      "import * as command from '@pulumi/command'\nimport * as pulumi from '@pulumi/pulumi'\nimport * as config from './config'\n\n// ref: https://developers.cloudflare.com/cloudflare-one/tutorials/kubectl/\nconst installCloudflaredScript = pulumi.interpolate `\necho \"Installing cloudflared\"\n\nset -e\n\necho \"Creating config directory\"\nmkdir -p /etc/cloudflared\n\necho \"Writing credentials\"\ncat << EOF > /etc/cloudflared/credentials.json\n${config.k8sApiTunnelCredentials}\nEOF\n\necho \"Writing config\"\ncat << EOF > /etc/cloudflared/config.yml\ntunnel: ${config.k8sApiTunnelId}\ncredentials-file: /etc/cloudflared/credentials.json\ningress:\n  - hostname: ${config.k8sApiTunnelHost}\n    service: tcp://localhost:6443\n    originRequest:\n      proxyType: socks\n  - service: http_status:404\nEOF\n\necho \"Downloading cloudflared\"\ncurl -sfLO https://github.com/cloudflare/cloudflared/releases/download/2022.3.4/cloudflared-linux-arm64\n\necho \"Updating cloudflared permissions\"\nchmod +x cloudflared-linux-arm64\n\necho \"Moving cloudflared to bin\"\nmv cloudflared-linux-arm64 /usr/local/bin/cloudflared\n\nif [ ! -f /etc/systemd/system/cloudflared.service ]\nthen\n    echo \"Installing cloudflared service\"\n    cloudflared service install\n    echo \"Starting cloudflared service\"\n    systemctl start cloudflared \nelse\n    echo \"Restarting cloudflared service\"\n    systemctl restart cloudflared\nfi\n`\n\nconst installK3sMaster = new command.remote.Command('install-k3s-master', {\n    connection: config.masterConn,\n    create: 'curl -sfL https://get.k3s.io | sh -'\n})\n\nnew command.remote.Command('install-cloudflared', {\n    connection: config.masterConn,\n    create: installCloudflaredScript,\n    triggers: [ installCloudflaredScript ]\n})\n\nconst readKubeconfig = new command.remote.Command('read-kubeconfig', {\n    connection: config.masterConn,\n    create: 'cat /etc/rancher/k3s/k3s.yaml'\n}, { dependsOn: installK3sMaster })\n\nexport const kubeconfig =\n    pulumi\n        .all([readKubeconfig.stdout, config.k8sApiTunnelHost])\n        .apply(([kubeconfig, host]) => kubeconfig.replace('127.0.0.1', host))\n\nconst readToken = new command.remote.Command('read-token', {\n    connection: config.masterConn,\n    create: 'cat /var/lib/rancher/k3s/server/node-token'\n}, {dependsOn: installK3sMaster })\n\nconst token = readToken.stdout.apply(token => token.replace('\\n', ''))\n\nfor (const [i, conn] of [config.agent1Conn, config.agent2Conn].entries()) {\n    new command.remote.Command(`install-k3s-agent-${i}`, {\n        connection: conn,\n        create: pulumi.interpolate `curl -sfL https://get.k3s.io | K3S_URL=\"https://${config.masterConn.host}:6443\" K3S_TOKEN=\"${token}\" sh -`,\n        triggers: [token]\n    })\n}"
              }
          }
      }
      div {
          span { text "Then I can assume the " }

          code {
              _class "language-none"
              text "infrastructure-deployer"
          }

          span { text " role (needed to access KMS key in order to decrypt secrets) and deploy the cluster." }
      }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"
              span { text "$env:AWS_PROFILE=\"infrastructure-deployer\"\npulumi up" }
          }
      }
      div {
          _class "bg-gray-200 dark:bg-gray-800 rounded p-2"

          span {
              text
                  "I must run this on the local network in order to access the Raspberry Pis. I could set up SSH with cloudflared running on each Pi but have not done it yet 😃"
          }
      }
      h4 {
          _class "mt-4"
          _id "02504c7d07d74c2685db65e08c85920c"
          span { text "Connecting" }
      }
      div {
          span {
              text
                  "In order to connect to the cluster using kubectl I have to configure a few things. First I need to export the kubeconfig file from the Cluster stack."
          }
      }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"
              span { text "pulumi stack output --show-secrets kubeconfig > ~/.kube/kubeconfig" }
          }
      }
      div { span { text "Then I have to configure the kubeconfig file to use a proxy." } }
      pre {
          _class "language-yaml"

          code {
              _class "language-yaml"

              span {
                  text
                      "apiVersion: v1\nclusters:\n- cluster:\n    certificate-authority-data: ...\n    proxy-url: socks5://localhost:1234\n    server: https://k8s.andrewmeier.dev:6443\n  name: default\ncontexts:\n- context:\n    cluster: default\n    namespace: andrewmeier\n    user: default\n  name: default\ncurrent-context: default\nkind: Config\npreferences: {}\nusers:\n- name: default\n  user:\n    client-certificate-data: ...\n    client-key-data: ..."
              }
          }
      }
      div {
          span { text "Note that I configured the tunnel in the installation script with " }

          code {
              _class "language-none"
              text "proxyType: socks"
          }

          span { text " and pointed to the Kubernetes API " }

          code {
              _class "language-none"
              text "tcp://localhost:6443"
          }

          span { text " as shown below." }
      }
      pre {
          _class "language-yaml"

          code {
              _class "language-yaml"

              span {
                  text
                      "tunnel: ${config.k8sApiTunnelId}\ncredentials-file: /etc/cloudflared/credentials.json\ningress:\n  - hostname: ${config.k8sApiTunnelHost}\n    service: tcp://localhost:6443\n    originRequest:\n      proxyType: socks\n  - service: http_status:404"
              }
          }
      }
      div { span { text "Then on my laptop I connect to the tunnel using the cloudflared CLI." } }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"
              span { text "cloudflared access tcp --hostname=k8s.andrewmeier.dev --url=localhost:1234" }
          }
      }
      div {
          span { text "This sets up a proxy server on my laptop which will direct requests from " }

          code {
              _class "language-none"
              text "localhost:1234"
          }

          span { text " to " }

          code {
              _class "language-none"
              text "k8s.andrewmeier.dev"
          }

          span {
              text
                  ". Above I configured the kubeconfig to use this proxy. In a different shell, when I first run a kubectl command, a browser window will open asking me to authenticate with GitHub (I set up an access policy for the Kubernetes API in the Managed Infrastructure stack). Additional details about using kubectl with Cloudflare Tunnels can be found in the "
          }

          a {
              _href "https://developers.cloudflare.com/cloudflare-one/tutorials/kubectl/"
              span { text "Cloudflare documentation" }
          }

          span { text "." }
      }
      div {
          span {
              text
                  "To make this easier for me, I added a function to my PowerShell profile to connect to the tunnel as a Job."
          }
      }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"

              span {
                  text
                      "function start-ameier-k8s-tunnel {\n    $env:KUBECONFIG=\"C:\\Users\\andy\\.kube\\ameier-kubeconfig\"\n    Start-Job -Name ameier-k8s-tunnel -ScriptBlock { cloudflared access tcp --hostname=k8s.andrewmeier.dev --url=localhost:1234 }\n}"
              }
          }
      }
      div {
          span { text "Then before running my kubectl commands I just need to make sure to run " }

          code {
              _class "language-none"
              text "start-ameier-k8s-tunnel"
          }

          span { text " first." }
      }
      h3 {
          _class "mt-6"
          _id "f1d5b2ab35694d7cac0fdc80862ef03d"
          span { text "Cluster Services Stack" }
      }
      div {
          span {
              text
                  "This stack defines all the cluster-wide services that are used for running and monitoring applications. I first created the stack using the Pulumi CLI."
          }
      }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"

              span {
                  text
                      "$env:AWS_PROFILE=\"infrastructure-deployer\"\npulumi new aws-typescript --secrets-provider \"awskms://{pulumi key id}?region=us-east-1\""
              }
          }
      }
      div {
          span {
              text
                  "Next, because this stack will connect to the Kubernetes cluster, I need to configure the stack to use the kubeconfig file that I created above."
          }
      }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"
              span { text "cat ~/.kube/kubeconfig | pulumi config set --secret kubernetes:kubeconfig" }
          }
      }
      h4 {
          _class "mt-4"
          _id "f42bc896f4364370a9370e116069d86d"
          span { text "Reverse Proxy" }
      }
      div {
          span { text "I use Traefik for the reverse proxy which is used to manage request routing. Traefik " }

          a {
              _href "https://rancher.com/docs/k3s/latest/en/networking/#traefik-ingress-controller"
              span { text "ships with k3s" }
          }

          span { text " so there is not much to set up. In my case I want to use " }

          a {
              _href "https://developers.cloudflare.com/ssl/origin-configuration/ssl-modes/full-strict/"
              span { text "Cloudflare’s  Full Strict SSL" }
          }

          span { text " which requires the use of of a Cloudflare origin certificate to terminate TLS. " }
      }
      div {
          span { text "I first created the origin certificate, leveraging the convenient " }

          a {
              _href "https://www.pulumi.com/registry/packages/tls/"
              span { text "Pulumi TLS package" }
          }

          span { text "." }
      }
      pre {
          _class "language-typescript"

          code {
              _class "language-typescript"

              span {
                  text
                      "import * as cloudflare from '@pulumi/cloudflare'\nimport * as pulumi from '@pulumi/pulumi'\nimport * as tls from '@pulumi/tls'\nimport * as config from '../config'\n\nexport const originCertPrivateKey = new tls.PrivateKey('origin-cert', {\n    algorithm: 'RSA'\n})\n\nconst originCertRequest = new tls.CertRequest('origin-cert-request', {\n    privateKeyPem: originCertPrivateKey.privateKeyPem,\n    subject: {\n        commonName: config.andrewmeierDotDevDomain,\n        organization: 'andrewmeier.dev'\n    }\n})\n\nexport const originCert = new cloudflare.OriginCaCertificate('origin-cert', {\n    csr: originCertRequest.certRequestPem,\n    requestType: 'origin-rsa',\n    hostnames: [\n        config.andrewmeierDotDevDomain,\n        pulumi.interpolate `*.${config.andrewmeierDotDevDomain}`\n    ]\n})"
              }
          }
      }
      div { span { text "Next I configured Traefik to use this certificate for terminating TLS." } }
      pre {
          _class "language-typescript"

          code {
              _class "language-typescript"

              span {
                  text
                      "import * as k8s from '@pulumi/kubernetes'\nimport { originCert, originCertPrivateKey } from '../cloudflare/originCertificate'\n\n// Traefik is deployed as part of k3s\n\nconst originCertSecret = new k8s.core.v1.Secret('origin-cert', {\n    metadata: { namespace: 'kube-system' },\n    stringData: {\n        'tls.crt': originCert.certificate,\n        'tls.key': originCertPrivateKey.privateKeyPem\n    }\n})\n\n// Enables Cloudflare Full Strict SSL\nnew k8s.apiextensions.CustomResource('tls-store', {\n    apiVersion: 'traefik.containo.us/v1alpha1',\n    kind: 'TLSStore',\n    metadata: {\n        name: 'default',\n        namespace: 'kube-system'\n    },\n    spec: {\n        defaultCertificate: {\n            secretName: originCertSecret.metadata.name\n        }\n    }\n})"
              }
          }
      }
      h4 {
          _class "mt-4"
          _id "ee00759c3def4058ba87450e0d264d27"
          span { text "Tunnel" }
      }
      div {
          span { text "In order to connect to applications from outside the cluster I created another " }

          a {
              _href "https://developers.cloudflare.com/cloudflare-one/connections/connect-apps/"
              span { text "Cloudflare Tunnel" }
          }

          span { text ". " }
      }
      pre {
          _class "language-typescript"

          code {
              _class "language-typescript"

              span {
                  text
                      "import * as cloudflare from '@pulumi/cloudflare'\nimport * as pulumi from '@pulumi/pulumi'\nimport * as random from '@pulumi/random'\n\nconst k8sTunnelSecret = new random.RandomPassword('k8s-tunnel-secret', {\n    length: 32\n})\n\nexport const k8sTunnel = new cloudflare.ArgoTunnel('k8s', {\n    accountId: cloudflare.config.accountId!,\n    name: 'k8s',\n    secret: k8sTunnelSecret.result.apply(s => Buffer.from(s).toString('base64'))\n})\n\nexport const k8sTunnelCredentials = pulumi.all([\n    k8sTunnel.accountId,\n    k8sTunnel.id,\n    k8sTunnel.name,\n    k8sTunnel.secret\n]).apply(([accountId, tunnelId, tunnelName, tunnelSecret]) => {\n    return JSON.stringify({\n        AccountTag: accountId,\n        TunnelID: tunnelId,\n        TunnelName: tunnelName,\n        TunnelSecret: tunnelSecret\n    })\n})"
              }
          }
      }
      div {
          span {
              text
                  "I then created a deployment for the cloudflared daemon and configured it to forward requests from the tunnel to the Traefik proxy. "
          }
      }
      pre {
          _class "language-typescript"

          code {
              _class "language-typescript"

              span {
                  text
                      "import * as k8s from '@pulumi/kubernetes'\nimport * as pulumi from '@pulumi/pulumi'\nimport * as repository from '../aws/repository'\nimport * as tunnel from '../cloudflare/tunnel'\nimport * as config from '../config'\n\nconst identifier = 'cloudflared'\n\nconst cloudflaredConfig = pulumi.interpolate `\ntunnel: ${tunnel.k8sTunnel.id}\ncredentials-file: /var/secrets/cloudflared/credentials.json\nmetrics: 0.0.0.0:2000\nno-autoupdate: true\ningress:\n  - hostname: ${config.andrewmeierDotDevDomain}\n    service: http://traefik.kube-system\n  - hostname: '*.${config.andrewmeierDotDevDomain}'\n    service: http://traefik.kube-system\n  - service: http_status:404\n`\n\nconst cloudflaredSecret = new k8s.core.v1.Secret(identifier, {\n    metadata: { namespace: 'kube-system' },\n    stringData: {\n        'config.yaml': cloudflaredConfig,\n        'credentials.json': tunnel.k8sTunnelCredentials\n    }\n})\n\nconst registrySecret = new k8s.core.v1.Secret(`${identifier}-registry`, {\n    metadata: { namespace: 'kube-system' },\n    type: 'kubernetes.io/dockerconfigjson',\n    stringData: {\n        '.dockerconfigjson': repository.cloudflaredDockerconfigjson\n    }\n})\n\nconst labels = { 'app.kubernetes.io/name': identifier }\n\nnew k8s.apps.v1.Deployment(identifier, {\n    metadata: {\n        name: identifier,\n        namespace: 'kube-system'\n    },\n    spec: {\n        replicas: 1,\n        selector: { matchLabels: labels },\n        template: {\n            metadata: {\n                labels: labels,\n                annotations: {\n                    'prometheus.io/scrape': 'true',\n                    'prometheus.io/path': '/metrics',\n                    'prometheus.io/port': '2000',\n                }\n            },\n            spec: {\n                imagePullSecrets: [{\n                    name: registrySecret.metadata.name\n                }],\n                containers: [{\n                        name: identifier,\n                        image: repository.cloudflaredImageName,\n                        args: [\n                            'tunnel',\n                            '--config', '/var/secrets/cloudflared/config.yaml',\n                            'run'\n                        ],\n                        livenessProbe: {\n                            httpGet: { path: '/ready', port: 2000 },\n                            failureThreshold: 1,\n                            initialDelaySeconds: 10,\n                            periodSeconds: 10\n                        },\n                        volumeMounts: [{\n                            name: 'cloudflared',\n                            mountPath: '/var/secrets/cloudflared',\n                            readOnly: true\n                        }]\n                }],\n                volumes: [{\n                    name: 'cloudflared',\n                    secret: { secretName: cloudflaredSecret.metadata.name }\n                }],\n                nodeSelector: { 'kubernetes.io/arch': 'arm64' }\n            }            \n        }\n    }\n})"
              }
          }
      }
      div {
          span { text "This will allow me to use the " }

          a {
              _href "https://doc.traefik.io/traefik/routing/providers/kubernetes-crd/#kind-ingressroute"
              span { text "Traefik IngressRoute" }
          }

          span { text " to direct traffic to services running in the cluster." }
      }
      h4 {
          _class "mt-4"
          _id "442c43c7c180479c8ce304ba110de961"
          span { text "Monitoring " }
      }
      div {
          span {
              text
                  "I use Prometheus for monitoring applications. It is easy to expose metrics endpoints using client libraries such as "
          }

          a {
              _href "https://github.com/prometheus-net/prometheus-net"
              span { text "prometheus-net" }
          }

          span {
              text
                  ". Then you just need to annotate the pods and Prometheus will take care of scraping each pod. I used "
          }

          a {
              _href "https://helm.sh/"
              span { text "Helm" }
          }

          span { text " as part of the " }

          a {
              _href "https://www.pulumi.com/registry/packages/kubernetes/"
              span { text "Pulumi Kubernetes package" }
          }

          span { text " to deploy." }
      }
      pre {
          _class "language-typescript"

          code {
              _class "language-typescript"

              span {
                  text
                      "import * as k8s from '@pulumi/kubernetes'\nimport * as pulumi from '@pulumi/pulumi'\nimport * as namespace from './namespace'\n\nconst chart = new k8s.helm.v3.Chart('prometheus', {\n    chart: 'prometheus',\n    version: '15.5.3',\n    fetchOpts: { repo: 'https://prometheus-community.github.io/helm-charts' },\n    namespace: namespace.monitoringNamespace,\n    values: {\n        serviceAccounts: {\n            alertmanager: { create: false },\n            pushgateway: { create: false }\n        },\n        alertmanager: { enabled: false },\n        pushgateway: { enabled: false }\n    }\n})\n\nconst internalHost =\n    pulumi.all([chart, namespace.monitoringNamespace]).apply(([chart, namespace]) => {\n        const meta = chart.getResourceProperty('v1/Service', namespace, 'prometheus-server', 'metadata')\n        return pulumi.interpolate `${meta.name}.${meta.namespace}.svc.cluster.local`\n    })\n\nconst internalPort =\n    pulumi.all([chart, namespace.monitoringNamespace]).apply(([chart, namespace]) => {\n        const spec = chart.getResourceProperty('v1/Service', namespace, 'prometheus-server', 'spec')\n        return spec.ports[0].port\n    })\n\nexport const internalUrl = pulumi.interpolate `http://${internalHost}:${internalPort}`"
              }
          }
      }
      div {
          span {
              text
                  "I use Promtail and Loki to aggregate the logs from all the applications. Promtail takes care of scraping the pod logs and sending to Loki in the format it expects. Loki is nice as it integrates with Grafana and uses the same query structure as Prometheus. I also used Helm to deploy Loki and Promtail. You can see the "
          }

          a {
              _href "https://github.com/ameier38/infrastructure/tree/main/4-cluster-services"
              span { text "rest of the code on GitHub" }
          }

          span { text "." }
      }
      div {
          span { text "Again, like with the previous stacks, I can deploy the stack by assuming the " }

          code {
              _class "language-none"
              text "infrastructure-deployer"
          }

          span {
              text
                  " role and using the Pulumi CLI. I also need to connect to the tunnel so that the stack can connect to the Kubernetes API."
          }
      }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"
              span { text "$env:AWS_PROFILE=\"infrastructure-deployer\"\nstart-ameier-k8s-tunnel\npulumi up" }
          }
      }
      h3 {
          _class "mt-6"
          _id "23ec3e2370f34b5eb33d4dd87df8d901"
          span { text "App Services Stack" }
      }
      div {
          span {
              text
                  "This stack is used for any application specific resources such as databases and DNS records. In my case I do not use any databases at the moment so it is just used for DNS and configuring access to internal services. Again, I first created the stack using the Pulumi CLI."
          }
      }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"

              span {
                  text
                      "$env:AWS_PROFILE=\"infrastructure-deployer\"\npulumi new aws-typescript --secrets-provider \"awskms://{pulumi key id}?region=us-east-1\""
              }
          }
      }
      div {
          span {
              text
                  "Next I created the DNS records for the applications which all point to the hostname of the tunnel created in the Cluster Services stack (this tunnel routes requests to Traefik)."
          }
      }
      pre {
          _class "language-typescript"

          code {
              _class "language-typescript"

              span {
                  text
                      "import * as cloudflare from '@pulumi/cloudflare'\nimport * as config from '../config'\n\nexport const traefikRecord = new cloudflare.Record('traefik.andrewmeier.dev', {\n    zoneId: config.andrewmeierDotDevZoneId,\n    name: 'traefik',\n    type: 'CNAME',\n    value: config.k8sTunnelHost,\n    proxied: true\n})\n\nexport const whoamiRecord = new cloudflare.Record('whoami.andrewmeier.dev', {\n    zoneId: config.andrewmeierDotDevZoneId,\n    name: 'whoami',\n    type: 'CNAME',\n    value: config.k8sTunnelHost,\n    proxied: true\n})\n\nexport const grafanaRecord = new cloudflare.Record('grafana.andrewmeier.dev', {\n    zoneId: config.andrewmeierDotDevZoneId,\n    name: 'grafana',\n    type: 'CNAME',\n    value: config.k8sTunnelHost,\n    proxied: true\n})\n\nexport const andrewmeierRecord = new cloudflare.Record('andrewmeier.dev', {\n    zoneId: config.andrewmeierDotDevZoneId,\n    name: '@',\n    type: 'CNAME',\n    value: config.k8sTunnelHost,\n    proxied: true\n})"
              }
          }
      }
      div {
          span { text "Next I created " }

          a {
              _href "https://developers.cloudflare.com/cloudflare-one/applications/"
              span { text "Cloudflare Access Applications" }
          }

          span { text " for each of the applications that I want to authenticate user access." }
      }
      pre {
          _class "language-typescript"

          code {
              _class "language-typescript"

              span {
                  text
                      "import * as cloudflare from '@pulumi/cloudflare'\nimport * as record from './record'\nimport * as config from '../config'\n\nexport const traefik = new cloudflare.AccessApplication('traefik', {\n    name: 'Traefik',\n    domain: record.traefikRecord.hostname,\n    allowedIdps: [ config.githubIdentityProviderId ],\n    autoRedirectToIdentity: true,\n    accountId: cloudflare.config.accountId,\n    logoUrl: config.logoUrl,\n    httpOnlyCookieAttribute: false\n})\n\nexport const whoami = new cloudflare.AccessApplication('whoami', {\n    name: 'Whoami',\n    domain: record.whoamiRecord.hostname,\n    allowedIdps: [ config.githubIdentityProviderId ],\n    autoRedirectToIdentity: true,\n    accountId: cloudflare.config.accountId,\n    logoUrl: config.logoUrl,\n    httpOnlyCookieAttribute: false\n})\n\nexport const grafana = new cloudflare.AccessApplication('grafana', {\n    name: 'Grafana',\n    domain: record.grafanaRecord.hostname,\n    allowedIdps: [ config.githubIdentityProviderId ],\n    autoRedirectToIdentity: true,\n    accountId: cloudflare.config.accountId,\n    logoUrl: config.logoUrl,\n    httpOnlyCookieAttribute: false\n})"
              }
          }
      }
      div {
          span { text "Lastly I created " }

          a {
              _href "https://developers.cloudflare.com/cloudflare-one/policies/access/"
              span { text "Cloudflare Access Policies" }
          }

          span { text " to authorize my email." }
      }
      pre {
          _class "language-typescript"

          code {
              _class "language-typescript"

              span {
                  text
                      "import * as cloudflare from '@pulumi/cloudflare'\nimport * as app from './accessApplication'\nimport * as config from '../config'\n\nnew cloudflare.AccessPolicy('traefik-user-access', {\n    name: 'Traefik User Access',\n    precedence: 1,\n    accountId: cloudflare.config.accountId,\n    applicationId: app.traefik.id,\n    decision: 'allow',\n    includes: [{\n        emails: [ config.email ]\n    }]\n})\n\nnew cloudflare.AccessPolicy('whoami-user-access', {\n    name: 'Whoami User Access',\n    precedence: 1,\n    accountId: cloudflare.config.accountId,\n    applicationId: app.whoami.id,\n    decision: 'allow',\n    includes: [{\n        emails: [ config.email ]\n    }]\n})\n\nnew cloudflare.AccessPolicy('grafana-user-access', {\n    name: 'Grafana User Access',\n    precedence: 1,\n    accountId: cloudflare.config.accountId,\n    applicationId: app.grafana.id,\n    decision: 'allow',\n    includes: [{\n        emails: [ config.email ]\n    }]\n})"
              }
          }
      }
      div {
          span { text "As before, I assume the " }

          code {
              _class "language-none"
              text "infrastructure-deployer"
          }

          span { text " role and deploy the stack using the Pulumi CLI." }
      }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"
              span { text "$env:AWS_PROFILE=\"infrastructure-deployer\"\npulumi up" }
          }
      }
      h3 {
          _class "mt-6"
          _id "246a7816de444b889fd4e169e6017b31"
          span { text "Managed Apps Stack" }
      }
      div {
          span { text "This stack is used for any 3rd party applications that I want to deploy. I am currently using " }

          a {
              _href "https://grafana.com/"
              span { text "Grafana" }
          }

          span { text " to visualize logs and metrics and " }

          a {
              _href "https://github.com/traefik/whoami"
              span { text "whoami" }
          }

          span { text " to debug routing and requests. I also expose the " }

          a {
              _href "https://doc.traefik.io/traefik/operations/dashboard/"
              span { text "Traefik Dashboard" }
          }

          span { text " in this stack. As before, I first created the stack using the Pulumi CLI." }
      }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"

              span {
                  text
                      "$env:AWS_PROFILE=\"infrastructure-deployer\"\npulumi new aws-typescript --secrets-provider \"awskms://{pulumi key id}?region=us-east-1\""
              }
          }
      }
      div {
          span { text "I also need to add the kubeconfig file since I will need to connect to the Kubernetes cluster." }
      }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"
              span { text "cat ~/.kube/kubeconfig | pulumi config set --secret kubernetes:kubeconfig" }
          }
      }
      div {
          span {
              text
                  "Next I created the Grafana resources. I use Helm again to deploy the Grafana application and use the "
          }

          a {
              _href "https://doc.traefik.io/traefik/routing/providers/kubernetes-crd/#kind-ingressroute"
              span { text "Traefik IngressRoute CRD" }
          }

          span { text " to route any requests to " }

          code {
              _class "language-none"
              text "grafana.andrewmeier.dev"
          }

          span { text " to the Grafana service." }
      }
      pre {
          _class "language-typescript"

          code {
              _class "language-typescript"

              span {
                  text
                      "import * as k8s from '@pulumi/kubernetes'\nimport * as pulumi from '@pulumi/pulumi'\nimport * as random from '@pulumi/random'\nimport * as config from '../config'\n\nconst identifier = 'grafana'\n\nconst rawAdminPassword = new random.RandomPassword('admin-password', {\n    length: 20\n})\n\nexport const adminPassword = rawAdminPassword.result\n\nconst secret = new k8s.core.v1.Secret(identifier, {\n    metadata: { namespace: config.monitoringNamespace},\n    stringData: {\n        user: 'admin',\n        password: adminPassword\n    }\n})\n\nconst chart = new k8s.helm.v3.Chart(identifier, {\n    chart: 'grafana',\n    version: '6.24.1',\n    fetchOpts: { repo: 'https://grafana.github.io/helm-charts' },\n    namespace: config.monitoringNamespace,\n    values: {\n        // Use old version to provision notifiers\n        image: { tag: '8.2.7' },\n        testFramework: { enabled: false },\n        persistence: {\n            inMemory: { enabled: true }\n        },\n        admin: {\n            existingSecret: secret.metadata.name,\n            userKey: 'user',\n            passwordKey: 'password'\n        },\n        'grafana.ini': {\n            server: {\n                root_url: pulumi.interpolate `https://${config.grafanaHost}`,\n            },\n            smtp: {\n                enabled: true,\n                host: 'email-smtp.us-east-1.amazonaws.com:587',\n                user: config.smtpUserAccessKeyId,\n                password: config.smtpUserSmtpPassword,\n                from_address: '"

                  text "grafana@andrewmeier.dev"

                  text
                      "'\n            },\n            users: {\n                auto_assign_org_role: 'Admin'\n            },\n            'auth.proxy': {\n                enabled: true,\n                header_name: 'Cf-Access-Authenticated-User-Email',\n                header_property: 'email'\n            }\n        },\n        datasources: {\n            'datasources.yaml': {\n                apiVersion: 1,\n                datasources: [\n                    {\n                        name: 'Prometheus',\n                        type: 'prometheus',\n                        url: config.prometheusUrl,\n                        access: 'proxy',\n                        isDefault: true\n                    },\n                    {\n                        name: 'Loki',\n                        type: 'loki',\n                        url: config.lokiUrl,\n                        access: 'proxy',\n                        jsonData: { maxLines: 1000 }\n                    }\n                ]\n            }\n        },\n        notifiers: {\n            'notifiers.yaml': {\n                notifiers: [\n                    {\n                        name: 'email-notifier',\n                        type: 'email',\n                        uid: 'email1',\n                        org_id: 1,\n                        is_default: true,\n                        settings: { addresses: config.email }\n                    }\n                ]\n            }\n        }\n    }\n})\n\nconst internalPort =\n    pulumi.all([chart, config.monitoringNamespace]).apply(([chart, namespace]) => {\n        const spec = chart.getResourceProperty('v1/Service', namespace, identifier, 'spec')\n        return spec.ports[0].port\n    })\n\nnew k8s.apiextensions.CustomResource(`${identifier}-route`, {\n    apiVersion: 'traefik.containo.us/v1alpha1',\n    kind: 'IngressRoute',\n    metadata: { namespace: config.monitoringNamespace },\n    spec: {\n        entryPoints: ['web'],\n        routes: [{\n            kind: 'Rule',\n            match: pulumi.interpolate `Host(\\`${config.grafanaHost}\\`)`,\n            services: [{\n                kind: 'Service',\n                name: identifier,\n                namespace: config.monitoringNamespace,\n                port: internalPort\n            }]\n        }]\n    }\n})"
              }
          }
      }
      div {
          span {
              text
                  "Grafana plays nicely with proxy authentication and I can configure it to automatically log in the user by specifying that it should use the "
          }

          code {
              _class "language-none"
              text "Cf-Access-Authenticated-User-Email"
          }

          span { text " header which Cloudflare Access will set after authenticating the user." }
      }
      div {
          span { text "Next I created the resources for whoami and the Traefik dashboard. You can see the " }

          a {
              _href "https://github.com/ameier38/infrastructure/tree/main/5-app-services"
              span { text "rest of this code on GitHub" }
          }

          span { text "." }
      }
      div {
          span { text "I can deploy the stack by assuming the " }

          code {
              _class "language-none"
              text "infrastructure-deployer"
          }

          span { text " role, connecting to the Kubernetes API tunnel, and using the Pulumi CLI." }
      }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"
              span { text "$env:AWS_PROFILE=\"infrastructure-deployer\"\nstart-ameier-k8s-tunnel\npulumi up" }
          }
      }
      h3 {
          _class "mt-6"
          _id "2ac83008618d4529be74301b94f49b13"
          span { text "Apps Stacks" }
      }
      div {
          span {
              text
                  "Lastly, each app gets its own stack which is used to deploy the application into the cluster. For instance, this blog has its own stack which you can "
          }

          a {
              _href "https://github.com/ameier38/andrewmeier.dev/tree/main/pulumi"
              span { text "see on GitHub" }
          }

          span { text ". The main difference with the blog stack is that it has a separate " }

          code {
              _class "language-none"
              text "blog-deployer"
          }

          span { text " role which can be assumed by GitHub actions from the blog repo." }
      }
      h2 {
          _class "mt-8"
          _id "96bd7ae28beb485bb4aa482490ff5536"
          span { text "CI/CD" }
      }
      div {
          span {
              text
                  "In order to deploy the stacks that connect to the Kubernetes cluster using GitHub, I need to set up GitHub actions to connect to the Kubernetes API tunnel before running the Pulumi CLI. Because I can’t use the browser to log in, this is where I use the service token that I created in the Managed Infrastructure stack. In my GitHub workflow file, I added a step which connects to the cloudflared tunnel using the managed Docker image."
          }
      }
      pre {
          _class "language-yaml"

          code {
              _class "language-yaml"

              span {
                  text
                      "- name: Start Tunnel\n  run: |\n    docker run \\\n      -d \\\n      -p 1234:1234 \\\n      cloudflare/cloudflared:2022.3.2 \\\n      access tcp \\\n        --hostname=k8s.andrewmeier.dev \\\n        --url=0.0.0.0:1234 \\\n        --service-token-id=${{ secrets.tunnel-token-id }} \\\n        --service-token-secret=${{ secrets.tunnel-token-secret }}"
              }
          }
      }
      div {
          span { text "Also, in order to assume the " }

          code {
              _class "language-none"
              text "infrastructur-deployer"
          }

          span { text " AWS role I am using GitHub as a trusted OIDC provider to assume the role with the permission " }

          code {
              _class "language-none"
              text "AssumeRoleWithWebIdentity"
          }

          span { text ". I configured this in the Identity stack. I can then use the " }

          code {
              _class "language-none"
              text "aws-actions/configure-aws-credentials"
          }

          span { text " GitHub action to assume the role." }
      }
      pre {
          _class "language-yaml"

          code {
              _class "language-yaml"

              span {
                  text
                      "- name: Configure AWS Credentials\n  uses: aws-actions/configure-aws-credentials@v1\n  with:\n    aws-region: us-east-1\n    role-to-assume: arn:aws:iam::400689721046:role/infrastructure-deployer\n    role-session-name: github"
              }
          }
      }
      div {
          span { text "You can see all the workflows in " }

          a {
              _href "https://github.com/ameier38/infrastructure/tree/main/.github/workflows"
              span { text "the GitHub repo" }
          }

          span { text "." }
      }
      h2 {
          _class "mt-8"
          _id "f18fa32e5c574e258ca8797fda272420"
          span { text "Conclusion" }
      }
      div {
          span {
              text
                  "In this post I covered how I created my personal “cloud” that I use to run my apps and services. It has a lot of pieces but once it is set up it rarely changes. The other nice thing is that it only requires the up front cost of purchasing the equipment. The ongoing cost is near zero as I am using the free tiers of AWS and Cloudflare."
          }
      }
      div {
          span {
              text
                  "I have also found that this structure scales nicely and I use effectively the same setup for my work environments. As more people are contributing to the infrastructure, having separate stacks gives me confidence that deployment errors will be less likely to happen."
          }
      }
      div { span { text "I hope you find this useful!" } } ]

let article = Article.create metadata (ArticlePage.primary metadata content)
