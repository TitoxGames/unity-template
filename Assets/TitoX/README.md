# Game Template

A comprehensive Unity game development template that provides essential systems for building games quickly and efficiently.

## Features

### Core Systems
- **GameManager**: Central game state management
- **LevelManager**: Level loading and management
- **PlayerData**: Player data persistence
- **LevelData**: Level configuration system
- **GameEvents**: Event-driven architecture

### Event System
- **EventManager**: Centralized event management
- **IEvent & IEventListener**: Event interface system
- **MonoBehaviourEventListener**: Easy event listening for MonoBehaviours

## Installation

1. Add this package to your Unity project via Package Manager
2. Import the samples if you want to see examples
3. Start using the systems in your game

## Quick Start

```csharp
// Initialize the game manager
GameManager.Instance.Initialize();

// Subscribe to events
EventManager.Instance.Subscribe<GameStartEvent>(OnGameStart);

// Load a level
LevelManager.Instance.LoadLevel("Level1");
```

## Documentation

See individual README files in each module for detailed documentation:
- [Core README](Core/README.md)
- [EventSystem README](EventSystem/README.md)

## License

This template is provided as-is for educational and development purposes. 