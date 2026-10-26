# Coin Climber – HFGE1181 FA2 Platformer Prototype Part 2

**Neo Nti · 33HA2608708 · Unity 6000.0.63f1**

Start from `Assets/Scenes/MainMenu` and press Play.
Controls: **A/D** move · **Space** jump (press again in the air to double jump) · **E** interact · **Esc** pause.

---

## Section A – UI

| Element | Where / how |
| --- | --- |
| Health UI | Top-left. 3 life icons; a used life turns dark. With no lives left, the next hit shows Game Over and reloads the level. |
| Interactive UI | An **E** key appears above the player when near a lever or switch; press **E** to use it. |
| Score UI | Top-right. Rises when a coin is collected (bronze 5, silver 10, gold 25) and pops gold. On win/lose the final score counts up and pulses in large text. |
| Main Menu | `MainMenu` scene: **Play** loads Level 1, **Quit** exits. |
| Pause Menu | **Esc** pauses: **Resume** or **Main Menu**. |

## Section B – Prefabs

| Prefab | Notes |
| --- | --- |
| Player | New `PlayerController` + `HealthSystem` (3 lives). Animator: Idle, Run, Jump, Fall using `isMoving`, `isGrounded`, `Jump`. |
| Enemy | New `Enemy` script, damage 1. Animator: Idle/Walk via `Idle` bool. Head-stomp trigger on top of the head destroys the enemy (with camera shake and a small bounce). |
| Coins | `Coin_Bronze` (5), `Coin_Silver` (10), `Coin_Gold` (25) variants with spin animations. Destroyed on pickup; the score resets to 0 on death. |
| Interactive (scripted) | `Lever_Bridge` makes a bridge appear over a pit. `Lever_Door` opens a door that blocks the path. `Switch_Spikes` turns off a spike floor. All use the supplied `Switch` + `Interactable` scripts. |
| Hazards | Spikes use the supplied `Hazard` script (lose a life + knockback + camera shake). |

## Section C – Bug fixes and iterative improvements

**FA1 feedback:** *"Game is not functional purely due to level design."*

### Feedback implementation (level design)

| FA1 problem | Fix in FA2 |
| --- | --- |
| **The player sank a full tile into the ground.** The grass-top tiles used a sprite-based collider that produced no collision, so only the dirt underneath was solid. | Every ground tile now uses a full-cell **Grid** collider, so the player stands on the visible surface. |
| The enemy's Patrol Point A was a **child of the enemy** (it moved with it, so it could never be reached), and Patrol Point B sat **inside the ceiling tiles**, so the enemy slid diagonally through walls. | Each enemy now sits in an Enemy Group with **sibling** patrol points on the same floor height, with open space above it so it can be stomped. |
| The main corridor was only **2 tiles high**: every jump hit the ceiling and stomping was impossible. | The level was rebuilt with open sky above every platform and enemy. |
| The level was 22 tiles wide; the upper platform and the Box couldn't be reached, and the disappearing platform could be skipped. | A new 82-tile level teaches each mechanic in order: coins → steps → spike pit → enemy → bridge lever → disappearing platforms → door lever → spike switch → enemy → flag. |
| Touching the flag just reloaded the scene, so finishing looked like a bug. | The flag now shows a **Level Complete** screen with the final score. |

### Bug fixes and tuning

| Area | Change |
| --- | --- |
| Asset preparation | The tileset was sliced on a 16-px grid, but the sheet has 1-px gaps, which caused dark seams between tiles. Re-sliced to the correct 17-px spacing. |
| Inspector values | Player: speed 6, jump 10, gravity 3, fall multiplier 2 (snappier, fair jumps). Disappearing platform: 0.5 s → 1.5 s. Hazard knockback: 999 → 8. Enemy speed 2, delay 1. |
| Collectibles | `Coin` now only reacts to the Player (enemies could collect coins before) and can't be counted twice. |
| Feedback and responsiveness | Hits now cost a life with blink, knockback and camera shake instead of an instant restart. The score is shown on screen (FA1 only logged it to the Console). |
| Camera | `FollowCam` moved onto a CameraRig parent so `CameraShake` works; bounds updated for the new level. |
| Start-up error | Script Execution Order set (`HealthManager` −100, `UIManager` −50), fixing a null reference error when the game started. |
| Old scripts | The FA1 `DeathOnCollission` script removed from the Enemy (it skipped the health system). |
| Project | `.gitignore` moved to the project root so Library/Temp aren't uploaded. |
