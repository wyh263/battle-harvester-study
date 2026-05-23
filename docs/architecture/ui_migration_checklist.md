# UI Migration Checklist

## Purpose

This document maps the current codebase to the UI rules defined in [ui_architecture_guidelines.md](E:\gamekaifa\battle-harvester-study\docs\architecture\ui_architecture_guidelines.md).

It answers three practical questions:

1. which existing UI modules are already healthy enough to keep extending
2. which modules are acceptable temporary bridges
3. which modules must be migrated before larger systems like equipment, shop, quest, or settings UI are built on top

## Status Legend

- `Healthy`
  - aligned with the current architecture direction
  - safe to extend in the near term
- `Transitional`
  - acceptable for now
  - should not become the long-term pattern for all future UI
- `Migrate Before Expansion`
  - still works
  - but likely to cause repeated rework if new systems are built directly on top

## Current Inventory UI Mapping

### Screen / Session Ownership

- [`InventoryUiController.cs`](E:\gamekaifa\battle-harvester-study\scripts\presentation\InventoryUiController.cs)
  - Role: inventory screen/session ownership
  - Status: `Healthy`
  - Why:
    - owns open/close rules
    - owns active external container
    - already separates session state from rendering
  - Future note:
    - can evolve into a more general `ScreenSessionController` pattern later

### Input Boundary

- [`GameplayInputGate.cs`](E:\gamekaifa\battle-harvester-study\scripts\presentation\GameplayInputGate.cs)
  - Role: UI/gameplay blocking boundary
  - Status: `Healthy`
  - Why:
    - centralizes input-block intent
    - reduces scattered `BlocksGameplayInput` lookups
  - Future note:
    - should become the standard access point for any modal gameplay-facing screen

### Interaction State

- [`InventoryInteractionController.cs`](E:\gamekaifa\battle-harvester-study\scripts\presentation\InventoryInteractionController.cs)
  - Role: focus, selection, drag, double-click transfer, preview, transfer submission
  - Status: `Transitional`
  - Why:
    - correct ownership direction
    - but still large and feature-dense
  - Risk:
    - if equipment, shop, and crafting behavior are added directly here, it will become another monolith
  - Rule:
    - extend only if the new behavior is still generic item interaction
    - do not put merchant pricing, crafting recipe logic, or equipment-slot-specific rules directly here

### Screen Composition

- [`ItemScreenCoordinator.cs`](E:\gamekaifa\battle-harvester-study\scripts\presentation\ItemScreenCoordinator.cs)
  - Role: inventory screen composition and signal wiring
  - Status: `Healthy`
  - Why:
    - composition ownership is already centralized here
    - the project no longer needs a separate inventory-only coordinator shell
  - Rule:
    - continue extending this as the shared item screen coordinator
    - avoid reintroducing inventory-only wrapper coordinators

### Panel Rendering

- [`PlayerInventoryWindowPresenter.cs`](E:\gamekaifa\battle-harvester-study\scripts\presentation\PlayerInventoryWindowPresenter.cs)
  - Role: player inventory panel rendering
  - Status: `Healthy`
  - Why:
    - panel responsibility is clear
    - only renders one bounded region
  - Future note:
    - later may be generalized into a reusable grid container panel presenter

- [`ContainerWindowPresenter.cs`](E:\gamekaifa\battle-harvester-study\scripts\presentation\ContainerWindowPresenter.cs)
  - Role: external container panel rendering
  - Status: `Healthy`
  - Why:
    - mirrors current panel responsibilities well
  - Future note:
    - likely reusable for stash/shop stock/container-like views with small adaptations

### Small Presenters

- [`InventoryStatusPresenter.cs`](E:\gamekaifa\battle-harvester-study\scripts\presentation\InventoryStatusPresenter.cs)
  - Status: `Healthy`

- [`InventoryDragHintPresenter.cs`](E:\gamekaifa\battle-harvester-study\scripts\presentation\InventoryDragHintPresenter.cs)
  - Status: `Healthy`

- [`DraggableWindow.cs`](E:\gamekaifa\battle-harvester-study\scripts\presentation\DraggableWindow.cs)
  - Status: `Healthy`
  - Caveat:
    - still needs runtime attention whenever title bar structure changes

### Widget Layer

- [`InventoryGridView.cs`](E:\gamekaifa\battle-harvester-study\scripts\presentation\InventoryGridView.cs)
  - Role: grid widget/view
  - Status: `Healthy`
  - Why:
    - view responsibilities are mostly contained
    - interaction is surfaced through signals instead of embedding gameplay rules
  - Future note:
    - likely reusable for stash, shop stock, loot grid, and backpack views

### Transfer Rules

- [`InventoryTransferService.cs`](E:\gamekaifa\battle-harvester-study\scripts\inventory\InventoryTransferService.cs)
  - Role: shared transfer rule layer
  - Status: `Healthy`
  - Why:
    - begins consolidating quick transfer, targeted transfer, and same-container reposition logic

