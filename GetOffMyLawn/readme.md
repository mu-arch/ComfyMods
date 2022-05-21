# GetOffMyLawn

  * Set the health of any player building piece in the game to a configurable value.
    * The default value is set to 100,000,000.
  * You can also download the mod at:
    * Nexus Mods: https://www.nexusmods.com/valheim/mods/1349
    * Thunderstore: https://valheim.thunderstore.io/package/ComfyMods/GetOffMyLawn/

## User Interface

  * This fuctionality is available on repaired objects or placed build objects.
  * Set the health of the currently targeted build object to a high value by repairing it.
    * Note: build object must be either unwarded or on a ward the user can access).

## Usage Notes

- Health will be set when you place the item or when you use the repair hammer on it. You can only change the health of building pieces that you own or are permitted on.
- You can activate a ward to set the health value of all building pieces within range to the configured value.
- You can use this mod to ignore stability. Press F1 and find "GetOffMyLawn". Change the PieceValue by pressing "ctrl+A" and then just hold down the 1 key until you see it turn into 1.111111E+17 at current maximum. This makes makes it "ignore stability". *Warning:*stability in the game helps keep buildings smaller to reduce lag. When you cheat stability you can create larger buildings, but this means more lag because more instances of objects. Keep an eye on your instances with F2 if this is a concern for you. (Why this works: Stability ticks down HP over time. With high enough health it just takes such a long time to tick down(years) it doesn't matter.)
- Great for boats. There are often "lag" issues with the game that cause boats to take more damage than they're supposed to. Just set your boat to high health. 
- All items placed with the hammer have their health changed. If you're using a mod that spawns things with the hammer such as ore please keep in mind the ore will likely have high health. Simply disable GOML in the configuration manager if you're trying to place normal health ore veins for example.
- Disabling the mod does not change the health of previously placed/repaired pieces. If you want to lower the health again you'll need to set the health value to low in GOML and repair the piece or activate a ward and everything in the ward radius will be set to that health.

## Manual Install Instructions

  1. Extract `GetOffMyLawn.dll` to your `/Valheim/BepInEx/plugins/` folder.
  2. Place a new building piece or repair a building piece to set its health value to the configured value.
  3. Activate a ward to set the health value of all building pieces within range to the configured value.

## Changelog

### 1.3.1
  * Destroy Carts & Boats with the Hammer like regular build pieces

### 1.2.1

  * Actually check the `enablePieceHealthDamageThreshold` config value for the WearNTear.ApplyDamage() patch.

### 1.2.0

  * Added new optimization/configuration option `enablePieceHealthDamageThreshold`.
    * Pieces with health that exceed 100K **will not** execute `WearNTear.ApplyDamage()` meaning they will not
      take any piece damage. Subsequently, they **will not** send a `WNTHealthChanged` message to the server.
    * This reduces the overall send and receive rates for every player on the server as they will no longer receive
      the message used only for syncing the visual condition of pieces across clients.

### 1.0.1

  * Added null-checks for Piece and Piece.ZNetView references in the ward-interaction method.

### 1.0.0

  * Updated for Hearth & Home.
