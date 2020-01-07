# TurboHUD plugin

This project was to extend an existing plugin (HeroIsNear.cs), so that it didn't communicate with other 3rd party tools via common methods (such as HUD or File updates).

Specifically, the client wanted to log registry entries depending on game state. The reason for this bizarre alteration is/was that TurboHUD started blocking interactions with the file system.

## Testability

This was an interesting product to test, as the stand-alone `.cs` file couldn't be run in isolation. In order to check it's functionality, it needed be run with TurboHUD and Diablo 3 active.

To further complicate things, most registry reviewers don't auto-update their displays every second. This meant that the plugin had to be gradually built up, and tested in slices, so that no actions would overwrite/obfuscate the results. 