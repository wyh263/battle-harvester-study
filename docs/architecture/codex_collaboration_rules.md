# Codex Collaboration Rules

## Purpose

This document records how Codex should collaborate on new gameplay systems in this project.
It exists to avoid a repeated pattern:

1. User proposes a new system or mode.
2. Codex implements the simplest working version immediately.
3. The user then points out coupling, reuse, and extensibility problems.
4. The system has to be restructured after partial implementation.

For this project, that workflow is considered incorrect unless the user explicitly asks for a throwaway prototype.

## Default Rule For New Systems

When the user proposes a new system, Codex must not jump directly to the easiest local implementation.
Codex must first treat the request as an architecture task.

Default order:

1. Clarify the system boundary.
2. Identify all likely future users of the system.
3. Identify whether it is actor-specific or shared infrastructure.
4. Design the extensibility points first.
5. Decide which part is temporary and which part is intended to become production structure.
6. Only then implement the smallest version that still respects the designed structure.

## Mandatory Questions Codex Must Answer Internally First

Before implementing a new system, Codex must first answer these:

- Is this really a `Player` feature, or is it a shared combat/gameplay feature?
- Will enemies, bosses, neutral units, summons, or AI need the same capability later?
- Is the request about behavior rules, or about architecture and ownership?
- What data should be stored centrally?
- What should other systems read from this system?
- What should this system never depend on directly?
- What extension paths are likely within the next few iterations?

If these answers are not clear, Codex should present the structure first instead of coding immediately.

## Project-Specific Rule Learned From Targeting

Targeting, lock-on, aim mode, mouse-follow aim, nearest-target aim, and similar features are not `Player`-owned logic.
They are shared combat targeting infrastructure.

Therefore:

- Do not anchor targeting architecture inside `Player`.
- Do not treat mouse aim as the system itself.
- Do not let `AttackExecutor` know where targeting decisions came from.
- Do not mix input collection, target selection, target state, and attack consumption in one class.

Preferred split:

- `Targetable` or equivalent: declares what can become a target.
- `TargetingController` / `CombatAimController`: stores targeting state and resolves final aim.
- Input adapter layer: player-only input interpretation such as mouse, double-click, hotkeys.
- AI adapter layer: enemy/Boss target selection logic.
- Consumers such as attacks/skills: only read final aim result and current target state.

Codex must also separate these concepts before implementation:

- combat state: for example `Free` vs `Locked`
- acquisition mode: how a lock request gets its target
- selection strategy: how a strategy-based mode chooses or orders targets
- input entry: which player action triggers the request

These are not interchangeable. Codex should not collapse them into one enum or one input script.

## Additional Rule Learned From Targeting Iterations

When the user is still exploring behavior, Codex must avoid turning every temporary behavior difference into a new top-level mode immediately.

Before coding, Codex must classify the request as one of:

- a combat state change
- a lock acquisition mode
- a target selection strategy
- an input binding / adapter change
- a consumer-side interpretation change

If Codex cannot name which layer is changing, Codex should not implement yet.

Examples from this project:

- `Free` / `Locked` are combat states
- `StrategyBased`, `MouseDoubleClick`, `MouseFollow` are acquisition modes
- `Nearest`, `Farthest`, `LowestHealth` are strategies
- `Tab`, double-click, and temporary debug hotkeys are input entries

Codex should also avoid these targeting-specific anti-patterns:

- implementing two lock-entry methods directly in the input adapter before introducing a configurable acquisition mode
- treating "mouse follow" as only an input trick instead of a distinct acquisition/aim behavior
- treating "nearest" as a universal strategy without first deciding whether it is snapshot-based or continuously refreshed
- adding a new mode by only wiring a new key without first deciding whether the user should configure it as a default mode

## Additional Rule Learned From Inventory Iterations

Inventory, container UI, drag/drop, quick-transfer, window closing rules, and similar interaction-heavy systems must not be treated as "safe to refactor continuously" while behavior is still moving.

This project now treats the following workflow as incorrect:

1. implement an interaction feature
2. immediately start structural refactors before the interaction rules stabilize
3. rely on compile success as the main validation signal
4. discover at runtime that wiring, input routing, or UI behavior regressed
5. rework both behavior and structure in the same loop

For interaction-heavy systems, Codex must instead separate work into two phases:

1. behavior stabilization
2. structural cleanup

### Behavior Stabilization Rule

Before doing medium or large refactors in an interaction-heavy system, Codex must first treat the following as behavior that should be frozen for at least the next short iteration:

