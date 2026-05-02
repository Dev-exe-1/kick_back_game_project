# Kick Back: Game Prototype

## The Chosen Mechanic

The core gameplay centers around a **"Recoil-Based Movement"** mechanic. The player has a single projectile; firing the weapon applies an opposite physical force to the player, launching them through the air to navigate an **infinitely generating environment**.

To prevent soft-locks and maintain a fast-paced arcade feel, if the player wastes their only bullet and fails to launch, the game immediately triggers a fail-state. This encourages quick decision-making and precise timing.

---

## Controls

* **Mouse Click / Touch:** Aim and fire to propel the player in the opposite direction.

---

##  How to Play

* Press **Start**
* Aim using mouse or touch
* Shoot to move using recoil
* Avoid obstacles and survive as long as possible

---

## Objective

Climb as high as possible and achieve the highest score.

---

## Game Loop

* Player starts from the main menu
* Enters an endless generated level
* The game ends upon collision or failed movement
* Player can instantly restart and try again

---

## Project Structure

The project follows **clean coding practices** and **SOLID-inspired principles**, with a focus on modularity and low coupling:

* **Feature-First Organization:** Scripts are grouped by features (e.g., `Player`, `Hazards`, `LevelGeneration`, `VFX`) rather than by file types.
* **Event-Driven Architecture (Observer Pattern):** Systems primarily communicate through an event-driven approach using static event buses (e.g., `GameEvents.cs`, `PlayerEvents.cs`), reducing direct dependencies between systems like gameplay, UI, and audio.
* **Managers:** Global states and settings are handled using Singleton-style managers (`GameManager`, `SoundManager`, `DifficultyManager`) organized within a Core module.

---

## Design Decisions & Features

### Procedural Level Generation

A chunk-based endless generation system is implemented. Level segments are dynamically spawned and recycled using object pooling, ensuring a seamless infinite gameplay experience with minimal performance overhead.

### Interactive Environment

To enhance gameplay variety and challenge:

* **Moving Platforms:** Platforms that move between points, requiring timing and precision.
* **Fragile Platforms:** Platforms that collapse shortly after contact, encouraging faster decision-making.

### Game Feel & Juice

* Camera shake and screen flash effects on player death
* Use of `Time.unscaledDeltaTime` to ensure visual feedback remains consistent regardless of timescale changes

### Dynamic Difficulty

A `DifficultyManager` progressively increases game speed based on player progression, creating a smooth and engaging difficulty curve.

### Mobile-Ready UI

The UI is built with responsive design in mind:

* Dynamic anchors
* Canvas Scaler (Match Width/Height)
  Ensuring consistent appearance across both mobile and desktop aspect ratios.

---

*Developed for Tornet Technical Evaluation.*
