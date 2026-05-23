# Targeting Architecture

## Goal

Rebuild targeting as shared combat infrastructure instead of `Player`-owned logic.

This system must support:

- player-controlled actors
- enemies
- bosses
- neutral units
- future AI target selection
- future input-specific adapters

## Core Principle

Targeting is split into five layers:

1. `Targetable`
2. `CombatAimController`
3. lock acquisition configuration and requests
4. `TargetSelectionStrategy` / lock providers
5. external adapters and consumers

The targeting system itself should not depend on player input code.

## Components

### `Targetable`

Purpose:

- declares that an actor can become a target
- provides a stable world position source

Responsibilities:

- expose whether the actor can currently be targeted
- expose the target node / target position

It should not:

- select targets
- read input
- own aim mode

### `CombatAimController`

Purpose:

- shared aim state controller for any combat actor

Responsibilities:

- store current `TargetingState`
- store current locked target
- store current lock strategy
- resolve final aim direction
- validate locked target lifecycle
- publish state and locked-target changes

It should not:

- hardcode player-only input behavior
- hardcode AI logic
- know about specific skills

### `LockAcquisitionMode`

Purpose:

- stores which lock-entry style the player currently uses

Current examples:

- `StrategyBased`
- `MouseDoubleClick`
- `MouseFollow`

This is not the combat state. It is the configured way a lock request should acquire its target.

Important distinction:

- acquisition mode decides how lock is entered
- selection strategy decides how a strategy-based mode chooses targets
- combat state decides whether the actor is currently free or locked

### `LockRequest`

Purpose:

- unify every "enter lock" attempt into one request shape

Examples:

- strategy-driven lock request
- explicit clicked-target lock request

### `TargetingState`

Current states:

- `Free`
- `Locked`

Meaning:

- `Free`: targeting system does not override direction
- `Locked`: use the current locked target if valid

### `TargetSelectionStrategy`

Purpose:

- choose an initial target
- build an ordered list used for later target cycling

Current examples:

- `Nearest`
- `Farthest`

Strategies may also differ in timing semantics.
For example, `Nearest` may be implemented as:

- snapshot selection at lock-enter time
- continuously refreshed nearest targeting while locked

That timing rule must be decided explicitly instead of being assumed.

Adding a new target-selection strategy should normally mean adding a new strategy, not changing controller state shape.

### `LockTargetProvider`

Purpose:

- consume a `LockRequest`
- resolve how that request enters lock

Examples:

- strategy lock provider
- mouse double-click explicit target provider
- mouse-follow provider

### `PlayerTargetingPreferences`

Purpose:

- store the player's default lock-entry mode
- store the player's default strategy when using strategy-based locking

This is where "which lock mode the player wants to use" should live.

### `PlayerTargetingInputAdapter`

Purpose:

- translate player input into targeting requests

Examples later:

- lock using current preferred mode
- unlock
- cycle target
- change preferred mode / preferred strategy

The input adapter should never become the place where default lock behavior is hardcoded.
It should only:

- read current preferences
- create lock requests
- send those requests to the right provider

This adapter should not own targeting state itself.

## Consumers

Current combat consumer:

- `AttackExecutor`

Rule:

- `AttackExecutor` only asks the aim controller for a resolved direction
- if no targeting direction is available, it falls back to actor-facing direction

This keeps combat execution decoupled from the source of targeting.

## Future Extensions

These should be added as separate adapters, strategies, or lock providers, not inside the controller core:

- new lock-entry modes
- new lock providers
- keyboard target cycling
- nearest-target selection
- lowest-health selection
- manual click selection
- AI threat / aggro target selection
- UI target selection

## Rules Learned From Current Iteration

- Do not hardcode multiple lock-entry behaviors side-by-side before adding a configurable default acquisition mode.
- Do not model every new behavior as a new combat state.
- When adding a new behavior, first decide whether it belongs to:
  - state
  - acquisition mode
  - strategy
  - input adapter
  - consumer interpretation
- If a future feature mainly changes how a target is chosen, prefer extending strategy/provider layers instead of changing `CombatAimController` state shape.

## Important Rule

Do not put targeting ownership back into `Player`.

`Player` may use targeting.
`Player` should not define targeting architecture.
