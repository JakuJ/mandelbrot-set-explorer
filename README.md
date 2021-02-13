Mandelbrot Set explorer
=============
![License - MIT](https://img.shields.io/github/license/JakuJ/mandelbrot-set-explorer.svg)

OpenTK–based C# program that generates images of the Mandelbrot set.
Zooming will take you as far as the floating point precision allows on your computer (10^-4.5 zoom level for single, 10^-13.5 for double precision).

![](./Examples/titular.png?raw=true)

Installation
------

Run `make` in `Mandelbrot/OpenCL` folder to compile the native DLL which allows for parallel rendering on GPU/CPU using OpenCL.
The DLL is copied to the output directory automatically by the build system.

Build and run the project using the `dotnet` utility:

```shell
dotnet build  # to build
dotnet run    # build and run
```

By default, the program uses OpenCL to render images.
If for some reason this method cannot be used, it will fall back to using a `Parallel.For`–based approach. 

The OpenCL implementation selects available devices according to this order of importance:

1. GPUs supporting double precision
2. CPUs supporting double precision
3. GPUs supporting single precision

If there are no OpenCL-ready GPUs, then the CPUs are used.
There are two OpenCL kernels used for rendering, one based on an `unsigned char` buffer and one based on `image2d`.
The former is used with CPUs, the later with GPUs, to benefit from GPU image processing power.

OpenCL implementation on the CPU is about 4x faster than the `Parallel.For`–based implementation (tested on Intel® Core™ i5-5257U CPU @ 2.70GHz).

If the OpenCL implementation doesn't work for you for some reason, use release mode to enable optimizations and set the target to `x64` for major speed improvements over `x86` while using the `Parallel.For`–based implementation.

Usage
------

* Left click anywhere in the image to zoom in on that location, right click to zoom out
* Use the `Space` key to toggle between modes dictating the parameter your mouse wheel changes. Currently available modes are:
1. Zooming factor
2. Image resolution
3. Number of iterations per pixel
4. Iteration escape radius (changes coloring smoothness).
* Use the mouse wheel to control the selected parameter.
* Press S to save the current image. Images are stored in the "Captured" folder.
* The Escape key terminates the program.

To Do:
-----
* Use an arbitrary floating point precision library to ~~go even further beyond!~~ allow for deeper zooming in the OpenCL kernel.

Parameters
----
Setting the escape radius to a value higher than 2 gives the background a wavy pattern.
These are images of the same region, rendered with `R = 2` and `R = 8192` respectively.

![](./Examples/low_radius.png?raw=true)
![](./Examples/high_radius.png?raw=true)

The escape radius is set to `R = 2` by default, as it creates smoother looking images.
Viewing structures "from afar" can create some nicely blended color patterns:

![](./Examples/nice_colours.png?raw=true)

The number of iterations per pixel determines how detailed the image is.
Lower values cause pixels to prematurely be assigned the black color.
Here are two images of the same region with `N = 1375` and `N = 4000`:

![](./Examples/low_iterations.png?raw=true)
![](./Examples/high_iterations.png?raw=true)

Gallery
-------

![](./Examples/minibrot.png?raw=true)
![](./Examples/dragon.png?raw=true)
![](./Examples/recursion.png?raw=true)
![](./Examples/blade.png?raw=true)
![](./Examples/infinite_detail.png?raw=true)