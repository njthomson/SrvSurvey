# Inara session integration rewrite

Status: implementation specification for the rewrite of [PR #1025](https://github.com/njthomson/SrvSurvey/pull/1025).

## Objective

Replace the process-wide static Inara integration with a session-owned module whose lifetime is exactly the lifetime of one initialized `Game`. The rewrite must preserve Inara compatibility while making it impossible for one commander's name, FID, API key, mapped state, queued events, or final session events to be used by another commander.

The architectural rule is:

> One `Game` owns zero or one `Inara` instance. That instance may process and transmit data only for the exact `CommanderSettings` loaded for that `Game` session's FID.

## Scope

This rewrite covers:

- ownership and lifetime of the Inara integration;
- commander identity and API-key isolation;
- journal replay and live-event ingestion;
- queueing, batching, retry, and response validation;
- coordinated shutdown and final flushing;
- Inara settings UI and persistence;
- multiboxing safeguards and user guidance;
- diagnostics, tests, attribution, and PR documentation.

Exact timestamp correlation between `Cargo.json` and a journal `Cargo` event is explicitly deferred. The rewrite must continue to suppress sidecar-derived data conservatively when multiple Elite processes have been observed.

## Session and credential invariants

These invariants are mandatory:

1. `Game` has a regular nullable `Inara` member. There is no static `Game.inara`, global `currentGame`, or lookup through `Game.activeGame` inside the Inara module.
2. `Inara` is created only after `Game` has established its journal, first `Fileheader`, commander settings, commander name, and FID.
3. The payload commander name is the bound `CommanderSettings.commander`. It is not editable independently and is never taken from global last/current-commander state.
4. The API key is read only from the same bound `CommanderSettings` object. Each configured key therefore belongs to one FID-keyed commander profile.
5. Stable session values are captured at creation: commander name, FID, game version, and Live/beta eligibility. The API key remains dynamically readable from the bound settings object because the user may add, replace, or clear it during the session.
6. Every queued event carries the bound session identity and the API-key snapshot under which it was accepted. A key replacement or clear must discard events associated with the old key; they must never be sent under a replacement key.
7. Changing commanders disposes and finalizes the old `Game` and its Inara module before constructing the new session module. No queue, mapper, timer, task, or credentials survive that boundary.
8. No code path may fall back to `Settings.lastCommander`, `Settings.lastFid`, `Game.Commander`, or another active `Game` to resolve upload identity.

## Module boundary

`Inara` is a deep, session-scoped module. `Game` should know only how to create it, feed journal activity to it, and stop it. Queue implementation, replay, mapping policy, transport, response parsing, retries, and concurrency remain hidden inside the module.

The intended external shape is equivalent to:

```csharp
internal sealed class Inara : IDisposable
{
    public static Inara? Create(Game game);
    public void OnJournalEntry(JObject entry);
    public Task StopAsync(InaraStopReason reason);
    public void Dispose();
}
```

Names may follow existing repository conventions. The semantic requirements are:

- `Create` returns a fully usable instance or `null`; it must not return an object with a null client, timer, or session identity.
- `OnJournalEntry` does not accept a `Game` parameter because ownership already establishes the source.
- shutdown is awaitable or otherwise synchronously coordinated; it must not start a fire-and-forget flush and then dispose the `HttpClient` that flush needs.
- focused `internal` test seams may remain and are exposed only to `SrvSurvey.Tests` through `InternalsVisibleTo`.

## Lifecycle

### Creation

1. `Game.initializeFromJournal` establishes the session and its first `Fileheader`.
2. Validate that the journal filepath, `CommanderSettings`, commander name, FID, and version are available.
3. If identity is invalid, log the complete exception/diagnostic and return `null` without disrupting normal SrvSurvey journal processing.
4. Construct all required Inara resources locally. If any step fails, dispose partially created resources in the factory and return `null`.
5. Seed mapper state from the current journal before starting the periodic timer. Check the journal filepath before resetting or replaying state.
6. Count malformed replay lines and emit an aggregate diagnostic; never log raw journal lines.
7. Subscribe the completed instance to subsequent session activity and start its timer.

The established `Game` invariant that the first journal entry is `Fileheader` is authoritative. Do not add a `LoadGame` fallback. Resolve and cache the version once from that first entry. `OfType<T>().FirstOrDefault()` is already lazy, so this change is about stable session state rather than avoiding a full enumeration.

### Live operation

- Feed eligible live journal entries to the bound mapper even when no API key is configured. This keeps derived state warm if the user opts in during the session.
- API-key presence is the per-commander opt-in for collecting and uploading mapped events. Remove the global `Settings.inaraUpload` switch.
- When the key is added, newly mapped uploadable events use that key snapshot.
- When the key changes, discard queued groups carrying the old key before any send under the new key.
- When the key is cleared, stop collection for upload and explicitly discard pending events. Continue warming local mapper state.
- Grouping by credential snapshot may remain to make mid-session key transitions explicit, although a single Inara instance can never contain multiple commander identities.

### Commander switch and normal session end

1. Stop accepting new entries from the old `Game`.
2. Stop the periodic timer.
3. Wait for any active send to finish under a real concurrency gate.
4. For a normal session end with a still-valid key, create the final credit/session report from the old instance's bound state.
5. Perform at most one bounded final flush.
6. Dispose transport and other resources.
7. Only then allow the old `Game` to finish teardown and a new `Game`/Inara session to be created.

Clearing a key is an explicit opt-out and discards pending data; it is not equivalent to a normal session end. A commander switch must never consult the newly active game while finalizing the old one.

Use a `SemaphoreSlim`, a tracked active-send task, or an equivalent awaitable mechanism. An `Interlocked` skip flag alone is insufficient because shutdown must wait for an in-progress send and then make an exactly-once final-flush decision.

## Eligibility and multiboxing

- Cache the session's Live/beta classification at creation.
- Preserve existing eligibility rules for unsupported modes such as multicrew.
- Use `Elite.hadManyGameProcs` as the shared indication that multiple Elite processes have been observed.
- Remove Inara-specific process enumeration, `GetGameProcs`, `countGameProcesses`, and `DetectMultiboxing` logic.
- Do not use `FormMultiFloatie.current` as evidence; the form may be absent while multiboxing is active.
- Do not infer whether EDMC uploads to Inara from the presence of an EDMC process. Display static guidance telling users to enable Inara uploads in only one application at a time.

## Settings and persistence

The Inara settings form must be bound to one captured `CommanderSettings` object for its entire lifetime.

- Display the captured commander's `CommanderSettings.commander` as read-only context.
- Allow editing only that profile's `inaraApiKey`.
- Rename the confirmation action to `OK`.
- Add an explicit `Clear Key` action that confirms opt-out behavior and discards pending uploads for that bound session.
- Save only the captured `CommanderSettings`. The child form must not call `Game.settings.Save()` or commit unrelated edits in the parent settings window.
- If the active game changes while the form is open, the form must neither retarget nor save the key to the new commander. Its displayed commander and save target stay pinned to the originally captured FID profile.
- Remove `CommanderSettings.inaraCommanderName` and its control. Old serialized values may be ignored during deserialization; no migration to another identity field is needed.
- Remove `Settings.inaraUpload`. Key presence is the per-commander opt-in.

The missing-commander settings-dialog behavior noted in review remains deferred, apart from an optional TODO.

## Development mode

For the current Inara application-validation phase, set `isBeingDeveloped = true` directly when constructing the payload and remove the persisted developer-mode checkbox/setting. Before production release, reintroduce a hidden or advanced setting only if still needed, default it to `false`, and update tests and PR documentation in the same change.

## Transport, retry, and response handling

- Preserve EDMC-compatible constants: a 20-second HTTP request timeout and a 35-second batching cadence. They represent different concerns and intentionally differ.
- On HTTP failure, log the numeric status and `ReasonPhrase` without logging credential-bearing headers or request bodies.
- An empty successful response is incomplete according to the Inara integration guidance. Retain the batch and retry after a bounded delay with backoff/jitter.
- A missing `events` property is malformed/incomplete, not an empty successful event list. It follows the retry/requeue path.
- Validate that every response event token is a `JObject` with the expected status shape before casting. Do not silently drop malformed tokens with `OfType`, and do not blindly `Cast` before validation.
- Log safe, truncated `header.eventStatusText` and per-event name/status/text where useful. Never log the outbound payload or API key.
- Remove the generic `RunIsolated` helper. Use local exception boundaries where the recovery policy is visible, and log full exceptions (message and stack trace) safely.

## Attribution and documentation

- Add immutable commit-pinned EDMC permalinks beside each materially derived implementation file or section.
- Preserve the applicable GPL attribution.
- Rewrite the PR description to remove claims about a global upload checkbox, an editable Inara commander name, and a persisted/default-off developer checkbox.
- State that finalization occurs on every `Game` session disposal, not only application shutdown.
- State explicitly that API keys are FID/commander-profile bound and are never carried across sessions.

## File-level implementation map

- `SrvSurvey/game/Game.cs`
  - replace the static Inara property with a nullable instance member;
  - create it after session initialization;
  - route journal activity to it without passing `Game` back into the module;
  - coordinate its stop/finalization before the rest of session teardown.
- `SrvSurvey/net/Inara.cs`
  - convert static/global access to session ownership;
  - add all-or-nothing creation;
  - bind stable session identity and dynamic access to the exact session key;
  - remove active-game/process-form dependencies and `RunIsolated`;
  - implement awaited send/finalization ordering.
- `SrvSurvey/net/InaraModels.cs`
  - retain explicit session/key snapshots on queued events as needed;
  - make malformed response distinctions testable.
- `SrvSurvey/net/InaraEventMapper.cs` and `SrvSurvey/net/InaraCreditTracker.cs`
  - keep state instance-owned and ensure reset/replay never crosses a `Game` boundary.
- `SrvSurvey/game/CommanderSettings.cs`
  - retain `inaraApiKey`;
  - remove `inaraCommanderName`.
- `SrvSurvey/Settings.cs`
  - remove `inaraUpload` and the temporary persisted developer-mode setting.
- `SrvSurvey/forms/FormInaraIntegration.cs` and designer
  - pin the form to one captured commander profile;
  - display commander identity read-only;
  - add `OK`, `Clear Key`, and single-uploader guidance;
  - save no global settings.
- `SrvSurvey.Tests`
  - retain justified internal seams and add the regression matrix below.
- PR body and privacy/user-facing documentation
  - describe the actual per-commander opt-in, data flow, and session boundary.

## Required regression coverage

1. **Commander A to B:** A and B have distinct names, FIDs, and keys. After switching, no A identity, queued event, mapper state, key, or final event appears in B's request, and vice versa.
2. **Dialog switch race:** Open settings for A, switch the active game to B, edit/save, and prove that only A's captured `CommanderSettings` changes.
3. **Key added mid-session:** Start without a key, process state-building journal events, add A's key, and prove subsequent upload mapping uses the warmed A state.
4. **Key replacement:** Queue under A-key-1, change to A-key-2, and prove old-key events are discarded and never sent under A-key-2.
5. **Key clear:** Queue events, clear the key, and prove the queue is discarded and no final upload occurs for the cleared key.
6. **Normal disposal:** With a valid key, prove the active send is awaited, the final credit/session event is generated once, one bounded final flush occurs, and transport is disposed afterward.
7. **Timer/shutdown race:** Overlap timer, journal shutdown, and `Game.Dispose`; prove no duplicate send, disposed-client use, or lost exactly-once finalization.
8. **Invalid creation:** Missing FID, commander, first `Fileheader`, filepath, or client initialization returns `null`, cleans partial resources, logs safely, and leaves normal game processing alive.
9. **Multiboxing:** `Elite.hadManyGameProcs` suppresses the required sidecar-derived events without depending on `FormMultiFloatie` or new process enumeration.
10. **Response handling:** Cover HTTP reason phrases, empty response, missing `events`, non-object event tokens, rejected event status text, retry preservation, and credential-safe logs.
11. **Serialization cleanup:** Existing `inaraApiKey` values remain attached to their FID-keyed profiles; obsolete `inaraCommanderName` and global upload fields do not affect runtime behavior.
12. **Warm-state isolation:** Two sequential sessions with identical event shapes prove that mapper and credit-tracker state begins fresh for the second `Game`.

## Acceptance criteria

The rewrite is complete only when:

- all session and credential invariants above are enforced by code and focused tests;
- no Inara code references `Game.activeGame`, global current/last commander identity, or a static Inara singleton;
- a key can never be selected from a commander profile other than the one bound at `Inara.Create`;
- an open settings dialog cannot retarget its save when the active commander changes;
- key add/change/clear and normal session disposal have distinct, tested queue semantics;
- finalization is coordinated and exactly once;
- no API key or outbound payload can appear in logs;
- all relevant tests pass in the repository's supported x86 configuration and the application builds in Release x64;
- the final PR diff and description match this specification, including immutable EDMC attribution links.

## Validation checklist

Run the repository's focused Inara tests first, then the full supported test suite and build. At minimum, record:

```powershell
dotnet test .\SrvSurvey.Tests\SrvSurvey.Tests.csproj -p:Platform=x86
dotnet build .\SrvSurvey.sln -c Release -p:Platform=x64
git diff --check
```

Also manually verify the settings form for two FID-distinct commander profiles and inspect captured test requests to confirm that commander name, FID, API key, and event data always come from the same session. A passing unit suite is not a substitute for this cross-session payload inspection.
