Mandelbrot set viewer
=============
![](https://img.shields.io/github/license/JakuJ/mandelbrot-set-viewer.svg)
![](https://img.shields.io/github/repo-size/JakuJ/mandelbrot-set-viewer.svg)

OpenTK based C# program which generates images of the Mandelbrot set. Zooming will take you as far as the `double` floating point type precision allows on your computer.

Installation
------

Compile and run the project solution in Visual Studio. Use release mode to enable optimizations and set the target to x64 for major speed improvements over x86.

Usage
------

* Use the mouse wheel to control the resolution (smaller resolution - faster image generation)
* Click anywhere in the image to zoom to that sector
* The Escape key terminates the program

To Do:
-----

* Enable the user to control the zooming factor
* Use an arbitrary floating point precision library or write my own to allow for deeper zooms.