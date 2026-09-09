# Prework: arrive ready to practice

Complete environment setup and the baseline readiness check **before** the
session. Choose Java or .NET, not both. This guide is only for the .NET track.
Do not install the GitHub Copilot upgrade extension yet; installing it is the
first participant lab activity.

## 1. Confirm access and connectivity

Confirm that you can view
[`frye/dotnet-modernization-lab`](https://github.com/frye/dotnet-modernization-lab)
and clone it. The public starter does not require an invitation or a token to
clone. If it is inaccessible, contact the facilitator. Do not embed access
tokens in clone URLs or repository files.

Use your own authorized GitHub account for GitHub Copilot in VS Code.
Microsoft's installation instructions
allow Free or paid subscriptions, but quota exhaustion, organization policies,
or extension restrictions can still prevent use. Arrange approved access in
advance; an Azure account is not required.

You need network access to GitHub, VS Code Marketplace, GitHub Copilot services,
Microsoft .NET downloads, and the public NuGet feed
`https://api.nuget.org/v3/index.json` (including its package download endpoints).
Use organization-approved proxy/certificate configuration; never disable TLS
verification or copy proxy credentials into the repository. A cached restore
does not prove a new package can be downloaded during upgrade.

## 2. Choose a development environment

The recommended path uses the repository's devcontainer. The local path remains
available when a container engine is unavailable or prohibited. Use one path
consistently for the readiness check and lab.

### Recommended: Dev Containers

Install [Git](https://git-scm.com/downloads), current stable
[Visual Studio Code](https://code.visualstudio.com/), a Docker-compatible
container engine supported by your organization, and the Microsoft
**Dev Containers** VS Code extension. Start the engine, clone the repository,
open its root in VS Code, and run **Dev Containers: Reopen in Container** from
the Command Palette.

The container installs exact .NET SDKs `8.0.425` and `10.0.401` side by side,
GitHub Copilot CLI, the Microsoft C# extension, and GitHub Copilot Chat. It also
runs the locked NuGet restore and forwards port 5080. Building the container
requires access to Microsoft Container Registry and GitHub Container Registry,
in addition to the services listed above. Container setup never stores a GitHub
token in the repository or image.

Inside the container terminal, verify:

```console
git --version
dotnet --list-sdks
dotnet --list-runtimes
dotnet --info
copilot --version
```

Both exact SDKs must appear, and `copilot` must be available. `global.json`
still selects `8.0.425` from the repository root. Sign in to Copilot Chat with
your authorized GitHub account. Copilot CLI authentication is user-specific;
run `copilot` and use `/login` if you plan to use the optional CLI interface.
Do not place a personal access token in `.devcontainer/devcontainer.json` or
another repository file.

### Alternative: local SDK installation

Install [Git](https://git-scm.com/downloads), **.NET SDK 8.0.425**, and
**.NET SDK 10.0.401** for your OS and CPU architecture. The source SDK is an exact
requirement because `global.json` disables roll-forward. Runtime-only
installations cannot build the lab. Both SDKs include the runtimes needed for
their respective exercises.

Use the official [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
and [.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) download
pages. Expand the appropriate release to find the exact SDK, rather than
selecting a preview or an unrelated newer SDK.

| Platform | Setup path |
| --- | --- |
| Windows | Use the official Windows SDK installers matching x64 or ARM64. Install both versions side by side. Git for Windows provides Git; PowerShell can run all baseline .NET commands. Visual Studio and desktop workloads are not required. |
| macOS | Use the official SDK `.pkg` installers matching Apple Silicon ARM64 or Intel x64. Install both into the same SDK root. Mixing Homebrew and official installer locations can hide one version from the active `dotnet` executable. |
| Linux | Follow the [distribution-specific install instructions](https://learn.microsoft.com/en-us/dotnet/core/install/linux). Use a supported distribution and its required native dependencies. If its package feed does not offer the exact SDKs, use Microsoft's [script/manual install guidance](https://learn.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual) with explicit versions and architecture, in one user-local SDK root. Do not mix incompatible distribution and Microsoft package feeds. |

For manual archive installs, compare SHA-512 with the official
[8.0 release metadata](https://builds.dotnet.microsoft.com/dotnet/release-metadata/8.0/releases.json)
or [10.0 release metadata](https://builds.dotnet.microsoft.com/dotnet/release-metadata/10.0/releases.json)
before extraction. A user-local SDK needs the documented `PATH`/`DOTNET_ROOT`
setup. Open a new terminal and restart VS Code after installation.

Check in a terminal:

```console
git --version
dotnet --list-sdks
dotnet --list-runtimes
dotnet --info
```

Both `8.0.425` and `10.0.401` should appear in the SDK list for the `dotnet`
you will use. If only one appears, check the active executable with
`command -v dotnet` on macOS/Linux or `Get-Command dotnet` in PowerShell. Avoid
accidentally mixing ARM64 and x64 installations. Do not set
`DOTNET_ROLL_FORWARD=Major` or `LatestMajor` to compensate for a missing runtime.
The baseline should use the actual .NET 8 runtime, not a newer runtime override.

The source and target versions are different from the VS Code extension's own
SDK needs. Automatic acquisition by an extension is not proof that either SDK is
available to your terminal or selected by this repository.

Install [GitHub Copilot CLI](https://docs.github.com/en/copilot/how-tos/copilot-cli/set-up-copilot-cli/install-copilot-cli)
using an official method for your platform, then run `copilot --version`.
Authentication is user-specific; use `/login` when you first run `copilot`.
Never put a personal access token in the repository.

## 3. Check VS Code and Copilot tooling

Install the current stable [Visual Studio Code](https://code.visualstudio.com/)
and use a supported OS. If you chose the local SDK path, in Extensions
(Ctrl+Shift+X, or Cmd+Shift+X on macOS):

1. Install **GitHub Copilot**, sign in, and confirm Copilot Chat is usable.
2. The Microsoft **C#** extension is useful for editing and diagnostics; install
   it if needed. This lab does not require Visual Studio or C# Dev Kit.

The devcontainer requests both extensions automatically. In either environment,
review Workspace Trust prompts and open Copilot Chat. Confirm ordinary Copilot
Chat is usable with this bounded setup check:

```text
Summarize the purpose of this repository without changing files or running
commands.
```

Do **not** install GitHub Copilot upgrade or test `@upgrade` during prework.
Participants perform and observe that installation in the lab. Resolve GitHub
Copilot installation, sign-in, policy, or reload issues now.

Record your OS/architecture, VS Code version (Help > About), installed extension
versions, environment path (devcontainer or local), source/target SDKs, Copilot
CLI version, and the setup-check result for the facilitator. Do not record
credentials or tokens.

## 4. Clone and prove baseline readiness

```console
git clone https://github.com/frye/dotnet-modernization-lab.git
cd dotnet-modernization-lab
git status --short
dotnet --version
dotnet nuget list source
dotnet restore BookCatalog.sln --locked-mode
dotnet build BookCatalog.sln --configuration Release --no-restore
dotnet test BookCatalog.sln --configuration Release --no-build --no-restore
dotnet run --project src/BookCatalog.Api/BookCatalog.Api.csproj --configuration Release --no-build --no-restore --no-launch-profile --urls http://127.0.0.1:5080
```

Run these from the repository root, including in the VS Code integrated terminal.
Expect a clean Git status, SDK **8.0.425**, only the public nuget.org source,
successful restore/build, **24 passed / 0 failed / 0 skipped** tests, and a server
listening at `http://127.0.0.1:5080`. Visit `/books` in a browser: three books
should appear, starting with `The Lantern Atlas`. Stop with Ctrl+C.

Optional terminal equivalents for that GET: `curl http://127.0.0.1:5080/books`
on macOS/Linux, or `Invoke-RestMethod http://127.0.0.1:5080/books` in PowerShell.
Use the [lab's fuller smoke check](lab.md#2-explore-the-http-contract) for
creation and errors.

Do not "fix" an SDK error by removing `global.json` or selecting .NET 10 early.
Do not regenerate locks just to bypass a baseline locked-restore failure. First
confirm SDK, repository revision, feed, and unchanged project/lock files.

**Ready means:** baseline checks pass, both SDKs are visible, Copilot Chat works,
Copilot CLI is present, and repository/feed/Marketplace access works. The
GitHub Copilot upgrade extension should still be uninstalled. If any item is
blocked, send the exact command, error, installed versions, and attempted fix to
the facilitator.

## What has actually been tested

The authoring CLI baseline was exercised on macOS 26.6 ARM64. The Ubuntu 24.04
ARM64 devcontainer was built and exercised through locked restore, Release
build, all 24 tests, Copilot CLI discovery, and live HTTP checks. Windows,
native Linux, PowerShell examples, VS Code extension installation, and upgrade
agent behavior are documented from official guidance but have not been executed
during authoring. See the [evidence and rehearsal requirements](facilitator.md#authoring-evidence).
