# Participant lab: .NET 8 to .NET 10 with GitHub Copilot

Complete [prework](prework.md) first. Work locally with fictional data only.
This is the .NET option for the shared 30-minute session; there is no Java setup
here. The only planned installation is GitHub Copilot upgrade for VS Code or
Copilot CLI in step 3; all environment dependencies belong in prework. Spend
the rest of the session practicing assessment, decision review, and validation.
A reviewed plan plus a recorded blocker is a valid finish.

## 1. Establish a baseline and working branch

Open the repository root in VS Code (File > Open Folder). In its terminal:

```console
git status --short
git rev-parse HEAD
git switch -c workshop/dotnet10
dotnet --version
dotnet restore BookCatalog.sln --locked-mode
dotnet build BookCatalog.sln --configuration Release --no-restore
dotnet test BookCatalog.sln --configuration Release --no-build --no-restore
```

Start from a clean distributed checkout. If Git status shows changes, preserve
them and resolve with your facilitator rather than resetting them. Save the SHA
printed by `git rev-parse HEAD` as your **baseline SHA** for later diff review.
If the branch name already exists, choose a new descriptive name rather than
overwriting an earlier attempt.

Expect SDK `8.0.425` and 24 passing tests on .NET 8. Record the result. Stop and
document any failure before attributing it to the upgrade.

## 2. Explore the HTTP contract

Run in the first terminal:

```console
dotnet run --project src/BookCatalog.Api/BookCatalog.Api.csproj --configuration Release --no-build --no-restore --no-launch-profile --urls http://127.0.0.1:5080
```

Keep it running while making the requests below in a second terminal. Start
with a freshly restarted server for deterministic IDs.

### macOS/Linux (bash or zsh)

```bash
curl -i http://127.0.0.1:5080/books
curl -i http://127.0.0.1:5080/books/1
curl -i http://127.0.0.1:5080/books/999
curl -i http://127.0.0.1:5080/books -H 'Content-Type: application/json' --data '{"title":"Maps of Tomorrow","author":"Tessa Reed"}'
curl -i http://127.0.0.1:5080/books/4
curl -i http://127.0.0.1:5080/books -H 'Content-Type: application/json' --data '{"title":" ","author":"Tessa Reed"}'
curl -i http://127.0.0.1:5080/books
```

### Windows (PowerShell)

These examples use PowerShell's own HTTP commands, not its `curl` alias. The
catch block handles only the expected 400/404 responses, rethrowing other errors.

```powershell
$base = 'http://127.0.0.1:5080'
Invoke-RestMethod "$base/books"
Invoke-RestMethod "$base/books/1"
$created = Invoke-WebRequest -UseBasicParsing -Uri "$base/books" -Method Post -ContentType 'application/json' -Body '{"title":"Maps of Tomorrow","author":"Tessa Reed"}'
$created.StatusCode
$created.Headers.Location
$created.Content
Invoke-RestMethod "$base/books/4"

foreach ($case in @(
    @{ Uri = "$base/books/999"; Method = 'Get'; Expected = 404 },
    @{ Uri = "$base/books"; Method = 'Post'; Expected = 400; ContentType = 'application/json'; Body = '{"title":" ","author":"Tessa Reed"}' }
)) {
    $expected = $case.Expected
    $parameters = $case.Clone()
    $parameters.Remove('Expected')
    try {
        $response = Invoke-WebRequest -UseBasicParsing @parameters -ErrorAction Stop
        throw "Expected HTTP $expected; got $($response.StatusCode)"
    } catch {
        if ($null -eq $_.Exception.Response -or [int]$_.Exception.Response.StatusCode -ne $expected) { throw }
        "Expected HTTP $expected"
    }
}
Invoke-RestMethod "$base/books"
```

The list starts with IDs 1-3. Known IDs give 200; unknown IDs give 404. Creation
returns 201, ID 4, and `Location: /books/4`; that location returns the created
book. Invalid creation gives 400 without adding a book. The final list contains
exactly four books. Valid strings retain their original spacing.

Stop the first terminal's server with Ctrl+C before upgrading or rerunning this
sequence. Restarting resets data; tests independently create their own hosts.

