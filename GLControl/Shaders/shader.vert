#version 330 core

layout(location = 0) in vec3 position;
layout(location = 1) in vec3 color;

uniform mat4 view;

out vec3 vColor;

void main() 
{
    vColor = color;
    gl_Position = view * vec4(position, 1.0);
}