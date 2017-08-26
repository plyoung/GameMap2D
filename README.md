# GameMap2D

Wanna tip me? Get it on [Unity Asset Store](https://assetstore.unity.com/packages/tools/gamemap-94641)

GameMap is a simple 2D grid based solution for creating and keeping track of game map/ level data. It includes an editor where you can quickly define new maps and tiles. The editor has a map editor to easily "paint" tiles on the map.
GameMap is a simple 2D grid based solution for creating and keeping track of game map/ level data in Unity. It includes an editor where you can quickly define new maps and tiles. The editor has a map editor to easily "paint" tiles on the map.

A core feature of this kit is the ability to add fields to the asset, map and tile classes and be able to edit those fields in the map editor without having to make changes to the editor code; much like you would expect from scripts (components) shown in the Inspector.

This tool is useful for when you do not want to draw maps/levels (grid of tile sprites) directly in scenes, for example via Unity's new tile editor, and will rather place the sprites at runtime from map data or do something else with the map data at runtime.

What you do with the "map data" is up to you. This kit does not try to dictate how the grid of tiles are rendered or placed at runtime. It simply provides the editor to edit this data and make it available at runtime. As such this is a tool geared towards programmers or a team which has a programmer who will know what to do with the runtime data. Example of runtime use is included.

The editor also includes a simple Auto-tiles system. These are tiles which automatically changes depending on which other tiles are placed around them. It does not support transition to other tile-sets though. It supports both 16-tile and 47-tile systems.

![](https://user-images.githubusercontent.com/837362/29742219-52a1e686-8a7b-11e7-9659-a32e8d8bd323.png)
