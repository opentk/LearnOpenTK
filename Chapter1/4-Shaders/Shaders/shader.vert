#version 330 core

layout(location = 0) in vec3 aPosition;  // the position variable has attribute position 0

// This variable uses the keyword out in order to pass on the value to the 
// next shader down in the chain, in this case the frag shader
out vec4 vertexColor;

void main(void)
{
    gl_Position = vec4(aPosition, 1.0); // see how we directly give a vec3 to vec4's constructor

	// Here we assign the variable a dark red color to the out variable
	vertexColor = vec4(0.5, 0.0, 0.0, 1.0);
}