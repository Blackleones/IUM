module LWC 
open System.Windows.Forms
open System.Drawing

    type LWC() = 
        let mutable parent : Control = null
        let mutable location = PointF()
        let mutable size = SizeF()

        member this.Invalidate() = 
            if parent <> null then parent.Invalidate()

        member this.Parent 
            with get() = parent
            and set(v) = parent <- v

        member this.Location 
            with get() = location
            and set(v) = location <- v ; this.Invalidate()

        member this.Size 
            with get() = size
            and set(v) = size <- v ; this.Invalidate()

        abstract HitTest : PointF -> bool
        default this.HitTest p = 
            (RectangleF(PointF(), size)).Contains(p)

        abstract OnPaint : PaintEventArgs -> unit
        default this.OnPaint _ = ()
        
        abstract OnMouseUp : MouseEventArgs -> unit
        default this.OnMouseUp _ = ()

        abstract OnMouseDown : MouseEventArgs -> unit
        default this.OnMouseDown _ = ()

        abstract OnMouseMove : MouseEventArgs -> unit
        default this.OnMouseMove _ = ()
(*------------------------------------------------------------------*)
    type LWContainer() = 
        inherit UserControl()

        let controls = ResizeArray<LWC>()

        let cloneMouseEvent (c : LWC) (e : MouseEventArgs) = 
            new MouseEventArgs(e.Button, e.Clicks, e.X - int(c.Location.X), e.Y - int(c.Location.Y), e.Delta)

        let correlate (e : MouseEventArgs) (f : LWC->MouseEventArgs->unit) = 
            let mutable found = false
            for i in { (controls.Count - 1) .. -1 .. 0 } do 
                if not found then
                    let c = controls.[i]
                    if c.HitTest(PointF(single(e.X) - c.Location.X, single(e.Y) - c.Location.Y)) then
                        found <- true
                        f c (cloneMouseEvent c e)

        member this.LWControls = controls

        override this.OnPaint e = 
            e.Graphics.SmoothingMode <- Drawing2D.SmoothingMode.AntiAlias
            controls |> Seq.iter(fun c ->
                let s = e.Graphics.Save()
                e.Graphics.TranslateTransform(c.Location.X, c.Location.Y)
                c.OnPaint e
                e.Graphics.Restore(s)
            )
            base.OnPaint(e)