# Simple Game Template System

A lightweight Unity game template system that provides a basic foundation for level-based games with Addressables integration, Resources fallback, and Singleton pattern.

## Overview

This template system consists of four core components that work together to create a simple game management solution:

1. **GameManager** - Singleton game controller and state manager
2. **LevelManager** - Singleton level progression, unlocking, and reload functionality
3. **LevelLoader** - Singleton level data loading using Addressables or Resources
4. **Level** - Basic level instance with state management
5. **LevelData** - Simple level configuration data structure with unlock requirements
6. **PlayerData** - Player progress persistence with settings

## Architecture

```
GameManager (Singleton)
├── LevelManager (Singleton)
│   └── LevelLoader (Singleton)
│       └── LevelData (Configuration)
└── Level (Active Instance)
    └── LevelData (Runtime Data)
```

## Core Components

### GameManager (Singleton)
The central controller that manages basic game state and player data.

**Key Features:**
- Singleton pattern with automatic initialization
- Game state management (Initializing, Playing, Paused, GameOver, etc.)
- Player data persistence using PlayerPrefs
- Settings management (audio, vibration)
- Auto-save functionality
- Event-driven communication

**Usage:**
```csharp
// Access the singleton instance
var gameManager = GameManager.Instance;

// Start the game
gameManager.StartGame();

// Pause/Resume
gameManager.PauseGame();
gameManager.ResumeGame();

// Save/Load
gameManager.SaveGame();
gameManager.LoadGame();

// Access player data and settings
var playerData = gameManager.PlayerData;
playerData.SetMasterVolume(0.8f);
playerData.SetVibrationEnabled(false);
```

### LevelManager (Singleton)
Manages level progression, unlocking, and provides reload functionality.

**Key Features:**
- Singleton pattern
- Level loading and unloading
- Level progression (next/previous)
- Level unlocking system
- Level reloading with transition delays
- Event-driven level state management

**Usage:**
```csharp
// Access the singleton instance
var levelManager = LevelManager.Instance;

// Start a specific level (checks if unlocked)
bool started = levelManager.StartLevel(0); // Returns true if unlocked

// Navigate levels
bool nextLoaded = levelManager.LoadNextLevel(); // Returns true if unlocked
levelManager.LoadPreviousLevel();

// Reload current level
levelManager.ReloadCurrentLevel();

// Check unlock status
bool isUnlocked = levelManager.IsLevelUnlocked(1);
var unlockedLevels = levelManager.GetUnlockedLevels();
```

### LevelLoader (Singleton)
Handles loading levels from Addressables or Resources.

**Key Features:**
- Singleton pattern
- Dual loading support (Addressables + Resources)
- Level data caching
- Level instance creation
- Unlock requirement checking
- Fallback loading mechanism

**Usage:**
```csharp
// Access the singleton instance
var levelLoader = LevelLoader.Instance;

// Load all levels
levelLoader.LoadAllLevels();

// Get level data
var levelData = levelLoader.GetLevelById(0);

// Get unlocked levels
var unlockedLevels = levelLoader.GetUnlockedLevels(playerData);

// Create level instance
levelLoader.CreateLevel(levelData, parent, (level) => {
    // Level created callback
});
```

### Level
Basic level instance with state management and event handling.

**Key Features:**
- Level state management (NotStarted, Playing, Completed, Failed)
- Event-driven state changes
- Flexible base class for game-specific implementations

**Usage:**
```csharp
// Get the current level
var level = FindObjectOfType<Level>();

// Control level state
level.StartLevel();
level.CompleteLevel();
level.FailLevel();
level.RestartLevel();

// Check level state
bool isCompleted = level.IsCompleted();
bool isFailed = level.IsFailed();
bool isFinished = level.IsFinished();
```

### LevelData
Simple level configuration data structure with unlock requirements.

**Key Features:**
- Basic level information (ID, name)
- Dual loading support (Addressables + Resources)
- Unlock requirements system
- JSON serialization support

**Usage:**
```csharp
// Create level data with unlock requirement
var levelData = new LevelData(1, "Level 1", "Levels/Level1", "Levels/Level1", 0);

// Check if level can be unlocked
bool canUnlock = levelData.CanBeUnlocked(playerData);

// Get loading path based on method
string path = levelData.GetLoadingPath();

// Serialize to JSON
string json = levelData.ToJson();
LevelData loadedData = LevelData.FromJson(json);
```

### PlayerData
Player progress and settings persistence.

**Key Features:**
- Level progress tracking
- Game statistics
- Settings management (audio, vibration)
- Automatic level unlocking
- Save date management

