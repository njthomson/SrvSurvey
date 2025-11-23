# ED3D-Galaxy-Map
ED3D-Galaxy-Map is a 3D galactic map (web app) for the game Elite: Dangerous.
Work with JSon file to embed stellar systems data.

## Demo
* [Elite: Dangerous Milky Way](http://en.ed-board.net/3Dgalaxy/)
* [3D Galnet News](http://en.ed-board.net/3Dgalnet/)
* [EDSM Dump test](http://en.ed-board.net/3Dedsm_test/)

## Features
* JSON file to fill the map.
* Manage click event to display information related to a system (if detailed infos, need to be in the JSON file).
* Display a global view of the galaxy that reproduces the ingame galaxy - Generated from a volumetric particle cloud from an ingame screenshot (a kind of heightmap).
* HUD with customizable categories to filter the map. [Optional]
* CSS+HTML HUD to fit to any style.
* Systems colorizations using color attribute that can be set on each category and soon use color from ingame stellar type (if stellar type defined in JSon file for each system). [Optional]
* Can show exploration routes.

## Requirement
Require Three.js (r75) & JQuery

## Options
**container:** The container ID. *(Required)*

**jsonPath:** The JSon containing systems data. *(Required)*

**jsonContainer:** Optionaly set an Id of container with JSon systems data. (Can be used on small project, JSon file stay better).

**json:** Optionaly set an object to load the systems data.

**basePath:** Custom base path

### Sprites
**starSprite:** Add a custom sprite image for stars, exemple for round stars: ```"textures/lensflare/star_round.png"``` (default: ```"textures/lensflare/star_grey2.png"```)

**systemColor:** Set custom color(Hex) for system's sprite *Default: #eeeeee*

**effectScaleSystem:** Set an array with min & max value for system scale effect (sprite size) on zoom in/out *Default: [10,800]*

### Camera
**startAnim:** Use initial animation (Zoom into the galaxy) *Default: true*

**playerPos: [x,y,z]** Player position, ex.:```[150,269,28]``` (used for initial camera target position)  *Default: [0,0,0]*

**cameraPos: [x,y,z]** Initial camera position, ex.:```[0,45000,-45000]``` (used for initial camera position)

### Hud
**withHudPanel:** Enable the HUD (panel to filter and navigate throughout systems). *Default: false*

**withOptionsPanel:** Enable the HUD (panel to filter and navigate throughout systems). *Default: false*

**hudMultipleSelect:** Enable showing multiple filters at same time *Default: true*

**withFullscreenToggle:** Enable button to toggle fullscreen mode *Default: false*

**popupDetail:** Popup mode for onclick details *Default: false*


### Labels
**showGalaxyInfos:** Show Milky Way info on start *Default: false*

**showNameNear:** Show labels close from the camera target (Default: false)

## Methods
**Ed3d.init({options}):** Initialize the scene

**Ed3d.rebuild():** Destroy & rebuild the scene (Hard way to refresh the scene to load new JSon data)


## System properties

* name
* coords
* infos
* url

## Event on click

$( document ).on( "systemClick", function( event, name, infos, url ) {

});

## Exemple
```
<!-- My container -->
<div id="edmap"></div>
<!-- jQuery -->
<script src="https://code.jquery.com/jquery-2.1.4.min.js"></script>
<!-- Three.js -->
<script src="https://cdnjs.cloudflare.com/ajax/libs/three.js/r73/three.min.js"></script>
<!-- Launch ED3Dmap -->
<link href="css/styles.css" rel="stylesheet" type="text/css" />
<script src="js/ed3dmap.min.js"></script>
<script type="text/javascript">
  Ed3d.init({
      container   : 'edmap',
      jsonPath    : "./map/system.json"
  });
</script>
```
## JSon sample
### Simple systems list
```
[
  {
    "name": "Sol",
    "coords": {
      "x": 0,
      "y": 0,
      "z": 0
    }
  },
  {
    "name": "Solati",
    "coords": {
      "x": 66.53125,
      "y": 29.1875,
      "z": 34.6875
    }
  }
]
```
### Systems list with routes
```
{
  "systems": [
    {
      "name": "Wolf 906",
      "coords": {
          "x": "-27",
          "y": "-25",
          "z": "29"
      }
    },
    {
      "name": "Checkpoint 1",
      "coords": {
          "x": "-6644",
          "y": "-21",
          "z": "11738"
      }
    },
    {
      "name": "Checkpoint 2",
      "coords": {
          "x": "-10534",
          "y": "-21",
          "z": "16384"
      }
    },
    {
      "name": "Checkpoint 3",
      "coords": {
          "x": "-4969",
          "y": "-21",
          "z": "22262"
      }
    },
    {
      "name": "Checkpoint 4",
      "coords": {
          "x": "3553",
          "y": "-21",
          "z": "8732"
      }
    },
    {
      "name": "Checkpoint 5",
      "coords": {
          "x": "6297",
          "y": "-21",
          "z": "14626"
      }
    },
    {
      "name": "Checkpoint 6",
      "coords": {
          "x": "18975",
          "y": "-21",
          "z": "25676"
      }
    },
    {
      "name": "Sagittarius A*",
      "coords": {
          "x": "25",
          "y": "-21",
          "z": "25900"
      }
    }
  ],
  "routes": [
    {
      "title":"1st test route",
      "points": [
        {"s":"Wolf 906","label":"My home"},
        {"s":"Checkpoint 1"},
        {"s":"Checkpoint 2","label":"Great discovery here"},
        {"s":"Checkpoint 3"},
        {"s":"Sagittarius A*","label":"Center of the galaxy"}
      ]
    },
    {
      "title":"2nd test route",
      "points": [
        {"s":"Checkpoint 4"},
        {"s":"Checkpoint 5"},
        {"s":"Checkpoint 6"}
      ]
    }
  ]
}
```
