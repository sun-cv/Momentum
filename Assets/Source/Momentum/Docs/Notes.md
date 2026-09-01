# Notes

## To do — 2026-09-02
- Implement `EventBus` (lives in `Common`). Needed to decouple `Service`/`ServiceRoster` registration from `Engine.Scheduler` without adding a cross-assembly reference between `Service.asmdef` and `Engine.asmdef` (currently neither references the other).
