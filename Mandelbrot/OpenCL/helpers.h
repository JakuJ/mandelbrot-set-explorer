#include <stdio.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <unistd.h>

#ifdef __APPLE__
#include <OpenCL/opencl.h>
#else
#include <CL/cl.h>
#endif

#define MAX_DEVICES 16
#define MAX_NAME 1024

char *read_file(const char *filepath);
void read_binary(unsigned char* target, size_t size, const char *filepath);
void write_binary(unsigned char *content, size_t length, const char *filepath);

void print_bits(size_t const size, void *pointer);
bool check_double_support(cl_device_id *devices, int num_devices);
void list_platform_devices(cl_platform_id platformId, cl_device_type device_type);
void check_error_code(const char *message, cl_int error);