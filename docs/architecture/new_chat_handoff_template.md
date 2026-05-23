# New Chat Handoff Template

Use this when starting a new conversation on the same project.

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

Current stage:
- The project already includes shared combat runtime, generic stats, health, death closure, a formal gameplay HUD, and a working item/container/inventory runtime with UI.
- A temporary 3-hit basic combo exists.
- The current temporary test inputs are attack / skill_u / skill_i / skill_o / skill_p.
- Inventory currently has item definitions, item instances, grid containers, stacking, transfer, container access, loot filling, equipment slots, and active drag/drop style UI.

Confirmed rules:
1. Skill definitions only store static configuration. Runtime cooldown, resource, chain, health, and death state do not live inside SkillDefinition.
2. Stats are actor runtime data, not player-only data.
3. Current health is separate from max-health stats.
4. Death is not defined by one side effect like "cannot be targeted"; it is a controller-owned consequence layer.
5. HUD reads runtime results and should not own gameplay rules.
6. When adding new systems, keep ownership shared where future enemies / bosses / AI will need the same capability.
7. Item definitions and container definitions are static resources. Runtime stack counts, rotation, placement, and transfer belong to runtime classes.
8. UI should not own inventory fit, stack, or transfer rules. Those belong to container / inventory runtime.

Task requirements for this chat:
- First summarize the real current architecture.
- Then identify what is already stable and what is still temporary.
- Then suggest the safest next step.
- Wait for confirmation before editing code.

Do not:
- Skip the architecture summary and jump straight into code edits.
- Put runtime cooldown, combo, resource, health, or death state back into SkillDefinition.
- Collapse stats and current health back into one class.
- Push all gameplay logic back into one actor class.
- Put container placement / stack / rotation validation into UI-only code.
```
