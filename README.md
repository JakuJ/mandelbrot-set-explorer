Mandelbrot Set viewer
=============
![](https://img.shields.io/github/license/JakuJ/mandelbrot-set-viewer.svg)

OpenTK based C# program which generates images of the Mandelbrot set. Zooming will take you as far as the floating point precision allows on your computer (10^-4.5 for single, 10^-13.5 for double precision).

![An example](./Examples/brot.png?raw=true "An example of what this can do")

Installation
------

For now, build and run the project solution in **Visual Studio** (works under **VS for Mac** at least). Currently there are two `MandelbrotSet` child classes you can use in `Main()` to render images:

* `OpenCLMandelbrot` (fast, under development)
* `ParallelMandelbrot` (slow, safe and reliable)

The OpenCL implementation selects available devices according to this order of importance:

1. GPUs supporting double precision
2. CPUs supporting double precision
3. GPUs supporting single precision

If there are no OpenCL-ready GPUs, then the CPUs are used.
OpenCL implementation on the CPU is about 4x faster than `Parallel.For` implementation (tested on Intel® Core™ i5-5257U CPU @ 2.70GHz).

If OpenCL implementation doesn't work for you for some reason, use release mode to enable optimizations and set the target to `x64` for major speed improvements over `x86` while using `ParallelMandelbrot` CPU implementation.

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