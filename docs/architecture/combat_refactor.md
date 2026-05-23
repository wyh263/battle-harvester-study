# Combat / Stats / Life / Inventory Runtime Context

## Purpose

This document records the current real runtime architecture of combat, stats, health, death, HUD, and the new inventory/container runtime so a new chat can resume work without depending on thread memory.

It should describe the code as it exists now.

## Current Stage

The project is no longer only a combat execution refactor.

It now has seven connected layers:

1. combat execution core
2. cast-gating rules
3. requirement / chain runtime
4. shared actor runtime for player and enemy actors
5. generic stats / health / damage calculation
6. minimal formal HUD and minimal death closure
7. early item / container / inventory runtime foundation

This means the project has moved from "combat refactor" into "shared gameplay runtime is being formalized", with inventory/container groundwork now starting beside combat.

## Stable Architecture Principles

These should be treated as current project rules unless we explicitly decide to change direction.

1. `SkillDefinition` is still a static definition resource.
   It may store:
   - timings
   - labels
   - resource cost
   - cooldown base values
   - requirement resources
   - cancel / interrupt config
   - attack behavior
   - effect list

2. Runtime cooldown state does not live inside `SkillDefinition`.
   Cooldowns are actor runtime systems.

3. Runtime resource state does not live inside `SkillDefinition`.
   Resources are actor runtime systems.

4. Combo / chain history does not live inside `SkillDefinition`.
   Chain runtime state belongs to `SkillChainTracker`.

5. Stats are generic actor data.
   They are not player-only and they do not belong inside skills.

6. Current health is a resource layer, not a stat definition layer.
   `MaxHealth` is a stat.
   `CurrentHealth` belongs to `HealthComponent`.

7. Death is not defined by "cannot be locked".
   Losing targetability is only one consequence of death handling.

8. Presentation stays read-only relative to gameplay runtime.
   HUD reads data from runtime systems and does not own casting, targeting, or stat logic.

9. Item definitions are static resources.
   Runtime stack counts, rotation state, and container placement do not live inside `ItemDefinition`.

10. Containers own placement rules.
    UI should not decide whether an item fits, stacks, rotates, or transfers successfully.

11. Loot generation and container access rules should stay outside container UI.
    "What can spawn here?" and "who can open this?" are separate systems.

## Current Folder Structure

### Combat Code

- `scripts/combat/core`
  - execution chain, hit resolution, skill definition

- `scripts/combat/behaviors`
  - attack behavior definitions and shape definitions

- `scripts/combat/effects`
  - effect definitions

- `scripts/combat/cooldowns`
  - cooldown runtime and cast-block results

- `scripts/combat/resources`
  - skill resource pool runtime

- `scripts/combat/casting`
  - cast context, requirements, chain runtime, status query helpers

### Attributes / Life Code

- `scripts/attributes`
  - stats
  - health
  - death closure
  - damage formulas

### Presentation Code

- `scripts/presentation`
  - combat HUD
  - phase labels
  - lock indicator

### Inventory / Item Code

- `scripts/items`
  - static item definitions
  - runtime item instances

- `scripts/inventory`
  - container definitions
  - runtime container layout / transfer logic
  - inventory-level container coordination

### Combat Resources

- `resources/combat/skills`
- `resources/combat/behaviors`
- `resources/combat/shapes`
- `resources/combat/effects`
- `resources/combat/requirements`

### Inventory Resources

- `resources/inventory`
  - current test container definitions

## Current Core Runtime

### Inventory / Item Runtime

- `scripts/items/ItemCategory.cs`
  - shared item category enum

- `scripts/items/ItemDefinition.cs`
  - static item definition resource
  - currently includes:
    - item id
    - display name
    - category
    - grid width / height
    - can rotate
    - max stack
    - base value
    - tags

- `scripts/items/ItemInstance.cs`
  - runtime item instance
  - currently owns:
    - unique instance id
    - stack count
    - rotation state
    - runtime footprint calculation
    - stack add / stack-space helpers

- `scripts/inventory/GridContainerDefinition.cs`
  - static container definition
  - currently includes:
    - container id
    - display name
    - grid columns / rows
    - accepted item categories
    - auto-sort toggle

