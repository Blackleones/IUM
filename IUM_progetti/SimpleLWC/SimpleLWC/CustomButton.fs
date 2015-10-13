namespace CustomButton

module CustomButton =
    open System.Windows.Forms
    open System.Drawing
    open LWC.LWC

    type CustomButton() as this =
        inherit LWC()

        let mutable text = ""
        let mutable textColor = Brushes.Red
        let mutable backgroundColor = Color.Azure
        let mutable font = new Font("arial", 12.f)

        do this.Size <- SizeF(32.f, 32.f)

        member this.Text 
            with get() = text
            and set(v) = text <- v; this.Invalidate()

        member this.TextColor 
            with get() = textColor
            and set(v) = textColor <- v; this.Invalidate()

        member this.BackgroundColor
            with get() = backgroundColor
            and set(v) = backgroundColor <- v; this.Invalidate()
        
        member this.Font 
            with get() = font
            and set(v) = font <- v; this.Invalidate()


        override this.OnPaint e = 
            printfn "OnPaint - CustomButton"
            let g = e.Graphics
            let body = Rectangle(int this.Location.X, int this.Location.Y, int this.Size.Width, int this.Size.Height)
            let stringLength = g.MeasureString(this.Text, font)
            let textShift = PointF((stringLength.Width + this.Size.Width) / 2.f, (stringLength.Height + this.Size.Height) / 2.f)
            g.DrawRectangle(Pens.Black, body)
            g.DrawString(text, font, textColor, this.Location.X + textShift.X, this.Location.Y + textShift.Y)
            printfn "%A" (this.Location.X, textShift.X, this.Location.Y, textShift.Y)
