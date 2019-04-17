namespace LearnOpenGL_TK
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Window window = new Window(800, 600, "LearnOpenTK - Coordinates Systems"))
            {
                window.Run(60.0);
            }
        }
    }
}
