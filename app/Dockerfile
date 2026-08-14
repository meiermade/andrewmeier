FROM --platform=$BUILDPLATFORM node:24.11.1-alpine AS browser-builder

WORKDIR /app/browser
COPY browser/package.json browser/package-lock.json ./
RUN npm ci --ignore-scripts
COPY browser/src src
COPY browser/tsconfig.json ./
RUN mkdir -p /app/src/App/wwwroot/scripts \
    && npm run check \
    && npm test \
    && npm run build

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-noble AS builder

WORKDIR /app

# install tailwindcss v4 CLI (match builder platform)
RUN ARCH=$(uname -m | sed 's/aarch64/arm64/' | sed 's/x86_64/x64/') \
    && case "$ARCH" in \
        arm64) SHA256=55fd0b241214eff3de1e8ee4f22796662f2d2e7a49bcfca7477cfd0bac398195 ;; \
        x64) SHA256=dc61b3ac6b8c9ca874c0cc4c57b2409791a64c5540404ca5f5367360babc313a ;; \
        *) echo "Unsupported architecture: $ARCH" >&2; exit 1 ;; \
    esac \
    && curl -fsSLo /usr/local/bin/tailwindcss "https://github.com/tailwindlabs/tailwindcss/releases/download/v4.3.3/tailwindcss-linux-${ARCH}" \
    && echo "$SHA256  /usr/local/bin/tailwindcss" | sha256sum --check \
    && chmod +x /usr/local/bin/tailwindcss

# restore tools
COPY .config .config
RUN dotnet tool restore

# install F# dependencies
COPY paket.dependencies paket.lock ./
RUN dotnet paket install && dotnet paket restore

# copy projects and build script
COPY src src
COPY --from=browser-builder /app/src/App/wwwroot/scripts/telemetry.js src/App/wwwroot/scripts/telemetry.js
COPY fake.sh .
RUN chmod +x ./fake.sh

# publish the server 
RUN SKIP_BROWSER_BUILD=true ./fake.sh Publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS runner

WORKDIR /app

# copy compiled code from build image
COPY --from=builder /app/src/App/out .

ENTRYPOINT ["dotnet", "App.dll"]
