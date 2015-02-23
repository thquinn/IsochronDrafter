Isochron Drafter
================

A hacked-together way to draft custom Magic: the Gathering sets online.

## Features
* Draft custom Magic sets online with your friends!
* Graphical deck builder allows you to build your deck as you draft!
* That's it. [Screenshot.](http://i.imgur.com/ssYb7TB.jpg)

## Requirements
Windows and .NET 4.0, until someone wants to do a Mono port.

## What's Broken?
Well, a lot of stuff. For starters:
* Slow image downloading
* Little to no error handling when parsing set files, loading images, etc.
* Graphical glitches galore

## Coming Eventually
* More error handling
* More info about table order, who has how many packs during drafts
* Better image downloading

## How to Use
* Install the included Magic Set Editor script. (Put the magic-isochrondrafter.mse-export-template folder into the data folder of your MSE install.)
* Export your set from Magic Set Editor using the script. (File->Export->HTML...->Isochron Drafter Exporter)
* Export card images from your set -- filename must be "cardname.full.jpg".
* Upload the card images from your set somewhere.
* In Isochron Drafter, start a server. Browse to the exported set file and enter the remote image directory.
* Have everyone connect to the IP you're hosting the server on. (Make sure port 10024 is forwarded if you have connection issues.)
* Start the draft on the server.
* Enjoy.

## Notes
* Isochron Drafter was written by Tom Quinn, and is available under a [Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License](http://creativecommons.org/licenses/by-nc-sa/4.0/). It's a real hack job and not at all representative of my overall coding style or quality.
* It uses Craig Baird's [C# TCP Server](http://www.codeproject.com/Articles/488668/Csharp-TCP-Server) and skatamatic's [Event-Driven TCP Client](https://www.daniweb.com/software-development/csharp/code/422291/user-friendly-asynchronous-event-driven-tcp-client), each with some minor changes.
* The MSE export script is cobbled from from pieces of LuridTeaParty's [Cockatrice exporter](http://www.reddit.com/r/custommagic/comments/17d7gw/ive_made_a_script_for_mse_to_export_into/).
* Find my other stuff at my [portfolio site](http://cargocollective.com/tomquinn), and let me know what you think!
