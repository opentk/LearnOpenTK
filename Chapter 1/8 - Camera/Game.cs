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
        //We also need a boolean set to true to detect whether or not the mouse has been moved for the first time
        //Finally we add the last position of the mouse so we can calculate the mouse offset easily
        Camera camera;
        bool firstMove = true;
        Vector2 lastPos;

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

            //We initialize the camera so that it is 3 units back from where the rectangle is
            //and give it the proper aspect ratio
            camera = new Camera(Vector3.UnitZ * 3);
            camera.AspectRatio = Width / (float)Height;
            //We make the mouse cursor invisible so we can have proper FPS-camera movement
            CursorVisible = false;
            
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
            shader.SetMatrix4("view", camera.GetViewMatrix());
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());

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
            
            if (input.IsKeyDown(Key.W))
                camera.Position += camera.Front * camera.Speed * (float)e.Time; //Forward 
            if (input.IsKeyDown(Key.S))
                camera.Position -= camera.Front * camera.Speed * (float)e.Time; //Backwards
            if (input.IsKeyDown(Key.A))
                camera.Position -= camera.Right * camera.Speed * (float)e.Time; //Left
            if (input.IsKeyDown(Key.D))
                camera.Position += camera.Right * camera.Speed * (float)e.Time; //Right
            if (input.IsKeyDown(Key.Space))
                camera.Position += camera.Up * camera.Speed * (float)e.Time; //Up 
            if (input.IsKeyDown(Key.LShift))
                camera.Position -= camera.Up * camera.Speed * (float)e.Time; //Down

            //Get the mouse state
            MouseState mouse = Mouse.GetState();

            if (firstMove) // this bool variable is initially set to true
            {
                lastPos = new Vector2(mouse.X, mouse.Y);
                firstMove = false;
            }
            else if (Focused) // check to see if the window is focused
            {
                //Calculate the offset of the mouse position
                float deltaX = mouse.X - lastPos.X;
                float deltaY = mouse.Y - lastPos.Y;
                lastPos = new Vector2(mouse.X, mouse.Y);
                
                //Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                camera.Yaw += deltaX * camera.Sensitivity;
                camera.Pitch -= deltaY * camera.Sensitivity; // reversed since y-coordinates range from bottom to top
            }
            
            base.OnUpdateFrame(e);
        }

        //This function's main purpose is to set the mouse position back to the center of the window
        //every time the mouse has moved. So the cursor doesn't end up at the edge of the window where it cannot move
        //further out
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            if (Focused) // check to see if the window is focused
                Mouse.SetPosition(X + Width/2f, Y + Height/2f);
            
            base.OnMouseMove(e);
        }

        //In the mouse wheel function we manage all the zooming of the camera
        //this is simply done by changing the FOV of the camera
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            camera.Fov -= e.DeltaPrecise;
            base.OnMouseWheel(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            //We need to update the aspect ratio once the window has been resized
            camera.AspectRatio = Width / (float)Height;
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