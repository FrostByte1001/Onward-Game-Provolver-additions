# Onward ForceTube Pistol Plugin v2.0.0 - by FrostByte

This mod just adds ForceTube haptic feedback support for pistols in Onward VR.  This has flexible per-device channel configuration.

When I purchased my ProVolver, I had already had a ForceTube for a couple of years.  The rifles would of course work with my original ForceTube, but pistols had no haptics.  This I thought was sensible for a single ForceTube.   But I was very disappointed when my new Provolver had exactly the same level of support.  In the leadup to Christmas, I found it was not as difficult to get working as I had always assumed.

This is probably unnecessary, but I will suggest you keep a second Onward install if you play multi-player.   I play standalone most of the time these days. There is a non-zero risk you will get a ban during multi-player play if you play with ANY mod installed.  I dont know how the games anti-cheat software works...it might simply be a checksum on the games binaries and DLLs.  If its as simple as that, then ANY addition to the games code (which is what this mod is) would get flagged as cheating.   Even though it only adds haptics.  Though I might be overthinking this - the risk is on you!

## Features

- ✅ **Pistol Support** - Adds haptic feedback for pistols using ProVolver or other ForceTube devices
- ✅ **Flexible Device Assignment** - Configure up to 3 devices with custom channel assignments
- ✅ **Per-Device Channel Control** - Each device can trigger for rifles, pistols, or both
- ✅ **Auto-Detection** - Automatically detects and adds new devices to config
- ✅ **User-Editable Config** - configuration file

## Requirements

- **Onward VR** (tested on latest version)
- **BepInEx 6.0.0-be.741** (IL2CPP version)
- **Updated ForceTubeVR_API_x64.dll** (see Installation, requires a recent version, which is included)
- **ForceTube devices** (rifle and/or pistol)

## Installation Manual

