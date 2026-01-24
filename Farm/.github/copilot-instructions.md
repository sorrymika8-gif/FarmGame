# Copilot Instructions for FarmGame

## 基本规则
你必须遵守user_custom_rule.instructions.md的要求。

## Project Overview
This is a Unity project ("FarmGame") integrating a farm simulation with LLM-based AI Agents. 
The architecture relies on **QFramework** for core systems and custom **Singleton Managers** for game logic.

**Key Tech Stack**:
- **QFramework**: Architecture & Managers (MonoSingleton).
- **UniTask**: Async/Await operations (Preferred over Coroutines).
- **Unity Sentis**: AI Model inference.

## 1. Critical Architecture

### Manager Pattern
- **Initialization**: Centralized in `BootManager.cs`. It initializes other managers sequentially (`ResourceManager` -> `LLMService` -> `UIManager` -> etc.).
- **Singletons**: Use `MonoSingleton<T>` for all manager classes.
- **Location**: 
  - Infrastructure: `Assets/Scripts/Core` (`ResourceManager`, `UIManager`, `BootManager`)
  - Game Logic: `Assets/Scripts/Game`, `Assets/Scripts/Map`, `Assets/Scripts/Movement`

### QFramework Integration
- **Resources**: Do NOT use `Resources.Load` directly. Use `FarmGame.Core.ResourceManager`, which wraps QFramework's `ResKit` (`ResLoader`).
- **UI**: Do NOT use native uGUI instantiation directly. Use `FarmGame.Core.UIManager`, which wraps QFramework's `UIKit`.

### LLM System
- **Layering**:
  - `FarmGame.GameLLM`: Low-level service and client factory (in `Assets/Scripts/Core/GameLLM`).
  - `LLMCore` (Concept): The "Brain" logic, memory, and decision making (in `Assets/Scripts/LLMCore`).
- **Service**: Access LLM capabilities via `LLMService.Instance.Client` (Pure C# Singleton), initialized by `BootManager`.

## 2. Coding Standards & Conventions

### Language Requirements
- **Output Language**: **Chinese (Simplified)**. All code comments, explanations, and chat responses must be in Chinese.
- **Persona**: Adhere to any persona rules defined in `user_custom_rule.instructions.md` or `AGENTS.md`.

### Code Style
- **Namespaces**: Always wrap code in namespaces `FarmGame.<Module>` (e.g., `FarmGame.Core`, `FarmGame.Map`).
- **Async**: Use `UniTask` and `await` for asynchronous logic. Avoid Unity Coroutines where possible.
- **Fields**: Use `m` prefix for private fields (e.g., `private bool mIsInitialized;`).
- **Documentation**: Use XML documentation (`/// <summary>`) for all public classes, methods, and fields.
- **Properties**: Use PascalCase for public properties.

## 3. Workflow & Best Practices
- **Scene Flow**: The game starts from `Init.scene` which holds the `BootManager`.
- **Map Loading**: Use `GameManager.Instance.EnterScene(mapName, spawnPos)` instead of `SceneManager.LoadScene`. This handles Player creation and Camera binding.
- **Editing Logic**: When modifying Managers, check `BootManager.Initialize()` to ensure proper startup order.
- **AI Memory**: Use `MemoryBrain` (e.g., `AddMemoryAsync`) for AI data persistence.
- **Data Safety**: LLM "Brain" logic should be decoupled from the raw LLM "Service" implementation.

## 4. Directory Structure Map
- `Assets/Scripts/Core` - System frameworks (Boot, Res, UI).
- `Assets/Scripts/Game` - High-level game loop.
- `Assets/Scripts/LLMCore` - AI Agent "Brain", Memory, Prompt logic.
- `Assets/Scripts/Core/GameLLM` - LLM API integration layer.
- `Assets/Scripts/Map` - Grid/Tile management.0
