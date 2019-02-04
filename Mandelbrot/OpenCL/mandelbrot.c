#include "mandelbrot.h"
#include "helpers.h"

#ifdef __cplusplus
extern "C"
{
#endif

    // List all OpenCL devices
    EXPORT void ListOpenCLDevices(void)
    {
        cl_int error = CL_SUCCESS;

        // Get platform number
        cl_uint platformNumber = 0;
        error = clGetPlatformIDs(0, NULL, &platformNumber);
        check_error_code("clGetPlatformIDs", error);

        if (0 == platformNumber)
        {
            printf("No OpenCL platforms found.\n");
            return;
        }

        // Get platform identifiers
        cl_platform_id *platformIds = (cl_platform_id *)malloc(sizeof(cl_platform_id) * platformNumber);
        error = clGetPlatformIDs(platformNumber, platformIds, NULL);
        check_error_code("clGetPlatformIDs", error);

        // Print platform info
        for (cl_uint i = 0; i < platformNumber; ++i)
        {
            char name[MAX_NAME] = {'\0'};

            printf("Platform %d\n", i);

            error = clGetPlatformInfo(platformIds[i], CL_PLATFORM_NAME, MAX_NAME, &name, NULL);
            check_error_code("clGetPlatformInfo", error);
            printf("Name: %s\n", name);

            error = clGetPlatformInfo(platformIds[i], CL_PLATFORM_VENDOR, MAX_NAME, &name, NULL);
            check_error_code("clGetPlatformInfo", error);
            printf("Vendor: %s\n\n", name);

            list_platform_devices(platformIds[i], CL_DEVICE_TYPE_CPU);
            list_platform_devices(platformIds[i], CL_DEVICE_TYPE_GPU);
        }
        free(platformIds);
    }

    // OpenCL rendering of a Mandelbrot Set image
    EXPORT void OpenCLRender(unsigned char **memory, bool* format32bit, int width, int height, int N, int R, double xMin, double xMax, double yMin, double yMax)
    {
        cl_int error = 0;

        cl_uint num_devices = 0;
        cl_device_id devices[MAX_DEVICES];
        cl_context context;
        cl_kernel kernel;
        cl_command_queue cmd_queue[MAX_DEVICES];

        // Create OpenCL context
        bool double_support = FALSE;
        cl_device_type device_type;
        memset(&device_type, 0, sizeof(cl_device_type));
        context = create_context(&num_devices, &double_support, &device_type);
        if (num_devices == 0)
        {
            printf("No compute devices found\n");
            exit(EXIT_FAILURE);
        }

        // Get context devices
        error = clGetContextInfo(context, CL_CONTEXT_DEVICES, sizeof(cl_device_id) * MAX_DEVICES, &devices, NULL);
        check_error_code("clGetContextInfo", error);

        // Create command queues for each of the devices
        for (int i = 0; i < num_devices; i++)
        {
            cmd_queue[i] = clCreateCommandQueue(context, devices[i], 0, &error);
            check_error_code("clCreateCommandQueue", error);
        }

        // Declare the buffer size
        size_t buffer_size = sizeof(unsigned char) * width * height;
        unsigned char *host_image;
        cl_mem image;

        if ((*format32bit = (device_type == CL_DEVICE_TYPE_GPU)))
        {
            // Blue, green, red, alpha
            buffer_size *= 4;
            host_image = (unsigned char *)malloc(buffer_size);

            // Create image buffer for the rendered output
            cl_image_format img_format = {CL_RGBA, CL_UNSIGNED_INT8};
            cl_image_desc img_desc = {CL_MEM_OBJECT_IMAGE2D, width, height, 1, 1, 0, 0, 0, 0, NULL};
            image = clCreateImage(context, CL_MEM_WRITE_ONLY | CL_MEM_USE_HOST_PTR, &img_format, &img_desc, host_image, &error);
            check_error_code("clCreateImage", error);

            // Load the kernel from file and compile it
            kernel = load_kernel_from_file(context, devices, num_devices, double_support, "OpenCL/image_kernel.cl");
        }
        else
        {
            // Blue, green, red
            buffer_size *= 3;
            host_image = (unsigned char *)malloc(buffer_size);

            // create buffer for rendered output
            image = clCreateBuffer(context, CL_MEM_WRITE_ONLY | CL_MEM_USE_HOST_PTR, buffer_size, host_image, &error);
            check_error_code("clCreateBuffer", error);

            // Load the kernel from file and compile it
            kernel = load_kernel_from_file(context, devices, num_devices, double_support, "OpenCL/buffer_kernel.cl");
        }

        // Prepare iteration number, escape radius and location constants to be passed as additional arguments
        cl_int clN = (cl_int)N;
        cl_int clR = (cl_int)R;

        clSetKernelArg(kernel, 0, sizeof(cl_mem), &image);
        clSetKernelArg(kernel, 1, sizeof(cl_int), &clN);
        clSetKernelArg(kernel, 2, sizeof(cl_int), &clR);
        if (double_support)
        {
            cl_double clxMin = (cl_double)xMin;
            cl_double clxMax = (cl_double)xMax;
            cl_double clyMin = (cl_double)yMin;
            cl_double clyMax = (cl_double)yMax;

            // Setup double arguments to the kernel
            clSetKernelArg(kernel, 3, sizeof(cl_double), &clxMin);
            clSetKernelArg(kernel, 4, sizeof(cl_double), &clxMax);
            clSetKernelArg(kernel, 5, sizeof(cl_double), &clyMin);
            clSetKernelArg(kernel, 6, sizeof(cl_double), &clyMax);
        }
        else
        {
            cl_float clxMin = (cl_float)xMin;
            cl_float clxMax = (cl_float)xMax;
            cl_float clyMin = (cl_float)yMin;
            cl_float clyMax = (cl_float)yMax;

            // Setup float arguments to the kernel
            clSetKernelArg(kernel, 3, sizeof(cl_float), &clxMin);
            clSetKernelArg(kernel, 4, sizeof(cl_float), &clxMax);
            clSetKernelArg(kernel, 5, sizeof(cl_float), &clyMin);
            clSetKernelArg(kernel, 6, sizeof(cl_float), &clyMax);
        }

        // Enqueue the calculations divided between all devices
        // ! Assuming that num_devices divides width and height evenly
        const size_t device_work_size[3] = {width, height / num_devices, 1};
        for (int i = 0; i < num_devices; i++)
        {
            size_t device_work_offset[3] = {0, i * device_work_size[1], 0};
            size_t offset = device_work_offset[1] * width * 3;

            error = clEnqueueNDRangeKernel(cmd_queue[i], kernel, 2, device_work_offset,
                                           device_work_size, NULL, 0, NULL, NULL);
            check_error_code("clEnqueueNDRangeKernel", error);

            if (device_type == CL_DEVICE_TYPE_GPU)
            {
                // Non-blocking read to continue queuing up more kernels
                error = clEnqueueReadImage(cmd_queue[i], image, CL_FALSE,
                                           device_work_offset, device_work_size, 0, 0,
                                           host_image, 0, NULL, NULL);
                check_error_code("clEnqueueReadImage", error);
            }
            else
            {
                // Non-blocking read to continue queuing up more kernels
                error = clEnqueueReadBuffer(cmd_queue[i], image, CL_FALSE, offset,
                                            buffer_size / num_devices,
                                            host_image, 0, NULL, NULL);
                check_error_code("clEnqueueReadBuffer", error);
            }
        }

        // Force the command queues to complete the tasks
        for (int i = 0; i < num_devices; i++)
        {
            error = clFinish(cmd_queue[i]);
            check_error_code("clFinish", error);
        }

        // Pass the array to managed C# environment
        *memory = host_image;

        // Free OpenCL objects
        error = clReleaseMemObject(image);
        check_error_code("clReleaseMemObject", error);

        for (int i = 0; i < num_devices; i++)
        {
            error = clReleaseCommandQueue(cmd_queue[i]);
            check_error_code("clReleaseCommandQueue", error);
        }
        error = clReleaseContext(context);
        check_error_code("clReleaseContext", error);
    }

#ifdef __cplusplus
}
#endif

