#version 330 core
layout (location = 0) in vec3 aPos;

uniform mat4 lightSpaceMatrix;
uniform mat4 model;

void main()
{
    //gl_Position = lightSpaceMatrix * model * vec4(aPos, 1.0);
    gl_Position = vec4(aPos, 1.0) * model * lightSpaceMatrix;
}