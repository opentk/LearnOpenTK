#version 330 core

layout(location = 0) in vec3 aPosition;  // the position variable has attribute position 0

void main(void)
{
	// see how we directly give a vec3 to vec4's constructor
    gl_Position = vec4(aPosition, 1.0); 
}