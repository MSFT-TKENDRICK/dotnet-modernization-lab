# Facilitator guide

This is the .NET alternative for a public modernization workshop.
Attendees choose Java **or** .NET. Both use VS Code; do not import Windows,
Visual Studio, .NET Framework, Azure, or Docker requirements from the broader
inspiration workshops.

## Before distribution

Confirm the repository contains the intended .NET 8 starter revision, not a
participant-upgraded branch. Coordinate any commit/push/distribution separately
with the owner: authoring does not authorize publication.

Confirm attendee access with the actual attendee accounts. If private-repository
access cannot be arranged, obtain explicit owner approval for a visibility
change; do not make the repository public on assumption. Check organization
restrictions on Copilot and extensions. Validate GitHub, Marketplace, Microsoft
SDK-download, and NuGet connectivity on the workshop network.

Recheck Microsoft's [installation instructions](https://learn.microsoft.com/en-us/dotnet/core/porting/github-copilot-upgrade/install?pivots=vscode)
and [walkthrough](https://learn.microsoft.com/en-us/dotnet/core/porting/github-copilot-upgrade/how-to-upgrade-with-github-copilot)
for drift. Complete [prework](prework.md) on representative attendee machines.
Do not rely on automatic SDK acquisition to supply the correct terminal SDK.
No desktop workloads or cloud accounts are needed.

## Session checkpoints

Use the 30-minute session as a practice block, not an installation clinic.

| Segment | Checkpoint |
| --- | --- |
| Opening | Confirm track choice, baseline readiness, branch, and SDK 8.0.425; direct setup failures to blocker documentation |
| Assessment practice | Launch `@upgrade` with explicit .NET 10 target and guided mode; inspect actual findings and proposed options |
| Planning practice | Review all three projects, SDK pin, MVC Testing alignment, locks, validation, and scope exclusions before approving execution |
| Optional execution | Inspect changes, run locked restore/build/test, repeat HTTP checks, and review real tasks/results |
| Closing | State what was actually completed, retain the reviewed plan and blockers, and identify the next action |

Keep most of the block on assessment, plan review, and hands-on validation.
An honest plan-plus-blocker outcome is successful learning. Do not promise a
completed upgrade or pressure attendees to approve changes they have not read.

The sample intentionally has few moving parts. SDK, TFMs, framework-aligned
testing packages, and lock files may be the principal changes. Do not add
artificial vulnerabilities or legacy APIs to manufacture findings. Distinguish
necessary compatibility changes from unrelated "modernization" suggestions.

## Troubleshooting

| Symptom | Diagnose and recover |
| --- | --- |
| Private repository not found / clone denied | Confirm account, invitation, organization authentication, and granted access. Do not put credentials in URLs or change visibility without approval. |
| `dotnet` not found / exact SDK missing | Check `dotnet --list-sdks`, `dotnet --info`, and `command -v dotnet` or `Get-Command dotnet`. Install the pinned SDK; restart terminal/VS Code. Check architecture and competing SDK roots. |
| Baseline selects 9/10 or a preview | Run from the repository root and inspect `global.json`, active `dotnet`, and local changes. Expected source selection is exactly 8.0.425. Do not remove the pin or relax roll-forward. |
| Runtime missing despite an SDK being present | Check `dotnet --list-runtimes`, `DOTNET_ROOT`, and architecture. SDK listing and actual testhost runtime discovery must agree. Remove unintended runtime major-roll-forward overrides. |
| `NETSDK1045` after target change | Update `global.json` to the verified installed .NET 10 SDK as an approved upgrade change; all three TFMs must agree. |
| `NU1301`, TLS, proxy, or feed failure | Inspect `dotnet nuget list source` and restore output. The repo uses only nuget.org. Use approved network/proxy/certificate support; do not disable TLS or add secrets to config. |
| `NU1004` / lock mismatch on baseline | Verify SDK 8.0.425 and an unchanged starter revision first. Do not regenerate locks to conceal an unexpected difference. |
| Lock mismatch during approved upgrade | Review changed SDK/TFMs/package references, run `dotnet restore BookCatalog.sln --force-evaluate`, inspect locks, then return to `--locked-mode`. |
| `@upgrade` missing / no response | Check exact extension name, installation/reload state, sign-in, Copilot entitlement/quota, and organization policy. Use the current VS Code instructions, not Java or Visual Studio setup. |
| Agent runs ahead / proposes Azure or unrelated migrations | Say "pause," reaffirm guided stage boundaries and scope, inspect any changes, and revise options before execution. Do not blindly revert participant work. |
| Port 5080 is in use | Stop only the known lab process with Ctrl+C, or choose another loopback port and update every HTTP example consistently. Do not kill unrelated processes. |
| IDs or list counts differ in smoke test | Stop/restart this API, then run the request sequence once. Data is process-local and intentionally ephemeral. |
| Tests are stale after editing | Build again without `--no-build`; the documented sequence builds first, then tests with `--no-build`. Do not skip failing tests. |
| CI unavailable | Check Actions permissions/policy and hosted-runner allowance. Local commands remain the exercise; do not claim CI ran merely because YAML exists. |

If blocked, record the exact command, error, versions, network context without
secrets, attempted resolution, and next owner/action. Keep unexecuted stages
explicitly unexecuted. A screenshot alone is less useful than reproducible
commands and the actual generated assessment/plan.

## Reset safely through a fresh clone

Keep the original checkout intact, including uncommitted work and agent artifacts.
Stop its API with Ctrl+C. From a parent directory outside that checkout:

```console
git clone https://github.com/frye/dotnet-modernization-lab.git dotnet-modernization-lab-fresh
cd dotnet-modernization-lab-fresh
dotnet --version
dotnet restore BookCatalog.sln --locked-mode
dotnet build BookCatalog.sln --configuration Release --no-restore
dotnet test BookCatalog.sln --configuration Release --no-build --no-restore
```

Choose another unused directory name if that one exists. Open the new folder in
VS Code and create a new participant branch. If the owner distributed a
particular starter commit rather than the current default branch, check out that
approved revision in the new clone first. Do not use a destructive reset or delete
the original folder to get back to a green baseline.

## Prepare genuine rehearsal artifacts

No agent rehearsal artifacts are included in this starter. Before the session,
use a separate clone and branch of the actual distributed revision, with VS Code,
the real upgrade extension, both SDKs, and the intended network/account policy.
Follow the participant guide exactly, including the guided assessment/options/
plan gates and Git strategy. Do not upgrade the distributed starter.

Record baseline SHA, OS/architecture, VS Code and extension versions, SDKs,
package versions, prompt, actual `.github/upgrades/{scenarioId}/` files, full
diff including any agent commits, terminal commands, and real results. Preserve
blockers and partial outcomes without rewriting them as successes. Re-run
build/tests and HTTP smoke checks on any actual upgraded rehearsal branch.

Keep those artifacts separate and label them **facilitator rehearsal** with
their source revision and environment. Share only after inspecting for sensitive
data and obtaining any required publication approval. Do not copy them into the
starter's `.github/upgrades/` directory or imply that each attendee's project has
already been assessed.

If the live service fails, use genuine prepared artifacts for review. If none
exist, review a clearly labeled manual plan and documented blocker instead.
No scripted/fabricated agent reports are needed.

## Authoring evidence

| Item | Status |
| --- | --- |
| Baseline OS | macOS 26.6 ARM64 only |
| SDK/runtime | Official SDK 8.0.425 / .NET and ASP.NET Core 8.0.31; archive SHA-512 verified; session-local install |
| Dependencies | All three projects restored; lock files generated by SDK, then locked restore passed |
| Build | Release build passed, zero warnings/errors |
| Integration tests | 24 passed, zero failed/skipped; net8.0 |
| Real HTTP | List/known ID 200, unknown ID 404, create 201 with Location, created-book retrieval 200, invalid creation 400, state unchanged after rejection |
| Clean-source repeat | Passed locked restore, Release build, all 24 tests, and real HTTP smoke checks in a source-only snapshot without prior bin/obj, using empty NuGet package and HTTP caches; lock files remained unchanged |
| SDK guard | The machine's .NET 9-only installation correctly refused the exact .NET 8 pin instead of rolling forward |
| Dependency advisories | `dotnet list BookCatalog.sln package --vulnerable --include-transitive` reported no vulnerable packages from the current NuGet source; not a guarantee against future advisories |
| Windows/Linux and PowerShell | Not executed during authoring |
| GitHub-hosted CI | Workflow provided, not executed during authoring |
| VS Code setup and actual agent rehearsal | Not executed during authoring |
| .NET 10 upgrade execution | Not performed; starter remains net8.0 |
| Visibility/distribution | Private at authoring; attendee access and owner-authorized distribution remain preparation work |

The current machine's global .NET 9 installation was not replaced. Authoring
commands scoped `DOTNET_ROOT` and `PATH` to a downloaded .NET 8 SDK outside the
repository. No machine-specific path is required by the committed configuration.
The clean-source snapshot proves the baseline does not require the authoring
checkout's build outputs or NuGet cache; it is not a remote attendee clone or a
VS Code rehearsal. An attendee clone of the distributed revision must still be
checked after the owner authorizes distribution.
