namespace LearnOpenGL_TK
{
    public class Program
    {
        static void Main(string[] args)
        {
            using (var game = new Game(800, 600, "LearnOpenTK - Element Buffer Objects"))
            {
                game.Run(60.0);
            }
        }
    }
}
