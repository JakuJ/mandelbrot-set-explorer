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
typedef double2 real2_t;
typedef double4 real4_t;
#else
typedef float real_t;
typedef float2 real2_t;
typedef float4 real4_t;
#endif

__kernel void Render(__write_only image2d_t out, unsigned int max_iteration, unsigned int R, real_t xMin, real_t xMax, real_t yMin, real_t yMax)
{   
    int x_dim = get_global_id(0);
    int y_dim = get_global_id(1);

    size_t width = get_global_size(0);
    size_t height = get_global_size(1);

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

    uint4 colors = (0, 0, 0, 0);

    if (iteration != max_iteration)
    {
        real_t X = iteration + iteration - log2(zq.x + zq.y);

        real4_t vec = (real4_t)(0.031131627, 0.058925565, 0.25, 0);
        vec = 1 - cos(vec * X);
        vec = (real_t)127.5 * vec;

        colors = convert_uint4(vec);
    }
    write_imageui(out, (int2)(x_dim, y_dim), colors);
}
