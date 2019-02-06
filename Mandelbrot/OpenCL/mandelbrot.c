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
        cl_platform_id platformIds[platformNumber];
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
    }

    EXPORT void PrecompileKernels(void)
    {
        cl_int error = CL_SUCCESS, compilation_error = CL_SUCCESS;

        bool double_supported;
        cl_device_type device_type;
        cl_uint num_devices;

        cl_context context = create_context(&num_devices, &double_supported, &device_type);
        const char *filepath = (device_type == CL_DEVICE_TYPE_GPU) ? "OpenCL/image_kernel.cl" : "OpenCL/buffer_kernel.cl";

        // Get context devices
        cl_device_id devices[MAX_DEVICES];
        error = clGetContextInfo(context, CL_CONTEXT_DEVICES, sizeof(cl_device_id) * MAX_DEVICES, &devices, NULL);
        check_error_code("clGetContextInfo", error);

        // Print chosen devices
        printf("Chosen devices:\n");
        for (int i = 0; i < num_devices; i++)
        {
            char name[MAX_NAME] = {'\0'};
            error = clGetDeviceInfo(devices[i], CL_DEVICE_NAME, MAX_NAME, &name, NULL);
            check_error_code("clGetDeviceInfo", error);
            printf("%d - %s, %s precision, %s kernel\n", i, name, double_supported ? "Double" : "Single", (device_type == CL_DEVICE_TYPE_GPU) ? "Image" : "Buffer");
        }

        // Create program from source code
        char *program_source = read_file(filepath);
        cl_program program = clCreateProgramWithSource(context, 1, (const char **)&program_source, NULL, &error);
        free(program_source);
        check_error_code("clCreateProgramWithSource", error);

        // Compile program to binary
        if (double_supported)
            compilation_error = clCompileProgram(program, num_devices, devices, "-D CONFIG_USE_DOUBLE -cl-auto-vectorize-enable -cl-no-signed-zeros -cl-denorms-are-zero", 0, NULL, NULL, NULL, NULL);
        else
            compilation_error = clCompileProgram(program, num_devices, devices, "-cl-no-signed-zeros -cl-auto-vectorize-enable -cl-denorms-are-zero", 0, NULL, NULL, NULL, NULL);

        // Print build log on failure
        if (compilation_error != CL_SUCCESS)
        {
            for (int i = 0; i < num_devices; i++)
            {
                size_t length = 0;
                error = clGetProgramBuildInfo(program, devices[i], CL_PROGRAM_BUILD_LOG, 0, NULL, &length);
                check_error_code("clGetProgramBuildInfo", error);

                char build_log[length];
                error = clGetProgramBuildInfo(program, devices[i], CL_PROGRAM_BUILD_LOG, length, build_log, NULL);
                check_error_code("clGetProgramBuildInfo", error);

                printf("Build log:\n%s\n", build_log);
            }
        }
        check_error_code("clCompileProgram", compilation_error);

        // Get sizes of kernel binaries
        size_t binary_sizes[num_devices];
        error = clGetProgramInfo(program, CL_PROGRAM_BINARY_SIZES, sizeof(size_t) * num_devices, binary_sizes, NULL);
        check_error_code("clGetProgramInfo", error);

        // Get kernel binaries
        unsigned char *binaries[num_devices];
        for (int i = 0; i < num_devices; i++)
        {
            binaries[i] = (unsigned char *)malloc(binary_sizes[i]);
        }
        clGetProgramInfo(program, CL_PROGRAM_BINARIES, sizeof(unsigned char *) * num_devices, binaries, NULL);
        check_error_code("clGetProgramInfo", error);

        for (int i = 0; i < num_devices; i++)
        {
            char path[MAX_NAME];
            sprintf(path, "OpenCL/device_%d.clbin", i);
            write_binary(binaries[i], binary_sizes[i], path);
            free(binaries[i]);
        }
    }

    // OpenCL rendering of a Mandelbrot Set image
    EXPORT void OpenCLRender(unsigned char **memory, unsigned int width, unsigned int height, unsigned int N, unsigned int R, double xMin, double xMax, double yMin, double yMax)
    {
        cl_int error = CL_SUCCESS;

        cl_uint num_devices = 0;
        cl_device_id devices[MAX_DEVICES];
        cl_context context;
        cl_kernel kernel;
        cl_command_queue cmd_queue[MAX_DEVICES];

        // Four channels : BGRA
        size_t buffer_size = sizeof(unsigned char) * width * height * 4;
        unsigned char *host_image = (unsigned char *)malloc(buffer_size);
        if (!host_image)
        {
            fprintf(stderr, "Couldn't allocate buffer\n");
            exit(EXIT_FAILURE);
        }

        // Create OpenCL context
        bool double_support = FALSE;
        cl_device_type device_type;
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

        cl_mem image;
        if (device_type == CL_DEVICE_TYPE_GPU)
        {
            // Create image buffer for the rendered output
            cl_image_format img_format = {CL_RGBA, CL_UNSIGNED_INT8};
            cl_image_desc img_desc = {CL_MEM_OBJECT_IMAGE2D, width, height, 1, 1, 0, 0, 0, 0, NULL};
            image = clCreateImage(context, CL_MEM_WRITE_ONLY | CL_MEM_USE_HOST_PTR, &img_format, &img_desc, host_image, &error);
            check_error_code("clCreateImage", error);
        }
        else
        {
            // create buffer for rendered output
            image = clCreateBuffer(context, CL_MEM_WRITE_ONLY | CL_MEM_USE_HOST_PTR, buffer_size, host_image, &error);
            check_error_code("clCreateBuffer", error);
        }

        // Create kernel from binary (precompiled beforehand)
        kernel = load_kernel_binaries(context, num_devices, double_support);

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
            size_t offset = device_work_offset[1] * width * 4;

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

// Create OpenCL context
cl_context create_context(cl_uint *num_devices, bool *double_supported, cl_device_type *device_type)
{
    cl_int error = CL_SUCCESS;
    cl_uint num_cpus;
    cl_device_id *devices, gpus[MAX_DEVICES], cpus[MAX_DEVICES];

    // If there is no GPU then any CPU is selected
    // Otherwise follow this order of importance:
    // Double precision GPU -> Double precision CPU -> Single precision GPU

    // Get the CPU device as a fallback
    error = clGetDeviceIDs(NULL, CL_DEVICE_TYPE_CPU, MAX_DEVICES, cpus, &num_cpus);
    check_error_code("clGetDeviceIDs", error);

    // Find GPU devices
    error = clGetDeviceIDs(NULL, CL_DEVICE_TYPE_GPU, MAX_DEVICES, gpus, num_devices);
    devices = gpus;
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

    return context;
}

// Read and build OpenCL kernel from file
cl_kernel load_kernel_binaries(cl_context context, cl_uint num_devices, bool double_supported)
{
    cl_int error = CL_SUCCESS;
    struct stat statbuf;

    size_t binary_sizes[num_devices];
    cl_int errors[num_devices];
    unsigned char *binaries[num_devices];

    // Get context devices
    cl_device_id devices[MAX_DEVICES];
    error = clGetContextInfo(context, CL_CONTEXT_DEVICES, sizeof(cl_device_id) * MAX_DEVICES, &devices, NULL);
    check_error_code("clGetContextInfo", error);

    for (int i = 0; i < num_devices; i++)
    {
        char path[MAX_NAME];
        sprintf(path, "OpenCL/device_%d.clbin", i);
        stat(path, &statbuf);
        binary_sizes[i] = statbuf.st_size;
        binaries[i] = (unsigned char *)malloc(sizeof(unsigned char) * binary_sizes[i]);
        read_binary(binaries[i], binary_sizes[i], path);
    }

    cl_program program = clCreateProgramWithBinary(context, num_devices, devices, binary_sizes, (const unsigned char **)binaries, errors, &error);
    for (int i = 0; i < num_devices; i++)
    {
        check_error_code("kernel binaries loading", errors[i]);
        free(binaries[i]);
    }
    check_error_code("clCreateProgramWithBinary", error);

    if (double_supported)
        error = clBuildProgram(program, num_devices, devices, "-D CONFIG_USE_DOUBLE -cl-auto-vectorize-enable -cl-no-signed-zeros -cl-denorms-are-zero", NULL, NULL);
    else
        error = clBuildProgram(program, num_devices, devices, "-cl-no-signed-zeros -cl-auto-vectorize-enable -cl-denorms-are-zero", NULL, NULL);

    check_error_code("clBuildProgram", error);
    cl_kernel kernel = clCreateKernel(program, "Render", &error);
    check_error_code("clCreateKernel", error);

    return kernel;
}