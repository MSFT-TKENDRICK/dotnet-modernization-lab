# Prework: arrive ready to practice

Complete setup and the baseline readiness check **before** the session. Choose
Java or .NET, not both. This guide is only for the .NET track.

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

## 2. Install Git and both .NET SDKs

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

## 3. Install and check VS Code tooling

Install the current stable [Visual Studio Code](https://code.visualstudio.com/)
and use a supported OS. In Extensions (Ctrl+Shift+X, or Cmd+Shift+X on macOS):

1. Install **GitHub Copilot**, sign in, and confirm Copilot Chat is usable.
2. Search for **GitHub Copilot upgrade** and install it, as described in the
   [official VS Code setup](https://learn.microsoft.com/en-us/dotnet/core/porting/github-copilot-upgrade/install?pivots=vscode).
   Follow any required dependency-install or reload prompts.
3. The Microsoft **C#** extension is useful for editing and diagnostics; install
   it if needed. This lab does not require Visual Studio or C# Dev Kit.

Open the cloned repository folder, review Workspace Trust prompts, and open
Copilot Chat. Type `@upgrade` and confirm that it is recognized; the agent picker
may also show `Upgrade`. Send this bounded setup check:

```text
@upgrade Confirm you are available for this .NET lab. Do not assess, change files,
run an upgrade, or create commits yet.
```

If the agent is missing, resolve extension installation, sign-in, policy, or
reload issues now. Do not substitute the Java modernization extension or
Visual Studio's `@Modernize` instructions. The current Microsoft docs describe
SDK acquisition, tool registration, and guided workflow support, but extension
behavior must still be checked on the actual workshop machine.

Record your OS/architecture, VS Code version (Help > About), installed extension
versions, source/target SDKs, and the setup-check result for the facilitator.
Do not record credentials or tokens.

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

**Ready means:** baseline checks pass, both SDKs are visible, `@upgrade` responds,
and repository/feed/Copilot access works. If any item is blocked, send the exact
command, error, installed versions, and attempted fix to the facilitator. Do not
spend the practice session reinstalling tooling.

## What has actually been tested

The authoring CLI baseline was exercised on macOS 26.6 ARM64. Windows, Linux,
PowerShell examples, and VS Code extension/agent behavior are documented from
official guidance but have not been executed during authoring. See the
[evidence and rehearsal requirements](facilitator.md#authoring-evidence).
