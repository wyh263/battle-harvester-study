# UI Infrastructure Spec

## Status

This document is the authoritative UI infrastructure specification for the project.
It guides implementation, review, and migration decisions.

Rules in this document use these meanings:

- `Must`
  required project-wide rule
- `May`
  allowed extension point
- `Must Not`
  forbidden pattern

## 1. Purpose And Scope

This specification applies to all player-facing UI in the project, including:

- combat HUD
- inventory
- containers
- equipment
- shop and transaction UI
- crafting UI
- map, quest, and progression UI
- settings, pause, and save UI
- prompts, overlays, notifications, and status messaging

This specification defines shared infrastructure, not one feature screen.

Any new UI feature that cannot be clearly mapped into this specification must pause implementation and update the specification first.

## 2. Design Direction

The project follows a hybrid UI model:

- combat presentation follows a fighting-game-style readability model
- item management follows an extraction-looter-style organization model

Practical meaning:

- combat UI prioritizes fast reading, low clutter, stable placement, and center-screen visibility
- item UI prioritizes comparison, transfer, sorting, inspection, and decision speed

All features must still use one shared infrastructure for:

- screen ownership
- panel composition
- input blocking
- interaction semantics
- text resolution
- presentation state

## 3. Information Architecture

All UI information belongs to one of these priority classes:

### Critical

Must represent urgent state that demands immediate attention.

Examples:

- death
- hard block
- forced confirmation
- severe warning

### Primary

Must represent information needed continuously during the current mode.

Examples:

- health
- resource
- locked target
- selected item
- active container

### Secondary

Useful for decision quality, but not constant attention.

Examples:

- item count
- capacity summary
- cooldown remainder
- state labels

### Contextual

Only appears for hover, selection, preview, comparison, or mode-specific inspection.

Examples:

- item details
- comparison panel
- placement preview
- extended target details

### Debug

Development-only or training-only information.

Examples:

- input history
- hitbox state
- internal validation text

### Rules

- `HudScreen` Must contain only `Critical`, `Primary`, and tightly-bounded `Secondary` information.
- `HudScreen` Must Not permanently display dense instructional text.
- `ItemScreen` May display `Primary`, `Secondary`, and `Contextual` information at high density.
- `Debug` information Must Not ship as a default player-facing layer.

## 4. Layer Model

The project UI is organized as:

- `Screen`
- `Panel`
- `Widget/View`
- `Session State`
- `Interaction State`
- `Presentation Model`
- `Gameplay Rule State`

### 4.1 Screen

A `Screen` is a complete UI session or mode.

It Must own:

- open / close lifecycle
- modal or non-modal behavior
- panel composition
- blocking intent for gameplay input
- active session binding

It Must Not own:

- low-level widget behavior
- gameplay legality rules
- final sentence construction in gameplay systems

### 4.2 Panel

A `Panel` is a bounded region inside a screen.

It Must own:

- organization of one slice of information
- rendering of already-prepared presentation data
- local visual state

It Must Not own:

- whole-screen lifecycle
- unrelated panel coordination logic
- gameplay rule calculations

### 4.3 Widget / View

A `Widget` or `View` is a low-level display and interaction surface.

It Must own:

- direct display primitives
- narrow interaction capture
- signal emission to higher layers

It Must Not own:

- feature rules
- screen policy
- user-facing sentence composition from gameplay data

### 4.4 Session State

Describes what UI mode is active.

Examples:

- current screen kind
- whether a screen is modal
- active container
- visible panel set

Owner:

- screen controller

### 4.5 Interaction State

Describes how the user is interacting right now.

Examples:

- focus
- selection
- hover
- drag source
- preview rotation

Owner:

- interaction controller

### 4.6 Presentation Model

Describes what should be shown.

Examples:

- header text key
- summary values
- status severity
- slot readiness
- preview rectangles

Owner:

- presenter or view-model builder

### 4.7 Gameplay Rule State

Describes actual gameplay truth.

Examples:

- whether an item can be placed
- whether a transfer succeeded
- cooldown state
- target health
- access restrictions

Owner:

- gameplay systems

Gameplay rule state Must Not emit final user-facing sentences directly.

