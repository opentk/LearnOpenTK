using System;
using System.IO;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;


namespace LearnOpenTK.Common
{
    // A simple class meant to help create shaders.
    public class Shader : IDisposable
    {
        private readonly int _handle;

        
        // This is how you create a simple shader.
        // Shaders are written in GLSL, which is a language very similar to C in its semantics.
        // The GLSL source is compiled *at runtime*, so it can optimize itself for the graphics card it's currently being used on.
        // A commented example of GLSL can be found in shader.vert
        public Shader(string vertPath, string fragPath)
        {
            // There are several different types of shaders, but the only two you need for basic rendering are the vertex and fragment shaders.
            // The vertex shader is responsible for moving around vertices, and uploading that data to the fragment shader.
            //   The vertex shader won't be too important here, but they'll be more important later.
            // The fragment shader is responsible for then converting the vertices to "fragments", which represent all the data OpenGL needs to draw a pixel.
            //   The fragment shader is what we'll be using the most here.

            // Load vertex shader and compile
            // LoadSource is a simple function that just loads all text from the file whose path is given.
            var shaderSource = LoadSource(vertPath);

            // GL.CreateShader will create an empty shader (obviously). The ShaderType enum denotes which type of shader will be created.
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);

            // Now, bind the GLSL source code
            GL.ShaderSource(vertexShader, shaderSource);

            // And then compile
            CompileShader(vertexShader);

            
            // We do the same for the fragment shader
            shaderSource = LoadSource(fragPath);
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            CompileShader(fragmentShader);


            // These two shaders must then be merged into a shader program, which can then be used by OpenGL.
            // To do this, create a program...
            _handle = GL.CreateProgram();

            // Attach both shaders...
            GL.AttachShader(_handle, vertexShader);
            GL.AttachShader(_handle, fragmentShader);

            // And then link them together.
            LinkProgram(_handle);

            // When the shader program is linked, it no longer needs the individual shaders attacked to it; the compiled code is copied into the shader program.
            // Detach them, and then delete them.
            GL.DetachShader(_handle, vertexShader);
            GL.DetachShader(_handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);
        }


        private static void CompileShader(int shader)
        {
            // Try to compile the shader
            GL.CompileShader(shader);
            
            // Check for compilation errors
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int) All.True)
            {
                // We can use `GL.GetShaderInfoLog(shader)` to get information about the error.
                throw new Exception($"Error occurred whilst compiling Shader({shader})");
            }
        }
        
        private static void LinkProgram(int program)
        {
            // We link the program
            GL.LinkProgram(program);
            
            // Check for linking errors
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int) All.True)
            {
                // We can use `GL.GetProgramInfoLog(program)` to get information about the error.
                throw new Exception($"Error occurred whilst linking Program({program})");
            }
        }
        

        // A wrapper function that enables the shader program.
        public void Use()
        {
            GL.UseProgram(_handle);
        }


        // The shader sources provided with this project use hardcoded layout(location)-s. If you want to do it dynamically,
        // you can omit the layout(location=X) lines in the vertex shader, and use this in VertexAttribPointer instead of the hardcoded values.
        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(_handle, attribName);
        }


        // Just loads the entire file into a string.
        private static string LoadSource(string path)
        {
            using (var sr = new StreamReader(path, Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }
        
        // Uniform setters
        // Uniforms are variables that can be set by user code, instead of reading them from the VBO.
        // You use VBOs for vertex-related data, and uniforms for almost everything else.
        
        // Setting a uniform is almost always the exact same, so I'll explain it here once, instead of in every method:
        //     1. Bind the program you want to set the uniform on
        //     2. Get a handle to the location of the uniform with GL.GetUniformLocation.
        //     3. Use the appropriate GL.Uniform* function to set the uniform.

        /// <summary>
        /// Set a uniform int on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetInt(string name, int data)
        {
            GL.UseProgram(_handle);
            var location = GL.GetUniformLocation(_handle, name);
            GL.Uniform1(location, data);
        }
        
        /// <summary>
        /// Set a uniform float on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetFloat(string name, float data)
        {
            GL.UseProgram(_handle);
            var location = GL.GetUniformLocation(_handle, name);
            GL.Uniform1(location, data);
        }

        /// <summary>
        /// Set a uniform Matrix4 on this shader
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        /// <remarks>
        ///   <para>
        ///   The matrix is transposed before being sent to the shader.
        ///   </para>
        /// </remarks>
        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(_handle);
            var location = GL.GetUniformLocation(_handle, name);
            GL.UniformMatrix4(location, true, ref data);
        }

        /// <summary>
        /// Set a uniform Vector3 on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(_handle);
            var location = GL.GetUniformLocation(_handle, name);
            GL.Uniform3(location, data);
        }


        // This section is dedicated to cleaning up the shader after it's finished.
        // Doing this solely in a finalizer results in a crash because of the Object-Oriented Language Problem
        // ( https://www.khronos.org/opengl/wiki/Common_Mistakes#The_Object_Oriented_Language_Problem )
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                GL.DeleteProgram(_handle);

                disposedValue = true;
            }
        }

        ~Shader()
        {
            Dispose(false);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
