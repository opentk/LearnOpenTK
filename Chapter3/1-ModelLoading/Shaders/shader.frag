#version 330

in vec2 TexCoords;

out vec4 outputColor;

uniform sampler2D texture_diffuse1;

void main()
{
    outputColor = texture(texture_diffuse1, TexCoords);
}