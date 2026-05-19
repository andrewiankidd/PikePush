# Runner Mode

The arcade dash — auto-running pikeman moving forward at increasing speed, dodging environmental obstacles and meter-fighting any enemies you collide with. Score is "distance × speed", so the longer you survive the faster the number ticks.

Scene: `Game.unity` (build index 4). Master script: [`MainGame.cs`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/MainGame.cs).

## The loop

1. Scene loads. [`MainGame.Start()`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/MainGame.cs) reads `ColourOne` + `ColourTwo` from `PlayerPrefs` and paints the player pikeman. A "Press Space to Begin" message appears.
2. Player hits **Space**. The world starts scrolling. The score ticks up.
3. The first 3 tiles spawn obstacle-free; everything after is a die roll.
4. On collision with a `"Fight"`-tagged enemy → [meter-combat minigame](#meter-combat) opens.
5. On collision with a `"Finish"`-tagged object → game over.
6. Press **Escape** anytime to bail to the main menu.

## Player controls

| Action | Key | What it does |
|--------|-----|--------------|
| Jump | `W` | Apply upward impulse (`jumpHeight = 2.5`). Custom `gravity = 20.0` brings you down. |
| Crouch | `S` | Scale Y to `0.4`, halve `movementSpeed`. Useful for low obstacles + intentional slowdown. |
| Strafe Left | `A` | Move along X within `-strafeSpeed` to `+strafeSpeed` (≈ ±2.75 units). |
| Strafe Right | `D` | Same, opposite direction. |
| Fight | `Space` | Triggers `MeterGame` during fight encounters. |

Player controller lives in [`IRPlayer.cs`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/IRPlayer.cs). The Rigidbody has Z-axis position frozen and all rotation frozen — the player is constrained to the X plane and only moves forward via tile scrolling, not actual translation.

## Tile spawning

Obstacles come from [`PlatformTile`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/PlatformTile.cs) — a tile prefab with `startPoint`, `endPoint`, and an `obstacles[]` array. `MainGame` keeps a recycling pool of tiles:

| Setting | Default | Meaning |
|---------|---------|---------|
| `tilesToPreSpawn` | 15 | Number of tiles kept ahead of the player. |
| `tilesWithoutObstacles` | 3 | First N tiles spawned at game start are clean. |

When a tile passes behind the player it gets recycled to the front of the queue, and `ActivateRandomObstacle()` picks one of its obstacle children at random to enable (the rest are deactivated).

## Scoring + difficulty curve

```csharp
score += Time.deltaTime * IRPlayer.movementSpeed;
IRPlayer.movementSpeed = baseSpeed + (score / 500);
```

Every frame the score grows by `Δt × current speed`. Every frame the speed grows by `score / 500`. So it's a positive-feedback ramp — slow start, sudden acceleration. No score cap, no high-score persistence today (everything's runtime).

## Meter combat

When the player hits a `"Fight"`-tagged collider, [`MainGame`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/MainGame.cs) calls `startFight()` which awaits a [`MeterGame`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/ui/MeterGame.cs) result:

```csharp
public async Task<bool> Show() {
    // ...
    while (currentFill > 0f && currentFill < 1f) {
        if (Input.GetKey(KeyCode.Space)) currentFill += fillRate;
        else                              currentFill -= drainRate;
        await Task.Yield();
    }
    return currentFill >= 1f;
}
```

- Meter starts at `0.5` (half-full).
- Holding `Space` fills at `0.1 / frame`.
- Releasing drains at `0.5 / frame` — so the default drift is *down*; you have to actively push.
- Reach `1.0` → win, enemy is deactivated, game continues.
- Hit `0.0` → lose, `gameOver = true`, results screen.

## Customization

The player pikeman uses your saved colours and name from the [Customization menu](#customize). The colours load on `Start()` via [`PikemanCustomizer.Customize()`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/PikemanCustomizer.cs):

- `ColourOne` → hat / beret material colour.
- `ColourTwo` → torso cloth (shader property `_CLOTH4COLOR` on the `PT_Male_Peasant_01_upper` child).
- `ColourThree` → currently loaded but not applied visually (reserved).
