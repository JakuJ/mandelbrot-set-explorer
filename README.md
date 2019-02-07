Mandelbrot Set explorer
=============
![License - MIT](https://img.shields.io/github/license/JakuJ/mandelbrot-set-explorer.svg)

OpenTK based C# program which generates images of the Mandelbrot set. Zooming will take you as far as the floating point precision allows on your computer (10^-4.5 for single, 10^-13.5 for double precision).

![](./Examples/titular.png?raw=true)

Installation
------

Run `make` in `Mandelbrot/OpenCL` folder to compile the native .dll which will allow for parallel rendering on GPU/CPU using OpenCL. The .dll is copied to output directory automatically by Visual Studio.

Build and run the project solution in **Visual Studio** (works under **VS for Mac** at least). Currently there are two `MandelbrotSet` child classes you can use in `Main()` to render images:

* `OpenCLMandelbrot` (faster, may not work on older devices)
* `ParallelMandelbrot` (slow, safe and reliable)

The OpenCL implementation selects available devices according to this order of importance:

1. GPUs supporting double precision
2. CPUs supporting double precision
3. GPUs supporting single precision

If there are no OpenCL-ready GPUs, then the CPUs are used. There are two OpenCL kernels used for rendering, one based on an `unsigned char` buffer and one based on `image2d`. The former is used with CPUs, the later with GPUs, to benefit from GPU image processing speed.

OpenCL implementation on the CPU is about 4x faster than `Parallel.For` implementation (tested on Intel® Core™ i5-5257U CPU @ 2.70GHz).

If OpenCL implementation doesn't work for you for some reason, use release mode to enable optimizations and set the target to `x64` for major speed improvements over `x86` while using `ParallelMandelbrot` CPU implementation.

Usage
------

* Left click anywhere in the image to zoom in to that location, right click to zoom out
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
* Use an arbitrary floating point precision library to ~~go even further beyond!~~ allow for deeper zooming in OpenCL kernel.

Parameters
----
Setting the escape radius to a value higher than 2 gives the background a wavy pattern. These are images of the same region, rendered with R=2 and R=8192 respectively.

![](./Examples/low_radius.png?raw=true)
![](./Examples/high_radius.png?raw=true)

The escape radius is set to R=2 by default, as it creates smoother looking, and thus more beautiful images. Viewing structures "from afar" can create some nicely blended color patterns:

![](./Examples/nice_colours.png?raw=true)

The number of iterations per pixel determines how detailed the image is. Lower values cause pixels to prematurely be assigned black colour. Here are two images of the same region with N=1375 and N=4000:

![](./Examples/low_iterations.png?raw=true)
![](./Examples/high_iterations.png?raw=true)

Generating these images on a laptop in about 2.5 seconds was possible only with the OpenCL renderer. The `Parallel.For` C# renderer is painfully slow in comparison and results in the user losing patience.

Gallery
-------

Here are some more images. Note how in the last one setting high enough number of iterations makes an impression of infinite detail.

![](./Examples/minibrot.png?raw=true)
![](./Examples/dragon.png?raw=true)
![](./Examples/recursion.png?raw=true)
![](./Examples/blade.png?raw=true)
![](./Examples/infinite_detail.png?raw=true)