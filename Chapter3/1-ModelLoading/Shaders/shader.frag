#version 330

in vec2 TexCoords;

out vec4 outputColor;

uniform sampler2D texture0;

void main()
{
    outputColor = texture(texture0, TexCoords);
}