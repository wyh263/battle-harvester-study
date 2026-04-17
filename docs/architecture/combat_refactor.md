# Combat / Skill System Context

## Purpose

This document records the current real architecture of the combat and skill refactor so a new chat can resume without depending on thread memory.

It should describe the code as it exists now, not the older transition plan.

## Current Stage

The old mixed combat structure has already been replaced.

The project now uses a new combat core built around:

- `SkillDefinition`
- `AttackBehaviorDefinition`
- `EffectDefinition`
- `SkillExecutionContext`
- `HitResult`

Cold-down logic, combo logic, slot logic, and other surrounding systems are intentionally not part of this core yet.

## Confirmed Architecture Principles

These points should be treated as stable unless we explicitly decide to change direction.

1. The skill attack system only handles:
   - what skill is being executed
   - how that skill hits targets
   - what effects are applied on hit

2. Cooldown logic does not belong inside skill definitions.

3. Combo / link logic does not belong inside skill definitions.

4. Input binding does not belong inside skill definitions.

5. Attack behavior is a first-class layer.
   Shape data is not the root of the whole system.

6. Hit results are applied as a list of effects.
   We no longer want a single all-purpose payload bucket.

7. Runtime target-side reactions should stay in target components.
   For example:
   - bleed ticking belongs to status-style target components
   - launch motion belongs to launch / reaction components
   - movement slowdown belongs to stats modifiers applied by target-side systems

## Current Folder Structure

### Combat Code

- `Systems/Combat/Core`
  - execution chain and shared combat data

- `Systems/Combat/Behaviors`
  - attack behavior definitions

- `Systems/Combat/Effects`
  - effect definitions

### Combat Resources

- `resources/combat/skills`
  - full skill resources

- `resources/combat/behaviors`
  - reusable behavior resources

- `resources/combat/shapes`
  - reusable shape resources

- `resources/combat/effects`
  - reusable effect resources

## Current Core Files

### Core

- `Systems/Combat/Core/SkillDefinition.cs`
  - static skill definition
  - stores timing labels, timing values, attack behavior, and effect list

- `Systems/Combat/Core/SkillExecutionContext.cs`
  - runtime execution context for one skill cast
  - stores caster, origin, facing direction, current skill, and other cast-time data

- `Systems/Combat/Core/HitResult.cs`
  - standardized hit detection result
  - passed from behavior layer into effect resolution

- `Systems/Combat/Core/AttackExecutor.cs`
  - runs startup / active / recovery timing
  - enters the active window and triggers the selected attack behavior

- `Systems/Combat/Core/AttackState.cs`
  - state machine layer for skill execution
  - consumes the queued skill from `Player`

- `Systems/Combat/Core/Hitbox.cs`
  - runtime hit detector
  - executes the behavior-supplied shape logic and overlap checks

- `Systems/Combat/Core/CombatResolver.cs`
  - applies the skill's effect list to each `HitResult`

- `Systems/Combat/Core/Hurtbox.cs`
  - target-side hit receiver entry point

- `Systems/Combat/Core/DamageInfo.cs`
  - current damage transfer packet used by hurt / reaction flow

### Behaviors

- `Systems/Combat/Behaviors/AttackBehaviorDefinition.cs`
  - base class for attack behaviors

- `Systems/Combat/Behaviors/ShapeAttackBehaviorDefinition.cs`
  - generic shape-based behavior
  - uses a `ShapeDefinition`

- `Systems/Combat/Behaviors/ConeAttackBehaviorDefinition.cs`
  - forward cone behavior
  - currently uses broad-phase shape detection plus angle filtering

- `Systems/Combat/Behaviors/ShapeDefinition.cs`
  - reusable geometry definition
  - shape type, size, offset, rotation, repeat-hit settings, preview settings

### Effects

- `Systems/Combat/Effects/EffectDefinition.cs`
  - base class for all hit effects

- `Systems/Combat/Effects/DamageEffectDefinition.cs`
  - direct damage effect

- `Systems/Combat/Effects/LaunchEffectDefinition.cs`
  - launch / knock-up style effect

- `Systems/Combat/Effects/BleedEffectDefinition.cs`
  - applies bleed status

