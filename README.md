# MapGenerator - still in progress
Map Generator is a script I wrote in C# to procedural generation hexagonal map to TBS game. It was designed to integrate with the Unity Engine, and can be used in mainly strategic games and other which needs dynamic hexagonal map generation.</br>
The goal of this script is to generate maps for my game concept, "Hexiv", which will be mainly TBS mobile game.</br>
(Currently, it only generates terrain shapes, without biomes and only the Archipelago and Lakesoptions work correctly. The rest of configurations aren's satisfy me.)
## Generation Method
1. [Perlin noise](https://en.wikipedia.org/wiki/Perlin_noise) - I'm using a perlin noise with some modifications like octaves, frequency and amplitude to get an expected results. Also I wrote a pin-pong modification to make rivers, but I didn's used it yet.
2. [Voronoi Diagram](https://en.wikipedia.org/wiki/Voronoi_diagram) - I use logic from Voronoi diagram generation, but with modifications. Instead of creating the entire diagram, I'm creating some points with coordinates and for each center of hex, I'm finding the nearest "Voronoi point" and assign that point attribute to hex.
# Configurations
## Map Generation Options
1. Seed
2. Map Size
* Duel (44x26)
* Tiny (60x38)
* Small (74x46)
* Standard (84x54)
* Large (96x60)
4. Map Type
* Continents - a few massive continents
* Pangea - one supercontinent
* Archipelago - many little islands
* Small Continents - more, but smaller continents
* Continents and Islands - continents, but more little islands
* Lakes - mainly land, but with some water reservoirs
## Config in Unity Engine
There is a settings panel to configure map options and test Perlin Noise parameters</br></br>
<img src="MapGitHub/Settings.png" width="200">
# Visualization
I'm showing each map type with each size.
## Archipelago
<img src="MapGitHub/Archipelago_Large.png" width="350"> <img src="MapGitHub/Archipelago_Standard.png" width="350">
<img src="MapGitHub/Archipelago_Small.png" width="350"> <img src="MapGitHub/Archipelago_Tiny.png" width="350">
<img src="MapGitHub/Archipelago_Duel.png" width="350">
## Lakes
<img src="MapGitHub/Lakes_Large.png" width="350"> <img src="MapGitHub/Lakes_Standard.png" width="350">
<img src="MapGitHub/Lakes_Small.png" width="350"> <img src="MapGitHub/Lakes_Tiny.png" width="350">
<img src="MapGitHub/Lakes_Duel.png" width="350">
## Pangea - I need to correct (Tiny isn't even pangea)
<img src="MapGitHub/Pangea_Large.png" width="350"> <img src="MapGitHub/Pangea_Standard.png" width="350">
<img src="MapGitHub/Pangea_Small.png" width="350"> <img src="MapGitHub/Pangea_Tiny_.png" width="350">
<img src="MapGitHub/Pangea_Duel.png" width="350">
## The Rest
Continents - I will use some maths to assign correctly attributes to Voronoi Points and next mix with Perlin Noise.</br>
Small Continents - I will need to modify that math to Voronoi to generate smaller continents, and then mix with Perlin Noise.</br>
Continents and Islands - I will generate Continents and Archipelago, then combine them.</br>
## Biomes 
Once all types work correctly, I will generate more Voronoi Points, and assign attributes calculates by some maths - honestly I haven't searched yet
