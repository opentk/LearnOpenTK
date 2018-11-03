using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace LearnOpenGL_TK
{
    //This is where all OpenGL code will be written. For now, though, leave it blank outside of a simple constructor.
    class Game : GameWindow
    {
        //A simple constructor to let us set the width/height/title of the window.
        public Game(int width, int height, string title) : base(width, height, GraphicsMode.Default, title) { }

        //OpenTK allows for several functions to be overriden to extend functionality; this is how we'll be writing code.
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            KeyboardState input = Keyboard.GetState();

            if (input.IsKeyDown(Key.Escape))
            {
                Exit();
            }

            base.OnUpdateFrame(e);
        }
    }
}