// List all OpenCL devices on a given platform
void list_platform_devices(cl_platform_id platformId, cl_device_type device_type)
{
    cl_int error = CL_SUCCESS;
    const char *prefix = (device_type == CL_DEVICE_TYPE_CPU) ? "CPU" : "GPU";

    // Get device number
    cl_uint deviceNumber;
    error = clGetDeviceIDs(platformId, device_type, 0, NULL, &deviceNumber);
    check_error_code("clGetDeviceIDs", error);

    if (0 == deviceNumber)
    {
        printf("No OpenCL ready %s devices found on the platform\n", prefix);
        return;
    }

    // Get device identifiers
    cl_device_id *deviceIds = (cl_device_id *)malloc(sizeof(cl_device_id) * deviceNumber);
    error = clGetDeviceIDs(platformId, device_type, deviceNumber, deviceIds, &deviceNumber);
    check_error_code("clGetDeviceIDs", error);

    // Print device info
    for (cl_uint i = 0; i < deviceNumber; i++)
    {
        char name[MAX_NAME] = {'\0'};
        uint number;
        cl_device_fp_config fp_config;
        cl_device_exec_capabilities exec_capabilities;

        printf("%s Device %d\n", prefix, i);

        error = clGetDeviceInfo(deviceIds[i], CL_DEVICE_NAME, MAX_NAME, &name, NULL);
        check_error_code("clGetDeviceInfo", error);
        printf("Name: %s\n", name);

        error = clGetDeviceInfo(deviceIds[i], CL_DEVICE_VENDOR, MAX_NAME, &name, NULL);
        check_error_code("clGetDeviceInfo", error);
        printf("Vendor: %s\n", name);

        error = clGetDeviceInfo(deviceIds[i], CL_DEVICE_ADDRESS_BITS, sizeof(number), &number, NULL);
        check_error_code("clGetDeviceInfo", error);
        printf("Addressing: x%d\n", number);

        error = clGetDeviceInfo(deviceIds[i], CL_DEVICE_MAX_CLOCK_FREQUENCY, sizeof(number), &number, NULL);
        check_error_code("clGetDeviceInfo", error);
        printf("Max Frequency: %dMHz\n", number);

        error = clGetDeviceInfo(deviceIds[i], CL_DEVICE_MAX_COMPUTE_UNITS, sizeof(number), &number, NULL);
        check_error_code("clGetDeviceInfo", error);
        printf("Max parallel cores: %d\n", number);

        error = clGetDeviceInfo(deviceIds[i], CL_DEVICE_VERSION, MAX_NAME, &name, NULL);
        check_error_code("clGetDeviceInfo", error);
        printf("Version: %s\n", name);

        error = clGetDeviceInfo(deviceIds[i], CL_DRIVER_VERSION, MAX_NAME, &name, NULL);
        check_error_code("clGetDeviceInfo", error);
        printf("Driver version: %s\n", name);

        error = clGetDeviceInfo(deviceIds[i], CL_DEVICE_DOUBLE_FP_CONFIG, sizeof(fp_config), &fp_config, NULL);
        check_error_code("clGetDeviceInfo", error);
        printf("Single precision support: ");
        print_bits(sizeof(char), &fp_config);
        printf(" (%s)\n", (fp_config & (CL_FP_ROUND_TO_NEAREST | CL_FP_INF_NAN)) ? "YES" : "NO");

        error = clGetDeviceInfo(deviceIds[i], CL_DEVICE_DOUBLE_FP_CONFIG, sizeof(fp_config), &fp_config, NULL);
        check_error_code("clGetDeviceInfo", error);
        printf("Double precision support: ");
        print_bits(sizeof(char), &fp_config);
        printf(" (%s)\n", (fp_config != 0) ? "YES" : "NO");

        error = clGetDeviceInfo(deviceIds[i], CL_DEVICE_EXECUTION_CAPABILITIES, sizeof(exec_capabilities), &exec_capabilities, NULL);
        check_error_code("clGetDeviceInfo", error);
        printf("OpenCL kernels support: ");
        print_bits(sizeof(char), &exec_capabilities);
        printf(" (%s)\n", (exec_capabilities & CL_EXEC_KERNEL) ? "YES" : "NO");
    }

    printf("\n");
    free(deviceIds);
}

