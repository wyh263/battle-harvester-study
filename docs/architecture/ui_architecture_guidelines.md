# UI Architecture Guidelines

## Purpose

This document defines UI architecture rules for the whole project, not only inventory.

It exists to prevent a repeated failure pattern:

1. a feature-specific UI is implemented quickly
2. the feature works locally
3. more systems need similar UI behavior
4. text, input routing, state ownership, and screen composition all diverge
5. large rework is needed when inventory, equipment, shop, quest, map, or settings UI begin to overlap

This project now treats UI as shared game infrastructure.
It should not be designed screen-by-screen in isolation.

## Scope

These rules apply to all gameplay-facing UI, including:

- HUD
- inventory
- equipment
- loot containers
- shops
- crafting / dismantling
- map / quest / progression
- settings / pause / save screens
- system prompts and notifications

## Core Principle

UI must be designed as a reusable framework with shared rules for:

- state ownership
- screen composition
- text rendering
- input blocking
- interaction behavior

Feature work should plug into this framework.
Feature work should not create a separate UI architecture for each subsystem.

## UI Layers

All UI should be reasoned about in these layers:

### 1. Screen

A `Screen` is a complete UI session or mode.

Examples:

- inventory screen
- shop screen
- crafting screen
- map screen
- settings screen

A screen decides:

- which panels are present
- whether the screen is modal
- whether gameplay input is blocked
- which session data it is currently bound to

### 2. Panel

A `Panel` is a reusable area inside a screen.

Examples:

- player inventory panel
- external container panel
- equipment slots panel
- item details panel
- transaction summary panel

A panel should only manage:

- rendering one bounded slice of UI
- binding view controls to already-computed state
- refreshing when its bound state changes

A panel should not own whole-screen open/close rules.

### 3. Widget / View

A `Widget` or `View` is the lowest display unit.

Examples:

- grid view
- equipment slot view
- tooltip view
- status label
- drag hint

Widgets should not contain business rules.

## State Ownership

UI systems must separate these state types:

### UI Session State

This describes what overall UI is currently active.

Examples:

- which screen is open
- which external container is active
- whether the screen is modal
- which panels are visible

This state should live in a screen/session controller, not in a panel.

### Interaction State

This describes how the user is currently interacting with the UI.

Examples:

- focused panel
- selected item
- hovered cell
- drag state
- preview rotation

This state should live in an interaction controller, not in gameplay rules and not in the lowest view.

### Presentation State

This describes what is currently shown on screen.

Examples:

- current header text
- summary content
- preview rectangles
- status labels
- tooltip content

Panels and presenters should render presentation state.
They should not invent gameplay rules.

### Gameplay Rule State

This is the underlying system truth.

Examples:

- what item exists
- what container contains
- whether an access rule passes
- whether an item can be placed
- whether a transfer succeeded

Gameplay rule state must not be represented as hardcoded UI strings.

## Screen Composition Rule

No feature should assume that the current game will only need one fixed screen layout.

For example, "inventory" should not permanently mean:

- player panel on the left
- container panel on the right

Future screens may combine:

- player inventory
- equipment panel
- backpack panel
- external container panel
- merchant stock panel
- crafting recipe panel
- item details panel

Therefore screens must be built from panels, not from one special-purpose monolith.

## Item UI Rule

All item-driven screens must reuse the same item interaction model whenever possible.

Examples of shared behavior:

- drag and drop
- quick transfer
- double-click transfer
- stack merge
- targeted placement
- placement preview
- selection and focus
- failure feedback

Inventory, equipment, shop, stash, and crafting screens should feel like variants of one interaction system, not unrelated UIs.

If one screen needs special rules, the special rule should be expressed explicitly.
It should not silently fork the interaction model.

## Localization Rule

Localization is part of UI architecture, not a later content pass.

### Required Rules

- UI text must come from a unified localization/text layer
- dynamic text must use `text_key + arguments`
- gameplay and rule layers must not build final user-facing sentences directly
- open screens must be refreshable when language changes

### Forbidden Patterns

- hardcoding final display sentences in gameplay systems
- concatenating user-facing text directly in controllers
- treating one language's sentence order as the project-wide format

Examples:

Allowed:

- `inventory.status.transfer_success` with `{ item_name }`
- `inventory.summary.space_used` with `{ used }` and `{ total }`

Not allowed:

- `"MOVED " + itemName`
- `"SPACE " + used + "/" + total`

## Input Boundary Rule

UI input and gameplay input must be treated as separate domains.

The project must always be able to answer:

- is a screen modal
- does it block movement
- does it block skill input
- does it block targeting
- does it block world interaction
- can UI still receive drag/drop and double-click while gameplay is blocked

These answers must come from a shared input gate or context rule, not from ad-hoc checks spread across unrelated systems.

## Behavior Stabilization Rule

For interaction-heavy systems, Codex must not begin medium or large structural refactors while these are still changing:

- open / close rules
- focus rules
- drag rules
- double-click rules
- quick transfer rules
- gameplay input blocking rules
- panel visibility rules

When these are still changing, Codex should prefer:

- small behavioral fixes
- minimal adapters
- local bug fixes

Large structure cleanup should wait until behavior is stable enough for the next short iteration.

## Verification Rule

For UI work, build success is never sufficient by itself.

UI work must be verified at two separate levels:

1. compile correctness
2. runtime interaction correctness

Typical runtime checklist items:

- open screen
- close screen
- switch focus
- drag item
- double-click transfer
- block gameplay input
- refresh visible content
- ensure buttons still work

If runtime interaction is not directly confirmed, that must be stated clearly.

## New UI Checklist

Before implementing a new UI feature, Codex should answer:

1. Which screen does this belong to?
2. Which panels are required?
3. What session state does it need?
4. What interaction state does it need?
5. What text keys will it require?
6. Does it block gameplay input?
7. Does it reuse existing item interaction rules?
8. Does it require a new panel type, or only a new panel composition?

If Codex cannot answer these, Codex should design first and code second.

## Current Project Interpretation

Based on the current codebase, the project is moving toward this shape:

- `InventoryUiController`: screen/session ownership
- `InventoryInteractionController`: interaction ownership
- `ItemScreenCoordinator`: screen composition
- `PlayerInventoryWindowPresenter` and `ContainerWindowPresenter`: panel rendering
- `GameplayInputGate`: UI/gameplay boundary
- `InventoryTransferService`: shared transfer rule layer

This direction is considered healthy.
Future systems should extend this pattern instead of bypassing it.

## Short Version

For this project:

- design UI as shared infrastructure
- organize UI as `screen -> panel -> widget`
- separate session state, interaction state, presentation state, and gameplay rule state
- localize through unified text keys and arguments
- unify gameplay blocking through one input gate
- reuse item interaction rules across inventory-like screens
- freeze interaction behavior before large refactors
- verify runtime behavior, not only builds
