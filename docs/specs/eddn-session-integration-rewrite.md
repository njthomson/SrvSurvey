# EDDN session integration rewrite

## Goal

Add explicit, global opt-in EDDN sharing without allowing uploader identity or
session state to cross Commander sessions. The implementation must remain safe
across Commander changes, multiple SrvSurvey processes, multiple Elite clients,
temporary network failures, and application restarts.

## Lifetime model

- `Game` exposes one application-lifetime EDDN service as a static singleton.
  It is explicitly created only after `Settings.Load()` and owns consent checks,
  runtime safety, transport, the exclusive queue lease, and durable retry
  delivery.
- Each initialized `Game` owns one session publisher. It captures an immutable
  EDDN header containing that session's Commander and version values, and owns
  location, crew, signal-batch, companion-read, and deduplication state.
- Disposing a `Game` cancels its companion reads and flushes only a valid signal
  batch captured for that session. Already queued messages remain owned by the
  application service and retain their captured Commander header.
- No EDDN component is constructed from `Game`'s static initializer, and no
  EDDN session reads `Game.activeGame` or mutable current-Commander globals.

## Consent and user interface

- EDDN sharing defaults off and is configured from a dedicated **Configure EDDN
  Sharing** button directly above the Inara button on Settings > External Data.
- The dialog provides the single opt-in checkbox and explains the journal and
  companion data sent, Commander uploader identifier and EDDN obfuscation,
  lack of an EDDN account/API key, local durable retry queue, deletion on
  opt-out, global scope, multi-client pause, and duplicate-upload risk.
- The warning must say: “Enable EDDN uploads in only one application at a
  time—for example, SrvSurvey or EDMC—to avoid duplicate submissions.”
- Disabling sharing immediately cancels active delivery and deletes pending
  uploads. A failed settings save must not change the active consent state.

## EDDN policy

- Uploads use EDDN's live upload endpoint.
- All schema references append `/test`. This is an internal release policy and
  is not stored in Settings or exposed as a user choice. A future release can
  change the single policy constant after production readiness is confirmed.
- Companion files are revalidated against the triggering journal event and
  runtime attribution immediately before enqueueing.

## Delivery and batching

- An immutable payload, including the captured session header, is durable before
  its enqueue operation succeeds.
- Retryable failure of one message must not block other due messages. Each
  message retains its own bounded exponential backoff.
- Invalid durable entries are quarantined without preventing valid entries from
  loading or sending.
- `FSSSignalDiscovered` batches capture their system, expansion flags, header,
  and safety generation with the first signal. A later jump or location event
  flushes the batch with that captured system rather than the destination.
- The journal consumer must not hold a session lock while performing durable
  queue disk I/O or network I/O.

## Verification

- Tests reference the real `SrvSurvey` project through `InternalsVisibleTo`;
  production source files are not linked into a duplicate test assembly.
- The test project is included in the solution and exercised by CI.
- Regressions cover startup ordering, Commander A-to-B isolation, captured
  signal-batch location, retry fairness, consent deletion, companion-read
  invalidation, fixed `/test` schemas, queue restart, and exclusive ownership.
- Release x64 build and the full test projects must pass against current `main`,
  including the merged Inara integration, before the branch is published.
