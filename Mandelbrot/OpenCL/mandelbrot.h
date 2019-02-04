#pragma once

#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <unistd.h>
#include <assert.h>

#ifdef __APPLE__
#include <OpenCL/opencl.h>
#else
#include <CL/cl.h>
#endif

#if __has_declspec_attribute(dllexport)
#define EXPORT __declspec(dllexport)
#else
#define EXPORT __declspec()
#endif

#define MAX_NAME 1024
#define MAX_DEVICES 16

#ifdef __cplusplus
extern "C" {
#endif

EXPORT void ListOpenCLDevices(void);
EXPORT void OpenCLRender(unsigned char **memory, int width, int height, int N, int R, double xMin, double xMax, double yMin, double yMax);

#ifdef __cplusplus
}
#endif

void list_platform_devices(cl_platform_id platformId, cl_device_type device_type);
char *read_file(const char *filepath);
cl_context create_context(cl_uint *num_devices, bool* double_supported);
bool check_double_support(cl_device_id *devices, int num_devices);
cl_kernel load_kernel_from_file(cl_context context, bool double_supported, const char *filename);
void check_error_code(const char *message, cl_int error);
void print_bits(size_t const size, void *pointer);
