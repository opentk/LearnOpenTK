#version 330 core

out vec4 outputColor;

uniform vec4 ourColor; // the input variable from the vertex shader (same name and same type)

void main()
{
    outputColor = ourColor;
}