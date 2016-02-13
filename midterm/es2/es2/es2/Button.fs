module Button
open System.Windows.Forms
open System.Drawing
open LWC

type Button() = 
    inherit LWC()

    let mutable start = 0
    let mutable stop = 0
    let mutable color = Brushes.Red

    do
        base.Size <- SizeF(50.f, 50.f)

    member this.Move mills =
        if (mills >= start) && (mills < stop) then 
            this.Location <- PointF(this.Location.X + 1.f, this.Location.Y)

    member this.Start 
        with get() = start
        and set(v) = start <- v

    member this.Stop 
        with get() = stop
        and set(v) = stop <- v

    member this.Color 
        with get() = color
        and set(v) = color <- v

    override this.OnPaint e =
        let rect = RectangleF(this.Location.X, this.Location.Y, base.Size.Width, base.Size.Height)
        e.Graphics.FillRectangle(color, rect)
