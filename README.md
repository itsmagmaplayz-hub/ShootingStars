## Please take it to your attention, that this README and the uploaded files are outdated. The next update to those files and the README are coming 06.03. at 21:00 (UTC+1)

# ShootingStars

A Brawl Stars-inspired game implementation in C# featuring Shooters with unique attacks, ultimate abilities, and various game modes.

## Features

### Shooters
- **Blastian** (Assault) - Explosive rapid-fire shooter
- **ShadowShell** (Sniper) - Precision long-range sniper
- **Inferno** (Caster) - Fire-based explosive caster
- **Icebound** (Caster) - Crowd control ice mage
- **ThunderStrike** (Caster) - Lightning elemental master
- **VoidWalker** (Assassin) - Burst damage dark assassin

### Game Mechanics

#### Attacks
- Each Shooter has a unique Normal Attack with:
  - Damage output
  - Range
  - Cooldown between uses
  - Special attack type (Projectile, Melee, Slow, etc.)

#### Ultimate Abilities
- More powerful attacks that charge during combat
- Require 100% charge to use
- Deal increased damage
- Have special effects:
  - **Explosion**: Area damage
  - **Slow**: Reduces enemy movement
  - **Stun**: Disables enemies temporarily
  - **Heal**: Restores health
  - **Shield**: Provides protection
  - **Blind**: Reduces accuracy
  - **Knockback**: Forces enemies back

### Game Modes

1. **Free for All** - Last player standing wins
2. **Team Battle** - Two teams eliminate each other
3. **King of the Hill** - Control center point for victory
4. **Last Shooter Standing** - Survival elimination mode
5. **Zone Domination** - Control multiple map zones
6. **Endurance Run** - Survive waves of enemies

### Map Features

Maps include:
- Dynamic obstacle generation
- Multiple spawn points
- Various terrain types:
  - Grass (walkable)
  - Walls (obstacles)
  - Water (hazardous)
  - Rocks (cover)
  - Bush (stealth areas)
  - Lava (damage zones)
  - Sand (slowing terrain)

### Map Types
- **Arena** - Balanced circular combat area
- **Jungle** - Dense forest with bushes
- **City** - Urban blocks for tactical gameplay
- **Ice** - Slippery frozen lake with ice patches

## Project Structure

```
ShootingStars/
├── Models/
│   ├── Position.cs         - Grid position system
│   ├── Attack.cs           - Attack definition and types
│   ├── Ultimate.cs         - Ultimate ability definition
│   ├── Shooter.cs          - Shooter class with factory
│   ├── Player.cs           - Player state and health management
│   ├── Tile.cs             - Map tile system
│   ├── Map.cs              - Map generation and management
│   └── GameMode.cs         - Game modes with factory
├── Game/
│   ├── MapGenerator.cs     - Procedural map generation
│   ├── GameState.cs        - Game state management and events
│   └── GameEngine.cs       - Main game loop and logic
├── UI/
│   └── GameUI.cs           - Console UI and display
├── Program.cs              - Entry point
└── ShootingStars.csproj    - Project configuration
```

## How to Run

```bash
cd ShootingStars
dotnet build
dotnet run
```

## Game Flow

1. Select a game mode
2. Choose Shooters for all players
3. Random map generation
4. Battle simulation with:
   - AI movement and positioning
   - Automatic attack targeting
   - Ultimate ability usage
5. Display real-time game state, map, and events
6. Game ends when one player remains or time expires

## Combat System

- **Distance-based targeting**: Characters can only hit enemies within range
- **Cooldown management**: Attacks have cooldowns between uses
- **Ultimate charging**: Charge builds with every attack used
- **Health system**: Players are eliminated at 0 HP
- **Event tracking**: All actions logged and displayed

## Extensibility

The project is designed to be easily extended:
- Add new Shooters via `ShooterFactory`
- Create new attack types
- Implement custom game modes
- Design custom map generation algorithms
- Add visual UI with WPF/Unity/MonoGame
