# Design 04: Parallel work plan (up to 5 worktree agents)

Status: draft. Agents do not start until Bob approves docs 00 to 03.

## 1. Principle
Parallel agents collide on shared files, not on features. So: (1) a short **Phase 0** on one branch freezes the shared
contracts (interfaces, option shapes, catalog codes, file layout); (2) five lanes then build behind those contracts,
each owning a disjoint set of files; (3) the few unavoidably shared files are edited only by the lead, or via append-only
rules below.

## 2. Phase 0: freeze the contracts (1 agent, this worktree lineage, small)
Deliverables, interfaces and stubs only, no behavior:
- `BobEvent`, `BobEventSpec`, `IBobEventInterceptor`, `BobBehaviorRegistry` shape (doc 01).
- `BobFieldBase<T>` skeleton, `IBobValueConverter<T>`, `BobFieldError`, `IBobErrorTextProvider`, `BobBindTiming` (doc 02).
- `BobOptions.Events`, `.Inputs`, `.Culture` option groups.
- Reserved error codes (all of doc 01 s6 and doc 02 s7.2) entered in `error-catalog.json` with English text.
- Reserved CSS file names and an `@import` block in `bob.css` with empty placeholder sheets (so lanes never edit `bob.css`).
- `docs/design/CONTRACTS.md` listing the frozen signatures. After this merges, changing a signature needs the lead.
- Merge Phase 0 to the integration branch, then cut the five lane worktrees from it.

## 3. Lanes

Branch naming: `lane/<letter>-<slug>` cut from `integration/design-0.3`. Worktree path:
`E:\Dev\BobsComponents\.claude\worktrees\lane-<letter>`.

### Lane A: Event runtime and behavior migration
- Owns: `wwwroot/js/bob.core.js` (+ feature modules), `Infrastructure/Events/**`, `BobEventScope`, `BobEventTarget`,
  `DevKit` Events panel, JS tests (`package.json` scripts), `Tests/Events/**`.
- Tasks: runtime delegation/keys/throttle/projection; pipeline; sync JS interceptors; behavior registry population API;
  migrate `ModalSheet`/`ConfirmDialog` focus trap, escape, backdrop, history onto named behaviors (edits those two
  components' `.razor`/`.cs` only for event wiring); dev kit panel.
- Depends on: Phase 0. Provides: working event pipeline others call through `OnBefore/OnAfter`.

### Lane B: Fields and forms
- Owns: `Components/Fields/**`, `Components/Forms/**`, `wwwroot/css/components/field*.css`, `text*.css`, `number*.css`,
  `checkbox.css`, `switch.css`, `radio.css`, `form*.css`, `Tests/Fields/**`, `Content/accessibility/{field,text,...}.md`.
- Tasks: doc 02 section 10 order; migrates `SelectField` (its `.razor` only).
- Depends on: Phase 0. Uses the text provider through the frozen interface (stub returns English catalog until lane D lands).

### Lane C: Overlay and collection primitives
- Owns: `Components/Primitives/**` (`BobPortal`, `BobAnchor`, `BobPopover`, `BobFocusScope`, `BobVirtualizer`,
  `BobCollection<T>`), their CSS/tests/a11y docs, then Tier 2 `Tooltip`, `Menu`, `Drawer` variants.
- Depends on: Phase 0 (events interface). Uses lane A's pipeline only through the frozen `BobEvent` contract; real
  integration test after A merges.

### Lane D: Error text, translation, dev kit and quality infrastructure
- Owns: `Errors/Text/**` (provider chain, ICU-subset formatter, culture resource files, `en` + a sample second
  culture to prove the path), `IBobFieldErrorSink`, new catalog codes' final wording, `BobErrorCard` text wiring,
  trimmed/AOT publish CI job, render-count test helper, a11y smoke tooling, `scripts/**` additions.
- Tasks include resolving `BOB-CB-002` (raise it or retire it, with Bob's approval for the retire).
- Depends on: Phase 0.

### Lane E: Docs engine and migration tables
- Owns: `BobsComponents.Docs/**`, `Content/migration/**`, generated "Events and behaviors" section, docs drift tests for
  the new registry/codes, `docs/api-surface.txt` refresh process, `docs/CHANGELOG.md` entries, CSS-CONTRACT.md entries
  (append-only via a per-lane fragment file merged by the lead).
- Later (after B/C land): Tier 3 `Combobox` design doc and the Calendar core, since the docs lane is lightest.
- Depends on: Phase 0.

## 4. File ownership and shared-file rules
| Shared file | Rule |
| --- | --- |
| `wwwroot/bob.css` | Phase 0 adds all `@import`s; lanes only fill the placeholder sheets |
| `error-catalog.json` | Phase 0 reserves codes; lanes edit only text of their own codes; catalog test guards it |
| `CSS-CONTRACT.md` | Each lane writes `docs/design/fragments/<lane>-contract.md`; lead merges at integration |
| `docs/api-surface.txt` | Never edited by lanes; regenerated once on the integration branch |
| `docs/CHANGELOG.md` | Fragments per lane, lead merges |
| `BobOptions.cs` | Phase 0 only; later changes via the lead |
| `BobServiceCollectionExtensions.cs` | Append-only registrations, one line each, in a lane-labeled block |

## 5. Merge order and gates
1. Phase 0 -> `integration/design-0.3`.
2. Lanes A, D, E merge first (infrastructure); then B and C (consumers), rebasing on integration.
3. Each lane PR must pass: `dotnet test BobsComponents.Tests`, `dotnet build BobsComponent.Client`, drift tests, and its
   own new tests. The integration branch additionally gets the API snapshot refresh and a demo smoke run.
4. Integration -> PR to the release branch. Nothing pushes to main/master; no force pushes; no merge without Bob.
5. Publishing a prerelease to the private feed is Bob's go-ahead only (existing rule for publish).

## 6. Agent sizing (per Bob's guidance: small tasks, isolated worktrees)
Each lane is split into tasks of roughly 1 to 3 hours of agent work with a defined test as the exit gate, handed over as a
short brief: goal, files owned, frozen contracts to read, the checklist, the test commands. No lane task edits files
owned by another lane; if it needs to, it stops and reports. A lead session (this one) reviews each lane's PR diff before
integration (a read that is not anchored on the author's own view: `code-review` skill on each lane).

## 7. Risks
| Risk | Mitigation |
| --- | --- |
| Phase 0 contracts turn out wrong after lanes start | Keep Phase 0 small and prove it with one vertical slice (a text field raising `bob.field.commit-on-enter`) before fan-out |
| Event runtime and fields integrate late | Lane B ships against the frozen interface with a fake pipeline; one integration test lands the day A merges |
| Merge conflicts in shared files | Rules in section 4 |
| JS tooling (jsdom) absent in CI | Lane A adds the npm test step; lane D wires it into CI |
| Scope creep into the grid | Grid is Tier 5; no lane starts it without a separate design doc |

## 8. Start commands (after approval)
```powershell
# Phase 0 (lead, this worktree)
cd E:\Dev\BobsComponents\.claude\worktrees\design-events-inputs
git switch -c integration/design-0.3

# Then, per lane (example for lane A)
git worktree add E:\Dev\BobsComponents\.claude\worktrees\lane-a -b lane/a-events integration/design-0.3
```
