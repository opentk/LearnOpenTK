using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection.Metadata;
using System.ComponentModel;
using OpenTK.Compute.OpenCL;

namespace LearnOpenTK.Common
{
    // A simple class that can be used to associate a uniform location dictionary to a shader program.
    public class Shader
    {
        public readonly int Handle;

        public readonly Dictionary<string, int> UniformLocations;

        public static Shader FromFile(string vertexPath, string fragmentPath)
        {
            // There are several different types of shaders, but the only two you need for basic rendering are the vertex and fragment shaders.
            // The vertex shader is responsible for moving around vertices, and uploading that data to the fragment shader.
            // The fragment shader is responsible for then converting the vertices to "fragments", which represent all the data OpenGL needs to draw a pixel.

            // Load vertex shader and compile
            string vertexSource = File.ReadAllText(vertexPath);
            string fragmentSource = File.ReadAllText(fragmentPath);

            int program = CompileProgram(vertexSource, fragmentSource);

            Dictionary<string, int> uniformLocations = CreateUniformLocationsDict(program);

            return new Shader(program, uniformLocations);
        }
        
        private static int CompileProgram(string vertexSource, string fragmentSource)
        {
            // Compile vertex shader.
            int vertexShader = CompileShader(ShaderType.VertexShader, vertexSource);

            // We do the same for the fragment shader.
            int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentSource);

            // These two shaders must then be merged into a shader program, which can then be used by OpenGL.
            // To do this, create a program...
            int handle = GL.CreateProgram();

            // Attach both shaders...
            GL.AttachShader(handle, vertexShader);
            GL.AttachShader(handle, fragmentShader);

            // And then link them together.
            GL.LinkProgram(handle);

            // Check for linking errors
            GL.GetProgram(handle, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                // We can use `GL.GetProgramInfoLog(program)` to get information about the error.
                string infoLog = GL.GetProgramInfoLog(handle);
                throw new Exception($"Error occurred whilst linking Program({handle}):\n{infoLog}");
            };

            // When the shader program is linked, it no longer needs the individual shaders attached to it; the compiled code is copied into the shader program.
            // Detach them, and then delete them.
            GL.DetachShader(handle, vertexShader);
            GL.DetachShader(handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            return handle;
        }

        private static int CompileShader(ShaderType type, string source)
        {
            // GL.CreateShader will create an empty shader. The ShaderType enum denotes which type of shader will be created.
            int shader = GL.CreateShader(type);

            // Upload the GLSL source code.
            GL.ShaderSource(shader, source);

            // Try to compile the shader
            GL.CompileShader(shader);

            // Check for compilation errors
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                // We can use `GL.GetShaderInfoLog(shader)` to get information about the error.
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({shader}):\n{infoLog}");
            }

            return shader;
        }

        // This function creates a dictionary that maps uniform names to their location,
        // this is used to speed up uniform location queries.
        public static Dictionary<string, int> CreateUniformLocationsDict(int program)
        {
            // First, we have to get the number of active uniforms in the shader.
            GL.GetProgram(program, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            // Next, allocate the dictionary to hold the locations.
            Dictionary<string, int> uniformLocations = new Dictionary<string, int>();

            // Loop over all the uniforms,
            for (var i = 0; i < numberOfUniforms; i++)
            {
                // get the name of this uniform,
                var key = GL.GetActiveUniform(program, i, out _, out _);

                // get the location,
                var location = GL.GetUniformLocation(program, key);

                // and then add it to the dictionary.
                uniformLocations.Add(key, location);
            }

            return uniformLocations;
        }

        public Shader(int handle, Dictionary<string, int> uniformLocations)
        {
            Handle = handle;
            UniformLocations = uniformLocations;
        }
    }
}