- how a panel opens
- how a panel closes
- whether one or multiple windows are visible
- drag behavior
- double-click behavior
- quick-transfer behavior
- input blocking rules
- selection/highlight behavior

If these are still changing, Codex should prefer:

- small local fixes
- explicit bug fixes
- minimal adapters

Codex should avoid:

- renaming and redistributing responsibilities across many files at once
- changing both interaction rules and structure in the same step unless absolutely necessary

### Interaction Verification Rule

For UI and input work, compile success is necessary but not sufficient.

Codex must treat these as separate validation layers:

1. compile correctness
2. runtime interaction correctness

When changing drag/drop, double-click, hover, focus, close buttons, or gameplay input blocking, Codex must explicitly reason about runtime verification, not only build verification.

At minimum, Codex should maintain a concrete manual verification checklist for the changed flow, for example:

- open container
- close player inventory
- close external container
- drag item between windows
- drag item inside the same window
- quick-transfer item with double-click
- confirm gameplay input is blocked while UI is open

If Codex cannot run the scene directly, Codex must say that runtime interaction is not yet confirmed, and should not describe the behavior as fully validated.

### Assumption Control Rule For Referenced Games

When the user says "like Delta Force", "like Tarkov", "like Diablo", or references another game's interaction model, Codex must not invent the exact intended behavior unless it is already explicit in the user's wording.

Codex should only implement what is directly stated.

Examples:

- "double-click items to transfer" does not mean "double-click containers to open"
- "like Delta Force inventory" does not automatically authorize split stacks, sorting rules, equipment slots, or transfer priorities unless the user asked for them

If the user references a known game for feel or precedent, Codex should interpret that reference narrowly unless the behavior details are already locked.

### Single-Axis Change Rule

When a system is still stabilizing, Codex should change only one axis per iteration whenever possible.

Examples of different axes:

- close/open rules
- drag/drop behavior
- double-click transfer behavior
- gameplay input blocking
- structure refactor

Codex should avoid changing multiple axes at once, because when regressions appear it becomes much harder to identify whether the cause is:

- changed behavior
- changed architecture
- broken wiring
- misunderstood requirement

### Structural Cleanup Trigger

For interaction-heavy systems, Codex should only start medium or large structural cleanup when at least one of these is true:

- the user explicitly asks for structure cleanup
- the interaction rules appear stable for the next short iteration
- the current structure is actively blocking a bug fix or feature

If none of these are true, Codex should defer structural cleanup and continue with the smallest safe change.

## Required Collaboration Behavior

For any new system request, Codex should first give:

1. System ownership
2. Core responsibilities
3. Future extension points
4. Coupling risks
5. Minimal structure that preserves long-term flexibility

Only after that should Codex implement.

If the user wants implementation immediately, Codex should still choose the implementation shape that matches the designed ownership model, not the quickest local patch.

## Anti-Patterns To Avoid

Codex should avoid these unless the user explicitly asks for a disposable prototype:

- Putting shared gameplay systems directly into `Player`
- Letting one gameplay class both read input and own reusable gameplay state
- Encoding future mode switches with ad-hoc booleans in unrelated classes
- Expanding behavior by adding more conditionals into executors instead of extracting a controller/state layer
- Using a working prototype as justification for bad ownership boundaries
- Refactoring an interaction-heavy system before its open/close/drag/double-click/input-blocking rules are stable
- Treating build success as proof that UI or input behavior is correct
- Inferring unstated interaction behavior from a reference game and implementing it without confirmation

## Expected Workflow In Future Conversations

When the user says "implement a new system", Codex should respond in this order:

1. State understanding of the requested system.
2. State whether it should be actor-local or shared infrastructure.
3. Propose the minimal complete architecture.
4. Ask for confirmation only if there are materially different structural options.
5. Implement according to that architecture.

If there is only one clearly safer structure, Codex should choose it directly and explain the assumption.

When the user asks to "test whether the structure is good", Codex should not immediately solve the concrete feature request.
Codex should first pressure-test the structure from at least two angles:

1. player-facing usage and confusion risk
2. developer-facing add/remove/change cost for the next likely iteration

Only after that should Codex implement the smallest version that still respects the tested structure.

## Short Version

For this project:

- architecture first
- ownership first
- extensibility first
- low coupling first
- minimal implementation second
- for UI/input systems: freeze behavior before refactoring
- for UI/input systems: verify runtime behavior, not only builds
- do not guess interaction details from genre or reference-game shorthand

Do not default to "make it work in the nearest class first".
