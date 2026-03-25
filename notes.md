
# **Notes**






> ## **To be fixed**
>

> ## **Rework Required**
>
Effect system - Conditions applicable - not commands

# **To Do**

Global Damage System
Single entry point. Receives a DamageRequest (target, source, amount, damage type, flags). Runs it through its subsystems in order, applies the final value to health, then emits a local DamageEvent to the actor. Nothing else writes to health.

Mitigation Subsystem
Called by the damage system before application. Takes the raw damage and returns modified damage. Handles armor, resistances, damage type modifiers (fire vs physical vs poison), damage caps. Keeps all that math in one place and out of the damage system itself.

DoT Manager
Owns all active damage-over-time effects globally. Each DoT effect knows its target, damage type, amount per tick, interval, and remaining duration. On its tick interval it submits a DamageRequest to the damage system like any other source — no special pathing.

Combat Resolver (reduced scope)
Stops owning damage application entirely. Resolves what damage should occur from a combat interaction, constructs a DamageRequest, hands it to the damage system. Also still handles force events and effect application as it does now.

Actor Lifecycle (simplified)
Drops the health polling entirely. Listens for the local DamageEvent the damage system emits. Fires health change alerts from that, checks death transition from that. Becomes purely reactive.


# **Concepts**

> ### **Healing** 

> 1. Healing can be found in the world for small heals 25%
> 2. Full Heal and Safe point are cryogenic tubes
>    - It is not the tube that heals but the passage of time.
>    - E.g. Full heal is 6 day night cycles "Recovery" happening in 3 seconds as visuals speed up day and night cycle pass and that recovers health. 

> ### **Locations**

> #### buildings
> 1. Entry to buildings does not have load screen, it fades what would not be visible through the doorway to black and does a Tardis - the inside is bigger than the outside.


> ### **EnemieS**

> #### flying
> 1. flying drone - requires shield throw to down for attack
