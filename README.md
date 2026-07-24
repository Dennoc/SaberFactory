<p align="center">
    <h1 align="center">Saber Factory</h1>
</p>

<p align="center">
    <a alt="total downloads">
        <img src="https://img.shields.io/github/downloads/Dennoo11/SaberFactory/total" /></a>
    <a href="https://github.com/ToniMacaroni/SaberFactory/releases" alt="latest version">
        <img src="https://img.shields.io/github/v/tag/Dennoo11/SaberFactory?label=version" /></a>
</p>
<p align="center">
    <h4 align="center">A highly customizable saber mod for Beat Saber</h4>
</p>

<p align="center">
    <h4 align="center">A fork that revives the SaberFactory mod by ToniMacaroni</h4>
</p>

</br>
</br>


## What is Saber Factory?
Simply said: An all-rounder when it comes to sabers.

Combine different saber parts like lego pieces.  
Everything is built around customization.  
Change the shape, shaders, material properties, textures and more of parts and sabers.

**You can use and customize both parts and custom sabers in saber factory**

**Underground releases**

[Saber Factory 2.5.4 1.29.0](https://github.com/Dennoo11/SaberFactory/tree/2.5.4-1.29/assets/Saber%20Factory%201.29%20Releases).

## How do I install it
1) Download the first zip from [Here](https://github.com/Dennoo11/SaberFactory/releases)
2) Unpack it in your Beat Saber directory

## I have a saber that's broken and it appears in the left eye 
Beat Saber 1.30+ switch to Single-Pass Instanced (SPI) which broke most sabers since they were compiled in Single-Pass Stero (SPS)
this was removed in Unity 2020.1+ you will have to get [AssetBundleLoadingTools](https://beatmods.com/mods/306) and enable Muilt-Pass in the mod settings.

**For Saber Modelers**
if you want to miragte your saber to Unity 2021+ Single-Pass Instanced (SPI) you can read more here https://bsmg.wiki/models/shader-migration.html

## I want to create a saber
I highly recommend watching [this tutorial](https://www.youtube.com/watch?v=YqpcNTpzW4A).  
The unity project can be found [here](https://github.com/ToniMacaroni/AssetCreationProject)


## I have made a noodle map where the player moves and I want the trails to not stretch
By default if the player moves in a noodle map the trail behaves like a real trail and becomes longer the faster the player moves.
Sometimes you might have a different vission for your map or the map is less playable with such a long trail.
If you don't know what I'm talking about, take a look at this comparison: https://www.youtube.com/watch?v=UP0SqtMcr1g  
You can enable "relative movement of the trail to the player" by using a settable settings in your map like this:
```
"_difficultyBeatmaps": [
        {
          "_difficulty": "Expert",
          ...
          "_customData": {
            "_requirements": [
              "Chroma",
              "Noodle Extensions"
            ],
            "_settings": {
              "_saberFactory": {
                "_relativeTrailMode": true
              }
            },
          }
        }
      ]
```
The settings group is `_saberFactory` and the field is `_relativeTrailMode` (which can be either `true` or `false`)
