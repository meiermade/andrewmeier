FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS builder

WORKDIR /app

COPY .config .config
RUN dotnet tool restore

COPY paket.dependencies paket.lock ./
RUN dotnet paket install && dotnet paket restore

COPY src src
RUN dotnet publish src/MockNotion/MockNotion.fsproj --configuration Release --output /out --self-contained false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS runner

WORKDIR /app
COPY --from=builder /out .

EXPOSE 5015
ENTRYPOINT ["dotnet", "MockNotion.dll"]
