# Structural Cleanup Plan 2026-04-24

## Purpose

This document summarizes the current structural problems and dirty areas in the project.

It is not a "rewrite everything" instruction.
It defines:

- what is actually healthy right now
- what is currently dirty or fragile
- which problems are only prototype roughness
- which problems are real structural debt
- the safest cleanup order

The project is already on a better path than a typical prototype.
The main issue is no longer "missing architecture".
The main issue is that the current architecture direction is only partially enforced in code.

## Current High-Level Assessment

### Structurally Healthy

- combat runtime is meaningfully split from static skill definitions
- player and dummy now share the same broad combat runtime shape
- inventory/container rules mostly live in runtime services instead of UI widgets
- equipment, items, and inventory are separated conceptually
- UI extraction into `scenes/ui/game_ui_root.tscn` is the correct direction
- build health is currently good:
  - `dotnet build BattleHarvesterStudy.csproj`
  - `0 warning`
  - `0 error`

### Structurally Fragile

- multiple systems still depend on fixed scene tree shape instead of explicit wiring
- UI controllers still depend heavily on hardcoded node paths
- actor shells still duplicate runtime assembly logic
- some prototype text/content roughness has leaked into runtime-visible behavior

## Problem Classification

The current project problems fall into four different types.

### 1. Runtime-visible defects

These are bugs or dirty outputs the player can directly see.

Current confirmed example:

- `scripts/attributes/DeathStateController.cs`
  - default `DeathLabelText` is already mojibake

These should be fixed quickly because they reduce confidence in the whole runtime.

### 2. Structural fragility

These are systems that work now, but are too dependent on one specific node layout or scene composition.

Current examples:

- `scripts/locomotion/Mover.cs`
- `scripts/attributes/HealthComponent.cs`
- `scripts/combat/cooldowns/ActorSkillCooldownController.cs`
- multiple presentation scripts that rely on exact `GameUiRoot/...` paths

These are not necessarily broken today.
They are dangerous because future scene cleanup can silently break them.

### 3. Responsibility concentration

These are classes that currently own too many concerns.

Current strongest example:

- `scripts/presentation/InventoryInteractionController.cs`

It currently mixes:

- interaction state
- drop target resolution
- drag/drop commit flow
- item use entry
- equipment transfer flow
- status message emission
- multiple view lookups

This is still workable in prototype scope, but it is already large enough that new feature work will become expensive.

### 4. Incomplete architecture enforcement

These are places where the project rules are good, but the code does not fully enforce them yet.

Current examples:

- UI is conceptually panelized, but many presenters still find controls through long absolute paths
- actor runtime is conceptually shared, but `Player` and `Dummy` still duplicate setup logic
- components are conceptually reusable, but some still assume one hardcoded scene skeleton

This is the main "dirty area" category.
It is not chaos.
It is unfinished consolidation.

## Main Dirty Areas

### Dirty Area A: Runtime text / content leakage

Representative file:

- `scripts/attributes/DeathStateController.cs`

Current issue:

- runtime-facing exported default string is corrupted

Why it matters:

- this is user-visible dirt, not just internal debt
- it also confirms that text cleanup is not fully bounded yet

Target state:

- all exported default player-facing strings must be valid UTF-8 and intentional
- gameplay runtime should avoid carrying ad-hoc final display strings where a text key is more appropriate

### Dirty Area B: Scene-tree-coupled components

Representative files:

- `scripts/locomotion/Mover.cs`
- `scripts/attributes/HealthComponent.cs`
- `scripts/locomotion/MovementHost.cs`
- `scripts/combat/core/AttackExecutor.cs`

Current issue:

- components use relative parent walks or fixed sibling names to find required collaborators

Why it matters:

- scene refactors become risky
- reuse across actors or future wrappers becomes harder
- errors tend to appear at runtime, not compile time

Target state:

- runtime components should resolve dependencies through one of these patterns:
  - exported `NodePath`
  - owner-root lookup with explicit required component names
  - narrow initialization/injection from a parent host

Preferred rule:

- no new reusable runtime component should depend on `GetParent().GetParent()` style traversal

### Dirty Area C: UI path fragility

Representative files:

- `scripts/presentation/InventoryInteractionController.cs`
- `scripts/presentation/CombatHudPresenter.cs`
- `scripts/presentation/PlayerInventoryWindowPresenter.cs`
- `scripts/presentation/ContainerWindowPresenter.cs`
- `scripts/presentation/ItemScreenCoordinator.cs`

Current issue:

- presenters/controllers depend on exact deep UI tree paths

Why it matters:

- UI cleanup is expensive because wiring is distributed across many scripts
- visual layout iteration can accidentally become logic regression
- extracted UI scene still behaves like a fragile hardcoded tree

Target state:

- each presenter/controller should depend on explicit panel/view references
- `GameUiRoot` structure should be allowed to evolve without rewriting controller logic

Important note:

- this is not a signal to start a giant UI rewrite immediately
- interaction behavior should remain stable while wiring is improved in small safe slices

### Dirty Area D: Actor shell duplication

Representative files:

- `scripts/actors/Player.cs`
- `scripts/actors/Dummy.cs`

Current issue:

- both classes assemble overlapping runtime dependencies
- movement/facing/forced movement support is largely duplicated

Why it matters:

- shared runtime is already the project direction
- duplicated actor glue means future changes will fork subtly

Target state:

- shared actor capability wiring should live in a reusable base or assembly helper
- actor-specific differences should remain small:
  - player input behavior
  - dummy AI/counter logic
  - visual differences

### Dirty Area E: Overgrown interaction controller

Representative file:

