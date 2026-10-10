# HUDRevised

HUDRevised is a compact, Derail Valley-style driving HUD for Unity Mod Manager. It places live locomotive, signal, and train-consist information in a translucent overlay while you drive.

## Features

- Translucent industrial-style HUD with draggable positioning.
- Live locomotive data:
  - Altitude
  - Speed
  - Grade
  - Brake-pipe pressure
- Automatic speed units:
  - `mph` when Revised MPH is installed.
  - `km/h` when Revised MPH is not installed.
- Current and upcoming signal information when DVSignals is installed.
- Signal distance display.
- Train length and car-count display.
- Scrollable, multi-column train-consist list with each car's ID, derail stress,
  job ID, destination, and brake status (controlled by the car-list settings).
- Multiline car names for organized consist layouts.
- Optional display restriction so the HUD only appears while inside a locomotive.
- Position locking and a Reset position button.
- Cached signal/reflection lookups to reduce repeated work while the HUD is active.

## Installation

1. Close Derail Valley.
2. Install Unity Mod Manager for Derail Valley if it is not already installed.
3. Extract the contents of the release archive into:

   ```text
   Derail Valley/Mods/HUDRevised/
   ```

   The folder should contain:

   ```text
   HUDRevised.dll
   QuantitiesNet.dll
   info.json
   ```

4. Start the game and enable HUDRevised in Unity Mod Manager.

The built package is produced at `bin/Debug/netstandard2.0/HUDRevised-1.0.1.zip` when building this project.

## Compatible mods

HUDRevised is compatible with these Derail Valley mods:

- **DVCustomCarLoader** — supported in the HUD load order.
- **DVSignals** — adds current and upcoming signal information to the HUD when installed.
- **Revised_Mph** — changes the HUD speed display to `mph`. Without it, speed is shown in `km/h`.

All three are optional. HUDRevised can still display its driving and train information without them.

## Configuration

Open the HUDRevised settings in Unity Mod Manager. The active options include:

- Enable or disable driving-info, train-info, and signal sections.
- Choose the train-length unit: metres or feet.
- Show or hide the car list.
- Show the current signal, upcoming signal, and signal distance.
- Lock the HUD position.
- Show the HUD only while inside a locomotive.
- Enable diagnostic logging.
- Reset the HUD position with **Reset position**.

The driving-info provider order can be changed with the up/down controls. Disabled providers are not drawn.

## Multiline car names

The car list supports either real line breaks or the escaped `\\n` form in a car's naming scheme:

```text
L-001
BOXCAR
```

or:

```text
L-001\\nBOXCAR
```

Car cards reserve two lines for names so the columns stay aligned while showing identifiers and vehicle types.

## Derail Valley 99.7 compatibility

The current build uses the public 99.7-compatible data paths. Legacy locomotive-specific providers that depended on older game APIs are disabled until those APIs are stable. The supported live providers and train-consist display remain available.

## Troubleshooting

### The HUD does not appear

- Confirm HUDRevised is enabled in Unity Mod Manager.
- Confirm the HUD's `info.json` is inside the `HUDRevised` mod folder.
- Check that the HUD is enabled in its settings.
- If **Only show inside locomotive** is enabled, enter a locomotive.
- Use **Reset position** in the mod settings in case the HUD was moved off-screen.

### Speed shows km/h instead of mph

- Confirm the folder is named `Revised_Mph` or `RevisedMPH`.
- Confirm the folder contains the matching MPH DLL.
- Confirm the MPH `Info.json` uses the exact ID `Revised_Mph`.
- Fully restart Derail Valley after installing or changing the mod.

### Signals are missing

Install and enable DVSignals, then load into a world where signal objects are available. The HUD reports the DVSignals detection state in its Unity Mod Manager settings.

## Building from source

The project targets `.NET Standard 2.0` and references the Derail Valley managed assemblies configured in `HUDRevised.csproj`.

```powershell
dotnet build .\HUDRevised.csproj --no-restore
```

The build creates the DLL and release archive under `bin/Debug/netstandard2.0/`.

## Source and credits

Source code is available on [GitHub](https://github.com/matthewmaddox1993/dv-hud).

This project is based on the original DV HUD by **Miles Spielberg** (`mspielberg`), copyright 2020, and is distributed under the MIT license. Credit is also retained for **Zeibach**, who is listed as the original mod author in the mod metadata.

## License

See [LICENSE](LICENSE).
