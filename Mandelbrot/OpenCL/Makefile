CC?=gcc
CFLAGS=-Wall

all: OpenCLRendering.dll

OpenCLRendering.dll: kernel.cl mandelbrot.c mandelbrot.h
	${CC} ${CFLAGS} -c -fdeclspec mandelbrot.c
	${CC} ${CFLAGS} -shared -o OpenCLRendering.dll mandelbrot.o -framework OpenCL
	rm mandelbrot.o