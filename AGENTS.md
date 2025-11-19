# Repository Guidelines

## Project Structure & Module Organization
Unity assets live in `Assets/`, with gameplay prefabs, materials, and shaders under `Assets/Charactor` and `Assets/Scalable Grid Prototype Materials`. Scenes used for daily work belong in `Assets/Scenes` (the default scene is `SampleScene.unity`). Global render, quality, and input defaults are in `ProjectSettings/`. Keep dependency versions and scoped registries updated in `Packages/manifest.json` and never hand-edit `Packages/packages-lock.json`. Avoid committing artifacts from `Library/`, `Temp/`, or `Logs/`; those directories remain editor-generated.

## Build, Test, and Development Commands
- `Unity -projectPath "$(pwd)" -batchmode -quit -buildWindows64Player Builds/PoC6.exe` produces a Windows player; adjust the output path per platform.
- `Unity -projectPath "$(pwd)" -batchmode -quit -executeMethod UnityEditor.SceneView.ShowCompileErrorNotification` performs a script recompile check without opening the Editor GUI.
- `Unity -projectPath "$(pwd)" -batchmode -quit -runTests -testPlatform EditMode -testResults Logs/editmode.xml` runs Edit Mode tests; swap `PlayMode` for runtime coverage.
Use Unity 6000.0.61f1 (per `ProjectVersion.txt`) for consistent serialization and shadergraph compatibility.

## Coding Style & Naming Conventions
Write C# scripts with 4-space indentation and UTF-8 without BOM. Class, struct, and public property names follow PascalCase; private serialized fields use `_camelCase`. Prefer `SerializeField` over `public` for inspector-exposed state. Group MonoBehaviour lifecycle methods (Awake, Start, Update) at the top, then custom methods, and keep each script tightly scoped to one behaviour. Run Rider/Visual Studio analyzers before committing to catch nullable or allocation violations.

## Testing Guidelines
Place Edit Mode tests in `Assets/Tests/EditMode` and Play Mode tests in `Assets/Tests/PlayMode`, mirroring the Assembly Definition structure used by gameplay scripts. Name fixtures `<Feature>Tests` and individual tests `Does_When_Should`. Aim for smoke coverage of key interaction scripts and at least one regression test whenever you fix a bug. Capture `Logs/*.xml` artifacts from Unity Test Runner in CI for traceability.

## Commit & Pull Request Guidelines
Recent history (`init`, `Initial commit`) shows short, imperative messages; continue that style with a clear verb (“Add grid shader tweak”, “Fix character material”). Reference the related Jira or GitHub issue ID when applicable. Pull requests should summarize high-level intent, attach editor or in-game screenshots/gifs for visual changes, and enumerate manual test steps. Include checkboxes for Edit/Play Mode test passes and note any follow-up work.

## Security & Configuration Tips
Always commit paired `.meta` files to maintain GUID stability. Secrets such as API keys belong in environment variables or Unity’s `ProjectSettings/Secrets` (ignored by Git). Verify that new packages are pinned to specific versions in `Packages/manifest.json` before sharing the project.
