module App.Articles.Posts.DevelopmentEnvironment

open App.Articles
open App.Articles.Shared
open FSharp.ViewEngine
open System
open type Html

let private metadata =
    { permalink = "dev-env"
      title = "Development Environment"
      summary = "How I set up my computer for development"
      cover = "https://assets.meiermade.com/andymeier/articles/shared/gradient-purple-4776537cdf89.webp"
      tags = [| "Programming"; "Python"; "F#"; ".NET"; "AI" |]
      createdAt = DateTimeOffset(2020, 4, 4, 0, 0, 0, TimeSpan.Zero) }

let private content =
    [ h3 {
          _class "mt-6"
          _id "a49afb67c49944c49874c3e568079409"
          span { text "Table of Contents" }
      }
      ul {
          _class "list-disc"

          li {
              a {
                  _href "#c51d078bdf6a4feeb1194e5f8aa0d710"
                  span { text "Computer" }
              }

              span { text ": Recommended specs for computer." }
          }

          li {
              a {
                  _href "#17286da7f6d34a11a97c81678ecee5f6"
                  span { text "Dropbox" }
              }

              span { text ":  File storage." }
          }

          li {
              a {
                  _href "#e5da454cee68425fbd9bf06fd8abe011"
                  span { text "Windows Terminal" }
              }

              span { text ": Terminal for Windows." }
          }

          li {
              a {
                  _href "#0f093452c51644acb4b1509e9ac1da0f"
                  span { text "Windows Subsystem for Linux" }
              }

              span { text ": Run Linux on Windows." }
          }

          li {
              a {
                  _href "#b39403d15e5243889ccc920b6ce4211a"
                  span { text "Scoop" }
              }

              span { text ": Package manager for Windows." }
          }

          li {
              a {
                  _href "#1bb350e0e0234eecb508cce2b94780c2"
                  span { text "oh-my-posh" }
              }

              span { text ": Better terminal prompt." }
          }

          li {
              a {
                  _href "#f29adc05e09941fbb42cfd37caa8eb1d"
                  span { text "Vim" }
              }

              span { text ": Text editor to speed up your typing." }
          }

          li {
              a {
                  _href "#0de016a3d94a491c944d9e014acf4f02"
                  span { text "Vimium" }
              }

              span { text ": Google Chrome extension to speed up your Googling." }
          }

          li {
              a {
                  _href "#0fc2f8a2f2504da8b0185b0a80562d4e"
                  span { text "Visual Studio Code" }
              }

              span { text ": IDE to speed up your development." }
          }

          li {
              a {
                  _href "#d53358bfb6774cf1b1a73d743c81162e"
                  span { text "Git" }
              }

              span { text ": Version control for champs." }
          }

          li {
              a {
                  _href "#8788b21dfe394fb782765cf1e571d931"
                  span { text "Docker" }
              }

              span { text ": Container management." }
          }

          li {
              a {
                  _href "#987a17e9957446638ea3d4b5cc8aadae"
                  span { text "dotnet" }
              }

              span { text ": .NET Core CLI for building .NET applications." }
          }

          li {
              a {
                  _href "#a07c6ae342f94aca8894c237ecdc1388"
                  span { text "Node" }
              }

              span { text ": A JavaScript runtime." }
          }

          li {
              a {
                  _href "#9440715cc7e740cea2f4bdab7d77b9af"
                  span { text "Python" }
              }

              span { text ": Easy to read, learn, and use programming language." }
          }

          li {
              a {
                  _href "#dc8d7f26672e457b9f1d93c796f3c22c"
                  span { text "kubectl" }
              }

              span { text ": Kubernetes CLI." }
          }

          li {
              a {
                  _href "#09e3cf5418074ca6aa8cee17a05913e2"
                  span { text "kubectxwin" }
              }

              span { text ": CLI for configuring Kubernetes contexts." }
          }

          li {
              a {
                  _href "#8d6aae336cb64346a8596d6d328f63ff"
                  span { text "kubenswin" }
              }

              span { text ": CLI for configuring Kubernetes namespaces." }
          }

          li {
              a {
                  _href "#574b57c6b7eb40e6876b7e784fbd12a5"
                  span { text "Helm" }
              }

              span { text ": Package manager for Kubernetes." }
          }

          li {
              a {
                  _href "#25f686d21b1b4c198bd8815c22259c1f"
                  span { text "Pulumi" }
              }

              span { text ": Infrastructure as code." }
          }

          li {
              a {
                  _href "#ef28796ed44c4f50a81aa6a9f053b00c"
                  span { text "Lightshot" }
              }

              span { text ": Screen capture tool." }
          }

          li {
              a {
                  _href "#480356f622e441b1a1d8386ca7fffa97"
                  span { text "ScreenToGif" }
              }

              span { text ": Screen capture tool, but GIFs." }
          }

          li {
              a {
                  _href "#eab0baa1fec84f048158239eac825d77"
                  span { text "OBS Studio" }
              }

              span { text ": Screen recording tool." }
          }
      }
      div { _class "border-b-2 border-gray-300/60 dark:border-gray-700/60" }
      h3 {
          _class "mt-6"
          _id "6f29ab163e11464d9bf97f46915f281f"
          span { text "Computer" }
      }
      div { span { text "I use a MacBook Pro (M4 Max) with 64 GB of memory." } }
      ul {
          _class "list-disc"

          li {
              span {
                  _class "font-bold"
                  text "Memory"
              }

              span { text ": 64 GB" }
          }

          li {
              span {
                  _class "font-bold"
                  text "Chip"
              }

              span { text ": Apple M4 Max" }
          }

          li {
              span {
                  _class "font-bold"
                  text "Disk"
              }

              span { text ": 1 TB" }
          }
      }
      div { br }
      div { _class "border-b-2 border-gray-300/60 dark:border-gray-700/60" }
      h3 {
          _class "mt-6"
          _id "9e7a142ea9c24440bfce3ff77fffb24b"
          span { text "Google Drive" }
      }
      div {
          span {
              text
                  "Google Drive is great for scanning documents. I keep a single ‘Documents’ folder with all my files and then just use search when I need to find something. I use the file name convention "
          }

          code {
              _class "language-none"
              text "YYYY-MM-DD Description"
          }

          span { text " so I can also look back historically. The mobile app scanner works really well." }
      }
      h4 {
          _class "mt-4"
          _id "6bfdde616b0f478eb33017bd267f17ce"
          span { text "Installation" }
      }
      div {
          span { text "Navigate to " }

          a {
              _href "https://www.google.com/drive/download/"
              span { text "https://www.google.com/drive/download/" }
          }

          span { text " and install for your system." }
      }
      div { br }
      div { _class "border-b-2 border-gray-300/60 dark:border-gray-700/60" }
      h3 {
          _class "mt-6"
          _id "e5da454cee68425fbd9bf06fd8abe011"
          span { text "Windows Terminal" }
      }
      div { span { text "New terminal for Windows. Makes for a much better terminal experience." } }
      h4 {
          _class "mt-4"
          _id "8a2814bb222d4ad38e0d4a076d84336e"
          span { text "Installation" }
      }
      div { span { text "Open the Windows Store and search for ‘terminal’. Then click ‘Install’." } }
      img {
          _class "drop-shadow-xl rounded"
          _src "https://assets.meiermade.com/andymeier/articles/dev-env/windows-terminal-e5d8e6f409da.webp"
          _alt "Windows Terminal in Microsoft Store search results"
          _attr ("loading", "lazy")
          _attr ("width", "995")
          _attr ("height", "692")
      }
      h4 {
          _class "mt-4"
          _id "2c580980a8364e4182ebc62ba35b29a0"
          span { text "Resources" }
      }
      ul {
          _class "list-disc"

          li {
              a {
                  _href "https://docs.microsoft.com/en-us/windows/terminal/"
                  span { text "Windows Terminal Overview" }
              }
          }
      }
      div { br }
      div { _class "border-b-2 border-gray-300/60 dark:border-gray-700/60" }
      h3 {
          _class "mt-6"
          _id "0f093452c51644acb4b1509e9ac1da0f"
          span { text "Windows Subsystem for Linux (WSL)" }
      }
      div { span { text "Run Linux on Windows." } }
      h4 {
          _class "mt-4"
          _id "531e866e81ee4ce9a5ce775f6c47b765"
          span { text "Installation" }
      }
      div { span { text "Open PowerShell and run" } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "wsl --install" }
          }
      }
      div { span { text "Install Ubuntu" } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "wsl --install --distribution Ubuntu" }
          }
      }
      h4 {
          _class "mt-4"
          _id "c1f4bdd16d214044b30527c9e84c727a"
          span { text "Usage" }
      }
      div {
          span { text "Open " }

          a {
              _href "#e5da454cee68425fbd9bf06fd8abe011"
              span { text "Windows Terminal" }
          }

          span { text " and open Ubuntu." }
      }
      img {
          _class "drop-shadow-xl rounded"
          _src "https://assets.meiermade.com/andymeier/articles/dev-env/wsl-ubuntu-8852344c5af9.webp"
          _alt "Ubuntu 20.04 selected in the Windows Terminal profile menu"
          _attr ("loading", "lazy")
          _attr ("width", "758")
          _attr ("height", "401")
      }
      h4 {
          _class "mt-4"
          _id "cdc0611d9ffa4b0e95771f7ca5e39f28"
          span { text "References" }
      }
      ul {
          _class "list-disc"

          li {
              a {
                  _href "https://docs.microsoft.com/en-us/windows/wsl/install-win10"
                  span { text "Windows 10 Installation Guide" }
              }
          }
      }
      div { br }
      div { _class "border-b-2 border-gray-300/60 dark:border-gray-700/60" }
      h3 {
          _class "mt-6"
          _id "b39403d15e5243889ccc920b6ce4211a"
          span { text "Scoop" }
      }
      div { span { text "Package manager for Windows." } }
      h4 {
          _class "mt-4"
          _id "c70dea33136e423aa4c7c46bfca1bf5c"
          span { text "Installation" }
      }
      div { span { text "Set the execution policy." } }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"
              span { text "Set-ExecutionPolicy RemoteSigned -s CurrentUser" }
          }
      }
      div { span { text "Install scoop." } }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"
              span { text "iex (New-Object net.webclient).downloadstring('https://get.scoop.sh')" }
          }
      }
      div { span { text "Install sudo." } }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"
              span { text "scoop install sudo" }
          }
      }
      blockquote { span { text "This allows you to run a command as an ‘Administrator’." } }
      div { span { text "Add extras and versions buckets." } }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"
              span { text "scoop bucket add extras\nscoop bucket add versions" }
          }
      }
      h4 {
          _class "mt-4"
          _id "7d874f6738f74e2b811c630d71e919ba"
          span { text "Usage" }
      }
      div { span { text "Search for packages" } }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"
              span { text "scoop search vim" }
          }
      }
      div { span { text "Install a package" } }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"
              span { text "scoop install vim" }
          }
      }
      h4 {
          _class "mt-4"
          _id "2bf9d412a11948a38c2805d855b82751"
          span { text "References" }
      }
      ul {
          _class "list-disc"

          li {
              a {
                  _href "https://github.com/lukesampson/scoop"
                  span { text "Scoop GitHub" }
              }
          }
      }
      div { br }
      div { _class "border-b-2 border-gray-300/60 dark:border-gray-700/60" }
      h3 {
          _class "mt-6"
          _id "1bb350e0e0234eecb508cce2b94780c2"
          span { text "oh-my-posh" }
      }
      div { span { text "Better prompt." } }
      h4 {
          _class "mt-4"
          _id "51a14fb8b3b64a5e87cde0c750c070c0"
          span { text "Installation" }
      }
      div { span { text "Install with Scoop." } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"

              span {
                  text
                      "scoop install https://github.com/JanDeDobbeleer/oh-my-posh/releases/latest/download/oh-my-posh.json"
              }
          }
      }
      div { span { text "Open your profile." } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "vim $env:PROFILE" }
          }
      }
      div { span { text "Add the following line to the top of your profile to use the ‘paradox’ theme." } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"

              span {
                  text
                      "Invoke-Expression (oh-my-posh --init --shell pwsh --config \"$(scoop prefix oh-my-posh)/themes/paradox.omp.json\")"
              }
          }
      }
      h4 {
          _class "mt-4"
          _id "ba2c895b61374cb7b08713ef45dfbac7"
          span { text "References" }
      }
      ul {
          _class "list-disc"

          li {
              a {
                  _href "https://ohmyposh.dev/"
                  span { text "oh-my-posh" }
              }
          }

          li {
              a {
                  _href
                      "https://www.hanselman.com/blog/HowToMakeAPrettyPromptInWindowsTerminalWithPowerlineNerdFontsCascadiaCodeWSLAndOhmyposh.aspx"

                  span { text "How to Make A Pretty Prompt In Windows Terminal" }
              }
          }
      }
      div { br }
      div { _class "border-b-2 border-gray-300/60 dark:border-gray-700/60" }
      h3 {
          _class "mt-6"
          _id "f29adc05e09941fbb42cfd37caa8eb1d"
          span { text "Vim" }
      }
      div {
          span {
              text
                  "Text editor to speed up your typing. It takes a little while to get used to but the persistence pays of in the long run."
          }
      }
      h4 {
          _class "mt-4"
          _id "bbecbcc3379e47e69e4dfff724bab516"
          span { text "Installation" }
      }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "scoop install vim" }
          }
      }
      h4 {
          _class "mt-4"
          _id "ba7e07790fbc40e9a83985bec35d39ba"
          span { text "Usage" }
      }
      div { span { text "Open vim in current directory." } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "vim ." }
          }
      }
      div { span { text "Open a file in vim" } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "vim ./my-file.txt" }
          }
      }
      h4 {
          _class "mt-4"
          _id "91c8a98fd7d04c5192b499e56e7a0846"
          span { text "Resources" }
      }
      ul {
          _class "list-disc"

          li {
              a {
                  _href "https://www.openvim.com/"
                  span { text "Online Vim tutorial" }
              }
          }

          li {
              a {
                  _href "https://danielmiessler.com/study/vim/"
                  span { text "More Advanced Vim" }
              }
          }
      }
      div { br }
      div { _class "border-b-2 border-gray-300/60 dark:border-gray-700/60" }
      h3 {
          _class "mt-6"
          _id "0de016a3d94a491c944d9e014acf4f02"
          span { text "Vimium" }
      }
      div {
          span {
              text
                  "Google Chrome extension to speed up your Googling. Use Vim keybindings to navigate around the browser."
          }
      }
      h4 {
          _class "mt-4"
          _id "5e8f6d47794747a9913f0e455fd814c7"
          span { text "Installation" }
      }
      div {
          span { text "Head to " }

          a {
              _href "https://vimium.github.io/"
              span { text "Vimium homepage" }
          }

          span { text " and click the ‘Install’ button. Or search for ‘Vimium’ in the " }

          a {
              _href "https://chrome.google.com/webstore/category/extensions"
              span { text "Chrome Web Store" }
          }

          span { text " and install from there." }
      }
      h4 {
          _class "mt-4"
          _id "9554b968670548a987cde6b6332a6a7c"
          span { text "Usage" }
      }
      ul {
          _class "list-disc"

          li {
              code {
                  _class "language-none"
                  text "j,k"
              }

              span { text ": used to scroll up and down respectively." }
          }

          li {
              code {
                  _class "language-none"
                  text "d,u"
              }

              span { text ": used to page up and down respectively." }
          }

          li {
              code {
                  _class "language-none"
                  text "f-{other}"
              }

              span { text ": used to show links on page and then navigate to " }

              code {
                  _class "language-none"
                  text "{other}"
              }

              span { text " which was the shown link." }
          }

          li {
              code {
                  _class "language-none"
                  text "?"
              }

              span { text ": show the rest of the keybindings and other help." }
          }
      }
      div { _class "border-b-2 border-gray-300/60 dark:border-gray-700/60" }
      h3 {
          _class "mt-6"
          _id "0fc2f8a2f2504da8b0185b0a80562d4e"
          span { text "Visual Studio Code" }
      }
      div { span { text "IDE to speed up your development." } }
      h4 {
          _class "mt-4"
          _id "d4db4295bc23462eb4125f4214b91ce9"
          span { text "Installation" }
      }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "scoop install vscode" }
          }
      }
      h4 {
          _class "mt-4"
          _id "02254c35b9844c38bc9715d55adb7c88"
          span { text "Usage" }
      }
      div { span { text "Open current directory in VS Code." } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "code ." }
          }
      }
      div { span { text "Open file in VS Code." } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "code ./my-file.txt" }
          }
      }
      h4 {
          _class "mt-4"
          _id "6f403dc7ff0c4907a8ac41c3b166526f"
          span { text "References" }
      }
      ul {
          _class "list-disc"

          li {
              a {
                  _href "https://code.visualstudio.com/"
                  span { text "VS Code Homepage" }
              }
          }
      }
      div { br }
      div { _class "border-b-2 border-gray-300/60 dark:border-gray-700/60" }
      h3 {
          _class "mt-6"
          _id "d53358bfb6774cf1b1a73d743c81162e"
          span { text "Git" }
      }
      div { span { text "Version control for champs." } }
      h4 {
          _class "mt-4"
          _id "f2a83a7d09c74720ab03fc9ad1c05594"
          span { text "Installation" }
      }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"
              span { text "scoop install git" }
          }
      }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"
              span { text "git config --global credential.helper manager-core" }
          }
      }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"
              span { text "git config --global core.autocrlf true" }
          }
      }
      h4 {
          _class "mt-4"
          _id "c8419963dc974f17bda7ad813c5f29c7"
          span { text "Usage" }
      }
      div { span { text "Initialize a new repository." } }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"
              span { text "mkdir my-repo\ncd my-repo\ngit init" }
          }
      }
      div { span { text "Add a file to track." } }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"
              span { text "echo \"hello\" > hello.txt\ngit add hello.txt" }
          }
      }
      div { span { text "Commit changes." } }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"
              span { text "git commit -m 'added hello.txt'" }
          }
      }
      div { br }
      div { _class "border-b-2 border-gray-300/60 dark:border-gray-700/60" }
      h3 {
          _class "mt-6"
          _id "8788b21dfe394fb782765cf1e571d931"
          span { text "Docker" }
      }
      div { span { text "Container management." } }
      h4 {
          _class "mt-4"
          _id "d890c0d3b96a4e2ba392e895af391db7"
          span { text "Installation" }
      }
      div {
          span { text "Go to " }

          a {
              _href "https://docs.docker.com/desktop/windows/install/"
              span { text "Docker install page" }
          }

          span { text " to install." }
      }
      h4 {
          _class "mt-4"
          _id "14ebf66e1b184c26adcff2c3dc3f96d9"
          span { text "Usage" }
      }
      div { span { text "View running containers" } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "docker ps -a" }
          }
      }
      div { span { text "View local images" } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "docker images" }
          }
      }
      div { span { text "Pull image from DockerHub" } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "docker pull bash" }
          }
      }
      div { span { text "Run a container" } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "docker run -it bash" }
          }
      }
      div { br }
      div { _class "border-b-2 border-gray-300/60 dark:border-gray-700/60" }
      h3 {
          _class "mt-6"
          _id "987a17e9957446638ea3d4b5cc8aadae"
          span { text "dotnet" }
      }
      div { span { text "Cross platform toolchain for developing .NET applications." } }
      h4 {
          _class "mt-4"
          _id "5e1348c00dc547cc98c6a6faa0c836df"
          span { text "Installation" }
      }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "scoop install dotnet-sdk" }
          }
      }
      h4 {
          _class "mt-4"
          _id "d6ebf7cd79334fb89907022e29808aee"
          span { text "Usage" }
      }
      div { span { text "Create a new F# console application" } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "dotnet new console -lang F# -n my-fsharp-app" }
          }
      }
      div { span { text "Run F# Interactive" } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"

              span {
                  text
                      "dotnet fsi\n\nMicrosoft (R) F# Interactive version 12.0.0.0 for F# 6.0\nCopyright (c) Microsoft Corporation. All Rights Reserved.\n\nFor help type #help;;\n\n> let x = 1;;\nval x: int = 1\n\n> let y = 2;;\nval y: int = 2\n\n> x + y;;\nval it: int = 3"
              }
          }
      }
      div { br }
      div { _class "border-b-2 border-gray-300/60 dark:border-gray-700/60" }
      h3 {
          _class "mt-6"
          _id "a07c6ae342f94aca8894c237ecdc1388"
          span { text "Node" }
      }
      div { span { text "A JavaScript runtime." } }
      h4 {
          _class "mt-4"
          _id "fbddeed3fd6b4d2b92b0474925cc56e5"
          span { text "Installation" }
      }
      div {
          span { text "First install " }

          a {
              _href "https://github.com/coreybutler/nvm-windows"
              span { text "nvm-windows" }
          }

          span { text "." }
      }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "scoop install nvm" }
          }
      }
      div { span { text "List available versions." } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"

              span {
                  text
                      "nvm list available\n\n|   CURRENT    |     LTS      |  OLD STABLE  | OLD UNSTABLE |\n|--------------|--------------|--------------|--------------|\n|    17.8.0    |   16.14.2    |   0.12.18    |   0.11.16    |\n|    17.7.2    |   16.14.1    |   0.12.17    |   0.11.15    |\n|    17.7.1    |   16.14.0    |   0.12.16    |   0.11.14    |\n|    17.7.0    |   16.13.2    |   0.12.15    |   0.11.13    |\n|    17.6.0    |   16.13.1    |   0.12.14    |   0.11.12    |\n|    17.5.0    |   16.13.0    |   0.12.13    |   0.11.11    |\n|    17.4.0    |   14.19.1    |   0.12.12    |   0.11.10    |\n|    17.3.1    |   14.19.0    |   0.12.11    |    0.11.9    |\n|    17.3.0    |   14.18.3    |   0.12.10    |    0.11.8    |\n|    17.2.0    |   14.18.2    |    0.12.9    |    0.11.7    |\n|    17.1.0    |   14.18.1    |    0.12.8    |    0.11.6    |\n|    17.0.1    |   14.18.0    |    0.12.7    |    0.11.5    |\n|    17.0.0    |   14.17.6    |    0.12.6    |    0.11.4    |\n|   16.12.0    |   14.17.5    |    0.12.5    |    0.11.3    |\n|   16.11.1    |   14.17.4    |    0.12.4    |    0.11.2    |\n|   16.11.0    |   14.17.3    |    0.12.3    |    0.11.1    |\n|   16.10.0    |   14.17.2    |    0.12.2    |    0.11.0    |\n|    16.9.1    |   14.17.1    |    0.12.1    |    0.9.12    |\n|    16.9.0    |   14.17.0    |    0.12.0    |    0.9.11    |\n|    16.8.0    |   14.16.1    |   0.10.48    |    0.9.10    |"
              }
          }
      }
      div { span { text "Install the latest LTS." } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "nvm install 16.14.2" }
          }
      }
      div { span { text "Use installed version." } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "nvm use 16.14.2" }
          }
      }
      h4 {
          _class "mt-4"
          _id "a26cf862186f4a53a85384051bc506b4"
          span { text "Usage" }
      }
      div { span { text "Run JavaScript REPL." } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"

              span {
                  text
                      "node\nWelcome to Node.js v14.17.0.\nType \".help\" for more information.\n> var x = 1;\nundefined\n> var y = 2;\nundefined\n> x + y\n3"
              }
          }
      }
      h4 {
          _class "mt-4"
          _id "a4b8f32372fd4523bd69f77997d65523"
          span { text "Resources" }
      }
      ul {
          _class "list-disc"

          li {
              a {
                  _href "https://github.com/coreybutler/nvm-windows"
                  span { text "nvm-windows" }
              }
          }

          li {
              a {
                  _href "https://nodejs.org/en/docs/"
                  span { text "Node docs" }
              }
          }
      }
      div { br }
      div { _class "border-b-2 border-gray-300/60 dark:border-gray-700/60" }
      h3 {
          _class "mt-6"
          _id "9440715cc7e740cea2f4bdab7d77b9af"
          span { text "Python" }
      }
      div { span { text "Easy to read, learn, and use programming language." } }
      h4 {
          _class "mt-4"
          _id "c1d21d8adaef4e17a5f2c2cbd76711ee"
          span { text "Installation" }
      }
      div {
          span { text "Install " }

          code {
              _class "language-none"
              text "pyenv"
          }
      }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "scoop install pyenv" }
          }
      }
      div { span { text "List available versions" } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"

              span {
                  text
                      "pyenv install --list\n:: [Info] ::  Mirror: https://www.python.org/ftp/python\n3.8.9\n3.8.10\n3.9.0\n3.9.2\n3.9.6"
              }
          }
      }
      div { span { text "Install latest version." } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "pyenv install 3.9.6" }
          }
      }
      div { span { text "Set latest as global version." } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "pyenv global 3.9.6" }
          }
      }
      h4 {
          _class "mt-4"
          _id "82151d8fe59f4c148b64e4125434ee3b"
          span { text "Usage" }
      }
      div { span { text "Check version." } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "python --version" }
          }
      }
      div { span { text "Run Python REPL." } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"

              span {
                  text
                      "python\nPython 3.8.2 (tags/v3.8.2:7b3ab59, Feb 25 2020, 23:03:10) [MSC v.1916 64 bit (AMD64)] on win32\nType \"help\", \"copyright\", \"credits\" or \"license\" for more information.\n>>> x = 1\n>>> y = 2\n>>> x + y\n3"
              }
          }
      }
      h4 {
          _class "mt-4"
          _id "0dcaf2ab2d004f8887a3d902076b26da"
          span { text "References" }
      }
      ul {
          _class "list-disc"

          li {
              a {
                  _href "https://www.python.org/"
                  span { text "Python homepage" }
              }
          }
      }
      div { br }
      div { _class "border-b-2 border-gray-300/60 dark:border-gray-700/60" }
      h3 {
          _class "mt-6"
          _id "dc8d7f26672e457b9f1d93c796f3c22c"
          span { text "kubectl" }
      }
      div { span { text "Kubernetes CLI." } }
      h4 {
          _class "mt-4"
          _id "622bb5b9c2b94509bee7897d0f024308"
          span { text "Installation" }
      }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "scoop install kubectl" }
          }
      }
      div {
          span { text "Update your " }

          code {
              _class "language-none"
              text "KUBECONFIG"
          }

          span { text " environment variable and (optionally) add an alias." }
      }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "vim $PROFILE" }
          }
      }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "$env:KUBECONFIG = \"C:\\Users\\<user>\\.kube\\config;\"\nSet-Alias k kubectl" }
          }
      }
      div { span { text "Then reload your profile." } }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"
              span { text ". $PROFILE" }
          }
      }
      h4 {
          _class "mt-4"
          _id "32f1a4f3d38041e293220ee37a4d114b"
          span { text "Usage" }
      }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "k get pods --all-namespaces" }
          }
      }
      div { br }
      div { _class "border-b-2 border-gray-300/60 dark:border-gray-700/60" }
      h3 {
          _class "mt-6"
          _id "09e3cf5418074ca6aa8cee17a05913e2"
          span { text "kubectxwin" }
      }
      div { span { text "CLI for configuring Kubernetes contexts" } }
      h4 {
          _class "mt-4"
          _id "12d198f06176419182a932f3a336824b"
          span { text "Installation" }
      }
      div { span { text "Clone the repository." } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "git clone https://github.com/thomasliddledba/kubectxwin.git" }
          }
      }
      div { span { text "Add the executable to the PATH and (optionally) add an alias." } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "vim $PROFILE" }
          }
      }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "$env:Path += \";C:\\<path to kubectxwin repo>\\bin\"\nSet-Alias ktx kubectxwin" }
          }
      }
      div { span { text "Reload your profile." } }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"
              span { text ". $PROFILE" }
          }
      }
      h4 {
          _class "mt-4"
          _id "246edef1fe5e45cc932271dbab566aad"
          span { text "Usage" }
      }
      div { span { text "View contexts" } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "ktx ls" }
          }
      }
      div { span { text "Change context" } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "ktx set docker-desktop\nSwitched to context \"docker-for-desktop\"." }
          }
      }
      div { _class "border-b-2 border-gray-300/60 dark:border-gray-700/60" }
      h3 {
          _class "mt-6"
          _id "8d6aae336cb64346a8596d6d328f63ff"
          span { text "kubenswin" }
      }
      div { span { text "CLI for configuring Kubernetes namespaces" } }
      h4 {
          _class "mt-4"
          _id "2cccdfa0b9a945539c46515d7f437a5d"
          span { text "Installation" }
      }
      div { span { text "Clone the repository." } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "git clone https://github.com/thomasliddledba/kubenswin.git" }
          }
      }
      div { span { text "Add the executable to the PATH and (optionally) add an alias." } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "vim $PROFILE" }
          }
      }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "$env:Path += \";C:\\{path to kubenswin repo}\\bin\"\nSet-Alias kns kubenswin" }
          }
      }
      div { span { text "Reload your profile." } }
      pre {
          _class "language-powershell"

          code {
              _class "language-powershell"
              span { text ". $PROFILE" }
          }
      }
      h4 {
          _class "mt-4"
          _id "5b242f77f68a48a1b01d7a525f53170c"
          span { text "Usage" }
      }
      div { span { text "View namespaces" } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "kns ls" }
          }
      }
      div { span { text "Change namespace" } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "kns set kube-system" }
          }
      }
      div { _class "border-b-2 border-gray-300/60 dark:border-gray-700/60" }
      h3 {
          _class "mt-6"
          _id "574b57c6b7eb40e6876b7e784fbd12a5"
          span { text "Helm" }
      }
      div { span { text "Package manager for Kubernetes." } }
      h4 {
          _class "mt-4"
          _id "c7dfe063ad1049a7852d327944082712"
          span { text "Installation" }
      }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "scoop install helm" }
          }
      }
      h4 {
          _class "mt-4"
          _id "6c99bf09cf264266a0744b5e6475254e"
          span { text "Usage" }
      }
      div { span { text "Install a chart (i.e., package)." } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "helm install stable/postgresql --name my-postgres" }
          }
      }
      div { span { text "List charts." } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "helm list" }
          }
      }
      div { span { text "Show status of chart." } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "helm status my-postgres" }
          }
      }
      div { br }
      div { _class "border-b-2 border-gray-300/60 dark:border-gray-700/60" }
      h3 {
          _class "mt-6"
          _id "25f686d21b1b4c198bd8815c22259c1f"
          span { text "Pulumi" }
      }
      div { span { text "Infrastructure as code." } }
      h4 {
          _class "mt-4"
          _id "71b4c1ab23944ee5a4529ba4f3b0186c"
          span { text "Installation" }
      }
      div { span { text "Install using Scoop." } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "scoop install pulumi" }
          }
      }
      h4 {
          _class "mt-4"
          _id "4e74b911a37545f094daf9001a729ac2"
          span { text "Usage" }
      }
      div { span { text "Create a new project." } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "mkdir pulumi\npulumi new kubernetes-typescript" }
          }
      }
      div { span { text "Preview the changes." } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "pulumi preview" }
          }
      }
      div { span { text "Update the resources." } }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"
              span { text "pulumi up" }
          }
      }
      h4 {
          _class "mt-4"
          _id "cdf41758f1f245cb9107050198cc4c40"
          span { text "Resources" }
      }
      ul {
          _class "list-disc"

          li {
              a {
                  _href "https://www.pulumi.com/"
                  span { text "Pulumi" }
              }
          }
      }
      div { br }
      div { _class "border-b-2 border-gray-300/60 dark:border-gray-700/60" }
      h3 {
          _class "mt-6"
          _id "ef28796ed44c4f50a81aa6a9f053b00c"
          span { text "Lightshot" }
      }
      div { span { text "Screen capture tool. (Used for most of the images in this post.)" } }
      h4 {
          _class "mt-4"
          _id "20e7cdcb76034d26bf914fbaed5153da"
          span { text "Installation" }
      }
      div {
          span { text "Install Lightshot by downloading " }

          a {
              _href "https://app.prntscr.com/en/index.html"
              span { text "here" }
          }

          span { text "." }
      }
      h4 {
          _class "mt-4"
          _id "8e38c631ef9f4618ad060626297884b7"
          span { text "Usage" }
      }
      div {
          span { text "Take a screen shot by pressing the " }

          code {
              _class "language-none"
              text "PrtScr"
          }

          span {
              text
                  " button on your keyboard. You can then select an area on your screen, add lines or arrows, and save or copy the image to your clipboard."
          }
      }
      h4 {
          _class "mt-4"
          _id "9df5b39db26c45cab55e51e9d46f7855"
          span { text "Resources" }
      }
      ul {
          _class "list-disc"

          li {
              a {
                  _href "https://app.prntscr.com/en/index.html"
                  span { text "Lightshot homepage" }
              }
          }
      }
      div { br }
      div { _class "border-b-2 border-gray-300/60 dark:border-gray-700/60" }
      h3 {
          _class "mt-6"
          _id "480356f622e441b1a1d8386ca7fffa97"
          span { text "ScreenToGif" }
      }
      div { span { text "Screen capture tool, but GIFs." } }
      h4 {
          _class "mt-4"
          _id "ea103d45657e49fbacf5baa74f88b2d1"
          span { text "Installation" }
      }
      div {
          span { text "Download and install from " }

          a {
              _href "https://www.screentogif.com/"
              span { text "screentogif.com" }
          }

          span { text "." }
      }
      h4 {
          _class "mt-4"
          _id "cbf4b23e2545417db65f03591c08f94f"
          span { text "Usage" }
      }
      div { span { text "Start ScreenToGif." } }
      div {
          span { text "Drag the window around the area you would like to record and press " }

          code {
              _class "language-none"
              text "F7"
          }

          span { text " to start recording. Press " }

          code {
              _class "language-none"
              text "F8"
          }

          span { text " to stop recording." }
      }
      img {
          _class "drop-shadow-xl rounded"
          _src "https://assets.meiermade.com/andymeier/articles/dev-env/screentogif-demo-57717811367a.gif"
          _alt "ScreenToGif recording controls around a selected screen area"
          _attr ("loading", "lazy")
          _attr ("width", "945")
          _attr ("height", "492")
      }
      h4 {
          _class "mt-4"
          _id "b6bce6d12c4d44d1b006354c7ee0f290"
          span { text "Resources" }
      }
      ul {
          _class "list-disc"

          li {
              a {
                  _href "https://www.screentogif.com/"
                  span { text "ScreenToGif homepage" }
              }
          }
      }
      div { br }
      div { _class "border-b-2 border-gray-300/60 dark:border-gray-700/60" }
      h3 {
          _class "mt-6"
          _id "eab0baa1fec84f048158239eac825d77"
          span { text "OBS Studio" }
      }
      div { span { text "Screen recording tool." } }
      h4 {
          _class "mt-4"
          _id "932bd7a700414c9c95946428f679c2bc"
          span { text "Installation" }
      }
      div {
          span { text "Download and install from " }

          a {
              _href "https://obsproject.com/download"
              span { text "OBS Studio website" }
          }

          span { text "." }
      }
      h4 {
          _class "mt-4"
          _id "b05f575dc2d4480b8048552838561ce0"
          span { text "Usage" }
      }
      div {
          span { text "Check out " }

          a {
              _href "https://becomeablogger.com/obs/"
              span { text "becomeablogger.com" }
          }

          span { text " for some great resources on how to use OBS Studio." }
      }
      h4 {
          _class "mt-4"
          _id "9b787b11af064cac9a39effca7b98693"
          span { text "Resources" }
      }
      ul {
          _class "list-disc"

          li {
              a {
                  _href "https://obsproject.com/"
                  span { text "OBS Studio" }
              }
          }
      }
      div { br }
      div { _class "border-b-2 border-gray-300/60 dark:border-gray-700/60" } ]

let article = Article.create metadata (ArticlePage.primary metadata content)
