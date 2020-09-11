#version 330 core

out vec4 outputColor;


// The Uniform keyword allows you to access a shader variable at any stage
// Of the shader chain. Whatever you set this variable to it keeps it 
// Until you either reset the value or updated it

uniform vec4 ourColor; 

void main()
{
    outputColor = ourColor;
}