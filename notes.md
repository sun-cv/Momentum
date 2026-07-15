
# **Notes**


> ## **Ability System Rework**
>
- Full rebuild: `WeaponSystem` → `AbilitySystem`, `WeaponAction` → `Ability` (definition), `WeaponInstance` → `AbilityInstance` (runtime). No seam, port weapons then delete old.
- Lanes for concurrency: start with Action + Movement. One ability per lane, lanes run concurrently. Defense/parry stays in Action for now.
- `WeaponType` (Root/Combo/Interrupt) → rename `AbilityRole`; fold `Interrupt` into the cancel contract (leaves Root/Combo).
- Cancel contract = per-phase windows on the ability: permit-set (default `{Any}`) + outcome `Abort` (pre-commit charge) vs `Truncate` (post-commit anim-cancel). Cross-lane activation queries the held ability's window.
- Default mapping moves to the ability *set* (`Trigger → default Ability`), not self-declared on the ability. Retire `Availability.Default`; keep `OnPhase`/`OnHeld` on the ability.
- Facing claims key off the `AbilityInstance` (lane-ready), not a global weapon.
- Added `AbilityPhase.Release`. `DisablePhaseHandler` sets Release phase; checks become "are we in Release phase?" instead of `ReadyToRelease`. Tidier.





> ## **To be fixed**
>
Update weapon system with new registration for state handlers 
Review the `UpdatePriority` list once the ability system is updated.

> ## **Rework Required**
>
Effect system - Conditions applicable - not commands
Presence System - Sending presence state event as system enable/disable etc- should be its own event api;
Animation system - Rework animation system with atomic events for start stop and clean up.

# **To Do**

DoT Manager
Owns all active damage-over-time effects globally. Each DoT effect knows its target, damage type, amount per tick, interval, and remaining duration. On its tick interval it submits a DamageRequest to the damage system like any other source — no special pathing.


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
