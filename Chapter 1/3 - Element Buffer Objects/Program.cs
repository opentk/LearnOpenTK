namespace LearnOpenGL_TK
{
    public class Program
    {
        static void Main(string[] args)
        {
            using (var window = new Window(800, 600, "LearnOpenTK - Element Buffer Objects"))
            {
                window.Run(60.0);
            }
        }
    }
}
