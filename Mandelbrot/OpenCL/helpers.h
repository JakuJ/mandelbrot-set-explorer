#include <stdio.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <unistd.h>

#ifdef __APPLE__
#include <OpenCL/opencl.h>
#else
#include <CL/cl.h>
#endif

bool check_double_support(cl_device_id *devices, int num_devices);
char *read_file(const char *filepath);
void check_error_code(const char *message, cl_int error);
void print_bits(size_t const size, void *pointer);