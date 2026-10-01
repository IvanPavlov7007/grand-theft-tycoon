# Development

[Documentation](README.md) · [Getting started](getting-started.md) · [Architecture](architecture.md)

## Repository layout

```text
Assets/CarSeller/
  Scenes/                   Gameplay and authoring scenes
  Scripts/Runtime/          Main game assembly
  Scripts/Editor/           Tools and validators
  Resources/                Configs, database, artwork and inputs
  Tests/Editor/             Config resolver tests
Assets/CommonScripts/       Shared utilities and editor helpers
Assets/Plugins/             Bundled plugins
Packages/                   Manifest and dependency lock
ProjectSettings/            Editor and build settings
Krita/                      Source artwork and concepts
Recordings/                 Local footage (Git-ignored)
docs/                       Guides and tracked previews
```

Keep assets and their .meta files together. Rename/move assets through Unity to preserve GUID references. Generated caches, solutions, builds and raw recordings are ignored.

## Change gameplay configuration

1. Select `Assets/CarSeller/Resources/ScriptableObjects/[Main]/GameMainConfig.asset`.
2. Follow GameConfig, currently `[SimplifiedGP]GameplayConfig` under `Bootstrap/CityConfigs/SimplifiedGameplay`.
3. Inspect the city, economy, vehicle-controller and database references.
4. Duplicate configs through Unity for an experiment and assign the new config deliberately.
5. Enable any named scenes it requires, keeping Intro and City Scene at indices 0 and 1.
6. Run through Intro and exercise the changed flow.

Economy configs provide starting possessions and warehouse, car-spawn and shop offers. Balancing lives in [GameDatabaseContainer.asset](../Assets/CarSeller/Resources/Database/GameDatabaseContainer.asset), with import/model code in ToSort/Database/GameDatabaseContainer.cs. Preserve area IDs and corresponding level-list fields when updating spreadsheet-derived content.

## Add or vary a vehicle

1. Duplicate a matching asset under `Resources/ScriptableObjects/Core/Products/Bases`.
2. Configure its frame and intended part slots.
3. Duplicate/create a variant under `Core/Products/Variants` for optional changes. Set both override flags and values.
4. Provide valid fallback bases where required. ForceFallback changes base selection; a missing fallback can leave a slot empty, while missing required frame/base data can fail resolution.
5. Connect the vehicle to its intended spawn, offer or personal-vehicle config, including view/icon/prefab references.
6. Run resolver tests and exercise its representation and affected interactions.

The [variant validator](../Assets/CarSeller/Scripts/Editor/VariantConfigFallbackValidator.cs) warns when forced fallback has no base. Interpret it in the context of intended slot behavior.

## Author roads and destinations

1. Open `Assets/CarSeller/Scenes/[Editor]City Builder.unity`.
2. Inspect the graph root's RoadNodeAuthor, RoadEdgeAuthor, CityMarkerAuthor, CityAreaAuthor and TrafficLightAuthor components.
3. Edit geometry and data, keeping IDs and references consistent.
4. Select the root and use **City → Bake Graph From Selection**.
5. Save the CityGraphAsset and assign it to the intended CityConfig.
6. Run from Intro. Verify travel across edited edges, intersections, objectives, marker positions and area boundaries.

Other tools include **Tools → CarSeller → City → Connect Selected Road Nodes** and **City → Generate Test Graph (17x34)**. Use the test generator in a scratch scene. The [OLD] scene is a historical alternative.

## Extend a gameplay action

Trace an existing interaction through its profile, IProcess, rules, transaction and UI feedback. Keep UI availability and execution rules consistent. Register new transaction handlers in G.Initialize; a transaction class alone is insufficient. Use model services for relocation/ownership and GameEvents for notifications.

Use live processes rather than the obsolete GameFlowManager's commented workflows. Pair event subscriptions/unsubscriptions, and exercise scene changes and restarting.

See [Developer cheats and debug controls](getting-started.md#developer-cheats-and-debug-controls) for shortcuts used during testing.

## Run existing tests

[ConfigResolverTest.cs](../Assets/CarSeller/Tests/Editor/ConfigResolverTest.cs) belongs to the `Came.Core.EditorTests` assembly. It covers bases, variants, fallback, overrides, frames, slots and missing configuration.

Open **Window → General → Test Runner**, select **EditMode**, and run that assembly. Test Framework samples under Assets/Samples are separate examples.

For an unattended run with the editor closed, adjust the executable path to your installation:

```powershell
$unityEditor = 'C:\Program Files\Unity\Hub\Editor\6000.3.25f1\Editor\Unity.exe'
New-Item -ItemType Directory -Force -Path '.utmp/test-results' | Out-Null
& $unityEditor -batchmode -nographics -projectPath (Get-Location).Path `
    -runTests -testPlatform EditMode -assemblyNames Came.Core.EditorTests `
    -testResults '.utmp/test-results/editmode.xml' `
    -logFile '.utmp/test-results/editmode.log'
```

Inspect XML and logs for success. This command is documentation, not a reported test result from this change.

## Verification

| Change | Useful checks |
| --- | --- |
| Resolver or variants | Existing EditMode tests and the affected car view. |
| Economy or interaction | Inspect, drag, complete/reject, check money and control. |
| Scene flow | Intro, enter/exit, pause/resume and restart. |
| Road/marker data | Re-bake, traverse edges, reach objectives, inspect visibility. |
| Browser build | Actual Web build served through HTTP, with target input. |
| README media | Local links, GIF decoding/frames and Git inclusion. |

Review git status before committing. Imports and builds can modify serialized assets; include changes only when they belong to the task.