## 3. Install and launch the upgrade agent in guided mode

Choose one installation path. Neither installation is included in the
devcontainer or prework. Both require Marketplace access and a working GitHub
Copilot sign-in.

### Option A: VS Code

Open Extensions in VS Code, search for **GitHub Copilot upgrade**, and install
the extension published by Microsoft (`ms-dotnettools.upgrade-agent`), following
any reload prompts.

Open **GitHub Copilot Chat**. Confirm that `Upgrade` appears in the agent picker
or that `@upgrade` is recognized. Select `Upgrade` in the agent picker.

### Option B: GitHub Copilot CLI

Start GitHub Copilot CLI:

```console
copilot
```

In the Copilot CLI chat, add Microsoft's plugin marketplace and install the
upgrade agent:

```console
/plugin marketplace add microsoft/upgrade-agent-plugins
/plugin install upgrade-agent@upgrade-agent-plugins
```

Run `/agent` and confirm that `upgrade-agent` appears in the agent list, then
select it.

With the upgrade agent selected in either environment, paste the following
prompt. In VS Code, you can instead leave the current agent unchanged and prefix
the first line with `@upgrade`.

```text
Assess this BookCatalog.sln for an in-place upgrade from .NET 8 to .NET 10. Use
guided mode and remain in guided mode throughout. I have already created a
working branch; use it. Do not execute upgrade changes until I have reviewed
the assessment, upgrade options, and plan and explicitly approved execution.
Do not commit unless I separately authorize a commit strategy. Do not push,
publish, deploy, or change repository visibility.

Keep all three projects and preserve the HTTP contracts and all meaningful tests.
Use the installed stable .NET 10 SDK, update global.json explicitly, align
Microsoft.AspNetCore.Mvc.Testing to .NET 10, and regenerate/review NuGet locks.
Keep xUnit v2 and VSTest unless you demonstrate a compatibility blocker.
Do not introduce new services, architecture, cloud resources, or unrelated
package/technology migrations. First confirm target, branch, and guided mode,
then stop after assessment and proposed options for my review.
```

Confirm the source, requested target, branch, workflow mode, and Git strategy
when asked. Baseline SDK `8.0.425` must stay selected until you approve the
upgrade changes. The target candidate is SDK `10.0.401`, already installed
in prework; verify its availability instead of choosing a preview.

**Do not use automatic mode for this exercise.** Microsoft's overview notes
that "continue" can switch modes, so give explicit stage-limited instructions
such as "Proceed only to planning; remain in guided mode and pause before
execution." If the agent starts changing code before approval, tell it to
pause, inspect the changes, and reestablish the boundary.

## 4. Review assessment, options, and plan

The documented workflow writes its real output beneath
`.github/upgrades/{scenarioId}/`. The scenario ID is selected by the agent
(for example, `dotnet-version-upgrade`); inspect the actual folder rather than
assuming the name. No agent output is included in this starter.

| Artifact | What to review |
| --- | --- |
| `assessment.md` | Three projects, existing TFMs, dependencies, identified compatibility issues, and evidence for each claimed blocker |
| `upgrade-options.md` | Confirmed strategy and decisions: in-place upgrade, limited scope, no unnecessary architecture/testing migration |
| `plan.md` | Ordered work, SDK/package alignment, locks, risks, and validation commands before execution |
| `tasks.md` | Created during execution; task scope, progress, and validation results, not proof that every task passed |
| `scenario-instructions.md` | Persistent preferences, decisions, and constraints |
| `tasks/{taskId}/task.md` and `progress-details.md` | Per-task scope and execution evidence when generated |

The **Upgrade Dashboard is CLI/app-only, not available in VS Code**. Use the
Markdown files, Source Control, and chat. Existing scenario state can prompt the
agent to resume or start fresh; preserve earlier work and choose deliberately.

Review the assessment/options with these questions: are all three projects
included, are findings real for this code, are existing contracts protected,
and is the proposed package work necessary? For this small dependency graph an
all-at-once in-place upgrade may be sensible. Do not accept extra services,
database migrations, Central Package Management, or an xUnit migration merely
because the agent suggests them.

