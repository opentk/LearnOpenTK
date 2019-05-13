#version 330 core
struct Material {
    sampler2D diffuse;
    sampler2D specular;
    float     shininess;
};
//This light structure is pretty much the same as the one from the last few parts of the tutorials
struct Light {
    vec3 position;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

    //In the web version you can see why we need the constant the linear and the quadratic values.
    //However to keep a brief explanation here, it is to make the light more dim the further you go away.
    //These are constants defining the graph the intensity of the light follows.
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
    //The attenuation is the term we use when talking about how dim the light gets over distance
    float distance    = length(light.position - FragPos);
    //This formula is the so called attenuation formula used to calculate the attenuation
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));
    //To apply the attenuation simply multiply it into each of the elements
    ambient  *= attenuation;
    diffuse  *= attenuation;
    specular *= attenuation;

    vec3 result = ambient + diffuse + specular;
    FragColor = vec4(result, 1.0);
}