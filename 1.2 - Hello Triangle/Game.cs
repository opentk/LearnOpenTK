using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace LearnOpenGL_TK
{
    //Be warned, there is a LOT of stuff here. It might seem complicated, but just take it slow and you'll be fine.
    class Game : GameWindow
    {
        //Create the vertices for our triangle. These are listed in normalized device coordinates (NDC)
        //In NDC, (0, 0) is the center of the screen.
        //Negative X coordinates move to the left, positive X move to the right.
        //Negative Y coordinates move to the bottom, positive Y move to the top.
        //OpenGL only supports rendering in 3D, so to create a flat triange, the Z coordinate will be kept as 0.
        float[] vertices = {
            -0.5f, -0.5f, 0.0f, //Bottom-left vertex
             0.5f, -0.5f, 0.0f, //Bottom-right vertex
             0.0f,  0.5f, 0.0f  //Top vertex
        };

        //This will be explained down in OnLoad
        int VertexBufferObject;
        int VertexArrayObject;
        Shader shader;


        public Game(int width, int height, string title) : base(width, height, GraphicsMode.Default, title) { }

        
        //Now, we start initializing OpenGL.
        protected override void OnLoad(EventArgs e)
        {
            //This will be the color of the background after we clear it, in normalized colors. This is a deep green.
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            //We need to send our vertices over to the graphics card so OpenGL can use them.
            //To do this, we need to create what's called a Vertex Buffer Object (VBO).
            //These allow you to upload a bunch of data to a buffer, and send the buffer to the graphics card.
            //This effectively sends all the vertices at the same time.
            //Keep in mind that this function returns an int, which is a handle to the actual object.
            VertexBufferObject = GL.GenBuffer();


            //Now, bind the buffer. OpenGL uses one global state, so after calling this,
            //  all future calls that modify the VBO will be applied to this buffer until another buffer is bound instead.
            //The first argument is an enum, specifying what type of buffer it should be bound to.
            //There are multiple types of buffers, but for now, only the VBO is necessary.
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);


            //Finally, upload the vertices to the buffer.
            //Arguments:
            //  What type of buffer the data should be sent to
            //  How much data is being sent, in bytes.
            //  The vertices themselves
            //  How the buffer will be used, so that OpenGL can write the data to the proper memory space on the GPU
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);


            //We've got the vertices done, but how exactly should this be converted to pixels for the final image?
            //To decide this, we must create what are known as shaders; these are small programs that live on the graphics card, and transform the vertices into pixels.
            //The file shader.vert has an example of what shader programming is like.
            shader = new Shader("shader.vert", "shader.frag");

            //Now, enable the shader.
            //Just like the VBO, this is global, so every function that modifies a shader will modify this one until a new one is bound instead.
            shader.Use();


            //Ignore this for now.
            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);
            
            //Now, we need to setup how the vertex shader will interpret the VBO data; you can send almost any C datatype (and a few non-C ones too) to it.
            //While this makes them incredibly flexible, it means we have to specify how that data will be mapped to the shader's input variables.

            //To do this, we use the GL.VertexAttribPointer function
            //Arguments:
            //  Location of the input variable in the shader. the layout(location = 0) line in the vertex shader explicitly sets it to 0.
            //  How many elements will be sent to the variable. In this case, 3 floats for every vertex.
            //  The data type of the elements set, in this case float.
            //  Whether or not the data should be converted to normalized device coordinates. In this case, false, because that's already done.
            //  The stride; this is how many bytes are between the last element of one vertex and the first element of the next. 3 * sizeof(float) in this case.
            //  The offset; this is how many bytes it should skip to find the first element of the first vertex. 0 as of right now.
            //Stride and Offset are just sort of glossed over for now, but when we get into texture coordinates they'll be shown in better detail.
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            //Enable variable 0 in the shader.
            GL.EnableVertexAttribArray(0);


            //For a simple project, this would probably be enough. However, if you have a bunch of objects with their own shaders being drawn, it would be incredibly
            //tedious to do this over and over again every time you need to switch what object is being drawn. Because of this, OpenGL now *requires* that you create
            //what is known as a Vertex Array Object (VAO). This stores the layout you create with VertexAttribPointer/EnableVertexAttribArray so that it can be
            //recreated with one simple function call.
            //By creating the VertexArrayObject, it has automatically saved this layout, so you can simply bind the VAO again to get everything back how it should be.

            //Finally, we bind the VBO again so that the VAO will bind that as well.
            //This means that, when you bind the VAO, it will automatically bind the VBO as well.
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);


            //Setup is now complete! Now we move to the OnRenderFrame function to finally draw the triangle.

            base.OnLoad(e);
        }


        //Now that initialization is done, let's create our render loop.
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            //This clears the image, using what you set as GL.ClearColor earlier.
            //OpenGL provides several different types of data that can be rendered, and all of them can be cleared here.
            //However, we only modify the color, so ColorBufferBit is all we need to clear.
            GL.Clear(ClearBufferMask.ColorBufferBit);


            //To draw an object in OpenGL, it's typically as simple as binding your shader,
            //setting shader variables (not done here, will be shown in a future tutorial)
            //binding the VAO,
            //and then calling GL.DrawArrays

            //Bind the shader
            shader.Use();

            //Bind the VAO
            GL.BindVertexArray(VertexArrayObject);

            //And then call GL.DrawArrays
            //Arguments:
            //  Primitive type; What sort of geometric primitive the vertices represent. We just want a triangle, so PrimitiveType.Triangles is fine.
            //  Starting index; this is just the start of the data you want to draw. 0 here.
            //  How many vertices you want to draw. 3 for a triangle.
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);


            //OpenTK windows are what's known as "double-buffered". In essence, the window manages two images.
            //One is rendered to while the other is currently displayed by the window.
            //After drawing, call this function to swap the buffers. If you don't, it won't display what you've rendered.
            Context.SwapBuffers();


            //And that's all you have to do for rendering! You should now see a yellow triangle on a black screen.
            base.OnRenderFrame(e);
        }


        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            KeyboardState input = Keyboard.GetState();

            if (input.IsKeyDown(Key.Escape))
            {
                Exit();
            }

            base.OnUpdateFrame(e);
        }


        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            base.OnResize(e);
        }


        //Now, for cleanup. This isn't technically necessary since C# will clean up all resources automatically when the program closes, but it's very
        //important to know how anyway.
        protected override void OnUnload(EventArgs e)
        {
            // Unbind all the resources by binding the targets to 0/null.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            // Delete all the resources.
            GL.DeleteBuffer(VertexBufferObject);
            GL.DeleteVertexArray(VertexArrayObject);

            shader.Dispose();
            base.OnUnload(e);
        }
    }
}
