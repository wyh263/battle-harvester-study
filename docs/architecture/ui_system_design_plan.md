# UI System Design Plan

## Purpose

This document prepares the next stage of UI work for the project.
It translates the existing UI guidelines into a concrete, project-specific design plan.

It is meant to answer:

1. what UI structure the current project already supports
2. what shared UI infrastructure is still missing
3. how to expand from the current inventory HUD into a reusable full UI system

## Current Read Of The Project

The current project already contains the beginning of a healthy UI architecture.
The strongest direction is visible in the inventory flow:

- `InventoryUiController`
  - owns screen/session state
- `InventoryInteractionController`
  - owns interaction state for item grids
- `ItemScreenCoordinator`
  - composes the current inventory screen
- `PlayerInventoryWindowPresenter`
  - renders the player inventory panel
- `ContainerWindowPresenter`
  - renders the external container panel
- `GameplayInputGate`
  - centralizes gameplay blocking intent

This matches the direction described in `ui_architecture_guidelines.md`.

At the same time, the project still has two structural limitations:

- UI is still attached directly under `Player/HUD` inside `scenes/player.tscn`
- combat HUD is still a monolithic presenter in `CombatHudPresenter`

This means the project is ready for a proper UI system design pass, but not yet ready for large feature expansion by copying the current combat HUD pattern.

## Practical Interpretation

The project should treat UI as a small framework with three parallel goals:

1. support today's combat + inventory interface
2. support future item-driven screens like equipment, loot, shop, and crafting
3. keep gameplay rules separate from presentation and text formatting

The current inventory implementation is the seed of that framework.
The next design step should generalize it without doing a large speculative rewrite.

## Recommended UI Structure

### 1. Root Layer

Introduce a dedicated UI root scene or root node that is treated as application-level interface infrastructure instead of player-owned feature content.

Target shape:

- `GameUiRoot`
  - HUD layer
  - screen layer
  - overlay layer
  - tooltip / drag hint layer
  - notification layer

Reason:

- combat HUD and item screens are different UI concerns
- future pause/settings/map screens should not live as ad-hoc children of the player scene
- UI lifetime should be easier to reason about than "whatever is currently under player HUD"

Short-term note:

- this can still be instantiated from the main scene while continuing to bind to player-owned gameplay components
- the important design change is ownership, not immediate heavy refactor

### 2. Screen Layer

Use `Screen` as the unit of full UI session ownership.

Planned screen categories:

- `HudScreen`
  - always-on gameplay HUD
- `ItemScreen`
  - inventory / container / equipment / shop / crafting composition host
- `MenuScreen`
  - pause / settings / save
- `OverlayScreen`
  - modal prompts, tutorials, confirmations

Rules:

- a screen owns session state
- a screen decides whether gameplay input is blocked
- a screen does not own item transfer rules or combat rules

### 3. Panel Layer

Panels should become the reusable building blocks for every larger screen.

Recommended panel vocabulary for this project:

- `VitalsPanel`
  - player health / resource summary
- `TargetPanel`
  - locked target info
- `SkillBarPanel`
  - combat skill slots and cooldown display
- `GridContainerPanel`
  - generic grid-based storage panel
- `EquipmentPanel`
  - gear slots
- `ItemDetailsPanel`
  - current selection details
- `TransactionPanel`
  - buy/sell/crafting summary
- `StatusPanel`
  - short help text, state text, control hints

The immediate value is that inventory stops meaning only:

- player grid on the left
- container grid on the right

Instead, `ItemScreen` becomes a composition host that can assemble several panels for different use cases.

### 4. Widget Layer

Low-level reusable widgets should stay small and rule-light.

Examples already moving in the right direction:

- `InventoryGridView`
- `DraggableWindow`
- drag hint presenter

Additional widgets likely needed later:

- slot widget for equipment
- tooltip widget
- stat row widget
- notification entry widget

## State Model

The project should formalize four different state domains.

### Session State

Owned by a screen controller.

Examples:

- is item screen open
- which external container is active
- which panel set is visible
- whether the current screen is modal

Current mapping:

- `InventoryUiController` already plays this role for inventory

### Interaction State

Owned by an interaction controller.

Examples:

- focused panel
- selected cell
- hovered item
- drag source
- preview rotation

Current mapping:

- `InventoryInteractionController` already owns this for grid interactions

Design rule:

- keep this generic for shared item interaction
- do not push feature-specific shop, crafting, or equipment business rules directly into it

### Presentation State

Owned by presenters and panel view models.

Examples:

- header text
- summary text
- visible warnings
- preview rectangles
- slot highlight state

Design rule:

- presenters render already-decided data
- presenters should not calculate gameplay legality

### Gameplay Rule State

Owned by gameplay systems.

Examples:

- item placement validity
- transfer success
- access restrictions
- skill cooldown runtime
- target health

Design rule:

- gameplay systems should return structured results
- presenters should not become the place where game rules are inferred

## Input Architecture

The project already has the right entry point with `GameplayInputGate`.
That should become the shared boundary for all gameplay-facing UI.

Recommended expansion:

