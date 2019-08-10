namespace LearnOpenTK
open OpenTK
open OpenTK.Graphics
open OpenTK.Input
type Window(width : int, height : int, title : string) =
    inherit GameWindow(width, height, GraphicsMode.Default, title)

    override this.OnUpdateFrame(e : FrameEventArgs) = 
        let mutable input = Keyboard.GetState ()
        if input.IsKeyDown (Key.Escape) then this.Exit ()

        base.OnUpdateFrame (e)