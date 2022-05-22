#version 410

out vec4 outputColor;

in vec2 aPos;

uniform float xMin;
uniform float xMax;
uniform float yMin;
uniform float yMax;
uniform float R;
uniform float M;
uniform uint N;

void main()
{
    const float b = 0.23570226f, c = 0.124526508f;

    float zRe = 0, zIm = 0;
    float cRe = mix(xMin, xMax, .5 * (1 + aPos.x));
    float cIm = mix(yMin, yMax, .5 * (1 + aPos.y));
    float zReSqr = 0;
    float zImSqr = 0;
    float radius = R * R;

    for (uint i = 0; i < N; ++i)
    {
        if (zReSqr + zImSqr > radius)
        {
            float x = M * (i + i);
            float red = .5f * (1 - cos(x));
            float green = .5f * (1 - cos(b * x));
            float blue = .5f * (1 - cos(c * x));

            outputColor = vec4(red, green, blue, 1);
            return;
        }

        zIm = zIm * zRe * 2 + cIm;

        zRe = zReSqr - zImSqr + cRe;
        zReSqr = zRe * zRe;
        zImSqr = zIm * zIm;
    }

    outputColor = vec4(0, 0, 0, 1);
}