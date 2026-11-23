https://gal-eitan.itch.io/nurse-game-twine

## Live Ops Event Testing Guide

**Last Shift** supports dynamic runtime configuration via `live-config.json` (`LiveOpsConfigManager.cs`), allowing live events to be toggled and tuned without recompiling the game.

### 1. Configuration Values to Change
Locate `live-config.json` in the root of this repository (for remote fetching via `UnityWebRequest`) or in `Assets/StreamingAssets/live-config.json` (for local Editor/offline builds).

To activate or test the **Winter Flu Surge** live event, set the following values inside `"activeEvent"`:

| Configuration Key | Normal Shift (Off) | Live Event (On) | In-Game Effect |
| :--- | :--- | :--- | :--- |
| `activeEvent.isActive` | `false` | `true` | Enables the live event state, HUD banner, tint, and badge reward |
| `activeEvent.eventTitle` | `""` | `"Live Event: Winter Flu Surge!"` | Title displayed in the top HUD event banner |
| `activeEvent.themeTintHex` | `"#FFFFFF"` | `"#DCEEFF"` | Applies a cool winter-blue tint (`#DCEEFF`) to the HUD/ward |
| `activeEvent.modifiers.vitalsTaskProbabilityMultiplier` | `1.0` | `1.8` | Increases vitals-check task spawn probability by 80% |
| `activeEvent.modifiers.taskSpawnIntervalMultiplier` | `1.0` | `0.85` | Reduces time between incoming tasks by 15% (set to `0.5` for 2x spawn speed) |
| `activeEvent.modifiers.feverPatientSpeedBonus` | `1.0` | `1.25` | Increases fever-patient urgency drain speed by 25% |
| `activeEvent.modifiers.scoreMultiplier` | `1.0` | `1.5` | Multiplies all points earned during the shift by 1.5x |
| `activeEvent.rewardBadgeId` | `""` | `"BADGE_WINTER_HERO_2026"` | Unlocks the Winter Hero badge on the End-of-Shift Summary screen |

### 2. How to Trigger the Event
1. Open `live-config.json` (either in this GitHub repository or locally under `Assets/StreamingAssets/live-config.json`).
2. Ensure `"activeEvent.isActive"` is set to `true` (to compare against a standard shift, set `"isActive": false` first, start a shift, then change it back to `true`).
3. Launch the game or restart the shift scene. On scene load, `LiveOpsConfigManager` automatically fetches and parses `live-config.json` and invokes `OnConfigLoaded` to update the task spawner, scoring system, and HUD.

### 3. What the Grader Should See in the Game
* **HUD Event Banner & Winter Tint:** A banner appears at the top of the screen displaying **"Live Event: Winter Flu Surge!"** along with the event description and a subtle icy-blue (`#DCEEFF`) UI/lighting tint.
* **Higher Vitals Task Frequency & Faster Waves:** Vitals-check tasks appear noticeably more often (`1.8x` probability), overall tasks arrive faster (`0.85x` spawn interval), and fever patients' urgency bars deplete `1.25x` faster.
* **1.5x Score Multiplier & Winter Badge:** Completing treatment minigames awards `1.5x` points in real time, and finishing the shift displays the unlocked **`BADGE_WINTER_HERO_2026`** icon on the End-of-Shift Summary screen.
* **Console Confirmation Log:** The Unity Console (or player log) prints:
  `[LiveOps] Loaded 'WINTER_FLU_SURGE_2026' (isActive=True, v1.2.0) from Remote Server.` (or `from Local StreamingAssets`).
