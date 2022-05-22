Mandelbrot Set Explorer
=============
![License - MIT](https://img.shields.io/github/license/JakuJ/mandelbrot-set-explorer.svg)

OpenTK–based C# program that generates images of the Mandelbrot set.
Zooming will take you as far as the floating point precision allows on your computer (1e-4.5 zoom level for single, 1e-13.5 for double precision).

The default fragment shader-based renderer allows for real-time panning and zooming, but uses single-precision FP numbers.
At any time you can switch to a CPU-based renderer that uses double precision, but is ~2 orders of magnitude slower.

![](./Examples/titular.png?raw=true)

Installation
------

Build and run the project by invoking the `dotnet run` command.

Usage
------

* Left-click and hold to pan around, scroll wheel to zoom.
* Hold <kbd>1</kbd> and scroll to change the escape radius
* Hold <kbd>2</kbd> and scroll to change the color modifier
* Hold <kbd>3</kbd> and scroll to change the number of iterations
* Press <kbd>R</kbd> to switch between GPU/float and CPU/double rendering. 
* Press <kbd>0</kbd> to change resolution (for CPU rendering)
* Press <kbd>S</kbd> to save the image to a file.
* Press <kbd>Escape</kbd> to terminate the program.

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