- `scripts/inventory/GridContainerComponent.cs`
  - runtime grid container
  - currently handles:
    - placement fit checks
    - occupied-rect collision rules
    - add / remove / move item
    - add item anywhere
    - stack merge before placement
    - atomic accept-or-fail transfer receiving
    - auto-arrange trigger

- `scripts/inventory/ContainerAutoArrangeService.cs`
  - pure layout helper
  - currently performs:
    - area-first sort
    - first-fit packing

- `scripts/inventory/InventoryComponent.cs`
  - inventory-level runtime coordinator
  - currently handles:
    - configured container registration
    - cross-container item transfer
    - forwarding inventory-changed signals

- `scripts/inventory/ContainerItemRecord.cs`
  - runtime record:
    - item instance
    - grid origin
    - occupied rect helper

### Combat Execution

- `scripts/combat/core/SkillDefinition.cs`
  - static skill definition
  - currently includes:
    - base cooldown values
    - resource cost
    - cast requirements
    - chain buffer config
    - dash cancel config
    - interrupt config
    - interrupt armor values

- `scripts/combat/core/AttackExecutor.cs`
  - executes startup / active / recovery
  - updates `SkillChainTracker` with current runtime phase

- `scripts/combat/core/AttackState.cs`
  - consumes queued skills
  - commits cooldown/resource usage
  - handles direct continuation into next queued attack
  - handles minimal dash cancel and interrupt behavior

- `scripts/combat/core/Hitbox.cs`
  - runtime hit detection

- `scripts/combat/core/CombatResolver.cs`
  - applies `EffectDefinition[]`
  - records hit confirmation into `SkillChainTracker`

### Cast / Cooldown / Resource Runtime

- `scripts/combat/cooldowns/ActorSkillCooldownController.cs`
  - per-actor cooldown runtime
  - per-skill cooldown
  - cooldown groups
  - global cooldown
  - cast check and cast commit

- `scripts/combat/resources/ActorSkillResourceController.cs`
  - per-actor skill resource runtime

- `scripts/combat/cooldowns/SkillCastCheckResult.cs`
  - structured result for "why a skill cannot be cast"

- `scripts/combat/cooldowns/SkillCastBlockReason.cs`
  - currently includes:
    - `SkillCooldown`
    - `CooldownGroup`
    - `GlobalCooldown`
    - `ResourceInsufficient`
    - `RequirementNotMet`
    - `ChainWindowClosed`

### Requirement / Chain Runtime

- `scripts/combat/casting/SkillCastContext.cs`
  - context object used before queueing a skill

- `scripts/combat/casting/requirements/SkillCastRequirement.cs`
  - requirement base class

- `scripts/combat/casting/requirements/PreviousSkillRequirement.cs`
  - previous cast / previous hit requirement

- `scripts/combat/casting/requirements/TargetStatusRequirement.cs`
  - target status requirement

- `scripts/combat/casting/SkillChainTracker.cs`
  - tracks:
    - last cast skill
    - last hit skill
    - current skill
    - current phase
    - hit confirm on current skill
  - answers:
    - can the next skill be buffered?
    - can the current skill dash cancel?
    - can the current skill be interrupted by this hit?

### Attributes / Life Runtime

- `scripts/attributes/StatType.cs`
  - shared stat identifiers

- `scripts/attributes/StatsComponent.cs`
  - generic actor stat runtime
  - currently provides:
    - `MaxHealth`
    - `AttackPower`
    - `Defense`
    - `CritChance`
    - `CritDamage`
    - `MoveSpeed`
    - `RunMultiplier`
    - `DashSpeed`
    - `DashDuration`
    - `DashCooldown`
    - `DashInvulnerableDuration`
    - `CooldownRate`
    - `DropRate`
  - also supports generic stat modifiers

- `scripts/attributes/CombatStatFormulas.cs`
  - shared damage calculation entry
  - currently handles:
    - attack power
    - defense mitigation
    - crit chance
    - crit damage

- `scripts/attributes/HealthComponent.cs`
  - current health runtime
  - owns:
    - current health
    - max health lookup
    - damage / restore / refill
    - death event

