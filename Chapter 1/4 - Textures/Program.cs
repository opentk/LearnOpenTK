namespace LearnOpenGL_TK
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            using (var window = new Window(800, 600, "LearnOpenTK - Textures"))
            {
                window.Run(60.0);
            }
        }
    }
}
