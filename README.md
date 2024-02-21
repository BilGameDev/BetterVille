# BetterVille
# A Noise-based tile world generator

![Untitled design](https://github.com/BilGameDev/BetterVille/assets/107997032/fc1e0ea9-d58d-4b38-ab55-0c4cb3fcffb7)

This is another one of my personal projects. A noise based tile world generator that works quite similar to Minecraft worlds.

A random seed is assigned at the start and through that a perlin noise is generated. With the help of Object pooling, tiles in the scene are activated according to noise values.
Further code can then add edges and corners to make a visually appealing island using tiles.

In the showcase, I have applied an offset to the noise to procedurally generate land as the values change. This is called on Update with a limiter to avoid any performance issues.

This can be further developed to spawn trees, rocks, grass, props, creatures and even entire biomes based on different perlin noise scales and values. 

![Untitled desigdn](https://github.com/BilGameDev/BetterVille/assets/107997032/cc1daa77-9b98-4f93-9376-54aa122cbf15)


Because this is based on a seed, not only will the island be the same on each spawn, but even at world postions. Which means in a multiplayer scene, player can only generate the world around him, no matter what coordinates he may be. I thought that was cool and I wish to further develop this