- `GameplayInputGate`
  - reports whether gameplay movement/combat/interaction are blocked
- screen controllers
  - declare their blocking intent
- gameplay input adapters
  - consult the gate rather than individual feature controllers

Future-ready blocking dimensions:

- movement blocked
- combat input blocked
- targeting blocked
- world interaction blocked
- camera input blocked

For now, these can remain collapsed into one effective block if that keeps implementation small.
The design should still anticipate future separation.

## Text And Localization

This is the most important missing infrastructure gap.

Current risk:

- user-facing strings are embedded in presenters and controllers
- several strings in current files show encoding problems
- combat HUD and inventory text are formatted directly in code

Before UI expands further, the project should add a minimal text pipeline.

Recommended target:

- `UiTextService`
  - resolves `text_key + arguments`
- `UiTextKey` or plain string keys
  - shared identifiers for display text
- presenters
  - request resolved text instead of building final sentences

Minimum first step:

- centralize status text and panel summary text
- define keys for inventory, container, and combat HUD labels

This should be considered foundational work, not polish.

## Combat HUD Direction

`CombatHudPresenter` should not be copied as the pattern for future UI.

Recommended split:

- `VitalsPanelPresenter`
  - player health/resource
- `TargetPanelPresenter`
  - target name/health
- `SkillBarPresenter`
  - skill slot display and cooldown text

Benefits:

- aligns combat HUD with the same screen -> panel -> widget discipline used by inventory
- reduces the size of one class with many node-path dependencies
- makes localization migration much easier

This does not need to be a full visual redesign yet.
It is mainly an ownership and composition cleanup.

## Item Screen Direction

The most important medium-term target is a reusable `ItemScreen`.

Target idea:

- one screen session controller
- one item interaction controller
- several interchangeable item panels

Example compositions:

- inventory only
  - player grid + status panel
- inventory + container
  - player grid + container grid + status panel
- equipment + inventory
  - equipment panel + player grid + item details
- shop
  - merchant grid + player grid + transaction panel + item details
- crafting
  - recipe list + ingredient grid + result panel + item details

This should be the main UI system expansion path for the project.

## Scene Organization Recommendation

Current state:

- player scene contains gameplay nodes and all HUD/window nodes together

Recommended direction:

- move reusable UI scene content into dedicated UI scenes
- instantiate those scenes from a root UI layer
- let presenters bind to gameplay nodes through exported node paths, owner references, or setup methods

Suggested intermediate structure:

- `scenes/ui/game_ui_root.tscn`
- `scenes/ui/hud/combat_hud.tscn`
- `scenes/ui/screens/item_screen.tscn`
- `scenes/ui/panels/grid_container_panel.tscn`

This can be done gradually.
The first win is separating scene ownership, not perfecting every file layout immediately.

## Implementation Priorities

### Phase 1: Stabilize Foundations

Goals:

- keep current inventory flow working
- avoid large behavior rewrites
- prepare shared UI infrastructure

Tasks:

- define UI root ownership strategy
- define panel vocabulary
- define text/localization service boundary
- keep `InventoryInteractionController` generic

### Phase 2: Generalize Item Screen Composition

Goals:

- stop hardcoding inventory as exactly two windows

Tasks:

- continue evolving `ItemScreenCoordinator` as the shared item screen coordinator
- make grid panel presenters reusable by composition
- add optional details/status panel support

### Phase 3: Refactor Combat HUD Into Panels

Goals:

- align combat HUD with the same architecture as the newer inventory UI

Tasks:

- split `CombatHudPresenter`
- isolate vitals/target/skill bar panels
- move hardcoded strings behind the text layer

### Phase 4: Expand New Screens On Shared Infrastructure

Targets:

- equipment
- shop
- crafting
- map or progression overlays

Rule:

- new features should plug into the shared screen/panel/widget system
- they should not introduce their own one-off UI architecture

## Suggested Near-Term Decisions

Before we implement the next UI feature, it would be healthy to lock these decisions:

1. whether `HUD` remains under player temporarily or a new `GameUiRoot` is created now
2. what the first generic item-screen panel set will be
3. what the first minimal localization/text API looks like
4. whether combat HUD refactor happens before or after equipment UI

## Recommended Default Answers

If no stronger constraint appears, the safest next path is:

1. keep current runtime behavior
2. add a `GameUiRoot` scene as the new ownership target
3. generalize inventory into `ItemScreen` composition before building equipment/shop/crafting UI
4. add a minimal text service before expanding user-facing strings further
5. refactor combat HUD after item-screen composition is stable

This path keeps current progress, reduces rewrite risk, and prepares the project for multi-panel UI growth.

## Short Version

The project is already moving in the right direction.
Inventory UI provides the correct architectural seed.

The next UI system design should focus on:

- moving toward a dedicated UI root
- formalizing screen/panel/widget composition
- keeping interaction generic and reusable
- introducing a real text/localization layer
- refactoring combat HUD into smaller panels instead of copying its current monolithic shape

If we follow this path, the project can build inventory, equipment, container, shop, and crafting UI as variations of one system rather than separate feature-specific interfaces.
