using OpenTK;
using OpenTK.Graphics;

namespace LearnOpenGL_TK
{
    //This is where all OpenGL code will be written. For now, though, leave it blank outside of a simple constructor.
    class Game : GameWindow
    {
        //A simple constructor to let us set the width/height/title of the window.
        public Game(int width, int height, string title) : base(width, height, GraphicsMode.Default, title) { }
    }
}
