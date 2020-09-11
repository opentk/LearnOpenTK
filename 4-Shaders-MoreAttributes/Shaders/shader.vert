#version 330 core

layout(location = 0) in vec3 aPosition;  // the position variable has attribute position 0

// This is where the color values we assigned in the main program goes to
layout(location = 1) in vec3 aColor;

out vec3 ourColor; // output a color to the fragment shader

void main(void)
{
    gl_Position = vec4(aPosition, 1.0); // see how we directly give a vec3 to vec4's constructor

	// We use the outColor variable to pass on the color information to the frag shader
	ourColor = aColor;
}