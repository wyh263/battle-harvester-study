# New Chat Handoff Template

Use this when starting a new conversation on the same project.

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

Current stage:
- The skill attack system has already been refactored into SkillDefinition / AttackBehaviorDefinition / EffectDefinition.
- Cooldown, combo, and slot systems are not formally integrated yet.
- The current temporary test keys are J / U / I / O.

Confirmed rules:
1. Skill definitions only describe the skill itself. They do not own cooldown, combo, or slot input logic.
2. Attack behavior is responsible for how the skill hits.
3. Effects are responsible for what happens after a hit.
4. Shapes are only submodules used by some attack behaviors, not the center of the whole combat system.
5. When adding new skills, prefer resource composition and reuse of behaviors, shapes, and effects.

Task requirements for this chat:
- First summarize the real current architecture.
- Then identify what is already stable and what is still temporary.
- Then suggest the safest next step.
- Wait for confirmation before editing code.

Do not:
- Skip the architecture summary and jump straight into code edits.
- Put cooldown or combo logic back into SkillDefinition.
- Push all skill logic back into Hitbox.
```
