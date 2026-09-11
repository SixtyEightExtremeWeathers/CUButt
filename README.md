# CUButt

CUButt connects *Casualties: Unknown* to Intiface Central and turns character injuries and conditions into vibration patterns.

## Installation

1. Install [BepInEx 5](https://github.com/BepInEx/BepInEx).
2. Copy `CUButt.dll` into the `BepInEx/plugins` folder.
3. Install [Intiface Central](https://intiface.com/) and start the server.
4. Connect your device.
5. Launch *Casualties: Unknown* and enjoy.

## Main Features

CUButt converts various character conditions and events into vibration patterns:

- Pain level
- Pain spikes
- Pulsing for bleeding
- Fast heartbeat-like patterns for fibrillation and dying
- Chaotic vibration for radiation and electrical injuries
- Short feedback after falling
- Vibration during earthquakes
- Vibration stops upon death
- Strong vibration when hit by the sound cannon

### Wholesome Mode

The settings allow you to switch the mod to **Wholesome Mode**.

In this mode, vibration intensity depends on how happy Expie is.

## Configuration

The configuration file is located at:

`Casualties Unknown Demo\BepInEx\config\seew\.casualtiesunknown.cubutt.cfg`

Available settings:

- **WholesomeMode** — enables Wholesome Mode, where vibration depends on Expie's happiness.
- **GlobalMultiplier** — multiplies the intensity of all vibrations by this value.
- **PainMultiplier** — multiplies the intensity of pain vibrations by this value.

## Requirements

- [BepInEx 5](https://github.com/BepInEx/BepInEx)
- [Intiface Central](https://intiface.com/)

## For Modders

If you want to add CUButt support to your own mod, check the [CUButt API Guide](https://github.com/SixtyEightExtremeWeathers/CUButt/wiki).

## Credits

- Intiface developers
- Idea by AlexashaSur
- Testers: @thatonelad7227