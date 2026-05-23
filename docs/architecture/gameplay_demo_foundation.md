# Gameplay Demo Foundation

## Goal

This document defines the playable demo loop for `battle-harvester-study`.
It is not a broad vision note. It is the baseline used to decide what must exist before the project can be considered a representative demo.

## Core Loop

The demo loop is:

1. Prepare outside the run.
2. Enter the map with baseline skills and selected equipment.
3. Fight monsters with base combat skills.
4. Search containers and defeat enemies for loot.
5. Find collectibles and core items during the run.
6. Equip, carry, and use what was found.
7. Exit with value and convert that value into the next loadout.

If a feature does not strengthen one of those steps, it is not demo-critical.

## System Layers

### Base Combat

`Base Combat` is the player's guaranteed kit.

Must:

- support a stable baseline attack loop
- support dodge / movement expression
- support a small fixed set of default skills
- remain usable even with no rare equipment or core items

Must not:

- depend on run loot to function
- be displaced by equipment gimmicks

### Equipment Build

`Equipment Build` provides persistent stat identity and pre-run decision making.

Equipment is modeled as:

- slot
- archetype
- variant / quality
- rolled numbers

For demo scope, equipment should create understandable differences in:

- damage
- survivability
- mobility

Equipment must not become a second active-skill system.
Its primary responsibility is passive build shaping.

### Core Item Ability

`Core Item Ability` is a loot-backed active power layer.

Core items:

- are acquired in runs
- can be carried as items
- can be activated like abilities
- can be held and used by enemies
- can drop from enemies

They are not the same as base skills, and they are not standard consumables.
They are item-sourced active abilities.

### Loot Economy

`Loot Economy` exists to justify extraction.

Demo loot should be split into:

- equipment
- core items
- collectibles
- materials

Collectibles and materials primarily support value and progression.
They do not need to define combat identity by themselves.

### Run Economy

`Run Economy` connects in-run risk to out-of-run preparation.

For demo scope, it is enough that:

- collectibles have sell value
- sell value can buy baseline equipment
- equipment changes the next run

## Demo Content Targets

The first representative demo should target:

### Base Skills

- 1 basic attack chain
- 1 dodge / dash
- 2 default active skills

### Equipment

- 3 active demo slots: `MainHand`, `Chest`, `Feet`
- 2 archetypes per slot
- 2 to 3 numeric variants per archetype

### Core Items

- 1 offensive core item
- 1 defensive core item
- 1 control / utility core item

### Loot

- 2 to 3 collectibles
- 1 to 2 crafting / sell materials
- 3 to 5 equippable items

### Enemies

- 1 normal enemy
- 1 elite enemy
- elite enemy can hold and use a core item

### Level Slice

- 1 compact combat arena
- 1 container cluster
- 1 extraction / end-of-run resolution point

## Data Modeling Rules

### Equipment

Equipment must be split into:

- `EquipmentArchetypeDefinition`
- `EquipmentDefinition`
- runtime `ItemInstance`

`EquipmentArchetypeDefinition` answers:

- what slot family this item belongs to
- what build identity it supports
- what stat families it may roll

`EquipmentDefinition` answers:

- what exact rolled values this specific item has
- what quality / variant label it uses
- what item power it represents

This allows "same style, different numbers" without cloning behavior logic.

### Core Items

Core items should eventually be split into:

- a static definition layer
- a runtime state layer
- a shared caster / activator path for player and enemies

That layer is not fully implemented yet, but the demo roadmap assumes it will be modeled separately from base skills and separately from passive equipment.

## Priority Order

When choosing work for the demo, use this order:

1. complete the run loop
2. add representative content
3. improve clarity of stat / loot feedback
4. improve visuals and polish
5. expand breadth

Breadth before closure is specifically discouraged.

## Immediate Next Steps

The next implementation steps should be:

1. add equipment archetype resources and variant-ready item fields
2. create a first batch of equippable demo items
3. inject those items into container and enemy loot
4. surface equipment-driven stat changes in UI
5. define the first core item data model

## Non-Goals For This Demo Pass

The following are intentionally lower priority than the demo loop:

- full shop UI breadth
- large map count
- complex crafting
- complete localization switching UX
- exhaustive HUD refactors
- large equipment slot counts before the main 3 demo slots are proven
