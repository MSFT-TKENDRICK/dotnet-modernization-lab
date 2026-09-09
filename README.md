# .NET modernization lab

A small, original ASP.NET Core book catalog for a **public .NET modernization
workshop**. In this 30-minute application-modernization session,
attendees choose **Java OR .NET**, both using VS Code. This repository contains
only the .NET track.

**The delivered starter stays on .NET 8. Your exercise is to assess, plan, and
optionally execute an upgrade to .NET 10 with GitHub Copilot upgrade.**
Environment setup is prework. Installing GitHub Copilot upgrade for VS Code or
GitHub Copilot CLI is a deliberate first lab step. A reviewed plan and a
documented blocker are valid outcomes; finishing an upgrade is not guaranteed.

All books and authors are fictional. There is no proprietary code, real-world
data, authentication, persistent database, or external application service.
No Windows-only APIs, Azure account, or paid infrastructure is needed. The
recommended Dev Containers path requires a local Docker-compatible container
engine; a direct local SDK path remains available. This unauthenticated,
in-memory teaching API is for loopback use only, not production hosting.

## Start here

1. Complete [prework and the readiness check](docs/prework.md).
2. Follow the [participant lab](docs/lab.md) in VS Code.
3. Facilitators: use the [checkpoints, troubleshooting, and rehearsal guide](docs/facilitator.md).

**Access:** this starter is intended for public distribution. Cloning a public
repository does not require an invitation or grant write access. Participants
work on local branches; pushing, publishing, or changing repository settings is
not part of the exercise.

## Prerequisites and versions

Use a supported Windows, macOS, or Linux installation with Git, VS Code, and
GitHub Copilot access. The recommended path opens this repository in its
devcontainer, which supplies both .NET SDKs, C# tooling, and GitHub Copilot CLI.
The local path installs both SDKs directly. During the lab, participants install
**GitHub Copilot upgrade** as either a VS Code extension or a GitHub Copilot CLI
plugin. Copilot Free or a paid plan may provide access, subject to quotas and
organization policy; no cloud subscription is required.

| Component | Workshop selection |
| --- | --- |
| Recommended environment | VS Code Dev Containers with a Docker-compatible engine |
| Baseline SDK | **8.0.425**, selected exactly by `global.json` |
| Baseline TFM / ASP.NET Core runtime | `net8.0` / 8.0.31 |
| Participant target SDK / TFM | **10.0.401** / `net10.0` |
| Baseline `Microsoft.AspNetCore.Mvc.Testing` | **8.0.31** |
| Target MVC testing package candidate | **10.0.12**, confirm during assessment |
| `Microsoft.NET.Test.Sdk` | **18.9.0** |
| xUnit / VS Test adapter | **2.9.3** / **4.0.0** |

These stable versions were checked against Microsoft's release metadata and
NuGet on September 8, 2026. The adapter supports xUnit v2 on .NET 8+; its version
does not imply an xUnit framework migration. Keep xUnit v2 and VSTest for the
upgrade unless a demonstrated compatibility issue requires a change.

