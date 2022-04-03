using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace LearnOpenTK
{
    public static class Program
    {
        private static void Main()
        {
            NativeWindowSettings nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Title = "LearnOpenTK - Creating a Window",
                // This is needed to run on macos
                Flags = ContextFlags.ForwardCompatible,
            };

            // To create a new window, create a class that extends GameWindow, then call Run() on it.
            using (GameWindow window = new GameWindow(GameWindowSettings.Default, nativeWindowSettings))
            {
                window.Run();
            }

            // And that's it! That's all it takes to create a window with OpenTK.
        }
    }
}
