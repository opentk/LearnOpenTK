#version 330 core

out vec4 outputColor;


// The Uniform keyword allows you to access a shader variable at any stage of the shader chain
// It's also accessible across all of the main program
// Whatever you set this variable to it keeps it until you either reset the value or updated it

uniform vec4 ourColor; 

void main()
{
    outputColor = ourColor;
}