`global.json` disables SDK roll-forward and prereleases, so another installed SDK
(including .NET 10) cannot silently replace the baseline SDK. Explicit package
versions and checked-in `packages.lock.json` files make locked restores
repeatable. `NuGet.Config` uses only nuget.org, not machine-specific private
feeds. Do not delete the pin or lock files to bypass an error. The
[lab](docs/lab.md#5-execute-only-after-review) explains their intentional update.

.NET 8 is in maintenance support and reaches end of support on **November 10,
2026**; .NET 10 LTS is supported through **November 14, 2028**, according to the
[support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core).
Recheck servicing versions before distribution. Refresh pins, regenerate locks,
and rerun the baseline together when updating the starter. Reassess this
source-target pairing before using the lab after .NET 8 support ends.

## Quickstart

After completing prework, clone using your authorized GitHub account. Open the
folder in its devcontainer, or use the local SDK path documented in prework:

```console
git clone https://github.com/frye/dotnet-modernization-lab.git
cd dotnet-modernization-lab
dotnet --version
dotnet restore BookCatalog.sln --locked-mode
dotnet build BookCatalog.sln --configuration Release --no-restore
dotnet test BookCatalog.sln --configuration Release --no-build --no-restore
dotnet run --project src/BookCatalog.Api/BookCatalog.Api.csproj --configuration Release --no-build --no-restore --no-launch-profile --urls http://127.0.0.1:5080
```

Expect SDK `8.0.425`, a successful build, and **24 passing tests**. The final
command keeps running: open `http://127.0.0.1:5080/books` in a browser or use the
[HTTP examples](docs/lab.md#2-explore-the-http-contract) in another terminal.
Stop the server with Ctrl+C. Each restart restores the original three books.
All build/test/run commands work in POSIX shells and PowerShell; HTTP commands
are supplied separately for both.

## API contract

| Request | Result |
| --- | --- |
| `GET /books` | 200 JSON array ordered by ID, initially three fictional books |
| `GET /books/1` | 200 with book 1, `The Lantern Atlas` by `Mira Vale` |
| `GET /books/999` | 404, as for any unknown integer ID |
| `POST /books` with nonblank `title` and `author` | 201, server-assigned integer ID, JSON book, and retrievable `Location` |
| Missing, null, empty, or whitespace-only title/author | 400 `application/problem+json` with field errors; no state change |
| Malformed JSON, null request body, or incompatible JSON property type | 400; no state change |

Book JSON has `id`, `title`, and `author`. Valid title/author text is preserved
exactly, including surrounding spaces. Duplicate titles/authors are allowed.
The first creation after restart receives ID 4. There are no update/delete
endpoints. HTTP binding rejects invalid JSON; the API explicitly validates
fields rather than relying on newer minimal API validation features.

The catalog is a synchronized, per-host singleton. Tests use a new
`WebApplicationFactory<Program>` per case, never a shared mutable fixture.
Tests also cover concurrent creation and independence between two hosts.

## Repository layout

```text
BookCatalog.sln                       Three-project, .NET 8-readable solution
global.json                          Exact SDK selection
Directory.Build.props                Shared settings and NuGet lock generation
NuGet.Config                         Public feed only
.devcontainer/devcontainer.json      Reproducible SDK, CLI, and editor environment
src/BookCatalog.Domain/               Immutable Book and in-memory BookStore
src/BookCatalog.Api/                  Minimal API and creation request
tests/BookCatalog.Api.Tests/          xUnit/WebApplicationFactory integration tests
docs/prework.md                      Setup and baseline readiness
docs/lab.md                          Guided participant exercise
docs/facilitator.md                  Facilitation, blockers, reset, and rehearsal
.github/copilot-instructions.md       Behavior and workflow guardrails
.github/workflows/ci.yml              Locked restore, build, and test
```

Each project includes its generated `packages.lock.json`, even projects without
direct NuGet packages. `.github/upgrades/{scenarioId}/` is intentionally absent:
only a real agent session should create assessment/planning/execution artifacts.

## Execution evidence and remaining preparation

The .NET 8 baseline was restored, built, and tested on **macOS 26.6 ARM64** with
SDK 8.0.425 and .NET/ASP.NET Core 8.0.31. Build: zero warnings/errors. Tests:
24 passed, zero failed/skipped. Real loopback HTTP requests returned the expected
200/404/201/400 statuses, created-book retrieval, and unchanged state after
rejection.

The authoring machine originally had only .NET 9. Validation used an official,
SHA-512-verified .NET 8 SDK in session-local storage with scoped `DOTNET_ROOT`
and `PATH`, without changing the machine-wide installation.

A source-only snapshot with empty NuGet package and HTTP caches also passed
locked restore, build, all 24 tests, and live HTTP smoke checks without changing
the lock files. Details are recorded in the
[facilitator evidence section](docs/facilitator.md#authoring-evidence).
The Ubuntu 24.04 ARM64 devcontainer was built and exercised with both SDKs,
GitHub Copilot CLI, locked restore, build, all tests, and live HTTP checks.
Windows, native Linux, PowerShell examples, GitHub-hosted CI, VS Code extension
installation, and an actual upgrade-agent rehearsal have **not** been executed
during authoring. Cross-platform intent is not execution evidence. No .NET 10
upgrade has been performed or is claimed. Confirm those preparation items and
that the public clone works before the workshop.

This deliberately small app may need few source-code changes: the meaningful
exercise is reviewing scope, SDK/framework/package alignment, lock-file changes,
and retained behavior, not manufacturing upgrade drama.

## Reference sources

- [GitHub Copilot upgrade overview](https://learn.microsoft.com/en-us/dotnet/core/porting/github-copilot-upgrade/overview)
- [VS Code installation](https://learn.microsoft.com/en-us/dotnet/core/porting/github-copilot-upgrade/install?pivots=vscode)
- [GitHub Copilot CLI installation](https://learn.microsoft.com/en-us/dotnet/core/porting/github-copilot-upgrade/install?pivots=copilot-cli)
- [Upgrade walkthrough](https://learn.microsoft.com/en-us/dotnet/core/porting/github-copilot-upgrade/how-to-upgrade-with-github-copilot)
- [.NET downloads](https://dotnet.microsoft.com/en-us/download/dotnet) and [support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)
- [ASP.NET Core integration testing (.NET 10)](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0)
- [SDK selection (`global.json`)](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json) and [NuGet lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies)
- [Source SDK release metadata](https://builds.dotnet.microsoft.com/dotnet/release-metadata/8.0/releases.json) and [target SDK release metadata](https://builds.dotnet.microsoft.com/dotnet/release-metadata/10.0/releases.json)
- [NuGet package versions](https://www.nuget.org/packages/Microsoft.AspNetCore.Mvc.Testing)
- [Modernization for beginners](https://github.com/microsoft/dotnet-modernization-for-beginners) and [monolith modernization workshop](https://github.com/Azure-Samples/modernize-monolith-workshop): inspiration only; their .NET Framework/Windows/Visual Studio and Azure requirements do not apply here.
- [Companion Java quickstart](https://learn.microsoft.com/en-us/azure/developer/github-copilot-app-modernization/quickstart-upgrade?pivots=visual-studio-code): context only, a separate track with different tooling.
