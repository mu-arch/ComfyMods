# Atlas

*Shrugged?*

## Changelog

### 1.4.0

  * Created `PluginConfig` and added several ZoneSystem-related config options.
  * Added a new Terminal.ConsoleCommand `setworldtime <time>`.
  * Added a `ZNet.LoadWorld` postfix-patch to log the m_netTime read from the world file.

### 1.3.0

  * Extract various patch code into separate classes.
  * Rewrite `ZDOMan.SaveAsync()` to bypass an un-needed byte array copy, add more accurate timing.

### 1.2.0

  * Created this `README.md`.
  * Added temporary ZoneSystem patch to bypass checks that result in locationInstances to be cleared and regenerated.

### 1.1.0

  * Rewrite `ZDOMan.Load()` to handle possible duplicate ZDOs (same ZDOID) that can occur with async saving.

### 1.0.0

  * Initial release.