### 1. Install BepInEx
1. Download BepInEx 6.0.0-be.741 (IL2CPP, x64) from [BepInEx releases](https://github.com/BepInEx/BepInEx/releases)
2. Extract to your Onward game folder: `SteamLibrary\steamapps\common\Onward\`
3. Run the game once to generate BepInEx folders

### 2. Unzip the zip

Unzip this zip into the root folder of your Onward Installation folder and allow ForceTubeVR_API_x64.dll to be overwritten.

The games version of ForceTubeVR_API_x64.dll is quite old.  To support the seperate channels for Provolver(s) + ForceTube one needs the newer DLL from ProtubeVR.

Optionally, copy each element by hand :-

#### 2.1 Update ForceTube DLL
**IMPORTANT:** The stock Onward ForceTube DLL is outdated and lacks channel management functions.

1. **Backup the original DLL:**
   ```
   Onward\Onward_Data\Plugins\x86_64\ForceTubeVR_API_x64.dll
   ```

2. **Replace with newer DLL from ProTubeVR Companion App (can be downloaded on steam for free):**
   - Locate: `ProTubeVR Companion-App\build\ForceTubeVR_API_x64.dll`
   - Copy over: `Onward\Onward_Data\Plugins\x86_64\ForceTubeVR_API_x64.dll`

   **OR** use the included `ForceTubeVR_API_x64.dll` from this release zip.

#### 2.2. Install this mods Bepinex Plugin
1. Copy `OnwardForceTubePistolPlugin.dll` to:
   ```
   Onward\BepInEx\plugins\
   ```

### 3 Configuration

2. Start the game and equip any weapon

3. The config file will be auto-generated at:
   ```
   Onward\BepInEx\config\com.forcetube.onward.pistol.cfg
   ```

## Configuration

### First Run Setup

1. **Start Onward** and equip any weapon
2. **Check the log** at `BepInEx\LogOutput.log` to see detected devices
3. **Exit the game**
4. **Edit config** at `BepInEx\config\com.forcetube.onward.pistol.cfg`

### Example Configuration

#### Default (rifles and pistols separate):
```ini
[Device_1]
DeviceID = ForceTubeVR 1356051586
Channels = 2,3
FriendlyName = My Rifle Device

[Device_2]
DeviceID = ForceTubeVR 1319491275
Channels = 4,5
FriendlyName = My ProVolver
```
**Result:** Rifles trigger Device_1 only, pistols trigger Device_2 only

#### Combined Power (rifles trigger all devices):
```ini
[Device_1]
DeviceID = ForceTubeVR 1356051586
Channels = 2,3
FriendlyName = My Rifle Device

[Device_2]
DeviceID = ForceTubeVR 1319491275
Channels = 4,5,2,3
FriendlyName = My ProVolver
```
**Result:** Rifles trigger BOTH devices, pistols trigger Device_2 only

#### Left/Right Pistols:
```ini
[Device_1]
Channels = 2,3

[Device_2]
Channels = 4
FriendlyName = Left Pistol

[Device_3]
Channels = 5
FriendlyName = Right Pistol
```
**Result:** Rifles trigger Device_1, left-hand pistols trigger Device_2, right-hand pistols trigger Device_3

### Channel Reference

| Channel | Name | Triggers When |
|---------|------|---------------|
| 2 | RifleButt | In-game rifles/shotguns/LMGs fire |
| 3 | RifleBolt | In-game rifles/shotguns/LMGs fire |
| 4 | Pistol1 | In-game pistols fire (left hand) |
| 5 | Pistol2 | In-game pistols fire (right hand) |

**Examples:**
- `Channels = 2,3` - Device triggers only for rifles
- `Channels = 4,5` - Device triggers only for pistols
- `Channels = 2,3,4,5` - Device triggers for all weapons
- `Channels = 4` - Device triggers only for left-hand pistols

### Test Mode

To identify which device ID corresponds to which physical device:

1. Edit config:
   ```ini
   [Advanced]
   TestModeOnStartup = true
   TestRumbleDuration = 2      #note: I dont recall testinmg this recently...might not work
   ```

2. Start the game

3. Each device will rumble for 2 seconds with identification logged

4. Note which physical device rumbles and update config accordingly

## Troubleshooting

### No Haptics for Pistols
- Check `BepInEx\LogOutput.log` for device detection
- Verify Device_2 has `Channels = 4,5` (or includes 4,5)
- Ensure newer ForceTubeVR_API_x64.dll is installed

### Rifles Don't Work Anymore
- Check if you assigned pistol-only channels to rifle device
- Rifle device should have `Channels = 2,3`

### Config Not Generated
- Ensure you equipped a weapon in-game (triggers channel configuration)
- Check `BepInEx\LogOutput.log` for errors

### New Device Not Detected
- Device will be auto-added on next game startup
- Check log to confirm detection
- Manually add to empty Device_3 slot if needed

## How It Works

### Channel Assignment
The ForceTube API uses numbered channels (2-7). This plugin:
1. Detects connected devices on first weapon equip
2. Reads your config file
3. Assigns each device to specified channels using `AddToChannel()` API
4. When you fire a rifle, the game sends haptics to channels 2,3
5. When you fire a pistol, our plugin sends haptics to channels 4,5
6. Devices respond if they're assigned to those channels

### Plugin Architecture
- **Plugin.cs** - Main plugin, BepInEx initialization, channel configuration
- **PistolForceTubeHandler.cs** - Component that subscribes to pistol firing events
- **ForceTubeExtendedAPI.cs** - P/Invoke wrappers for newer DLL functions
- **DeviceConfig.cs** - BepInEx configuration management

## Version History

### v2.0.0 (2025-12-13)
- Added BepInEx configuration system
- Flexible per-device channel assignment
- Support for up to 3 devices
- Auto-detection of new devices
- Test mode for device identification
- Verbose logging option

### v1.0.0 (Initial)
- Basic pistol haptic support
- Hardcoded channel assignments

## Credits

- **Downpoor Interactive for the Onward game itself** - Its a pity they stopped adding to it.
- **ForceTube/ProTube VR** - For the haptic hardware and API
- **BepInEx** - For the framework to insert the mod
- FrostByte as mod author

## License

This is a personal mod for Onward VR. Free to use, but at your own risk.  You should not modify nor distribute the mod without written permission from the author.  Consult the author FrostByte on Github https://github.com/FrostByte1001/Onward-Game-Provolver-additions.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

## Support

For issues or questions, check:
1. `BepInEx\LogOutput.log` for detailed logging
2. Ensure `VerboseLogging = true` in config for maximum detail
3. Verify ForceTubeVR_API_x64.dll has been updated

