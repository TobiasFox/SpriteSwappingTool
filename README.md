# Sprite Swapping Tool

This tool is an Unity Editor plugin. It identifies and helps to sort overlapping and unsorted SpriteRenderers, since such renderers often lead to unwanted Sprite swaps (visual glitches).

[See video](./Video/SpriteSwappingPlugin_Demo.mp4)

It works currently with Unity Version 2019.3 or higher – 2D or 3D.

You can find binaries in the [binaries](./Binaries) folder.

## Features

- identifies Sprites which are overlapping and unsorted
- visual feedback to sort any found visual glitches
- using sorting criteria to generate a sorting order of visual sprites of a visual glitch 
- analyze sprites to use sorting criteria and more detailed sprite outlines as the Unity defined Bounds