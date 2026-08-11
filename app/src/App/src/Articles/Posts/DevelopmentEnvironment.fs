module App.Articles.Posts.DevelopmentEnvironment

open App.Articles
open App.Articles.Shared
open FSharp.ViewEngine
open System
open type Html

let private metadata =
    { permalink = "dev-env"
      title = "Development Environment"
      summary = "How I set up a Mac for agent-driven software development and infrastructure work"
      cover = "https://assets.meiermade.com/andymeier/articles/shared/gradient-purple-4776537cdf89.webp"
      tags = [| "Programming"; "AI"; "macOS"; "Python"; ".NET"; "Infrastructure" |]
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

let private content =
    [ p {
          text
              "This is the development environment I set up on a new Mac. The machine provides a dependable command-line foundation, each repository declares its own build and test dependencies, and the Pi coding agent is where I do almost all development. I use JetBrains IDEs to review and debug code and to work with databases."
      }
      p {
          text
              "I keep the global installation intentionally small. Tools that define the workstation belong in Homebrew; tools that define a project belong in that project's lock files and tool manifests. This makes the computer easy to recreate without letting global package versions silently change a build."
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
                  [ "computer", "Computer"
                    "macos", "macOS and Homebrew"
                    "terminal", "Terminal environment"
                    "source-control", "Source control and credentials"
                    "runtimes", "Language runtimes"
                    "pi", "Pi coding agent"
                    "ides", "JetBrains IDEs"
                    "infrastructure", "Containers and infrastructure"
                    "utilities", "Supporting command-line tools"
                    "projects", "Repository-owned tools"
                    "verify", "Verify the setup" ] do
                  li {
                      a {
                          _href $"#{id'}"
                          text label
                      }
                  }
          }
      }
      heading "computer" "Computer"
      paragraph
          "I use a 14-inch MacBook Pro with an Apple M5 Max chip and 128 GB of memory. The memory is more important to me than maximizing any single benchmark: I regularly have several Pi sessions, JetBrains IDEs, Docker workloads, browser tests, and local application processes open at the same time."
      paragraph
          "I keep macOS current and enable FileVault during initial setup. I also sign in to the services I use for work before installing developer tools, particularly 1Password, GitHub, Google Cloud, and JetBrains Toolbox."
      heading "macos" "macOS and Homebrew"
      p {
          text "I start by installing Apple's "
          link "https://developer.apple.com/xcode/resources/" "Xcode Command Line Tools"
          text ". They provide Git and the native build tools expected by many packages."
      }
      codeBlock "bash" "xcode-select --install"
      p {
          text "I use "
          link "https://brew.sh/" "Homebrew"
          text " for workstation-level packages. On Apple silicon, I add Homebrew to zsh after installation."
      }
      codeBlock
          "bash"
          "echo 'eval \"$(/opt/homebrew/bin/brew shellenv)\"' >> ~/.zprofile\neval \"$(/opt/homebrew/bin/brew shellenv)\""
      paragraph "These formulae make up the core command-line environment:"
      codeBlock
          "bash"
          "brew install cloudflared fnm gh googleworkspace-cli kubectx kubernetes-cli pulumi ripgrep starship tmux uv"
      paragraph "I install the desktop applications and larger SDKs as casks:"
      codeBlock
          "bash"
          "brew install --cask 1password-cli docker-desktop dotnet-sdk gcloud-cli ghostty jetbrains-toolbox"
      paragraph
          "I do not pin versions in this bootstrap list. Homebrew provides the workstation tools, while repositories pin the versions that can affect a build."
      heading "terminal" "Terminal environment"
      p {
          text "I use "
          link "https://ghostty.org/" "Ghostty"

          text
              " as my terminal. It is a native macOS application with tabs, splits, GPU-accelerated rendering, and support for the keyboard protocol Pi uses for modified key combinations."
      }
      p {
          text "Pi recommends one Ghostty binding so "
          inlineCode "Option+Backspace"
          text " behaves consistently. I add it to "
          inlineCode "~/Library/Application Support/com.mitchellh.ghostty/config"
          text "."
      }
      codeBlock "ini" "keybind = alt+backspace=text:\\x1b\\x7f"
      p {
          text "macOS already uses zsh. I add "
          link "https://starship.rs/" "Starship"
          text " for a compact prompt and initialize both Starship and fnm from "
          inlineCode "~/.zshrc"
          text "."
      }
      codeBlock "bash" "eval \"$(fnm env --use-on-cd --shell zsh)\"\neval \"$(starship init zsh)\""
      p {
          text "I use "
          link "https://github.com/tmux/tmux" "tmux"

          text
              " for application watchers, local servers, tunnels, and any other process that should keep running while Pi continues working. A named session is easy to inspect, reattach, or stop."
      }
      codeBlock "bash" "tmux new-session -s app\ntmux attach-session -t app"
      heading "source-control" "Source control and credentials"
      p {
          text "The Xcode Command Line Tools install Git. I configure my identity and use "
          link "https://cli.github.com/" "GitHub CLI"
          text " for authentication, repositories, issues, pull requests, checks, and releases."
      }
      codeBlock
          "bash"
          "git config --global user.name \"Your Name\"\ngit config --global user.email \"you@example.com\"\ngit config --global init.defaultBranch main\ngh auth login\ngh auth setup-git"
      p {
          text "The "
          link "https://developer.1password.com/docs/cli/" "1Password CLI"

          text
              " is a core part of the environment rather than an occasional utility. Pi skills use it to inject credentials into short-lived processes, which keeps secrets out of shell history, repositories, and ad hoc environment files."
      }
      codeBlock "bash" "op signin\nop vault list"
      paragraph
          "I authenticate service CLIs interactively on the workstation. Workloads and CI use workload identity, service accounts, or repository secrets rather than copying my personal credentials."
      heading "runtimes" "Language runtimes"
      paragraph
          "I install runtime managers globally, but let each repository declare the runtime and dependency versions it needs. My primary stacks are TypeScript and JavaScript, Python, and F#/.NET."
      subheading "node" "Node.js"
      p {
          text "I use "
          link "https://github.com/Schniz/fnm" "fnm"
          text " to install and switch Node.js versions. npm ships with Node.js and is also how I install Pi."
      }
      codeBlock "bash" "fnm install --lts\nnode --version\nnpm --version"
      paragraph
          "Projects commit their package manifest and lock file. I normally use npm ci for an existing repository so the installed dependency graph matches the lock file exactly."
      subheading "python" "Python and uv"
      p {
          text "I use "
          link "https://docs.astral.sh/uv/" "uv"

          text
              " for Python versions, virtual environments, project dependencies, lock files, scripts, and Python command-line tools. I do not manage a separate pyenv, pip, virtualenv, Poetry, or pipx setup."
      }
      codeBlock "bash" "uv python install\nuv init\nuv add requests\nuv add --dev pytest\nuv sync\nuv run pytest"
      p {
          text "For an existing project, "
          inlineCode "uv sync"
          text " restores the environment and "
          inlineCode "uv run"

          text
              " runs Python or a project command inside it. Keeping every Python invocation behind uv makes the selected interpreter and dependency environment explicit."
      }
      subheading "dotnet" ".NET and F#"
      p {
          text "The "
          link "https://dotnet.microsoft.com/download" ".NET SDK"

          text
              " provides the compiler, runtime, templates, test runner, and F# Interactive. Most of my application work in .NET is written in F#."
      }
      codeBlock "bash" "dotnet --info\ndotnet tool restore\ndotnet test"
      paragraph
          "Repositories restore Paket, FAKE, Fantomas, and other .NET tools through checked-in manifests or build scripts. I avoid installing project build tools globally."
      heading "pi" "Pi coding agent"
      p {
          text "The "
          link "https://pi.dev/" "Pi coding agent"

          text
              " is my primary development environment. Pi is a terminal coding harness that gives the model repository context and tools for reading, editing, running commands, and verifying the result. Its small core can be extended with AGENTS.md files, skills, prompt templates, extensions, and packages."
      }
      paragraph "After Node.js is available, I install Pi globally and start it from the repository I want to work in."
      codeBlock "bash" "npm install -g --ignore-scripts @earendil-works/pi-coding-agent\ncd ~/repos/example\npi"
      p {
          text "Inside Pi I run "
          inlineCode "/login"

          text
              " and select the OpenAI Codex subscription provider for my ChatGPT Pro account. I use the model selector when I want to change models and keep high reasoning as my normal default."
      }
      codeBlock
          "json"
          "{\n  \"defaultProvider\": \"openai-codex\",\n  \"defaultModel\": \"gpt-5.6-sol\",\n  \"defaultThinkingLevel\": \"high\",\n  \"theme\": \"dark\",\n  \"enableInstallTelemetry\": false\n}"
      p {
          text "The global settings file lives at "
          inlineCode "~/.pi/agent/settings.json"

          text
              ". Pi stores sessions by working directory, so I can resume a repository's previous work instead of rebuilding all context from scratch. Long sessions compact automatically while preserving the full JSONL history."
      }
      subheading "pi-context" "Instructions, skills, and packages"
      p {
          text "I keep global operating rules in "
          inlineCode "~/.pi/agent/AGENTS.md"
          text " and repository-specific rules in each repository's "
          inlineCode "AGENTS.md"

          text
              ". These files tell Pi how the project is structured, which package manager to use, how to run tests, and which deployment operations are safe."
      }
      paragraph
          "I also load a private skills package plus small packages for image generation and client context. Skills add repeatable workflows for GitHub, Google Cloud, Google Workspace, 1Password, Notion, browser testing, documents, infrastructure, and other services without placing every instruction in every prompt. Pi only loads a skill's full instructions when the task needs it."
      p {
          text
              "I review package source before installing it because Pi packages and skills can direct tools with full user permissions. "

          link
              "https://github.com/badlogic/pi-mono/blob/main/packages/coding-agent/docs/packages.md"
              "Pi's package documentation"

          text " describes the trust and installation model."
      }
      subheading "pi-workflow" "Daily workflow"
      paragraph
          "I do essentially all development through Pi. I start it in a repository, describe the outcome I want, and discuss the approach first when the work has meaningful design or operational tradeoffs. Pi inspects the repository, makes the changes, runs the relevant tests, and verifies the resulting application or artifact."
      paragraph
          "I steer the session as new information appears rather than waiting for a large batch of work to finish. For user-facing changes, Pi runs Playwright and captures rendered evidence. For infrastructure work, it previews changes and leaves deployment to the repository's established CI workflow. Git and GitHub CLI keep the branch, commit, pull request, and checks visible throughout the process."
      heading "ides" "JetBrains IDEs"
      p {
          text "I install "
          link "https://www.jetbrains.com/toolbox-app/" "JetBrains Toolbox"
          text " and use it to manage three IDEs:"
      }
      ul {
          _class "list-disc"

          li {
              strong { text "PyCharm" }
              text " for Python and data projects."
          }

          li {
              strong { text "Rider" }
              text " for F#, .NET, and mixed .NET/web solutions."
          }

          li {
              strong { text "WebStorm" }
              text " for TypeScript, JavaScript, and frontend projects."
          }
      }
      paragraph
          "I use these IDEs primarily to review the code Pi changed: navigating types and usages, inspecting diffs, running a debugger, and understanding an unfamiliar area of a repository. They complement the agent workflow rather than serving as the main place where I type code."
      paragraph
          "JetBrains also has the best database connection support of any IDE family I have tried. Its database explorer, query console, schema navigation, result viewer, and generated diagrams are a major reason I stay with the JetBrains ecosystem."
      heading "infrastructure" "Containers and infrastructure"
      p {
          text "I use "
          link "https://docs.docker.com/desktop/setup/install/mac-install/" "Docker Desktop"

          text
              " for local containers and Compose environments. I install the application globally, while repositories own their Dockerfiles, Compose files, and image versions."
      }
      codeBlock "bash" "docker version\ndocker compose version\ndocker ps"
      p {
          text "The "
          link "https://cloud.google.com/sdk/docs/install-sdk" "gcloud CLI"
          text " handles Google Cloud authentication, configuration, diagnostics, and direct resource inspection."
      }
      codeBlock "bash" "gcloud auth login\ngcloud auth application-default login\ngcloud config list"
      p {
          text "For Kubernetes I use "
          link "https://kubernetes.io/docs/reference/kubectl/" "kubectl"
          text " with "
          link "https://github.com/ahmetb/kubectx" "kubectx and kubens"

          text
              ". kubectx makes the active cluster explicit, while kubens switches the default namespace without repeatedly editing commands."
      }
      codeBlock "bash" "kubectl config get-contexts\nkubectx\nkubectx <context>\nkubens <namespace>\nkubectl get pods"
      p {
          text "I use "
          link "https://www.pulumi.com/docs/iac/" "Pulumi"

          text
              " for infrastructure as code. Projects select fully qualified stacks and use previews to review the proposed resource changes. Deployment is normally performed by CI after merge rather than by running an update manually from the laptop."
      }
      codeBlock "bash" "pulumi login\npulumi stack select <organization>/<project>/<stack>\npulumi preview"
      p {
          text "I install "

          link
              "https://developers.cloudflare.com/cloudflare-one/networks/connectors/cloudflare-tunnel/downloads/"
              "cloudflared"

          text
              " for local and administrative Cloudflare Tunnel work. Tunnel configuration itself belongs with the infrastructure that owns it rather than in global shell configuration."
      }
      heading "utilities" "Supporting command-line tools"
      p {
          text "Pi works best when the shell has small, composable tools. "
          link "https://github.com/BurntSushi/ripgrep" "ripgrep"
          text " searches repositories, "
          inlineCode "jq"
          text " filters JSON returned by CLIs, and "
          inlineCode "curl"

          text
              " is useful for health checks and direct HTTP diagnostics. macOS supplies jq and curl; I add ripgrep through Homebrew."
      }
      p {
          text "I also install the "
          link "https://github.com/googleworkspace/cli" "Google Workspace CLI"

          text
              " (gws). Pi uses it through dedicated skills to work with Gmail, Calendar, Drive, Docs, Sheets, and Slides. Interactive OAuth is enough for a personal workstation."
      }
      codeBlock "bash" "gws auth login\ngws drive files list --params '{\"pageSize\": 5}'"
      p {
          text "Browser testing is installed per repository with "
          link "https://playwright.dev/" "Playwright"

          text
              ". Pi uses it for end-to-end tests, screenshots, responsive checks, and visual inspection. Keeping it in the project's package manifest ensures the tests and browser tooling move together."
      }
      codeBlock "bash" "npm ci\nnpx playwright install\nnpx playwright test"
      heading "projects" "Repository-owned tools"
      paragraph
          "A new workstation should not require a hand-maintained global installation of every framework used by every repository. After cloning a project, I follow its AGENTS.md and README and let its package managers restore the rest."
      ul {
          _class "list-disc"

          li {
              text
                  "npm restores TypeScript, Tailwind CSS, Playwright, and other Node.js dependencies from the lock file."
          }

          li {
              text
                  "uv restores Python, dbt, Dagster, notebook, test, lint, and formatting dependencies from pyproject.toml and uv.lock."
          }

          li {
              text
                  "dotnet restores NuGet and Paket dependencies, while the local tool manifest restores FAKE, Fantomas, and related tools."
          }

          li {
              text
                  "Docker and Compose provide services such as Postgres, Airbyte, and other repository-specific infrastructure when a project needs them."
          }
      }
      paragraph
          "This division is important for Pi as well as for people. When every repository contains its own instructions and deterministic restore commands, the agent can reproduce the same environment locally and in CI instead of relying on undocumented global state."
      heading "verify" "Verify the setup"
      paragraph
          "I finish by opening a new Ghostty window and verifying that the main commands resolve from a clean shell:"
      codeBlock
          "bash"
          "git --version\ngh --version\nop --version\nfnm --version\nnode --version\nnpm --version\nuv --version\ndotnet --version\npi --version\ndocker version\ngcloud version\nkubectl version --client\nkubectx --version\nkubens --version\npulumi version\ncloudflared --version\ngws --version\nrg --version\njq --version\ntmux -V"
      paragraph
          "At that point the workstation is ready. Cloning a repository and following its checked-in setup instructions supplies the project-specific layer; Pi, the terminal tools, and the IDEs provide the consistent layer across all of them." ]

let article = Article.create metadata (ArticlePage.primary metadata content)
