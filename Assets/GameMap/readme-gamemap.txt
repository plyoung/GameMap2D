GameMap and Editor
by Leslie Young
http://forum.plyoung.com/


Introduction
------------

GameMap is a simple 2D grid based solution for keeping track of map data. It also includes an editor where you can quickly define new maps and tiles. The editor includes a map editor to easily "paint" tiles on the map.

A core feature of this kit is the ability to add fields to the asset, map and tile classes and be able to edit those fields in the map editor without having to make changes to the editor code; much like you would expect from scripts (components) shown in the Inspector.

This tool is useful for when you do not want to draw maps/levels (grid of tile sprites) directly in scenes, for example via Unity's new tile editor, and will rather place the sprites at runtime from map data or do something else with the map data at runtime.

What you do with the "map data" is up to you. This kit does not try to dictate how the grid of tiles are rendered or placed at runtime. It simply provides the editor to edit this data and make it available at runtime. As such this is a tool geared towards programmers or a team which has a programmer who will know what to do with the runtime data. Example of runtime use is included.

The editor also includes a simple Auto-tiles system. These are tiles which automatically changes depending on which other tiles are around them when you place a new tile. It does not support transition to other tile-sets though and corner tiles are not supported either.


Editor
------

The editor can be opened from menu: Window > GameMap Editor.


* Editor

Here you can make changes to editor settings.

Outside is Solid: This toggle allows you to set how auto-tiles should treat the "outside" if the map when calculating what tiles to place.

Auto-tile Size: Helps the editor determine how to render auto-tiles which are not using "full rect" sprites. You basically have to specify what size the tiles are, like 16, 32, etc. or leave it at 0 to not use this value.


* Assets

First you need to select or create a new Maps asset. This is the asset which will include all the maps and a reference to the tiles asset used by these maps. Then select or create a tiles asset to associate with the maps asset.

Now you should see options to define maps and tiles.


* Maps

Click on "select" button in the Maps area to create or select a map.

You will see an "ident" field, the "ren" button next to it allows you to change it. This name (or its ID) can be used to find the map at runtime. Each map is given a unique Integer ID which can't be changed.

The Size fields allows you to change the width and height of the map. Press "apply" to apply the new size. A map's 0x0 location is at the bottom-left with (width-1) x (height-1) position at the top-right.


* Layers

The layers of tiles in the selected map. In the editor grid the later layers apears behind tiles of layers above.

By default there is only one layer and this data is saved in GameMap.grid. This is known as the default layer or layer-0. The data for any addition layers you add are stored in GameMap.layers.


* Tiles

The tiles section allows you to define and select tiles to paint onto maps.

Each tile is given a unique Integer ID. This ID is added to the GameMap.grid when you paint tiles. -1 is used for "empty tiles" in the grid.

The Buttons in this area are as follow:

 - [x]: clear selected tiles. Left click in map editor canvas will remove tiles.
 - [<]: move selected tile left in the list of tiles.
 - [<]: move selected tile right in the list of tiles.
 - [+]: add a new tile.
 - [-]: remove selected tile.

The properties you see for editing will depend on what fields were added to the GameMapTile class.

To help visualise what a tile represents you can use a Sprite, Colour, Integer values, and String values. To tell the editor that a field should be used for this you simply add the [GameMapTilePreview] attribute to the field(s). Have a look at the source file of the GameMapTile class to learn more. By default a Color, Sprite, intVal, and strVal fields are included.


* Canvas

The editor canvas (a grid with black background) is where you can draw tiles onto the map. Click on a tile in the list of defined tiles to make it active, then click on the canvas to place the tile. You can also click-and-drag the mouse to place a series of tiles.

- Left-Click/Drag: Place tiles (or remove if no tile is selected).
- Right-Click/Drag: Remove tiles.
- Shift+Left-Click/Drag: Mark tiles in canvas.
- Del/Delete command: Delete marked tiles.
- Ctrl+C/Copy command: Copy marked tiles.
- Ctrl+V/Paste command: Make copied tiles available for pasting (move mouse and click in canvas to place the tiles).
- Esc/[x]: Cancel marking tiles.


Runtime
-------

Please have a look at the source code to learn more about what the runtime classes present.

**GameMapTilesAsset** has a list of defined tiles and is normally associated with a GameMapsAsset.

**GameMapsAsset** holds a list of all defined maps. It also has a reference to the tiles asset used by the maps.

**GameMap** represent one map. Its main property is the "grid" which is an array of Integer values. -1 is an "empty tile" while positive values relates to the GameMapTile.id (note, not the Index of a tile in GameMapTilesAsset). There are function to get direct references to the tile definition so that you do not have to look it up from the ID from the grid[]. You may also add additional properties to this class and edit the values in the map editor (only serializable fields will be presented in the editor).

**GameMapTile** is a tile definition. This is where you will place any properties about a tile needed by your game at runtime. These properties (any serializable fields) will be presented for editing in the Map editor.

