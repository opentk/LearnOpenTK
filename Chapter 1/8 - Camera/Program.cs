namespace LearnOpenGL_TK
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Game game = new Game(800, 600, "LearnOpenTK - Camera Tutorial"))
            {
                game.Run(60.0);
            }
        }
    }
}
