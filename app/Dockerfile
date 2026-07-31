FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-noble AS builder

WORKDIR /app

# install tailwindcss v4 CLI (match builder platform)
RUN ARCH=$(uname -m | sed 's/aarch64/arm64/' | sed 's/x86_64/x64/') \
    && case "$ARCH" in \
        arm64) SHA256=ad627e77b496cccada4a6e26eafff698ef0829081e575a4baf3af8524bb00747 ;; \
        x64) SHA256=4ab84f2b496c402d3ec4fd25e0e5559fe1184d886dadae8fb4438344ec044c22 ;; \
        *) echo "Unsupported architecture: $ARCH" >&2; exit 1 ;; \
    esac \
    && curl -fsSLo /usr/local/bin/tailwindcss "https://github.com/tailwindlabs/tailwindcss/releases/download/v4.2.2/tailwindcss-linux-${ARCH}" \
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
COPY fake.sh .
RUN chmod +x ./fake.sh

# publish the server 
RUN ./fake.sh Publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS runner

WORKDIR /app

# copy compiled code from build image
COPY --from=builder /app/src/App/out .

ENTRYPOINT ["dotnet", "App.dll"]