## 5. Screen Taxonomy

The project recognizes four top-level screen kinds:

- `HudScreen`
- `ItemScreen`
- `MenuScreen`
- `OverlayScreen`

### 5.1 HudScreen

Purpose:

- always-on gameplay HUD

Rules:

- Must default to non-modal
- Must remain stable in layout
- Must prioritize readability during combat and traversal
- Must Not become a container for unrelated feature windows

### 5.2 ItemScreen

Purpose:

- inventory, container, equipment, shop, stash, and crafting compositions

Rules:

- Must be the single entry point for inventory-like interaction
- Must own session state for active item UI
- Must declare gameplay blocking through the shared input boundary
- Must reuse shared item interaction semantics

### 5.3 MenuScreen

Purpose:

- pause, settings, save, and non-item menus

Rules:

- Usually modal
- Must explicitly declare whether gameplay pauses or merely blocks input

### 5.4 OverlayScreen

Purpose:

- prompts, tutorials, confirmations, strong alerts, and temporary overlays

Rules:

- Must explicitly declare input passthrough or capture behavior
- May stack over another screen
- Must not silently inherit input behavior

## 6. Lifecycle Rules

Every screen must support these lifecycle states conceptually:

- `Open`
- `Active`
- `Suspended`
- `Closing`
- `Closed`

Rules:

- `Suspended` screens May retain session state if explicitly documented.
- `Closing` screens Must not accept new destructive actions.
- Drag or preview interaction Must be resolved or canceled explicitly when a screen closes.
- Forced closure Must define how transient interaction state is cleaned up.

## 7. Input Boundary And Focus

### 7.1 Shared Input Boundary

Gameplay-facing UI Must route blocking intent through one shared input boundary.

This boundary must answer:

- does UI block movement
- does UI block combat input
- does UI block targeting
- does UI block world interaction
- does UI block camera input

The current project may collapse these into one effective block temporarily, but the interface must be designed to expand.

### 7.2 Priority Order

Input priority must follow:

- `OverlayScreen`
- `MenuScreen`
- `ItemScreen`
- `HudScreen`
- gameplay

Top-most active modal UI has priority.
Lower layers may continue passive refresh, but must not process conflicting active input.

### 7.3 Focus Semantics

The project distinguishes:

- `Hover`
- `Focus`
- `Selection`

Rules:

- `Hover` Must Not be treated as equivalent to `Selection`
- `Selection` Must Not be treated as equivalent to panel `Focus`
- keyboard and gamepad navigation Must work without mouse hover
- closing a panel or screen Must define focus return behavior

## 8. Item Interaction Model

All item-like systems must share one common interaction vocabulary.

Base actions:

- `Select`
- `Inspect`
- `BeginDrag`
- `RotatePreview`
- `CommitDrop`
- `QuickTransfer`
- `Merge`
- `Compare`
- `Cancel`

Rules:

- the action vocabulary Must be shared across inventory, container, equipment, shop, crafting, and stash flows
- feature-specific systems May specialize rule evaluation
- feature-specific systems Must Not redefine the core player input semantics without updating the spec

## 9. Text, Localization, And Layout

### 9.1 Text Source Rule

All player-facing text Must flow through a shared UI text layer.

Required form:

- `text_key + arguments + domain`

Gameplay systems:

- May return structured values and failure categories
- Must Not assemble final user-facing sentences as their primary output

### 9.2 Text Domains

At minimum, text must support these domains:

- `hud`
- `item_screen`
- `system`
- `errors`
- `tutorial`
- `debug`

### 9.3 Layout Adaptation Rule

Text infrastructure and layout infrastructure must be designed together.

Rules:

- `HudScreen` must prefer short, scannable text
- long explanations should move into contextual panels, tooltips, or overlays
- language expansion must not break the primary interaction area
- each panel must define how overflow is handled

### 9.4 Missing Content Handling

Rules:

- missing text keys must surface a visible fallback and a development-time warning
- missing format arguments must surface a visible fallback and a development-time warning
- open screens must support runtime refresh when language changes

## 10. Presentation Tokens

The UI must centralize presentation tokens for:

- typography
- spacing
- panel padding
- colors
- semantic state colors
- borders and radii
- elevations and overlays

