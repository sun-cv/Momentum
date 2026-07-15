# Momentum — project context

Unity 2D action game (top-down, sprite-based, melee combat: sword / shield / dash).
Solo project; `notes.md` at the root is the design scratchpad — read it for gameplay
concepts and the current rework checklist. Last verified: 2026-06-09.

## Architecture (stable)

- Logic lives in plain C# service classes, not MonoBehaviours. `Service.cs` defines the
  base: Enable/Active/Passive/Disable lifecycle, rate interfaces (`IServiceTick/Loop/Step/Util/Late`
  plus `IPassive*` low-rate variants for off-screen actors), priority-ordered each frame
  via `ServiceUpdatePriority`.
- Events over references: global + per-actor local bus (`owner.Bus.Emit.Local(...)`),
  `IMessage` structs.
- Service messaging pattern (settled 2026-06-11): requests go `Emit.Global` (addressed
  to the global service). Default to **fire-and-forget**: the requester constructs the
  API payload, stores it on the instance immediately, and never waits for an ACK —
  managers already tolerate ops on unknown ids via `TryGetValue` guards
  (CombatHitboxManager.cs:360), so a failed create costs one harmless Destroy.
  Hitboxes use this; the old Response round-trip's only effect was `OwnedHitboxes.Add`
  on success — an ACK with no consumer. If a response is ever genuinely needed, the
  rule is: reply on `request.Owner.Bus.Emit.Local`, not globally — delivery is the
  correlation. `GlobalEventHandler`/`LocalEventHandler` (EventBus.cs) retire with the
  old weapon system. Keep the `Message<TAction, TPayload>` wrapper: the action generic
  is a type-level channel selector (Request vs Response directions can't cross).
- Definition/Instance split everywhere: `Definition` classes are init-only data
  (constructor column-aligned, `[Definition]`); runtime state lives on `Instance` types.
- `Registry.*.cs` partials wire actors, assets, definitions, factories, services.
- Damage pipeline: dispatcher → parry/block → calculator → processor → resources,
  carried by a `DamageResult` context. Resources (health/armor/shield/mana) are
  consumers on the actor.
- Intent system (`ActorIntent.cs`): `IntentSystem` composes Command, Direction, Facing,
  Targeting, Aiming subsystems. Player and AI both drive it through driver classes
  (`PlayerDriver` / `AiDriver`) — the intent layer never reads input directly.
- File convention: a `Dep` suffix (`ActorIntentFacingDep.cs`, `AnimationControllerDep.cs`)
  marks a deprecated implementation kept alive until the replacement fully lands.

## Active work (uncommitted, mid-flight)

**Ability system rework** — full rebuild of the weapon system, no compatibility seam:
port weapons over, then delete the old system. `WeaponSystem` → `AbilitySystem`,
`WeaponAction` → `Ability`, `WeaponInstance` → `AbilityInstance`. New files:
`ActorAbility.cs`, `ActorAbilitySystem.cs`, `ActorAbilitySystemCooldown.cs`.

- Coexistence model (settled 2026-06-16, replaces fixed lanes): storage is a **flat
  list of live `AbilityInstance`s**, not `Dictionary<Lane, Instance>`. "Lane" demotes
  from a storage slot to a **tag/category** on the ability (Movement, Action, …). The
  three coexistence classes are presets of one knob ("who may live alongside me"):
  Single = permit none, Instant = permit all + **lane-free** (skips the gate entirely,
  unlimited parallel — e.g. quick-cast buff / thrown item), Coexist = the conditional
  middle. "One action at a time" is **not** a slot rule — it emerges: two Action-tagged
  abilities each permit none of the other's tag, so a second can only enter by the
  cancel/chain path.
- Permits are **one-directional and local** — this is the deliberate kill of the old
  global tag-override matrix (the path that spiralled before): only the *live* ability
  is asked about an *incoming* one; the newcomer never vetoes what's already running,
  and nothing reads anyone else's rules. Order decides the asymmetry — charge live +
  dash press = admitted (charge permits Movement); dash live + charge press = rejected
  (dash permits none). Two declared sets per ability, per-phase only where they differ:
  `CoexistWith` (tags that run alongside me, both live) and `CancelableBy` (tags that
  *truncate* me — I die, they start); in neither → reject. The old `Interrupt` type and
  the `{Any}` permit-set fold into `CancelableBy`. A combo chain = **named cancel**:
  `CancelableBy` by name (`instance.ComboAbilities`) rather than by tag — so "chains can
  cancel early" is not a third concept. `AbilityRole` Root/Combo may now be redundant
  with this.
- **Activation strategies dissolved** (settled 2026-06-09): no strategy objects. Each
  phase handler owns its own exit rules via a switch on `Activation` and calls
  `TransitionTo` itself — predicates stay pure. Charging: OnRelease fires if charge ≥
  minimum (checked *before* the early-release abort), aborts to Disable below minimum;
  OnPress/OnCharge fire on charge complete; WhileHeld disables on release. Fire:
  WhileHeld exits to FireEnd on release, others on fire-duration complete. The old
  system needed a WhileHeld-in-Fire carve-out in `ShouldDisableWeapon` because
  system-level termination ran before strategies; handler-owned release removes it.
- **`AbilityTermination` deleted** along with `ShouldTerminate`: OnRelease is expressed
  by `Activation`; OnRootRelease becomes "has `SustainTriggers`" (non-empty list = dies
  on sustain release; consider renaming to `SustainedBy` for discoverability);
  Manual/AfterFire were dead. The any-release vs root-release distinction survives in
  two homes: own-trigger release → `Activation`, dependency release → `SustainTriggers`.
  User accepted with reservations ("feels wrong") — revisit if a termination cause
  appears that neither mechanism covers; that's the moment an explicit flag earns
  its place. The sustain check stays in the
  system loop (phase-agnostic) but its outcome is phase-dependent, mirroring the
  cancel contract's Abort/Truncate split: Charging → Disable, Fire → FireEnd,
  FireEnd → no-op.
- `AbilitySet` shape (settled 2026-06-11): two parts — `abilities`
  (`Dictionary<string, Ability>`, today's `actions` renamed; full definition objects,
  built in the weapon definition's constructor) and `entries` (`Trigger → name`, the
  only new piece; cold-start mapping). Absence from `entries` = chain-only — replaces
  `Availability.Default` + `DefaultWeapon`. Loadout = union of equipped sets' two parts,
  references not copies, written only at equip/unequip. Combo continuations stay on
  `instance.ComboAbilities` (lifetime matches the running swing; loadout never gets
  temporary writes — avoids scrub-on-every-exit-path and cross-lane overwrites).
  Activation = two questions in order: held instance's `ComboAbilities` (trigger match
  via the named ability's own triggers), else loadout `entries`. Both resolve names
  through `abilities`. Kills `GetRootAbility`/`GetAbilityByTrigger(s)` LINQ scans.
  Watch-list: `AbilityRole` may be fully expressed by entries-membership — candidate
  for the `Termination` treatment once activation lands.
- **`AbilityAvailability` deleted** (settled 2026-06-12): `Default` → entries map,
  `OnPhase` → `ComboAbilities`. But `OnHeld` was a different concept hiding in the
  enum — input *freshness*, not reachability: `ShieldBlock` (Shield.cs:111) must
  re-engage from a still-held Attack2 after a bash ends, no fresh press. Survives as
  a bool on `AbilityLifecycle` (~`ActivatesFromHeld`); the activation pipeline reads
  it twice: accept held (not just new) commands, and skip command consumption
  (old ResolveCommandActivation:406 skipped consume for OnHeld so the held command
  stays visible and keeps the block alive).
- Facing claims key off the `AbilityInstance`, not a global weapon.
- New `AbilityPhase.Release`; `DisablePhaseHandler` sets it, replacing the
  `ReadyToRelease` flag check.
- Phase flow (settled 2026-06-11): Enable → Charging → Fire → FireEnd → Disable →
  Release. Release = "gone, free" (system deactivates on seeing it). Charging aborts go
  to Disable directly; never passing Fire means combo abilities were never populated, so
  an aborted ability is naturally non-chainable — the path still encodes fired-vs-aborted
  with no `HasFired` flag.
- Combo continuation window (revised 2026-06-16): **not** a `ClockTimer` started at
  FireEnd.Exit — that made no sense once a combo can truncate mid-Fire. It is
  **per-phase, `FrameCount`-derived**, the same mechanism as the cancel window. Combo
  abilities are populated into the available set at **Fire**; each phase's permission
  entry says whether they're reachable now (FrameCount past `ComboFrameOffset`). Kills the
  dedicated `ComboControlWindow` timer. The combo window is **broader** than the
  cancelable window: during Fire they coincide (strike2 truncates strike1 → fast ~.5s
  path), but it stays open through FireEnd/Disable recovery where nothing's left to
  cancel yet strike2 still chains at normal ~1s speed. `ComboFrameOffset` is its own
  field, separate from `CancelFrameOffset` (different ranges). `instance.ComboAbilities`
  stays instance-scoped, dies at Release.
- Permission structure (settled 2026-06-16) — WHO/WHEN split out of timing:
  new `AbilityPermissions` on the ability = `AbilityTag Tag` +
  `Dictionary<AbilityPhase, AbilityPermissionEntry> Entries`.
  `AbilityPermissionsEntry` = `CoexistWith` + `CancelableBy` — the *sets* only. **Offsets
  live in `AbilityTimingEntry`** (`CancelFrameOffset`, `ControlFrameOffset`), user's call
  2026-06-16: to him a frame number is timing and he'd rather not mix, even though
  co-locating offset-with-its-set reads better. Accepted split — the handler reads offset
  (timing) and WHO (permission) together when it needs both. `Cancelable`/`Instant` bools are **derived, not
  stored**: empty `CancelableBy` = not cancelable; `Tag == Instant` = lane-free (bypasses
  the gate, unlimited parallel). `AbilityLane` enum → `AbilityTag` (Action, Movement,
  Instant); the old top-level `Lane` field becomes `Permissions.Tag`. **Tag is singular**
  (exclusion identity, one membership test); the *sets* are lists — a dash-strike is one
  Action that *moves as an effect* (`MovementDefinition`), not two tags. Multi-tag is
  rejected: it reintroduces the any-vs-all precedence matrix.
- Timing stays **named per-phase fields** (`Timing.Charge`/`Fire`/`FireEnd`), permissions
  stay a **phase-keyed dict** — the asymmetry follows the access pattern, not a style slip.
  Timing is read by the *handler*, which knows its own phase statically (`ChargeHandler`
  → `Timing.Charge`, no lookup). Permissions are read by the *system's* activation pass,
  which indexes `Permissions.Entries[instance.Phase]` dynamically across all live
  instances — a dict kills the `switch(phase)` that named fields would force every tick.
  `AbilityTimingEntry` shrinks to `Frames` + `ForceRelease` (pure duration).
- Cancel = **stateless per-phase check** (settled 2026-06-16, supersedes the proposed
  `CancelWindow` latch — over-built). No latch field on the instance. The phase handler
  checks `FrameCount >= CancelFrameOffset` for its own phase, against the current phase's
  `CancelableBy`. Cancel is per-phase (charge and fire have different windows), so a
  per-phase frame check covers it with no carried state. Combo is the one window that
  genuinely spans phases (fire → recovery), so it keeps a **fire-started timer**
  (`ComboControlWindow` on the instance), not a per-phase frame check. Caveat to wire:
  stateless cancel reads the *current* phase's cancelers, so FireEnd/Disable stay
  cancelable only if they declare their own `CancelableBy` or the handler supplies it
  (empty entry = not cancelable) — that WHO carry-over is the only thing the dropped latch
  was buying.
- Activation calculus (settled 2026-06-16): the **instance is its own arbiter of WHEN** —
  phase handlers stamp `cancelable`/`comboable` bools on the instance each tick (the
  window state), so the system does **no frame/offset/phase math** during activation. The
  system still owns the **WHO**: a candidate clears a live instance only via one of three
  cheap tests — (1) `candidate.Tag ∈ instance.CoexistWith` (current-phase set, the one
  path with *no* bool — coexist isn't a window) → tolerated, doesn't block; (2)
  `instance.comboable && candidate.Name ∈ instance.ComboAbilities` → replaces it
  (truncate); (3) `instance.cancelable && candidate.Tag ∈ instance.CancelableBy` →
  replaces it (truncate); else the instance blocks → reject. Candidate is admitted only
  if **every** live instance lands on 1–3. The bool gates the window, the set-match gates
  who — dropping the set half would let anything cancel anything.
- Candidate gather (settled 2026-06-18): **two passes — combo → default**, ordered with
  **consume-as-you-go** (each admit consumes its command so the next pass can't double-fire
  one press). Order = precedence: continuation (combo, off running instances'
  `ComboAbilities`) > fresh cold-start (default, off loadout `entries`). **Held is not a
  third source** — it's a combo distinguished only by *freshness* via `ActivatesFromHeld`:
  flag false → activating trigger must be a fresh press (`inputBuffer`), consume it; flag
  true → activating trigger satisfied by the held command (`activeBuffer`), skip consume so
  the hold keeps re-engaging. The "hold RC then press another to throw" mix needs no new
  machinery — it's the existing `Triggers`/`SustainTriggers` split: sustains checked vs the
  held `activeBuffer`, the activating trigger vs whichever buffer the flag selects. Held
  candidates ride in the combo pass, so re-engage stays ahead of default automatically.
  Rejected one combined pool: it would need per-candidate source-priority + sort to recover
  combo-over-default, and would run all candidates against one un-consumed buffer (a single
  press could admit two).
- One pipeline, two feeders (settled 2026-06-18): freshness (`ActivatesFromHeld`) is
  orthogonal to source — both combo and default candidates can be held or fresh. So source
  determines *only* the gather list and order; everything after is a single shared
  `Validate → Gate → Commit` run per candidate. A candidate carries just the resolved
  `Ability` + the activating command; it needn't know it's a combo — the gate discovers the
  parent it truncates by hitting test 2 (comboable + name) on that live instance.
- Source is the wrong axis for the loop (caught 2026-06-18, impl had drifted): organizing
  by combo-pass / default-pass tangled *reachability* (source) with *admission* (the gate).
  The combo pass cycled live instances (did the gate); the default pass didn't — so a
  default ability that should cancel-interrupt during a cancel window never got checked.
  Fix: **source only picks the candidate pool + ordering (combo weighted first); the gate
  is relational and run identically for every candidate against all live instances.** Per
  candidate C, loop each live instance I in test order **combo → coexist → cancel → block**,
  first match wins: (1) `I.comboable && C.name ∈ I.ComboAbilities` → C truncates I (named
  continuation advances the chain); (2) `C.tag ∈ I.CoexistWith` → C fires alongside, I keeps
  going; (3) `I.cancelable && C.tag ∈ I.CancelableBy` → C truncates I (animation-cancel);
  (4) else I blocks C → reject. Admit iff no I hits (4); the I's caught by (1)/(3) are the
  victims to truncate, free from the same loop. The rows aren't competing — they describe
  different inputs (an instant matches (2) and fires regardless of any window; a dash that
  must cancel only works if (3)'s window is open). Combo leads so a continuation cuts the
  parent short rather than running parallel. "Fresh default activation" is not a branch —
  it's the gate passing with zero victims.
- Activation is **input-driven** (settled 2026-06-18): iterate *commands*, not instances.
  Each command runs `ResolveAbility` (combo continuation off a live instance first, else
  default binding) → one ability, then the gate against all live instances. Instance-driven
  made defaults second-class (the morning hole — defaults skipped the instance check).
  Held vs fresh = two buffers feeding the *same* resolve, not a separate logic path.
  **`ActivatesFromHeld` is combo-only** (settled 2026-06-22): it's a flag on a combo
  continuation meaning "read my activating trigger from `activeBuffer` (held) instead of a
  fresh `inputBuffer` press." Orthogonal to `AbilityActivation` (OnPress/OnCharge/OnRelease/
  WhileHeld), which is pure instance-lifetime — WhileHeld = "don't terminate while held,"
  never a re-activation. Eligibility rule replaces the old `held` bool: an
  `activeBuffer`-sourced command is eligible **only** for held combos; the default-binding
  branch reads `inputBuffer` exclusively, so a held trigger can never cold-start a default
  (this is what actually stops parry re-firing while block is held — parry's trigger left
  `inputBuffer` on consume). **`IsLive` retired**: a held activation is reachable only via a
  live instance's `ComboAbilities` offer, and firing ends the instance that offered it, so
  the offer vanishes the same tick — the continuation is one-shot by construction, no
  liveness flag needed. **Consume = promote** (`ConsumeCommand` moves pending→active,
  `:132-137`): there is no "skip consume" case; promoting *is* what makes a trigger
  held-available. The fresh press promotes; a held continuation rides the already-promoted
  command (nothing left in pending to move). **`OnCancel` animation field is dead** — a
  fresh `Request.Play` already interrupts whatever's playing iff the outgoing animation has
  `AllowInterrupt` (`AnimationController.cs:400-408`), so ending an interrupted instance is a
  flat `DeactivateAbility`, no cancel animation. Caveat: `AllowInterrupt` (anim gate) and
  `Cancelable` (gameplay gate) are now separate switches that must agree. **Instant is a tag,
  not a source/list**: it's a
  normal `binding`; its `Instant` tag makes the gate short-circuit to admit (lane-free).
  Loadout keeps one `bindings` list — membership there is the "root"-ness. **`AbilityRole`
  (Root/Combo) deleted** 2026-06-18: grep-confirmed write-only (set in `Sword.cs`, never
  read for any decision); reachability is structural (in `bindings` = cold-start, in an
  instance's `ComboAbilities` = chainable), which also lets an ability be *both* — the enum
  couldn't.
- Cooldown registers on **`Fire.Exit`** (settled 2026-06-22): cooldown gates a move's
  *effect*, so only things that actually fire pay. `Fire.Exit` runs on every departure
  from Fire (`TransitionTo` always calls the leaving phase's `Exit`), so: a held block
  pays on release (not on entry, which would tick the cooldown away mid-hold); a fixed
  strike pays when active frames end (recovery counts toward it); a mid-Fire cancel pays
  (can't dodge cooldown by cancelling); an **aborted charge pays nothing** (Charging→Disable
  never enters Fire). Rejected: `Fire.Enter` (breaks held), a "fail" cooldown (speculative —
  only needed if feint-cancel becomes exploitable; add a per-ability field then).
  `RegisterCooldown` no-ops when `Timing.Cooldown <= 0`.
- Charge minimum deferred (2026-06-16): a partial `MinimumFrames` (release at 30 of 60)
  only means something with **charge-scaling** (charge level changes damage/range/effect)
  — not built yet. Until then charge is a single `Frames` threshold: OnCharge auto-fires
  at completion, OnRelease fires on release at/after completion, release-before = abort.
  Drop the old float `MinimumCharge`; reintroduce `MinimumFrames` on the Charging timing
  entry the day charge-scaling lands (it's a second *duration* of the charge phase, not a
  permission gate — hence timing, not permissions).
- Parked question: `OnCharge` now aborts if the trigger is released mid-charge
  (must-hold) — a behavior change from the old press-and-forget trigger validation;
  accepted deliberately for now, revisit when real abilities use it.
- Combo controls are instance-scoped and need no cleanup: `instance.ComboAbilities`
  dies with the instance at Release; the per-phase FrameCount window is exactly its
  consultable period. Do **not** port the old `StoreAndReleaseInstance`/`PreviousInstance`
  stash (ActorWeaponSystem.cs:997) — porting it would give two competing answers to
  "what can chain right now."
- Intent capture (settled 2026-06-11): the **activating command** (the one whose
  arrival completes the trigger set — the newest press, not held sustains) owns the
  intent. Activation pipeline stamps `Intent` onto the `AbilityInstance` at creation;
  request methods read `instance.Intent`, never the command list. Replaces the old
  `StoreInputIntentSnapshot` loop where list order silently picked the winner.
  **TODO when building activation: implement the stamp.**
- **State**: handlers ported; movement/hitbox/animation/controls requests wired
  (`SendPhaseRequests`/`ClearPhaseRequests`), intent rides payloads (definitions stay
  immutable), fire-and-forget hitboxes in place. Remaining in `ActorAbilitySystem.cs`:
  effects requests, activation pipeline now built (`ResolveAbility`→`CanActivate`→
  `Validate`→`Commit`/`CancelAbility`, intent stamped at create, command consumed on
  commit, cooldown on `Fire.Exit`) — remaining is wiring it to a real `AbilitySet`,
  constructor wiring (handlers dict, buffers, cooldown/validator still weapon-typed),
  FireEnd duration (zero-length),
  movement clear scoped to owner not ability (:112 comment), `LockDirectionDuringPlayback`
  rework (:180 comment), stray usings.

**Intent/aiming rework** (same working tree): `ActorAimingSystem.cs` deleted, replaced
by `ActorIntentAim.cs`; `ActorIntentMovement.cs` split out; `ActorIntentFacing.cs`
heavily reworked (facing/direction resolution remapped). `TargetingSystem` is a stub.

- **Timing standardized to integer ticks** (2026-06-11): fixed 60Hz sim; all durations
  and thresholds (charge, fire, windows, minimum-charge) are int ticks — collapse
  `AbilityTimingEntry`'s dual `Duration`/`DurationFrame` to one int field. Floats only
  for derived presentation (`(float)elapsed / total` for UI/blends), never stored or
  compared for gameplay decisions. Naming: "frame" in animation, "tick" in services —
  accepted mix, both mean the same 60Hz step. Tick-rate change deemed YAGNI.

## Paths rejected

- **Fully custom physics engine** — built (`PhysicsEngine`), then reverted at commit
  `157ad38`: "out of the wheelhouse." Instead: Unity physics with level mass, velocity
  reduced per actor from internal mass/strength/bleed/resistance.
- **Interrupt as a weapon/ability type** — replaced by the per-phase cancel contract.
- **Abilities self-declaring their default trigger** — mapping moved to the ability set.
- **Animator stop-state** — removed (commit `8d56791`); play requests transition
  directly, fixing a tick-delay bug.

## Known debt (tracked in notes.md)

- Re-review the `UpdatePriority` list once the ability system lands.
- Effect system should take conditions, not commands.
- Presence state needs its own event API instead of riding system enable/disable.
- Animation system wants atomic start/stop/cleanup events.
- Planned: global DoT manager submitting plain `DamageRequest`s on tick.
- **Consolidate aim/facing/rotation locking during abilities** (found 2026-07-15): three
  overlapping mechanisms lock different quantities through different paths — the facing-claim
  `DirectionConstraint.Locked` (locks `intent.Facing.Facing` → `ResolvedFacing`), the
  `LockAimDuringPlayback` animation override (freezes the `ResolvedAim*` animator params to
  the intent snapshot via `UpdateParameters` override priority), and the latent
  `LockRotation`/`CanRotate` gate (freezes `ResolvedAim`/`ResolvedFacing` at the source).
  A swing blends on **aim** (`ResolvedAim`), not facing, so the facing-claim `Locked` didn't
  lock the swing — `LockAimDuringPlayback = true` on the anim entry is the current workaround.
  Collapse to one phase-scoped rotation-lock knob (sibling of `LockMovement`) so "lock my
  facing/aim during this phase" is a single declaration. The `LockAimDuringPlayback`/
  `LockDirectionDuringPlayback` fields are the temporary hacks to retire.