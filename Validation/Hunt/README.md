# v1.2.0 validation

- 27 deterministic rule checks passed: combo thresholds/expiry, energy cap, pulse tiers/cooldown, wave timing and damage recovery. See `rules-tests.txt`.
- 18 Unity Play Mode integration checks passed at each of 1280×720 and 1024×768. Reports and captured recharge/warning/hunt screens are in the corresponding folders.
- Scene checks include real tile consumption and duplicate-consumption prevention, normal/super pulse range and duration, dead-ghost exclusion, cooldown, hunt path choice, respawn and unchanged Level 1 scoring.
- WebGL and Windows x64 builds succeeded with Unity 6000.6.0f1.
- Browser smoke check against the actual WebGL build: main menu loaded, Level 2 opened with the new HUD, the round ended and returned to the menu. This is a startup/scene-flow check, not a complete human playthrough or a balance study.
- Existing Built-in rendering pipeline, camera framing and sprite import settings retained. Both HUD layouts visually inspected.