- `scripts/attributes/DeathStateController.cs`
  - generic death closure controller
  - currently does minimal closure:
    - cancel current attack
    - clear forced movement
    - zero desired movement and body velocity
    - disable configured gameplay nodes
    - disable `Targetable`
    - disable `Hurtbox`
    - tint visuals
    - mark label as `DEAD`

This is a transitional death-flow layer, not the final full death gameplay model.

## Current Combat Formula / Damage Flow

The current damage path is:

1. a skill's `DamageEffectDefinition` resolves
2. attacker and defender `StatsComponent` values are read
3. `CombatStatFormulas.CalculateDamage(...)` computes final damage
4. `DamageInfo` is created with:
   - final damage
   - base damage
   - crit result
   - interrupt strength
   - armor bypass flag
5. `Hurtbox.TakeDamage(...)` forwards the result to `HealthComponent`
6. `HealthComponent` updates current health
7. if health reaches zero, `DeathStateController` closes the actor down

## Current Shared Actor Runtime

### Player

The player currently uses:

- `ActorSkillLoadout`
- `ActorSkillCooldownController`
- `ActorSkillResourceController`
- `SkillChainTracker`
- `StatsComponent`
- `HealthComponent`
- `DeathStateController`
- `StateMachine`
- `CombatHudPresenter`

### Dummy

The dummy now also uses the shared runtime shape:

- `ActorSkillLoadout`
- `ActorSkillCooldownController`
- `ActorSkillResourceController`
- `SkillChainTracker`
- `StatsComponent`
- `HealthComponent`
- `DeathStateController`
- `StateMachine`

The dummy no longer proves combat through direct `AttackExecutor.Play(...)` ownership.
Instead it uses:

- `DummyCounterAttackController`
  - as an enemy-side cast attempt adapter

This is important: player and enemy actors now share the same queue / cooldown / chain / attack-state structure, while differing only in who requests casts.

## Current Inventory Runtime Status

Inventory is still runtime-foundation work, not a full user-facing feature yet.

What already exists:

- static item definitions
- runtime item instances with rotation + stack state
- static grid container definitions
- runtime grid container placement rules
- item stacking into existing records
- cross-container transfer coordinator
- simple auto-arrange service

What does not exist yet:

- loot tables / loot generator
- container access rules
- inventory UI / drag-and-drop
- equipment slots
- persistence / save format
- world pickup / search-container flow

## Current Skill Set

The player currently has these skills wired:

- `basic_attack`
  - combo hit 1

- `basic_attack_followup`
  - combo hit 2

- `basic_attack_finisher`
  - combo hit 3

- `laser_beam`
  - standalone skill

- `wide_bleed_slash`
  - standalone skill

- `slow_orb`
  - standalone skill

- `energy_wave`
  - standalone skill

Earlier temporary requirement locks on `laser_beam`, `wide_bleed_slash`, and `energy_wave` were removed.
The 3-hit basic combo remains the main live combo example.

## Current Input State

Temporary test bindings are currently:

- `attack`
  - resolves to combo hit 1 / 2 / 3

- `skill_u`
  - laser beam

- `skill_i`
  - wide bleed slash

- `skill_o`
  - slow orb

- `skill_p`
  - energy wave

These are still testing inputs, not the final skill bar / equipment / slot model.

## Current HUD State

- `scripts/presentation/CombatHudPresenter.cs`
  - reads runtime-only data
  - does not own gameplay logic

Current HUD elements:

- player HP
- player resource bar
- target lock label
- target HP
- skill readiness / cooldown text

The older debug cooldown panel was removed from the player scene.

## Current Build State

- `BattleHarvesterStudy.csproj` uses:
  - `<Nullable>annotations</Nullable>`

- `dotnet build BattleHarvesterStudy.csproj`
  - currently succeeds with:
    - `0 warning`
    - `0 error`

## Current Assessment

### What Is Structurally Solid

- combat core vs presentation split
- targeting core vs input split
- player and enemy actors now share casting runtime shape
- cooldown/resource runtime are separate from skill definitions
- health is separate from stats
- death closure is separate from health
- HUD is read-only relative to runtime state
- inventory runtime is starting from shared data rules, not UI shortcuts

### What Is Functionally Good Enough To Build On

