# Getting started

[Documentation](README.md) · [Architecture](architecture.md) · [Development](development.md)

## Play the prototype

Visit [Grand Theft Tycoon on itch.io](https://ivanpavlov.itch.io/grand-theft-tycoon) for the browser game and public information. Unity is needed to work on the source project.

## Prerequisites

- **Unity Hub and Unity 6000.3.25f1**, recorded in [ProjectVersion.txt](../ProjectSettings/ProjectVersion.txt).
- **Git on PATH** and network access for first import: several Package Manager dependencies use Git URLs.
- The matching editor's **Web Build Support** module for browser builds.

The project uses URP 17.3.0, Input System 1.20.0, Cinemachine 3.1.5, Localization 1.5.9, and Test Framework 1.6.0. [manifest.json](../Packages/manifest.json) also includes SoftMask for uGUI, NorskaLib.Spreadsheets, Hot Reload, Unity Recorder, and editor tools. Bundled plugins include Odin Inspector/Validator and Pixelplacement Surge. Keep [packages-lock.json](../Packages/packages-lock.json) and restore the existing dependencies before changing versions to fix import errors.

## Open and run

1. Obtain the project, retaining `Assets`, `Packages`, `ProjectSettings`, and Unity `.meta` files.
2. In Unity Hub, **Add project from disk** and select the repository root.
3. Open with the matching editor and wait for package resolution and asset import.
4. Open [Intro.unity](../Assets/CarSeller/Scenes/Intro.unity).
5. Press **Play**. The intro waits briefly before activating City Scene.

[IntroScene](../Assets/CarSeller/Scripts/Runtime/Systems/Intro/IntroScene.cs) loads **scene index 1**. Keep this ordering in the active build scene list:

| Index | Enabled scene |
| --- | --- |
| 0 | `Assets/CarSeller/Scenes/Intro.unity` |
| 1 | `Assets/CarSeller/Scenes/City Scene.unity` |

The current [EditorBuildSettings.asset](../ProjectSettings/EditorBuildSettings.asset) enables these two scenes. Append other scenes when the selected config loads them by name.

## Editor scene selection

The [Default Scene Loader](../Assets/CommonScripts/Editor/DefaultSceneLoader.cs) is enabled by default and starts Play mode from the first enabled build scene unless you select another.

- **Tools → Default Scene Loader → Select Scene…** selects an enabled build scene.
- **Tools → Default Scene Loader → Enabled**, or **Ctrl/Cmd + Shift + D**, toggles it.

This preference is stored per editor user. Check it if Play starts somewhere other than the open scene.

## Scene reference

All of these live in [Assets/CarSeller/Scenes](../Assets/CarSeller/Scenes):

| Scene | Purpose |
| --- | --- |
| `Intro` | Entry point and transition to City Scene. |
| `City Scene` | Main city gameplay. |
| `Warehouse North`, `Warehouse South`, `Warehouse South Second` | Warehouse scenes for configurations that use them. |
| `Car Shop` | Dedicated scene used by the car-shop configuration. |
| `[Editor]City Builder` | Road graph and city authoring. |
| `3D car builder`, `Prefab Builder` | Content-building scenes. |
| `[Editor]City Builder[OLD]` | Older authoring scene. |

Opening a warehouse or authoring scene alone does not bootstrap the entire game.

## Controls

Click/tap to inspect; drag the controlled vehicle to move, take a car, or interact with a destination. The current camera code also supports dragging empty map space, mouse-wheel zoom, and pinch zoom when enabled. UI can block map gestures.

[CarSeller_Inputs.inputactions](../Assets/CarSeller/Resources/Inputs/CarSeller_Inputs.inputactions) declares these additional bindings:

| Binding | Source behavior |
| --- | --- |
| Escape | Pause; Escape in the UI action map resumes. |
| Ctrl + R | Restart, subscribed by GameManager only in the Unity editor. |
| WASD / arrows / left stick | Declared Move action; the showcased vehicle interaction uses pointing and dragging. |

Declared actions do not guarantee implemented player behavior. Check listeners before treating other bindings as controls.

## Developer cheats and debug controls

These shortcuts use the **Player** action map and the debug components in [Core.prefab](../Assets/CarSeller/Resources/Prefabs/Core/Core.prefab). [DebugCommands](../Assets/CarSeller/Scripts/Runtime/%5BToSort%5D/DebugCommands.cs) compiles its handlers when `UNITY_EDITOR || DEBUG` is defined. Availability in a standalone build depends on its compilation symbols and included components. Pausing switches to the UI action map.

| Input | Action | Behavior |
| --- | --- | --- |
| **Left Shift**; gamepad left-stick press; XR trigger | Sprint | Toggle simulation speed between **1× and 10×**. Press again to return to normal; this is a toggle, not a hold-to-speed-up control with the current input setup. |
| **A** | A | Spawn a car at the currently controlled vehicle's city position. Requires an initialized run. |
| **S** | S | Spawn an unrestricted buyer at the currently controlled vehicle's city position. Requires an initialized run. |
| **L** | L | Toggle the visibility cheat: make tracked visibility aspects visible and mark them discovered. Turning it off does not undo discovery. |
| **W** | W | Toggle highlighting for all city areas. Requires CityAreasVisualsController in the scene. |
| **Q** | Q | Toggle CarFlexibleJunctionPolicy.IgnoreRules. Currently that policy returns all outgoing edges before its rule checks, so this flag has no effect on those checks. |
| **Ctrl + R** | Restart | Restart through scene index 0. GameManager subscribes to this only under `UNITY_EDITOR`. |
| **C**; gamepad east button | Crouch | Under `DEBUG`, DebugCustomActions invokes a click on the first Interactable found. This is a debug hook; the object is not selected by the pointer. |

**D** and **Space / gamepad south button** also have debug hooks, but their current handlers are empty.

The current Core prefab uses PlayerInput **Send Messages**, and these actions are buttons. The installed Input System forwards performed button events rather than release/canceled events in this mode, which is why the toggles change once per press. The speed multiplier comes from [GameManager](../Assets/CarSeller/Scripts/Runtime/Global/GameManager.cs); it changes simulation time, rather than just vehicle speed.


## Make a browser build

1. Install Web Build Support for this editor through Unity Hub.
2. Open **File → Build Profiles**, select/create a Web profile, and switch to it.
3. Keep Intro at index 0 and City Scene at index 1. Append warehouses/shop scenes needed by the active config.
4. Build into `Builds/Web` (Git-ignored).
5. Test through a local HTTP server or the hosting platform's preview, including stealing, selling, shopping, and scene transitions.

[BuildIncrementor](../Assets/CommonScripts/Editor/BuildIncrementor.cs) increments `Assets/Resources/Build.asset` and changes `PlayerSettings.bundleVersion` during builds. Review those asset/settings changes afterward.

## Troubleshooting

| Symptom | Check |
| --- | --- |
| Package restore fails | Git on PATH, network access, and the Git URLs in the manifest. Read the Package Manager error first. |
| Missing plugin types | Included plugin folders and successful import of their assemblies. |
| Intro loads incorrectly | City Scene is enabled at index 1 in the active build profile. |
| Play ignores the open scene | Default Scene Loader selection. |
| Warehouse/shop cannot load | Enable the scene named by the active config. Warehouses load using WarehouseConfig.Name. |
| Null services after opening a scene directly | Start from Intro; inspect ServicedMain and GameMainConfig references. |
| Variant fails to resolve | Base, fallback base, slot configuration and override flags. |
| Source system is absent in gameplay | Active config, scene components, initialization and event wiring. |

This guide was checked against source and assets; a fresh Unity import and Web build have not been verified as part of the documentation change.
