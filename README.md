# InAppUpdateManager

A simple in-app update management system for Android apps in Unity projects. The key feature is the prioritization of updates through a parameter value, ideally retrieved from Firebase Remote Config.

## Important

**The update priority parameter is essential for this system to work properly.** Without specifying a priority value, the update system won't function as intended. I recommend using Firebase Remote Config to dynamically control update priorities without releasing a new version.

## Features

- ✅ Automatic update availability check
- ✅ Support for flexible and immediate updates
- ✅ Update prioritization

## Installation

1. Clone the repository or add it as a Git submodule
2. Make sure you have the Google Play Core package for Unity installed

## Usage

### Basic Usage

Add the InAppUpdateManager prefab to your scene from the package folder, instantiate it programmatically, or create it yourself by adding InAppUpdateManager to a GameObject. Then call it with an update priority value:

```csharp
// Get priority from Firebase Remote Config or other source
int updatePriority = ConfigsManager.GetUpdatePriority();

if (InAppUpdateSystem.InAppUpdateManager.Instance != null)
{
    yield return InAppUpdateSystem.InAppUpdateManager.Instance.CheckForUpdate(updatePriority);
}
```

For best results, call this in a coroutine during your game's loading sequence to ensure the update check completes before showing gameplay:

```csharp
IEnumerator Start()
{
    // Get priority from Firebase Remote Config
    int updatePriority = ConfigsManager.GetUpdatePriority();
    
    // Check for updates first
    if (InAppUpdateSystem.InAppUpdateManager.Instance != null)
    {
        yield return InAppUpdateSystem.InAppUpdateManager.Instance.CheckForUpdate(updatePriority);
    }
    
    // Then load your game
    yield return LoadGame();
}
```

That's it! The system will automatically:
- Check for available updates
- Determine the appropriate update type
- Start the update process if available

### Update Priorities

- **Priority 4-5**: Immediate mandatory update
- **Priority 1-3**: Flexible background update
- **Priority 0**: No update

## Compatibility

- Safe to call on any platform (iOS, Editor, etc.), but work only on android.

## Future Improvements
- More priority levels (e.g. scale from 0 to 10)
- Track days since last update notification (for flexible updates).