**Usage:**
```csharp
// Create new player data
var playerData = new PlayerData();

// Update progress (automatically unlocks next level)
playerData.CompleteLevel(0);
playerData.SetCurrentLevel(1);

// Check progress
bool isCompleted = playerData.IsLevelCompleted(0);
bool isUnlocked = playerData.IsLevelUnlocked(1);

// Manage settings
playerData.SetMasterVolume(0.8f);
playerData.SetMusicVolume(0.6f);
playerData.SetSFXVolume(0.7f);
playerData.SetVibrationEnabled(true);
playerData.SetMusicEnabled(false);

// Get effective volumes
float musicVol = playerData.GetEffectiveMusicVolume();
float sfxVol = playerData.GetEffectiveSFXVolume();

// Get statistics
float winRate = playerData.GetWinRate();
string playTime = playerData.GetFormattedPlayTime();
```

## Level Unlocking System

The template includes a simple but flexible level unlocking system:

### Automatic Unlocking
- When a level is completed, the next level is automatically unlocked
- First level (ID 0) is unlocked by default
- Unlock requirements can be customized per level

### Custom Unlock Requirements
```csharp
// Level 0: Unlocked by default
var level0 = new LevelData(0, "Tutorial", "Levels/Tutorial", "Levels/Tutorial", -1);

// Level 1: Requires level 0 to be completed
var level1 = new LevelData(1, "Level 1", "Levels/Level1", "Levels/Level1", 0);

// Level 2: Requires level 1 to be completed
var level2 = new LevelData(2, "Level 2", "Levels/Level2", "Levels/Level2", 1);
```

### Checking Unlock Status
```csharp
// Check if level can be unlocked
bool canUnlock = levelData.CanBeUnlocked(playerData);

// Check if level is unlocked
bool isUnlocked = playerData.IsLevelUnlocked(levelId);

// Get all unlocked levels
var unlockedLevels = levelLoader.GetUnlockedLevels(playerData);
```

## Dual Loading Support

The system supports both Addressables and Resources for flexible development:

### Loading Methods
- **Addressables**: Production-ready, efficient resource management
- **Resources**: Quick prototyping, easy setup

### Configuration
```csharp
// Level data with both paths
var levelData = new LevelData(0, "Tutorial", "Levels/Tutorial", "Levels/Tutorial");

// Set loading method
levelData.loadingMethod = LoadingMethod.Addressable; // or LoadingMethod.Resource
```

### Fallback Mechanism
- LevelLoader tries the preferred method first
- Automatically falls back to the other method if the first fails
- Configurable default loading method

## Event System Integration

The template system integrates with the TitoX EventSystem, providing comprehensive event-driven communication:

### Game Events
- `GameInitializedEvent` - Game initialization complete
- `GameStartedEvent` - Game started
- `GamePausedEvent` - Game paused
- `GameResumedEvent` - Game resumed
- `GameEndedEvent` - Game ended
- `GameSavedEvent` - Game saved
- `GameLoadedEvent` - Game loaded

### Level Events
- `LevelInitializedEvent` - Level initialized
- `LevelStartedEvent` - Level started
- `LevelCompletedEvent` - Level completed
- `LevelFailedEvent` - Level failed
- `LevelRestartedEvent` - Level restarted

### Level Manager Events
- `LevelManagerLevelStartedEvent` - Level started by manager
- `LevelManagerLevelCompletedEvent` - Level completed by manager
- `LevelManagerLevelFailedEvent` - Level failed by manager
- `LevelManagerLevelReloadedEvent` - Level reloaded by manager
- `LevelManagerLevelUnlockedEvent` - Level unlocked

### Level Loader Events
- `LevelsLoadedEvent` - All levels loaded
- `LevelCreatedEvent` - Level instance created

**Event Usage Example:**
```csharp
public class GameUI : MonoBehaviourEventListener<GameStartedEvent>
{
    public override void OnEventTriggered(GameStartedEvent e)
    {
        // Show game UI
        ShowGameUI();
    }
}

public class LevelUI : MonoBehaviourEventListener<LevelCompletedEvent>
{
    public override void OnEventTriggered(LevelCompletedEvent e)
    {
        // Show level completion UI
        ShowLevelCompleteUI(e.Level);
    }
}

public class UnlockUI : MonoBehaviourEventListener<LevelManagerLevelUnlockedEvent>
{
    public override void OnEventTriggered(LevelManagerLevelUnlockedEvent e)
    {
        // Show level unlock notification
        ShowUnlockNotification(e.LevelId);
    }
}
```

## Setup Instructions

### 1. Basic Setup
1. Add the required components to your scene:
   - GameManager
   - LevelManager
   - LevelLoader

2. The components will automatically initialize as singletons.

### 2. Loading Method Setup

**For Addressables:**
1. Create a LevelDataCollection ScriptableObject with your level data
2. Add it to your Addressables with the address "LevelsCollection"
3. Create level prefabs and add them to Addressables
4. Set the dataObjectAddress in your LevelData to match the prefab addresses

**For Resources:**
1. Create a LevelDataCollection ScriptableObject with your level data
2. Place it in the Resources folder at "Levels/LevelDataCollection"
3. Create level prefabs and place them in the Resources folder
4. Set the resourcePath in your LevelData to match the prefab paths

