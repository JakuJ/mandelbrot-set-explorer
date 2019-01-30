Mandelbrot set viewer
=============
![](https://img.shields.io/github/license/JakuJ/mandelbrot-set-viewer.svg)

OpenTK based C# program which generates images of the Mandelbrot set. Zooming will take you as far as the `double` floating point type precision allows on your computer.

![An example](./Examples/math_is_beautiful.png?raw=true "Isn't math beautiful?")

Installation
------

Compile and run the project solution in **Visual Studio** (works under VS for Mac at least). Use release mode to enable optimizations and set the target to `x64` for major speed improvements over `x86`.

Usage
------

* Use the mouse wheel to control the resolution (smaller resolution - faster image generation)
* Click anywhere in the image to zoom to that sector
* The Escape key terminates the program

To Do:
-----

* Enable the user to control the zooming factor
* Add new coloring techniques
* Use an arbitrary floating point precision library to ~~allow for deeper zooming~~ go even further beyond!

More examples
----

![An example](./Examples/black_and_white.png?raw=true "A black and white rendering")
![An example](./Examples/swastika.png?raw=true "I think I've already seen this somewhere")
![An example](./Examples/minibrot.png?raw=true "A Minibrot - an example of fractal self-similarity")
![An example](./Examples/virus.png?raw=true "This one's shaped like some virus")
![An example](./Examples/rift.png?raw=true "A rift")