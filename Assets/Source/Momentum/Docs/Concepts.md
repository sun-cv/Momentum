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