- shared cast queue / cooldown / resource / chain flow works
- a basic 3-hit combo exists
- interrupt and dash-cancel rules exist in minimal form
- stats now affect damage
- HP and target HP are visible in the HUD
- death now meaningfully shuts actors down
- grid containers already support placement, stacking, and transfer as shared runtime rules

### What Is Still Temporary Or Prototype Quality

1. Death handling is still "minimal closure", not a full death flow.
   There is no formal `Alive / Dying / Dead / Respawning` state model yet.

2. Combo content is still mostly test content.

3. Interrupt / armor rules exist, but they are still early and not yet deeply authored across the move set.

4. Stats exist, but modifier sources are not formalized yet.
   Equipment, buffs, passives, and progression are not yet attached as reusable modifier providers.

5. Inventory runtime exists, but it is still headless.
   There is no user-facing inventory UI, no loot table system, and no container access rule layer yet.

6. Equipment is not implemented yet.
   Inventory and item runtime must stay separate from future equipment-slot logic.

7. No formal enemy combat authoring layer exists beyond the current dummy-side adapter.

## Recommended Next Steps

The safest next sequence is now:

1. Add loot runtime for containers
   - loot table resources
   - loot entry weights
   - loot generator
   - container fill flow

2. Add container access rules
   - distance
   - key / tag requirements
   - single-use / lock state

3. After loot + access exist, build the first inventory UI layer
   - read-only grid draw first
   - then drag / rotate / move
   - UI should call container / inventory runtime, not replace it

4. After inventory flow exists, add equipment as a separate layer on top
   - equipment slots
   - item restrictions
   - stat modifiers from equipped items

5. Then return to stat modifier sources more broadly
   - equipment modifiers
   - buff modifiers
   - passive modifiers

6. Upgrade death closure into a formal life-state flow later
   - `Alive`
   - `Dying`
   - `Dead`
   - later `Respawning` if needed

## How To Resume In A New Chat

Recommended instruction:

1. Ask the assistant to first read this file.
2. Then ask it to read the current anchor files:
   - `docs/architecture/codex_collaboration_rules.md`
   - `scripts/items/ItemDefinition.cs`
   - `scripts/items/ItemInstance.cs`
   - `scripts/inventory/GridContainerDefinition.cs`
   - `scripts/inventory/GridContainerComponent.cs`
   - `scripts/inventory/InventoryComponent.cs`
   - `scripts/attributes/StatsComponent.cs`
   - `scripts/attributes/HealthComponent.cs`
   - `scripts/attributes/DeathStateController.cs`
   - `scripts/combat/core/SkillDefinition.cs`
   - `scripts/combat/core/AttackState.cs`
   - `scripts/combat/cooldowns/ActorSkillCooldownController.cs`
   - `scripts/combat/casting/SkillChainTracker.cs`
   - `scripts/presentation/CombatHudPresenter.cs`
   - `scripts/actors/Player.cs`
   - `scripts/actors/Dummy.cs`
3. Ask for a summary before making code changes.

## Resume Prompt Template

You can paste a short prompt like this:

```text
This is a continuation of the same Godot gameplay project. Do not change code immediately.

Please first read:
- docs/architecture/combat_refactor.md
- docs/architecture/codex_collaboration_rules.md
- scripts/items/ItemDefinition.cs
- scripts/items/ItemInstance.cs
- scripts/inventory/GridContainerDefinition.cs
- scripts/inventory/GridContainerComponent.cs
- scripts/inventory/InventoryComponent.cs
- scripts/attributes/StatsComponent.cs
- scripts/attributes/HealthComponent.cs
- scripts/attributes/DeathStateController.cs
- scripts/combat/core/SkillDefinition.cs
- scripts/combat/core/AttackState.cs
- scripts/combat/cooldowns/ActorSkillCooldownController.cs
- scripts/combat/casting/SkillChainTracker.cs
- scripts/presentation/CombatHudPresenter.cs
- scripts/actors/Player.cs
- scripts/actors/Dummy.cs

Then summarize:
1. current combat / stats / health / inventory runtime architecture
2. what is already structurally stable
3. what is still temporary
4. the safest next structural step
```
