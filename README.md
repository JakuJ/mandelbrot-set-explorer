Mandelbrot Set viewer
=============
![](https://img.shields.io/github/license/JakuJ/mandelbrot-set-viewer.svg)

OpenTK based C# program which generates images of the Mandelbrot set. Zooming will take you as far as the `double` floating point type precision allows on your computer.

![An example](./Examples/brot.png?raw=true "An example of what this can do")

Installation
------

Compile and run the project solution in **Visual Studio** (works under VS for Mac at least). Currently there are classes you can use in `Main()` to render images:

* `ParallelMandelbrot`
* `OpenCLMandelbrot`

Use release mode to enable optimizations and set the target to `x64` for major speed improvements over `x86` if using `ParallelMandelbrot` CPU implementation. This is the default mode used to generate these example images.

For now the OpenCL GPU implementation is in development and uses `float` type, which makes rendering 10x faster, but not nearly as deep as with double precision CPU-based approach (if you have an OpenCL-ready GPU, otherwise it will use the CPU as fallback).

Usage
------

* Click anywhere in the image to zoom in to that location
* Use the `Space` key to toggle between modes dictating the parameter your mouse wheel changes. Currently available modes include:
1. Image resolution
2. Zooming factor
3. Number of iterations per pixel
4. Iteration escape radius (changes coloring smoothness)
* Use the mouse wheel to control the the selected parameter
* The Escape key terminates the program

To Do:
-----

* Use an arbitrary floating point precision library to ~~go even further beyond!~~ allow for deeper zooming in OpenCL kernel.

More examples
----

![An example](./Examples/math_is_beautiful.png?raw=true "Isn't math beautiful?")
![An example](./Examples/black_and_white.png?raw=true "A black and white rendering")
![An example](./Examples/swastika.png?raw=true "I think I've already seen this somewhere")
![An example](./Examples/minibrot.png?raw=true "A Minibrot - an example of fractal self-similarity")
![An example](./Examples/virus.png?raw=true "This one's shaped like some virus")
![An example](./Examples/rift.png?raw=true "A rift")