// Create OpenCL context
cl_context create_context(cl_uint *num_devices, bool *double_supported, cl_device_type *device_type)
{
    cl_int error;
    cl_uint num_cpus;
    cl_device_id *devices, cpus[MAX_DEVICES];

    devices = (cl_device_id *)malloc(MAX_DEVICES * sizeof(cl_device_id));

    // If there is no GPU then any CPU is selected
    // Otherwise follow this order of importance:
    // Double precision GPU -> Double precision CPU -> Single precision GPU

    // Get the CPU device as a fallback
    error = clGetDeviceIDs(NULL, CL_DEVICE_TYPE_CPU, MAX_DEVICES, cpus, &num_cpus);
    check_error_code("clGetDeviceIDs", error);

    // Find GPU devices
    error = clGetDeviceIDs(NULL, CL_DEVICE_TYPE_GPU, MAX_DEVICES, devices, num_devices);
    if ((error != CL_SUCCESS || *num_devices == 0) || (!check_double_support(devices, *num_devices) && check_double_support(cpus, num_cpus)))
    {
        devices = cpus;
        *num_devices = num_cpus;
        *device_type = CL_DEVICE_TYPE_CPU;
    }
    else
    {
        *device_type = CL_DEVICE_TYPE_GPU;
    }

    *double_supported = check_double_support(devices, *num_devices);

    cl_context context = clCreateContext(0, *num_devices, devices, NULL, NULL, &error);
    check_error_code("clCreateContext", error);

    // Print chosen devices
    printf("Chosen devices:\n");
    for (int i = 0; i < *num_devices; i++)
    {
        char name[MAX_NAME] = {'\0'};
        error = clGetDeviceInfo(devices[i], CL_DEVICE_NAME, MAX_NAME, &name, NULL);
        check_error_code("clGetDeviceInfo", error);
        printf("%d - %s, %s precision, %s kernel\n", i, name, *double_supported ? "Double" : "Single", (*device_type == CL_DEVICE_TYPE_GPU) ? "Image" : "Buffer");
    }

    return context;
}