- `Systems/Combat/Effects/SlowEffectDefinition.cs`
  - applies slow status

## Current Target-Side Runtime Components

- `scripts/effects/StatusEffectComponent.cs`
  - handles active bleed and slow instances
  - applies and removes speed modifiers through `StatsComponent`

- `scripts/effects/LaunchMotionComponent.cs`
  - prototype launch / knock-up motion handling

- `scripts/reactions/HitReactionComponent.cs`
  - visual hit feedback
  - still participates in damage reaction and fallback motion handling

## Current Skill Resources

These are the active skill resources currently wired into the player flow:

- `resources/combat/skills/basic_attack.tres`
  - basic attack

- `resources/combat/skills/laser_beam.tres`
  - laser beam

- `resources/combat/skills/wide_bleed_slash.tres`
  - cone slash with bleed and launch

- `resources/combat/skills/slow_orb.tres`
  - forward circular hit with damage and slow

## Current Test Inputs

At the moment the temporary test bindings are:

- `J`
  - basic attack

- `U`
  - laser beam

- `I`
  - wide bleed slash

- `O`
  - slow orb

These are temporary testing bindings, not the final slot / cooldown / combo architecture.

## Current Execution Chain

The current skill attack chain is:

1. Input is read in locomotion states.
2. `Player` queues a `SkillDefinition`.
3. `AttackState` consumes the queued skill.
4. `AttackExecutor` runs startup / active / recovery.
5. During the active phase, the skill's `AttackBehaviorDefinition` is executed.
6. The behavior uses `Hitbox` to find valid targets and produce `HitResult`.
7. `CombatResolver` iterates the skill's `EffectDefinition[]`.
8. Target-side systems receive and process the results.

This is the current stable attack-system backbone.

## What Has Been Removed

The following older mixed layers are no longer the active architecture:

- `AttackDefinition`
- `AttackPayload`
- `HitboxDefinition`
- `AttackSkillDefinition`

The system has been moved away from those transitional definitions.

## Current Limitations

These are known temporary limitations in the new structure:

1. Cooldown system is not integrated yet.

2. Combo / link system is not integrated yet.

3. Slot system is not integrated yet.

4. Launch motion is still prototype quality, not a full air-state combat model.

5. Some editor cache files under `.godot` still reference old script paths.
   This is cache noise, not the current source of truth.

6. The project still has many pre-existing nullable warnings during build.
   Current combat changes build successfully despite those warnings.

## Recommended Next Steps

The recommended next sequence is:

1. Keep stabilizing the skill attack core with more behavior / effect combinations.
2. Add cooldown as a separate system outside `SkillDefinition`.
3. Add combo / link as a separate system outside `SkillDefinition`.
4. Add slot-based input mapping as a separate system outside `SkillDefinition`.
5. Expand target-side status handling into a more formal status effect model.

## How To Resume In A New Chat

Recommended instruction:

1. Ask the assistant to first read this file.
2. Then ask it to read the current anchor files:
   - `Systems/Combat/Core/SkillDefinition.cs`
   - `Systems/Combat/Core/AttackExecutor.cs`
   - `Systems/Combat/Core/Hitbox.cs`
   - `Systems/Combat/Core/CombatResolver.cs`
   - `Systems/Combat/Behaviors/AttackBehaviorDefinition.cs`
   - `Systems/Combat/Effects/EffectDefinition.cs`
   - `scripts/entities/Player.cs`
3. Ask for a summary before making code changes.

## Resume Prompt Template

You can paste a short prompt like this:

```text
This is a continuation of the same Godot combat project. Do not change code immediately.

Please first read:
- docs/architecture/combat_refactor.md
- Systems/Combat/Core/SkillDefinition.cs
- Systems/Combat/Core/AttackExecutor.cs
- Systems/Combat/Core/Hitbox.cs
- Systems/Combat/Core/CombatResolver.cs
- Systems/Combat/Behaviors/AttackBehaviorDefinition.cs
- Systems/Combat/Effects/EffectDefinition.cs
- scripts/entities/Player.cs

Then summarize:
1. current skill attack architecture
2. what is already stable
3. what is still temporary
4. the safest next structural step
```
