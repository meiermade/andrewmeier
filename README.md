# Andy Meier

[![Deploy](https://github.com/meiermade/andymeier/actions/workflows/deploy.yml/badge.svg)](https://github.com/meiermade/andymeier/actions/workflows/deploy.yml)

Personal website for Andy Meier built with F#, Giraffe, Datastar, and Tailwind CSS.

## Structure

- `app/` - F# web application
  - `src/App/` - Main application and FSharp.ViewEngine article source
  - `src/Build/` - FAKE build script
  - `src/Tests/` - Expecto tests
- `pulumi/` - Infrastructure as code (Cloudflare and Kubernetes)

## Development

```bash
cd app
dotnet tool restore
dotnet paket restore
./fake.sh Watch
```

Articles are authored directly in `app/src/App/src/Articles/Posts` with FSharp.ViewEngine. `Watch` uses those source-controlled articles and requires no content-service credentials.

## Publishing articles

1. Add a post module under `app/src/App/src/Articles/Posts` and register it in `Articles/Catalog.fs`.
2. Upload article images to `gs://assets.meiermade.com/andymeier/articles/<permalink>/` using content-hashed filenames.
3. Reference each image's `https://assets.meiermade.com/andymeier/...` URL from the post.

Article assets are manually published and cached, so changing an image requires a new filename.

## Testing

```bash
cd app
./fake.sh Test
```