bool check_double_support(cl_device_id *devices, int num_devices)
{
    cl_int error = CL_SUCCESS;
    cl_device_fp_config double_support = ~0;

    for (int i = 0; i < num_devices; i++)
    {
        cl_device_fp_config device_fp_config;
        error = clGetDeviceInfo(devices[i], CL_DEVICE_DOUBLE_FP_CONFIG, sizeof(device_fp_config), &device_fp_config, NULL);
        check_error_code("clGetDeviceInfo", error);
        double_support &= device_fp_config;
    }

    return (bool)double_support;
}

// Read and build OpenCL kernel from file
cl_kernel load_kernel_from_file(cl_context context, cl_device_id *devices, cl_uint num_devices, bool double_supported, const char *filename)
{
    cl_int error, build_error;
    char *program_source = read_file(filename);

    cl_program program = clCreateProgramWithSource(context, 1, (const char **)&program_source, NULL, &error);
    check_error_code("clCreateProgramWithSource", error);

    if (double_supported)
        build_error = clBuildProgram(program, 0, NULL, "-D CONFIG_USE_DOUBLE -cl-no-signed-zeros -cl-denorms-are-zero", NULL, NULL);
    else
        build_error = clBuildProgram(program, 0, NULL, "-cl-no-signed-zeros -cl-denorms-are-zero", NULL, NULL);

    // Print build log on failure
    if (build_error != CL_SUCCESS)
    {
        for (int i = 0; i < num_devices; i++)
        {
            size_t length = 0;
            error = clGetProgramBuildInfo(program, devices[i], CL_PROGRAM_BUILD_LOG, 0, NULL, &length);
            check_error_code("clGetProgramBuildInfo", error);

            char *build_log = (char *)malloc(length * sizeof(char));
            error = clGetProgramBuildInfo(program, devices[i], CL_PROGRAM_BUILD_LOG, length, build_log, NULL);
            check_error_code("clGetProgramBuildInfo", error);

            printf("Build log:\n%s\n", build_log);
            free(build_log);
        }
    }

    check_error_code("clBuildProgram", build_error);

    cl_kernel kernel = clCreateKernel(program, "Render", &error);
    check_error_code("clCreateKernel", error);

    return kernel;
}
