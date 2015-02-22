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

## Coming Eventually
* Graphical bug fixes
* More error handling
* More info about table order, who has how many packs during drafts
* Resizable windows
* Better image downloading

## How to Use
* Export your set from Magic Set Editor using the script in the scripts/ directory.
* Export card images from your set -- filename must be "<cardname>.full.jpg".
* Upload the card images from your set somewhere.
* In Isochron Drafter, start a server, browse to the set file, enter the image directory.
* Have everyone connect to the IP you're hosting the server on.
* Start the draft on the server.
* Enjoy.

## Notes
* Isochron Drafter was written by Tom Quinn, and is available under a [Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License](http://creativecommons.org/licenses/by-nc-sa/4.0/).
* It uses Craig Baird's [C# TCP Server](http://www.codeproject.com/Articles/488668/Csharp-TCP-Server) and skatamatic's [Event-Driven TCP Client](https://www.daniweb.com/software-development/csharp/code/422291/user-friendly-asynchronous-event-driven-tcp-client), each with some minor changes.
* It is really hacked together, and not at all representative of my overall coding style or quality. Don't judge... ;_;
* Find my other stuff at my [portfolio site](http://cargocollective.com/tomquinn), and let me know what you think!
