namespace LearnOpenTK
{
    public static class Program
    {
        private static void Main()
        {
            using (var window = new Window(800, 600, "LearnOpenTK - Hello Triangle!"))
            {
                window.Run(60.0);
            }
        }
    }
}
