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

# Harmony tricks

Avoid using Traverse if possible:
- Each patch method (except a transpiler) can get all the arguments of the original method as well as the instance if the original method is not static and the return value.
- You only need to define the parameters you want to access.  
- See `docs/harmony/patching-injections.md` for details.