using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using LearnOpenGL_TK.Common;

namespace LearnOpenGL_TK
{
    //We now have a rotating rectangle but how can we make the view move based on the users input
    //In this tutorial we will take a look at how you could implement a camera class
    //and start responding to user input
    //You can move to the camera class to see a lot of the new code added
    //Otherwise you can move to Load to see how the camera is initialized
    
    //In reality we can't move the camera but we actually move the rectangle
    //I will explain this more in depth in the web version, however it pretty much gives us the same result
    //as if i could move the view
    public class Game : GameWindow
    {
        float[] vertices =
        {
            //Position          Texture coordinates
             0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // top left 
        };

        uint[] indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        int ElementBufferObject;
        int VertexBufferObject;
        int VertexArrayObject;

        Shader shader;
        Texture texture;
        Texture texture2;
        
        //I have removed the view and projection matrices as we dont need them here anymore
        //They can now be found in the new camera class
        
        //We need an instance of the new camera class so it can manage the view and projection matrix code
        Camera camera;
        //The movementSpeed is how many units the camera will move in one second
        float movementSpeed = 1.5f;
        //The sensitivity is as you might have guessed the sensitivity (or how fast) the mouse moves along the screen
        float sensitivity = .001f;

        double time = 0.0;


        public Game(int width, int height, string title) : base(width, height, GraphicsMode.Default, title) { }

        
        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            GL.Enable(EnableCap.DepthTest);

            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            ElementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            shader = new Shader("shader.vert", "shader.frag");
            shader.Use();

            texture = new Texture("container.png");
            texture.Use(TextureUnit.Texture0);

            texture2 = new Texture("awesomeface.png");
            texture2.Use(TextureUnit.Texture1);

            shader.SetInt("texture0", 0);
            shader.SetInt("texture1", 1);

            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexArrayObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);


            int vertexLocation = shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);


            int texCoordLocation = shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            //Initialize the camera, and set the perspective it needs the field of view and an aspect ratio
            //Go to the camera class to see more specifically how the camera is made
            //The fov is set to 45 as that is considered most realistic however most games use 90
            //You can think of the fov as the angle of the camera
            //The aspect ratio is just the width of the viewport divided by the height
            //Head to RenderFrame to see how we pass the matrices to the shader
            //Also remember to set the perspective OnResize, since the aspect ratio can change once the window is resized
            camera = new Camera();
            camera.SetPerspective(45, Width / Height);
            //We should have the camera moved a bit back at the start so we can see the rectangle,
            //otherwise we will start with the rectangle within our near clipping plane
            camera.Move(Vector3.UnitZ * -3);;

            base.OnLoad(e);
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            time += 4.0 * e.Time;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(VertexArrayObject);

            texture.Use(TextureUnit.Texture0);
            texture2.Use(TextureUnit.Texture1);
            shader.Use();

            Matrix4 model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(time));
            shader.SetMatrix4("model", model);
            //The camera now takes care of passing the view matrix and the projection matrix
            //You can head to UpdateFrame to see how we process the user input to update the camera
            shader.SetMatrix4("projection", camera.projection);
            shader.SetMatrix4("view", camera.view);

            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

            Context.SwapBuffers();

            base.OnRenderFrame(e);
        }


        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            KeyboardState input = Keyboard.GetState();

            if (input.IsKeyDown(Key.Escape))
            {
                Exit();
            }

            //Here some new inputs for the camera has been added
            //So now we can actually start listening for user inputs and make a responsible window
            //We want to check if the window should move
            //We multiply our movement with the time between frames to make the movement based on real time
            //This way you will move equally fast if you have a slow and/or fast computer
            //Then we multiply by the movementSpeed to apply that
            if (input.IsKeyDown(Key.W)) camera.Move(Vector3.UnitZ * (float)e.Time * movementSpeed);        //Move forwards
            if (input.IsKeyDown(Key.S)) camera.Move(-Vector3.UnitZ * (float)e.Time * movementSpeed);       //Move backwards
            if (input.IsKeyDown(Key.D)) camera.Move(-Vector3.UnitX * (float)e.Time * movementSpeed);       //Move to the right
            if (input.IsKeyDown(Key.A)) camera.Move(Vector3.UnitX * (float)e.Time * movementSpeed);        //Move to the left
            if (input.IsKeyDown(Key.Space)) camera.Move(-Vector3.UnitY * (float)e.Time * movementSpeed);    //Move up
            if (input.IsKeyDown(Key.LShift)) camera.Move(Vector3.UnitY * (float)e.Time * movementSpeed);  //Move down
            
            //To handle the rotation of the camera you should check out MouseMove
            
            base.OnUpdateFrame(e);
        }
        
        //This is called whenever the mouse position has changed from one frame to the next
        //For now we want to use this to rotate the camera
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            //Only perform the movement if the right mouse button is down
            if (e.Mouse.IsButtonDown(MouseButton.Right))
            {
                //Next we should get the x and y position of the mouse within the window
                //Then we should rotate the the camera based on that position
                camera.Rotate(new Vector3(sensitivity * -e.YDelta,
                    sensitivity * -e.XDelta,0));
            }
            
            base.OnMouseMove(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            camera.SetPerspective(45, Width / Height);
            base.OnResize(e);
        }


        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(VertexBufferObject);
            GL.DeleteVertexArray(VertexArrayObject);

            shader.Dispose();
            texture.Dispose();
            texture2.Dispose();

            base.OnUnload(e);
        }
    }
}
