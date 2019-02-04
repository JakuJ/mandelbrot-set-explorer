CC?=gcc
CFLAGS=-Wall -O2

all: OpenCLRendering.dll

OpenCLRendering.dll: mandelbrot.c mandelbrot.h helpers.c helpers.h
	${CC} ${CFLAGS} -c -fdeclspec mandelbrot.c helpers.c
	${CC} ${CFLAGS} -shared -framework OpenCL -o OpenCLRendering.dll mandelbrot.o helpers.o
	rm mandelbrot.o helpers.o