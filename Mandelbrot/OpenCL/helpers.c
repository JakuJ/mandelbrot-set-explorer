#include "helpers.h"

// Read file to a buffer
char *read_file(const char *filepath)
{
    FILE *f;
    char *content;
    struct stat statbuf;

    if (NULL == (f = fopen(filepath, "r")))
    {
        perror("Coudn't open file with fopen");
        exit(EXIT_FAILURE);
    }

    stat(filepath, &statbuf);
    content = (char *)malloc(statbuf.st_size + 1);

    fread(content, statbuf.st_size, 1, f);
    content[statbuf.st_size] = '\0';

    fclose(f);
    return content;
}

// Helper function printing <size> - byte objects bit by bit
void print_bits(size_t const size, void *pointer)
{
    unsigned char bit, *bytes = (unsigned char *)pointer;

    for (int i = size - 1; i >= 0; i--)
    {
        for (int j = 7; j >= 0; j--)
        {
            bit = (bytes[i] >> j) & 1;
            printf("%u", bit);
        }
    }
}

// Checks OpenCL error codes
void check_error_code(const char *message, cl_int error)
{
    if (error != CL_SUCCESS)
    {
        // Print error code info and abort
        printf("Encountered error code \"%d\" while executing \"%s\"\n", error, message);
        switch (error)
        {
        case -1:
            printf("Device not found\n");
            break;
        case -2:
            printf("Device not available\n");
            break;
        case -3:
            printf("Compiler not available\n");
            break;
        case -4:
            printf("Memory object allocation failure\n");
            break;
        case -5:
            printf("Out of resources\n");
            break;
        case -6:
            printf("Out of host memory\n");
            break;
        case -7:
            printf("Profiling info not available\n");
            break;
        case -8:
            printf("Memory copy overlap\n");
            break;
        case -9:
            printf("Image format mismatch\n");
            break;
        case -10:
            printf("Image format not supported\n");
            break;
        case -11:
            printf("Build program failure\n");
            break;
        case -12:
            printf("Map failure\n");
            break;
        case -30:
            printf("Invalid value\n");
            break;
        case -31:
            printf("Invaid device type\n");
            break;
        case -32:
            printf("Invalid platform\n");
            break;
        case -33:
            printf("Invalid device\n");
            break;
        case -34:
            printf("Invalid context\n");
            break;
        case -35:
            printf("Invalid queue properties\n");
            break;
        case -36:
            printf("Invalid command queue\n");
            break;
        case -37:
            printf("Invalid host pointer\n");
            break;
        case -38:
            printf("Invalid memory object\n");
            break;
        case -39:
            printf("Invalid image format descriptor\n");
            break;
        case -40:
            printf("Invalid image size\n");
            break;
        case -41:
            printf("Invalid sampler\n");
            break;
        case -42:
            printf("Invalid binary\n");
            break;
        case -43:
            printf("Invalid build options\n");
            break;
        case -44:
            printf("Invalid program\n");
            break;
        case -45:
            printf("Invalid program executable\n");
            break;
        case -46:
            printf("Invalid kernel name\n");
            break;
        case -47:
            printf("Invalid kernel defintion\n");
            break;
        case -48:
            printf("Invalid kernel\n");
            break;
        case -49:
            printf("Invalid argument index\n");
            break;
        case -50:
            printf("Invalid argument value\n");
            break;
        case -51:
            printf("Invalid argument size\n");
            break;
        case -52:
            printf("Invalid kernel arguments\n");
            break;
        case -53:
            printf("Invalid work dimension\n");
            break;
        case -54:
            printf("Invalid work group size\n");
            break;
        case -55:
            printf("Invalid work item size\n");
            break;
        case -56:
            printf("Invalid global offset\n");
            break;
        case -57:
            printf("Invalid event wait list\n");
            break;
        case -58:
            printf("Invalid event\n");
            break;
        case -59:
            printf("Invalid operation\n");
            break;
        case -60:
            printf("Invalid GL object\n");
            break;
        case -61:
            printf("Invalid buffer size\n");
            break;
        case -62:
            printf("Invalid mip level\n");
            break;
        case -63:
            printf("Invalid global work size\n");
            break;
        }
        exit(EXIT_FAILURE);
    }
}
