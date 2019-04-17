namespace LearnOpenGL_TK
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            using (Window window = new Window(800, 600, "LearnOpenTK - Multiple Textures"))
                window.Run(60.0);
        }
    }
}