State semantics must remain consistent across screens:

- default
- hover
- focus
- selected
- disabled
- warning
- invalid
- blocked

## 11. Panel Vocabulary

The project standard panel vocabulary includes:

- `VitalsPanel`
- `TargetPanel`
- `SkillBarPanel`
- `GridContainerPanel`
- `EquipmentPanel`
- `ItemDetailsPanel`
- `TransactionPanel`
- `StatusPanel`

## 12. Panel Composition Rules

Rules:

- screens are composed from panels
- panels may be reused across multiple screens
- a feature must prefer new composition over new architecture

Before implementation, new UI must answer:

1. which screen kind owns the feature
2. which existing panels it reuses
3. which new panel type is truly required
4. which session state it introduces
5. which interaction state it introduces

## 13. Notification And Feedback Rules

Feedback must be classified instead of treated as one generic message type.

Types:

- status line
- toast
- inline panel feedback
- blocking prompt
- persistent log or history

## 14. Empty, Missing, And Error States

Every new screen or panel must define:

- empty state
- unavailable state
- blocked state
- missing-data state
- text-missing fallback state

## 15. Configuration Boundaries

Recommended guidance:

- text content: configuration-driven
- visual tokens: configuration-driven
- panel composition: partially configuration-driven
- core interaction semantics: code-defined
- feature-specific rule evaluation: strategy or policy driven

## 16. Performance And Refresh Rules

UI systems must define refresh granularity intentionally.

Rules:

- not all UI may refresh every frame by default
- frequent HUD updates should be limited to truly real-time values
- derived text formatting should be centralized and reusable
- grid and panel refresh should prefer localized updates when possible
- language switching may trigger a broader refresh, but must be intentional

## 17. Verification Requirements

UI verification requires two levels:

1. compile correctness
2. runtime interaction correctness

Minimum runtime checks for gameplay-facing UI:

- open and close the relevant screen
- verify focus behavior
- verify hover / selection / drag behavior
- verify success and failure feedback
- verify gameplay input blocking
- verify visible content refresh
- verify empty and blocked states

## 18. Current Project Mapping

- `InventoryUiController`
  - current role: `ItemScreen` session ownership seed
  - status: retain and evolve
- `InventoryInteractionController`
  - current role: shared item interaction seed
  - status: retain, but keep generic
- `ItemScreenCoordinator`
  - current role: shared item screen composition coordinator
  - status: retain and keep general
- `PlayerInventoryWindowPresenter`
  - current role: panel presenter
  - status: retain direction
- `ContainerWindowPresenter`
  - current role: panel presenter
  - status: retain direction
- `GameplayInputGate`
  - current role: shared input boundary seed
  - status: expand into the standard UI/gameplay boundary
- `CombatHudPresenter`
  - current role: legacy monolithic HUD presenter
  - status: split before further HUD expansion

## 19. Implementation Order

The project should implement in this order:

1. freeze terminology, screen taxonomy, and panel vocabulary
2. freeze input boundary and focus rules
3. freeze text, localization, and layout rules
4. freeze item interaction semantics
5. define presentation-model interfaces
6. split combat HUD into smaller panel presenters
7. generalize inventory composition into `ItemScreen`
8. attach equipment, shop, crafting, map, and other future UI to the shared system

## 20. Forbidden Patterns

The following are forbidden unless this specification is explicitly updated:

- gameplay systems assembling final player-facing sentences as their main output
- panels owning whole-screen lifecycle
- widgets containing feature business rules
- new feature UIs inventing their own item interaction model
- bypassing the shared input boundary for gameplay blocking
- adding temporary hardcoded UI strings directly into presenters or controllers as the default pattern
- copying the current monolithic combat HUD structure as the template for new UI

## 21. Review Checklist

Every UI review should answer:

1. which screen kind owns this feature
2. whether it adds session state, interaction state, or both
3. whether it reuses existing panel vocabulary
4. whether it reuses shared item interaction semantics
5. whether it routes text through the shared text layer
6. whether it routes blocking through the shared input boundary
7. whether it defines empty, blocked, and error states
8. whether it violates a forbidden pattern
