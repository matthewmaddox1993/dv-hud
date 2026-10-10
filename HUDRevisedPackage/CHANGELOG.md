# Changelog

All notable changes to HUDRevised are recorded here.

## [1.0.1]

### Added

- Live train-consist mass, including car, bogie, cargo, and resource mass.
- Per-car stress, job ID, destination, and brake readings controlled by the existing car-list options.
- Clear STOP, PROCEED, and REDUCE SPEED labels for current and upcoming DVSignals signals, with distance to each.

### Improved

- Reworked the car list into compact, aligned cards across multiple columns with a bounded vertical scroll area for long trains.
- Improved HUD contrast and spacing; driving and signal panels stack when horizontal space is limited.
- Kept the HUD within the screen when its saved position or size would otherwise clip it.
- Showed brake-pipe pressure in bar with a useful decimal precision by default.

### Fixed

- Cars beyond the previously visible list limit can now be reached by scrolling.
- Scrolling the car list no longer repeatedly resets the HUD window height, which caused flicker.
- The mass field no longer displays a permanent placeholder.
- Signal status now ignores opposing-direction controllers and follows DVSignals' active route/head instead of always reading the nearest controller's first signal.
- Per-car stress now measures the live `TrainStress.stress` value against the game's derail-stress threshold instead of displaying normally dormant derail buildup.
- Signal direction matching now follows DVSignals' actual controlled direction, preventing the HUD from reporting the aspect of the signal head facing the opposite way.
- When stationary, signal lookup now uses Derail Valley's normalized reverser neutral point so reverse-selected locomotives search in the correct direction.
- DVSignals aspects such as `NEXT_STOP` now use their actual passing restriction, preventing proceed aspects from being mislabeled as STOP.
- Removed the inactive upcoming-track-info controls from the F10 settings menu.

## [1.0.0]

### Added

- Derail Valley-style translucent cab HUD layout.
- Locomotive data panels for altitude, speed, grade, and brake-pipe pressure.
- Current and upcoming signal panels with optional distance display.
- Train length, car count, and consist car list.
- Support for real and escaped line breaks in consist car names.
- Automatic MPH/KPH selection based on Revised MPH installation.
- Fallback Revised MPH detection using the installed mod folder and DLL.
- HUD position locking and Reset position support.
- Optional inside-locomotive visibility mode.
- Diagnostic logging and DVSignals status reporting.

### Changed

- Reduced repeated signal discovery and reflection work by caching lookups and throttling scans.
- Updated the HUD styling with dark steel panels, amber headings, and translucent backgrounds.
- Updated the speed provider to use `mph` with Revised MPH and `km/h` otherwise.
- Updated the HUD load order to run after `Revised_Mph` when it is installed.
- Updated train-consist rows to expand for multiline names.
- Preserved compatibility with the Derail Valley 99.7 public API by disabling unstable legacy locomotive providers.

### Fixed

- HUD polling no longer performs expensive signal searches every frame.
- Revised MPH detection no longer depends exclusively on Unity Mod Manager registration timing.
- Literal escaped line breaks in vehicle names are converted into visible HUD line breaks.

## Earlier development

- Initial HUD provider registration and Unity Mod Manager settings integration.
- Initial train-consist and DVSignals overlay support.
