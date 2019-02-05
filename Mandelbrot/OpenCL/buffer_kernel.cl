#if CONFIG_USE_DOUBLE

#if defined(cl_khr_fp64) // Khronos extension available?
#pragma OPENCL EXTENSION cl_khr_fp64 : enable
#define DOUBLE_SUPPORT_AVAILABLE
#elif defined(cl_amd_fp64) // AMD extension available?
#pragma OPENCL EXTENSION cl_amd_fp64 : enable
#define DOUBLE_SUPPORT_AVAILABLE
#else
#error "Double floating point precision not supported!"
#endif

#endif // CONFIG_USE_DOUBLE

#ifdef DOUBLE_SUPPORT_AVAILABLE
typedef double real_t;
typedef double2 real2_t;
#else
typedef float real_t;
typedef float2 real2_t;
#endif

__kernel void Render(__global unsigned char *out, unsigned int max_iteration, unsigned int R, real_t xMin, real_t xMax, real_t yMin, real_t yMax)
{
    int x_dim = get_global_id(0);
    int y_dim = get_global_id(1);

    size_t width = get_global_size(0);
    size_t height = get_global_size(1);

    int idx = 3 * (width * y_dim + x_dim);

    real_t c_re = xMin + (xMax - xMin) * x_dim / width;
    real_t c_im = yMin + (yMax - yMin) * y_dim / height;

    real2_t z = 0, zq = 0;
    real2_t c = (real2_t)(c_re, c_im);

    uint iteration = 0;
    real_t Radius = R * R;

    while (zq.x + zq.y <= Radius && iteration < max_iteration)
    {
        z.y = z.x * z.y;
        z.y = z.y + z.y;

        z.x = zq.x - zq.y;
        z = z + c;
        
        zq = z * z;
        
        ++iteration;
    }

    uchar blue = 0, green = 0, red = 0;
    if (iteration != max_iteration)
    {
        real_t X = iteration + iteration - log2(zq.x + zq.y);
        X = 0.25 * X;
        
        real_t green_param = 0.23570226 * X;
        real_t blue_param = 0.124526508 * X;

        red = convert_uchar(127.5 * (1 - cos(X)));
        green = convert_uchar(127.5 * (1 - cos(green_param)));
        blue = convert_uchar(127.5 * (1 - cos(blue_param)));
    }

    out[idx] = blue;
    out[idx + 1] = green;
    out[idx + 2] = red;
}
