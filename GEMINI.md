This is a mod for a Unity Mono game called MageQuit.
This uses BepInEx to inject, and Harmony to patch the game.

# About the game

A wizard brawler about spell drafting and beard envy, MageQuit allows up to 10 mages to battle online or locally. Beards grow longer with each kill; the wizard with the longest beard after 9 rounds wins!

FEATURES

    1-10 players local or online, featuring crossplay

    70 different spells

    Custom bots for online or offline play

    Anonymous skillbased or custom matchmaking

    Spell drafting

    Spell curving

    Full controller support

    Mouse and keyboard support

    Play as teams, free for all, or 1v1

    Dynamic original sound track 

WAYS TO PLAY MAGEQUIT

    Play offline with a group of up to 10

    Play solo in online quickplay or offline with bots

    Play online with a premade party of up to 10

    Join the online quickplay queue with a party of up to 5

    Play online with multiple players on the same PC 

# Documentation

Harmony documentation can be found in `./docs/harmony`.
The game's source code can be found in `./decompiled`. 


# Guidelines

Use the game's source code (`./decompiled`) to guide your patches.

Before implementing a patch, consult relevant articles in the Harmony documentation (`./docs/harmony`).

After implementing a patch, you should compile the application to verify its correctness.

Consider that patches should work for online mode, as well as offline mode.

## Harmony usage

Avoid using Traverse if possible:
- Each patch method (except a transpiler) can get all the arguments of the original method as well as the instance if the original method is not static and the return value.
- You only need to define the parameters you want to access.  
- See `docs/harmony/patching-injections.md` for details.

Always use nameof in the HarmonyPatch header if the method is not private.
- e.g. `[HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.DetermineBots))]` instead of `[HarmonyPatch(typeof(PlayerManager), "DetermineBots")]`

# Lessons Learned

## Networking
- **RPCs:** When modifying behavior that must be synchronized, use `photonView.RPC`. Always check if `PhotonNetwork.connected` and `photonView.isMine` before sending.

## Game Patterns
- **Damage Sources:** The game uses `int source` in damage methods. Values >= 0 usually correspond to the `SpellName` enum. Negative values represent environmental damage (e.g., Lava is `-4`).
- **Identity:** Most networked objects have an `Identity` component which tracks the `owner` (player ID) and `localOwner`.
- **Clones:** Always check `WizardController.isClone` when your logic should only apply to the "real" wizard and not illusions.