After confirming options, request:

```text
Proceed only to the planning stage using the confirmed options. Remain in guided
mode and pause after writing the plan. Do not execute upgrade changes or commit.
Include global.json, all three target frameworks, MVC Testing, lock-file
regeneration, build/test commands, and HTTP contract checks.
```

Open and review `plan.md`. Amend the plan or ask for clarification before any
execution. A small or empty API-breaking-change list is plausible: do not create
artificial breakage just to make the exercise more dramatic.

## 5. Execute only after review

Proceed only if the plan is understood, prerequisites work, and you choose to
attempt execution. Otherwise use the stopping point below.

```text
I approve execution of the reviewed plan. Remain in guided mode and pause at
stage boundaries or blockers. Preserve API behavior and meaningful tests.
Do not commit, push, publish, or deploy. Show the changed files and actual
validation results, and distinguish completed work from blocked work.
```

Inspect the resulting upgrade changes, not just the agent's summary:

| Surface | Expected intentional change |
| --- | --- |
| `global.json` | SDK `8.0.425` -> verified installed `10.0.401`; retain `rollForward: "disable"` and `allowPrerelease: false` |
| All three `.csproj` files | `net8.0` -> `net10.0` |
| Test project's MVC Testing reference | `8.0.31` -> stable compatible `10.0.12` candidate, reconfirmed on NuGet |
| `packages.lock.json` files | Regenerated for changed TFMs/dependency graph, not manually edited |
| API/domain/tests | Only proven compatibility fixes; observable contracts remain |
| CI / docs | CI follows `global.json`; update any instructions that now describe an upgraded checkout |

Do not make these edits before assessment just to help the agent along. Changing
the TFMs without updating the strict SDK pin causes `NETSDK1045`; the .NET 8 SDK
cannot build `net10.0`. Deleting `global.json` makes SDK selection accidental.

After the approved configuration/package changes, have the agent run (or run
yourself to inspect the result):

```console
dotnet --version
dotnet restore BookCatalog.sln --force-evaluate
git diff -- global.json src tests .github/workflows
dotnet restore BookCatalog.sln --locked-mode
dotnet build BookCatalog.sln --configuration Release --no-restore
dotnet test BookCatalog.sln --configuration Release --no-build --no-restore
```

Expect the exact target SDK, reviewed lock changes, and all 24 tests still
passing without weakened assertions. `--force-evaluate` deliberately refreshes
locks after configuration changes; it is not the normal baseline/CI restore
mode. Return to `--locked-mode`, and include updated locks with approved changes.
Keep existing compatible test packages rather than updating everything.

## 6. Review the whole diff and rerun HTTP checks

```console
git status --short
git diff
git diff --cached
```

New files (including upgrade artifacts) appear in status but not an ordinary
unstaged diff; open them in VS Code. If you separately authorized agent commits,
replace `BASELINE_SHA` below with the actual SHA saved in step 1:

```console
git log --oneline BASELINE_SHA..HEAD
git diff BASELINE_SHA -- .
```

This catches both committed changes and current tracked working-tree changes,
not just the last uncommitted edit. Do not assume a clean `git diff` means the
agent did no work.

Start the server again with the command in step 2 and repeat the HTTP sequence
on fresh data. Stop it with Ctrl+C. Review `tasks.md` alongside actual terminal
results. Do not mark upgrade validation complete just because a task has a
completion checkbox. No deployment or publication is part of this lab.

## Valid stopping point: plan and blockers

If you cannot or choose not to execute, keep the actual assessment/options/plan
and record a short note in your own `docs/participant-notes.md`: baseline SHA,
OS and tool versions, intended target, reviewed decisions, completed commands
and results, exact blocker/error, attempted fix, and the next action/owner.
Distinguish an unexecuted plan from a validated upgrade. Never write fictional
successes into `tasks.md`.

If `@upgrade` itself is blocked, write your own clearly labeled **manual draft**
and blocker note outside `.github/upgrades/`; do not impersonate agent output.
The facilitator can use genuine, separately prepared rehearsal artifacts for
discussion if available.