- `scripts/presentation/InventoryInteractionController.cs`

Current issue:

- one controller currently owns too much of the item-screen interaction stack

Why it matters:

- this increases regression risk for drag/drop
- it makes testing and future feature insertion harder
- it blurs the line between interaction state and panel wiring

Target state:

- retain one shared interaction authority for item-like screens
- split supporting responsibilities around it instead of replacing it with feature-specific controllers

This means:

- keep one item interaction model
- reduce class size and wiring burden
- do not fork behavior into separate one-off inventory/equipment/container controllers

## What Is Not Actually A Structural Problem

These should not trigger premature heavy refactors.

### Prototype-grade content

- limited item breadth
- rough equipment content coverage
- test scene focus
- rough quick-item polish

These are content or UX maturity issues, not architecture failures.

### Large git diff during migration

The worktree is currently dirty because a broad migration is in progress.
That alone is not a structural flaw.

The important question is whether old paths are still referenced.
Current inspection suggests the old `scripts/entities` and `Systems/...` references are largely cleaned up.

### Shared runtime still using node lookups

Not all node lookups are bad.

The real problem is:

- deep fragile hierarchy assumptions
- hidden required dependencies
- broad path scattering across many files

Simple owner-root lookup with clear expectations is still acceptable in this project.

## Rewrite Strategy

The safest rewrite is not "replace systems".
It is "finish enforcing the architecture already chosen".

## Rewrite Principles

1. Fix visible dirt before invisible structure debt.
2. Reduce path fragility before adding more UI complexity.
3. Remove scene hierarchy assumptions before expanding actor reuse.
4. Shrink large controllers by extracting helpers, not by splitting behavior semantics.
5. Do not change interaction behavior and UI structure in the same large step.

## Recommended Cleanup Order

### Phase 1: Runtime-visible dirt cleanup

Scope:

- fix mojibake/default text leakage
- inspect nearby exported display strings for the same issue

Primary target:

- `scripts/attributes/DeathStateController.cs`

Goal:

- no player-visible dirty default text in current demo scene

### Phase 2: Component dependency cleanup

Scope:

- replace the most fragile scene traversal patterns

Primary targets:

- `scripts/locomotion/Mover.cs`
- `scripts/attributes/HealthComponent.cs`

Preferred approach:

- export explicit paths where appropriate
- otherwise resolve via owner-root conventions with fail-fast checks

Goal:

- components should depend on intentional wiring, not incidental hierarchy depth

### Phase 3: UI wiring cleanup without changing interaction semantics

Scope:

- remove deep hardcoded path dependence from item-screen scripts
- keep current drag/drop, quick-transfer, and use-item behavior unchanged

Primary targets:

- `scripts/presentation/InventoryInteractionController.cs`
- `scripts/presentation/ItemScreenCoordinator.cs`
- `scripts/presentation/CombatHudPresenter.cs`

Preferred approach:

- introduce explicit panel/view references
- allow controllers to depend on local exported references instead of long owner-root paths
- keep one interaction controller for the item screen for now

Goal:

- UI layout changes should stop being dangerous by default

### Phase 4: Actor shell consolidation

Scope:

- reduce duplicated runtime setup across actor classes

Primary targets:

- `scripts/actors/Player.cs`
- `scripts/actors/Dummy.cs`

Preferred approach:

- extract a shared actor runtime host or base capability resolver
- keep actor-specific behavior local

Goal:

- future runtime changes should not require parallel manual edits in both actor types

### Phase 5: Interaction-controller decomposition

Scope:

- only after interaction behavior is stable

Primary target:

- `scripts/presentation/InventoryInteractionController.cs`

Preferred decomposition direction:

- keep session ownership in the screen/session controller
- keep interaction state ownership centralized
- extract service/helper responsibilities such as:
  - drop target resolution
  - transfer action orchestration
  - status message formatting adapter
  - item use bridge

Goal:

- smaller controller surface without inventing a second interaction architecture

## Explicit Non-Goals For This Cleanup

This plan does not recommend:

- rewriting combat architecture
- replacing the inventory runtime
- changing item interaction semantics now
- deleting the current UI extraction work
- rebuilding the project around a full dependency injection framework
- performing a giant all-at-once "clean architecture" rewrite

## Concrete Rewrite Proposal

If we execute this cleanup in the next iterations, the safest concrete plan is:

1. Apply the small correctness fix:
   - death label default text
2. Refactor the first fragile shared runtime component:
   - `Mover`
3. Refactor the second fragile shared runtime component:
   - `HealthComponent`
4. Introduce explicit UI panel references for the item screen wiring:
   - without changing player-facing behavior
5. Consolidate `Player` and `Dummy` runtime assembly
6. Only then decide whether `InventoryInteractionController` still needs internal extraction

## Review Standard For Future Structural Work

Any future cleanup proposal should answer:

1. Is this fixing visible dirt, structural fragility, or only aesthetic code cleanup?
2. Does it preserve current interaction behavior?
3. Does it reduce path fragility or hierarchy assumptions?
4. Does it reduce duplicate actor wiring?
5. Does it preserve the existing design rules in:
   - `docs/architecture/ui_infrastructure_spec.md`
   - `docs/architecture/ui_architecture_guidelines.md`
   - `docs/architecture/codex_collaboration_rules.md`

If the answer is mostly "no", the cleanup is probably premature.

## Current Recommendation

The project should not do one giant rewrite.

The correct next move is:

- first clean visible dirt
- then clean fragile shared runtime dependencies
- then clean UI wiring
- then consolidate actor shells

That sequence reduces risk while moving the codebase closer to the architecture it already wants.
