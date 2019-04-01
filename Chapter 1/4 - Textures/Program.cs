namespace LearnOpenGL_TK
{
    public class Program
    {
        static void Main(string[] args)
        {
            using (var game = new Game(800, 600, "LearnOpenTK - Textures"))
            {
                game.Run(60.0);
            }
        }
    }
}
