#version 410

out vec4 outputColor;

in vec2 texCoord;

uniform usampler2D texture0;

uniform float M;
uniform uint N;

void main()
{
    const float b = 0.23570226f, c = 0.124526508f;
    
    uint i = texture(texture0, texCoord).r;
    
    if (i == N) {
        outputColor = vec4(0, 0, 0, 1);
        return;
    }
    
    float x = M * float(i + i);
    float red = .5f * (1 - cos(x));
    float green = .5f * (1 - cos(b * x));
    float blue = .5f * (1 - cos(c * x));

    outputColor = vec4(red, green, blue, 1);
}