# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2026-10-31

### Added
- `WGUI/Flow`: reusable puzzle UI flow (Loading → Home ⇄ Gameplay → Win | Lose) driven by `UIFlowController`
  and `UIFlowConfig`; screens request moves through the static `UIFlow` API. See `Runtime/WGUI/Flow/README.md`.
- Game hook interfaces: `IGameFlowHooks` (default `LevelManagerFlowHooks`), `IUIFeedback` / `UIFeedback`,
  `IAdsGate` / `UIAds`, `IScreenTransition`, `ILoadingScreen`.
- Base screens: `LoadingScreenBase`, `IrisTransitionScreen`, `HomeScreenBase`, `WinScreenBase`, `LoseScreenBase`,
  `SettingScreenBase`.
- `WGUI/Components` from Cats In Crates (same script GUIDs): `SettingToggle`, `ButtonContentTint`, `PanelAnimation`,
  `StaggeredReveal`, `IrisHole`, `MultiplierSlider`, `ScaleToCoverParent`, `ExtendToScreenBottom`,
  `BackgroundFullScreen`, `RewardIconRiseEffect`.
- `UIManager`: `Show(Type)` / `ShowAsync(Type)` / `Hide(Type)`, `HideAllPopupsAsync`, `IsVisible`, `TryGet`, `Config`.
- `UIConfigSO`: `ResolveType` (a base screen type resolves to its single registered subclass) and `TryGetType(AssetReference)`.

### Fixed
- `UIManager`: two screens requested while the same prefab was still spawning awaited one UniTask twice.

## [1.0.0] - 2024-01-01

### Added
- Initial release of Game Template
- Core game management systems (GameManager, LevelManager, PlayerData)
- Event system with EventManager and interfaces
- Level data system with JSON support
- Editor menu tools for package management
- Comprehensive documentation

### Features
- Game state management
- Level loading and management
- Player data persistence
- Event-driven architecture
- Editor tools for deleting example folder
- Documentation access through menu

### Technical
- Assembly definitions for proper package structure
- Unity 2022.3 compatibility
- TextMeshPro dependency
- Sample project included 