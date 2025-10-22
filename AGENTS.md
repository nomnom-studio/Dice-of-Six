# Repository Guidelines

## Project Structure & Module Organization
- Core gameplay scripts live in `Assets/_Project/Core`; feature-specific namespaces mirror the folder layout (e.g., `Core.Rolling`, `Core.Progression`).
- Balancing assets and ScriptableObjects are stored in `Assets/_Project/DesignData`; update these instead of hard-coding constants.
- UI prefabs, canvases, and TMP assets live in `Assets/_Project/UI` and `Assets/TextMesh Pro`.
- Art, audio, and third-party packages stay in their respective top-level `Assets` folders; avoid committing to `Library`, `Logs`, or `UserSettings`.
- Scenes are grouped under `Assets/Scenes`; keep the active prototype scene in sync with prefab changes.

## Build, Test, and Development Commands
- Launch development via Unity Hub targeting the Unity 6 (URP) editor used in this repo.
- For quick C# iteration, open `Dice of Six.sln` in Visual Studio or Rider; `Assembly-CSharp.csproj` mirrors the runtime assembly.
- Batch-mode build (Windows example):\
  `Unity -batchmode -nographics -quit -projectPath "$(pwd)" -buildTarget Win64 -executeMethod BuildScripts.BuildWindows` — ensure the static build method exists before running CI.
- Regenerate input assets after editing `Assets/InputSystem_Actions.inputactions` with `Unity -quit -batchmode -projectPath "$(pwd)" -executeMethod UnityEditor.InputSystem.InputActionAssetEditor.GenerateWrapperCode`.

## Coding Style & Naming Conventions
- Adopt Unity C# defaults: 4-space indentation, `PascalCase` for public types/methods, `camelCase` for locals, and `[SerializeField] private` fields when exposing inspector data.
- Use explicit namespaces that mirror folder hierarchy (`Nomnom.DiceOfSix.Core.*` recommended).
- Prefer ScriptableObjects for tunable data; keep MonoBehaviour scripts lightweight and event-driven.
- Run the built-in formatter (`Ctrl+K, Ctrl+D` in Visual Studio) before committing; Rider users should enable the Unity inspection profile.

## Testing Guidelines
- Place Edit Mode tests under `Assets/_Project/Core/Tests/EditMode`; Play Mode tests go under `.../Tests/PlayMode`.
- Run tests in-editor via `Window > General > Test Runner`.
- CLI test run (Edit Mode):\
  `Unity -batchmode -quit -projectPath "$(pwd)" -runTests -testPlatform editmode`.
- Target ≥80% coverage on new systems; document gaps in the PR if lower.

## Commit & Pull Request Guidelines
- Follow the existing short, present-tense subject style (`Update README.md`, `Link GitHub commit history...`); keep to ≤72 characters.
- One logical change per commit; include references to relevant assets or scenes in the body if needed.
- PRs must describe gameplay impact, affected scenes/prefabs, and test evidence (screenshot, video, or Test Runner output). Link Jira/Trello tickets when available.
- Request review before merging to `main`; obtain a green Unity CI run when batch builds are configured.
