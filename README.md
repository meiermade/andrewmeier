# Andy Meier

[![Deploy](https://github.com/meiermade/andymeier/actions/workflows/deploy.yml/badge.svg)](https://github.com/meiermade/andymeier/actions/workflows/deploy.yml)

Personal website for Andy Meier built with F#, Giraffe, Datastar, and Tailwind CSS.

## Structure

- `app/` - F# web application
  - `src/App/` - Main application (Giraffe + Datastar)
  - `src/Build/` - FAKE build script
  - `src/Tests/` - Expecto tests
- `pulumi/` - Infrastructure as code (AWS ECR, Cloudflare, Kubernetes)

## Development

```bash
cd app
dotnet tool restore
dotnet paket restore
./fake.sh Watch
```

`Watch` starts a local MockNotion service with fictional articles, so normal local development does not require Notion credentials or a Pulumi ESC environment.

## Testing

```bash
cd app
./fake.sh Test
```
