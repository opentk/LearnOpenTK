#version 330 core
struct Material {
    sampler2D diffuse;
    sampler2D specular;
    float     shininess;
};
//The spotlight is a pointlight in essence, however we only want to show the light within a certain angle.
//That angle is the cutoff, the outercutoff is used to make a more smooth border to the spotlight.
struct Light {
    vec3  position;
    vec3  direction;
    float cutOff;
    float outerCutOff;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

    float constant;
    float linear;
    float quadratic;
};

uniform Light light;
uniform Material material;
uniform vec3 viewPos;

out vec4 FragColor;

in vec3 Normal;
in vec3 FragPos;
in vec2 TexCoords;

void main()
{
    //ambient
    vec3 ambient = light.ambient * vec3(texture(material.diffuse, TexCoords));

    //diffuse 
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(light.position - FragPos);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = light.diffuse * diff * vec3(texture(material.diffuse, TexCoords));

    //specular
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    vec3 specular = light.specular * spec * vec3(texture(material.specular, TexCoords));

    //attenuation
    float distance    = length(light.position - FragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance +
    light.quadratic * (distance * distance));

    //spotlight intensity
    //This is how we calculate the spotlight, for a more in depth explanation of how this works. Check out the web tutorials.
    float theta     = dot(lightDir, normalize(-light.direction));
    float epsilon   = light.cutOff - light.outerCutOff;
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0); //The intensity, is the lights intensity on a given fragment,
                                                                                //this is used to make the smooth border.    
    //When applying the spotlight intensity we want to multiply it.
    ambient  *= attenuation; //Remember the ambient is where the light dosen't hit, this means the spotlight shouldn't be applied
    diffuse  *= attenuation * intensity;
    specular *= attenuation * intensity;

    vec3 result = ambient + diffuse + specular;
    FragColor = vec4(result, 1.0);
    
}