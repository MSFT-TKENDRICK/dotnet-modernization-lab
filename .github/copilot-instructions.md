# Book catalog workshop

- This is a fictional, local-only .NET modernization lab with three projects.
  Preserve GET /books, GET /books/{id}, and POST /books behavior, including seed
  data/order, 404s, 201 with Location, and 400 validation without mutation.
- Keep tests meaningful and mutable state isolated per host/test. Do not weaken
  assertions or skip tests to make an upgrade pass.
- Do not add services, databases, Docker, Azure, credentials, or unrelated package
  migrations. Do not deploy, publish, push, or change visibility without approval.
- The distributed starter targets net8.0. An explicitly requested participant
  upgrade to net10.0 is intended: update all three TFMs, global.json, the
  framework-aligned MVC testing package, and generated lock files together.
  Preserve guided review gates and agree on Git/commit strategy before execution.
- Keep the SDK pinned and package locks trackable. Following intentional dependency
  changes, run `dotnet restore BookCatalog.sln --force-evaluate`, review the lock
  diff, then return to the standard commands below. Never hand-edit lock files.

Run from the repository root:

```console
dotnet restore BookCatalog.sln --locked-mode
dotnet build BookCatalog.sln --configuration Release --no-restore
dotnet test BookCatalog.sln --configuration Release --no-build --no-restore
dotnet run --project src/BookCatalog.Api/BookCatalog.Api.csproj --configuration Release --no-build --no-restore --no-launch-profile --urls http://127.0.0.1:5080
```