### 3. Level Data Setup
Create level data with unlock requirements:

```csharp
// Tutorial level (unlocked by default)
var tutorialLevel = new LevelData(0, "Tutorial", "Levels/Tutorial", "Levels/Tutorial", -1);

// Regular level (requires previous level)
var level1 = new LevelData(1, "Level 1", "Levels/Level1", "Levels/Level1", 0);
```

### 4. Level Prefab Setup
1. Create a GameObject with the Level component
2. Configure the level behavior
3. Save as a prefab and add to your chosen loading method
4. Reference the prefab path in LevelData

### 5. Event Listening
Subscribe to events in your components:

```csharp
public class MyGameComponent : MonoBehaviourEventListener<GameStartedEvent>
{
    public override void OnEventTriggered(GameStartedEvent e)
    {
        // Handle game start
    }
}
```

## Flow Example

Here's a typical game flow with unlocking:

1. **Game Start:**
   ```
   GameManager.Instance.Initialize() 
   → Loads PlayerData 
   → Triggers GameInitializedEvent
   ```

2. **Level Loading:**
   ```
   GameManager.Instance.StartGame() 
   → LevelManager.Instance.StartLevelManager() 
   → LevelLoader.Instance.LoadAllLevels() 
   → LevelManager loads first unlocked level 
   → LevelLoader.Instance.CreateLevel() 
   → Level.Initialize() 
   → Level.StartLevel()
   → Triggers LevelStartedEvent
   ```

3. **Level Completion:**
   ```
   Level.CompleteLevel() 
   → Triggers LevelCompletedEvent 
   → LevelManager.OnLevelCompleted() 
   → Updates PlayerData 
   → Unlocks next level
   → Triggers LevelManagerLevelUnlockedEvent
   → Triggers LevelManagerLevelCompletedEvent
   ```

4. **Level Unlocking:**
   ```
   PlayerData.CompleteLevel(0) 
   → Unlocks level 1 
   → Next level can be started
   ```

5. **Level Reload:**
   ```
   LevelManager.Instance.ReloadCurrentLevel() 
   → Cleanup current level 
   → Create new level instance 
   → Initialize and start 
   → Triggers LevelManagerLevelReloadedEvent
   ```

## Best Practices

### 1. Event Usage
- Use events for loose coupling between components
- Subscribe to events in `OnEnable()` and unsubscribe in `OnDisable()`
- Keep event handlers lightweight

### 2. Level Design
- Use meaningful level IDs and names
- Set appropriate unlock requirements
- Use Addressables for production, Resources for prototyping
- Keep level data simple and extensible

### 3. Data Management
- Use PlayerPrefs for simple data persistence
- Implement proper error handling for save/load operations
- Use auto-save features for better user experience

### 4. Performance
- Use Addressables for efficient resource loading
- Minimize allocations in update loops
- Use coroutines for asynchronous operations

### 5. Settings Management
- Use the built-in settings methods in PlayerData
- Apply settings changes immediately
- Save settings when they change

### 6. Debugging
- Enable logging during development
- Use meaningful event IDs for debugging
- Monitor event statistics for performance issues

## Extensions

The template system is designed to be extensible. Common extensions include:

### 1. Save System
Implement custom save systems by extending the save/load methods in GameManager.

### 2. Achievement System
Add achievement tracking by extending PlayerData and creating achievement events.

### 3. Analytics
Integrate analytics by listening to game and level events.

### 4. UI Integration
Create UI components that listen to game events for real-time updates.

### 5. Audio System
Integrate audio by listening to game state changes and level events.

### 6. Advanced Unlocking
Extend the unlock system with more complex requirements (score thresholds, achievements, etc.).

## Troubleshooting

### Common Issues

1. **Levels not loading:**
   - Check Addressables/Resources configuration
   - Verify level collection paths match
   - Check for build errors

2. **Levels not unlocking:**
   - Verify unlock requirements are set correctly
   - Check that previous levels are being completed
   - Ensure PlayerData is being saved properly

3. **Events not triggering:**
   - Ensure components are properly subscribed
   - Check event validation logic
   - Verify event system is enabled

4. **Save/Load issues:**
   - Check PlayerPrefs permissions
   - Verify JSON serialization
   - Implement proper error handling

5. **Performance issues:**
   - Monitor Addressables loading
   - Check for memory leaks
   - Optimize update loops

### Debug Tips

1. Enable logging in the EventSystem
2. Use the inspector to monitor component states
3. Check console for error messages
4. Use breakpoints to trace execution flow
5. Monitor Addressables loading performance

## License

This template system is part of the TitoX framework and follows the same licensing terms.

## Support

For issues and questions:
1. Check the troubleshooting section
2. Review the event system documentation
3. Examine the example implementations
4. Check Unity console for error messages 