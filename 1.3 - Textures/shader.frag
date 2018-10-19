#version 330

out vec4 outputColor;

in vec2 texCoord;

//sampler2D uniforms work slightly different than other ones
//The uniform is automatically bound by OpenGL when you bind the texture
uniform sampler2D texture0;

void main()
{
    outputColor = vec4(texCoord.xy, 1.0, 1.0);
}