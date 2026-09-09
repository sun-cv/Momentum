# To Do

## To do — 2026-09-02
- Implement `EventBus` (lives in `Common`). Needed to decouple `Service`/`ServiceRoster` registration from `Engine.Scheduler` without adding a cross-assembly reference between `Service.asmdef` and `Engine.asmdef` (currently neither references the other).

## To do — 2026-09-08
- Design and implement non-scaled tick rate for input drivers (Interface scope) — driver reads must stay exempt from per-entity/global `TimeScale`.
