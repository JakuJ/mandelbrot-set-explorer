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

    real_t c_re = xMin + (xMax - xMin) * x_dim / width;
    real_t c_im = yMin + (yMax - yMin) * y_dim / height;

    real_t z_re = 0.0;
    real_t z_im = 0.0;
    real_t z_re_sqr = 0.0;
    real_t z_im_sqr = 0.0;

    int iteration = 0;
    real_t Radius = (real_t)(R * R);

    while (z_re_sqr + z_im_sqr <= Radius && iteration < max_iteration)
    {
        z_im = z_re * z_im;
        z_im += z_im + c_im;

        z_re = z_re_sqr - z_im_sqr + c_re;
        z_re_sqr = z_re * z_re;
        z_im_sqr = z_im * z_im;

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
        real_t V = sqrt(z_re_sqr + z_im_sqr) / powr(2.0, (real_t)iteration);
        real_t X = log(V) / 2.0;

        real_t a = 1.4427 * X;
        real_t b = 0.34 * X;
        real_t c = 0.18 * X;

        out[idx] = convert_uchar(floor(255 / 2 * (1 - cos(c))));
        out[idx + 1] = convert_uchar(floor(255 / 2 * (1 - cos(b))));
        out[idx + 2] = convert_uchar(floor(255 / 2 * (1 - cos(a))));
    }
}