- [`GridContainerTransferService.cs`](E:\gamekaifa\battle-harvester-study\scripts\inventory\GridContainerTransferService.cs)
  - Status: `Transitional`
  - Why:
    - now mostly delegates
  - Recommendation:
    - keep for compatibility for now
    - avoid introducing new rule branches here

### Inventory Runtime

- [`GridContainerComponent.cs`](E:\gamekaifa\battle-harvester-study\scripts\inventory\GridContainerComponent.cs)
  - Status: `Healthy`
  - Why:
    - large, but still concentrated around container rules
    - not currently a UI ownership problem
  - Caveat:
    - do not let UI/presentation logic creep back into this class

- [`InventoryComponent.cs`](E:\gamekaifa\battle-harvester-study\scripts\inventory\InventoryComponent.cs)
  - Status: `Healthy`
  - Caveat:
    - UI still mostly consumes `GetPrimaryContainer()`
    - when multi-container inventory pages arrive, usage patterns must expand before the data model is considered fully realized

## Current Non-Inventory UI Mapping

### Combat HUD

- [`CombatHudPresenter.cs`](E:\gamekaifa\battle-harvester-study\scripts\presentation\CombatHudPresenter.cs)
  - Status: `Migrate Before Expansion`
  - Why:
    - still behaves like an older monolithic presenter
    - directly owns many HUD labels, bars, and slot labels
    - hardcoded strings and direct formatting are still common
  - Meaning:
    - it is acceptable for current combat HUD
    - but should not become the pattern for future map/quest/settings HUD work
  - Before larger HUD growth:
    - split into smaller HUD panels or presenters
    - prepare it for localization/text-key usage

### Targeting and Other Presenters

- [`TargetLockIndicatorPresenter.cs`](E:\gamekaifa\battle-harvester-study\scripts\presentation\TargetLockIndicatorPresenter.cs)
- [`SkillPhaseLabelPresenter.cs`](E:\gamekaifa\battle-harvester-study\scripts\presentation\SkillPhaseLabelPresenter.cs)
- [`FacingLabelController.cs`](E:\gamekaifa\battle-harvester-study\scripts\presentation\FacingLabelController.cs)
  - Status: `Healthy`
  - Why:
    - each is still narrow in scope
    - each is close to a single-purpose presenter/controller

## Localization Readiness Mapping

### Already Compatible With Future Localization Direction

- screen/session controller split
- panel presenter split
- small status/drag-hint presenters
- gameplay-rule vs presentation separation trend

### Still Needing Migration Before "Easy Language Switching" Is True

- [`InventoryUiController.cs`](E:\gamekaifa\battle-harvester-study\scripts\presentation\InventoryUiController.cs)
- [`InventoryInteractionController.cs`](E:\gamekaifa\battle-harvester-study\scripts\presentation\InventoryInteractionController.cs)
- [`PlayerInventoryWindowPresenter.cs`](E:\gamekaifa\battle-harvester-study\scripts\presentation\PlayerInventoryWindowPresenter.cs)
- [`ContainerWindowPresenter.cs`](E:\gamekaifa\battle-harvester-study\scripts\presentation\ContainerWindowPresenter.cs)
- [`CombatHudPresenter.cs`](E:\gamekaifa\battle-harvester-study\scripts\presentation\CombatHudPresenter.cs)

Reason:

- final user-facing strings are still embedded directly in code
- text templates are not yet centralized as keys + arguments
- runtime text refresh on language switch is not yet formalized

This means:

- structure is improving
- localization infrastructure is not finished yet

## Safe Expansion Rules Right Now

The project can safely continue implementing:

- equipment runtime rules
- backpack size switching
- new item interactions
- more container types

As long as:

- item transfer behavior continues to reuse `InventoryTransferService`
- modal UI input continues to route through `GameplayInputGate`
- new inventory-like screens follow the existing controller/presenter split

## Expansion That Should Wait For A Small Migration Step

Before building these directly on top of current inventory HUD composition, a small migration step is recommended:

- full equipment + inventory + container combined screen
- shop screen with transaction summary and pricing panels
- crafting screen with recipe list + ingredient container + result panel

Reason:

- these are no longer just "player window + container window"
- they need a more general `screen composed from multiple panels` pattern

## Next Recommended Migration Steps

### Step 1

Introduce a more general panel vocabulary for item-driven screens:

- grid container panel
- equipment panel
- item details panel
- transaction/summary panel

### Step 2

Evolve inventory-specific screen coordination into a more general item screen coordinator model.

### Step 3

Introduce a unified localization/text resolver before UI text multiplies further.

### Step 4

Refactor `CombatHudPresenter` so combat HUD follows the same architectural discipline as newer inventory UI.

## Short Version

Current project state:

- inventory UI direction: healthy
- inventory interaction controller: acceptable but should be watched
- combat HUD presenter: still old-style and should not be copied
- transfer rules: now much healthier than before
- localization readiness: structure is partially ready, text pipeline is not ready yet

This means the project is in a good state to continue gameplay work, but future UI expansion should follow the documented migration path instead of inventing feature-local UI structures.
