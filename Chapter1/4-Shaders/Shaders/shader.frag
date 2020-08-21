#version 330 core

out vec4 outputColor;

in vec4 vertexColor; // the input variable from the vertex shader (same name and same type)

void main()
{
    outputColor = vertexColor;
}