#version 330 core
//The material is a collection of some values that we talked about in the last tutorial,
//some crucial elements to the phong model.
struct Material {
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

    float shininess; //Shininess is the power the specular light is raised to
};
//The light contains all the values from the light source, how the ambient diffuse and specular values are from the light source.
//This is technically what we were using in the last episode as we were only applying the phong model directly to the light.
struct Light {
    vec3 position;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};
//We create the light and the material struct as uniforms.
uniform Light light;
uniform Material material;
//We still need the view position.
uniform vec3 viewPos;

out vec4 FragColor;

in vec3 Normal;
in vec3 FragPos;

void main()
{
    //ambient
    vec3 ambient = light.ambient * material.ambient; //Remember to use the material here.

    //diffuse 
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(light.position - FragPos);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = light.diffuse * (diff * material.diffuse); //Remember to use the material here.

    //specular
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    vec3 specular = light.specular * (spec * material.specular); //Remember to use the material here.

    //Now the result sum has changed a bit, since we now set the objects color in each element, we now dont have to
    //multiply the light with the object here, instead we do it for each element seperatly. This allows much better control
    //over how each element is applied to different objects.
    vec3 result = ambient + diffuse + specular;
    FragColor = vec4(result, 1.0);
}