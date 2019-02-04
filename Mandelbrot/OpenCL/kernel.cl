#if CONFIG_USE_DOUBLE

#if defined(cl_khr_fp64)  // Khronos extension available?
#pragma OPENCL EXTENSION cl_khr_fp64 : enable
#define DOUBLE_SUPPORT_AVAILABLE
#elif defined(cl_amd_fp64)  // AMD extension available?
#pragma OPENCL EXTENSION cl_amd_fp64 : enable
#define DOUBLE_SUPPORT_AVAILABLE
#else
#error "Double floating point precision not supported!"
#endif

#endif // CONFIG_USE_DOUBLE

#ifdef DOUBLE_SUPPORT_AVAILABLE
typedef double real_t;
#else
typedef float real_t;
#endif

__kernel void Render(__global unsigned char *out, int max_iteration, int R, real_t xMin, real_t xMax, real_t yMin, real_t yMax)
{
    int x_dim = get_global_id(0);
    int y_dim = get_global_id(1);

    size_t width = get_global_size(0);
    size_t height = get_global_size(1);

    int idx = 3 * (width * y_dim + x_dim);

    real_t dx = xMax - xMin;
    real_t dy = yMax - yMin;

    real_t x_origin = xMin + dx * x_dim / width;
    real_t y_origin = yMin + dy * y_dim / height;

    real_t x = 0.0;
    real_t y = 0.0;

    int iteration = 0;
    int Radius = R * R;

    while (x * x + y * y <= Radius && iteration < max_iteration)
    {
        real_t xtemp = x * x - y * y + x_origin;
        y = 2 * x * y + y_origin;
        x = xtemp;
        iteration++;
    }

    if (iteration == max_iteration)
    {
        out[idx] = 0;
        out[idx + 1] = 0;
        out[idx + 2] = 0;
    }
    else
    {
        real_t V = sqrt(x * x + y * y) / powr(2.0, (real_t)iteration);
        real_t K = 2.0;
        real_t X = log(V) / K;

        real_t a = 1.4427 * X;
        real_t b = 0.34 * X;
        real_t c = 0.18 * X;

        out[idx] = convert_uchar(floor(255 / 2 * (1 - cos(c))));
        out[idx + 1] = convert_uchar(floor(255 / 2 * (1 - cos(b))));
        out[idx + 2] = convert_uchar(floor(255 / 2 * (1 - cos(a))));
    }
}
