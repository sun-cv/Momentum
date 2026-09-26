# Concepts

## Component queries — lazy archetype caching
Instead of pre-generating each system's required-component query ahead of
time by reading what a system declares as its required components,
`Components` exposes a single query-style method that takes any number of
component types. On first call for a given combination, it builds an
archetype list (the entities holding all of those components) and caches
it, keyed by that combination; later calls with the same combination
return the cached list directly instead of rebuilding it.

Once a combination has been queried, its list is kept live from then on —
adding or removing a component that's part of a cached combination updates
that combination's list at the point of the add/remove, not lazily on the
next query.

Open questions:
- Cache key shape for arbitrary component-type combinations (order-independent — likely a sorted-type-set key, or a bitmask once component types carry stable indices).
- Cost of maintaining many live archetype lists as more distinct combinations get queried over a session — may need a bound or eviction strategy if this grows unchecked.

## Entity creation driven by definition, not per-type factory
Spawning reads its component list off the definition instead of going
through a hand-written factory per entity type — spawn-time lookup ("does
this definition specify `Health`? add it") instead of one bespoke factory
method per kind.

Open question:
- How a definition declares "these are the components I want" — a fixed
  list field, presence-of-a-sub-object per component, or something else.

## Meta components
Small components for facts about an entity's existence rather than its
gameplay state — `CreatedAt`/`CreatedBy` style, plain data fields same
shape as `Health`.

Also floated: a component for logging interactions an entity has been
part of. Unresolved — unbounded growth per entity is a concern, "log"
already means something specific in Diagnostic, and it's not decided
what would actually read this or whether it belongs as a component at
all versus something Diagnostic-side.

## Helmet LED light
A concept light on the helmet: it flashes or flickers, or can be turned
off outright. Unresolved what drives the flash/flicker state or when
it's toggled off.

## Shield skill upgrade: absorb or deflect
The shield skill upgrades down one of two branches.

- Absorb: the shield takes in a number of projectiles, and each one
  absorbed speeds up energy regen.
- Deflect: the shield deflects projectiles. A second upgrade on this
  branch sends them straight back at the attacker.

Open questions:
- Whether the branches are exclusive or both can be taken.
- How the absorb count and the regen boost scale.

## Upgraded parry: dash behind
An upgraded parry lets the player dash behind the enemy instantly,
so the follow-up attack starts from behind.

## Upgraded dash: chain strike
An upgraded dash goes to the nearest enemy, deals attack damage to
it, and lets the player fire the dash again.

Combo with the upgraded parry: a perfect parry at long range gives
the option to dash in behind the enemy and attack. With the upgraded
dash, that chain keeps going and the player can do it again.

## Drone cursor
A small drone floats with the player in place of an onscreen mouse
cursor. It hovers toward the mouse but never sits exactly on it.
When aiming, the drone projects a laser from its own position that
points exactly at the mouse.
