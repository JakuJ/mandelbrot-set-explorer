#version 410 core

layout(location = 0) in vec3 aPosition;

out vec2 aPos;

void main(void)
{
    aPos = aPosition.xy;

    gl_Position = vec4(aPosition, 1.0);
}