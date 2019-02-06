#include <stdlib.h>
#include <stdio.h>
#include <string.h>

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

#ifdef __cplusplus
extern "C"
{
#endif

    EXPORT void ListOpenCLDevices(void);
    EXPORT void PrecompileKernels(void);
    EXPORT void OpenCLRender(unsigned char **memory, unsigned int width, unsigned int height, unsigned int N, unsigned int R, double xMin, double xMax, double yMin, double yMax);

#ifdef __cplusplus
}
#endif

cl_context create_context(cl_uint *num_devices, bool *double_supported, cl_device_type *device_type);
cl_kernel load_kernel_binaries(cl_context context, cl_uint device_num, bool double_supported);