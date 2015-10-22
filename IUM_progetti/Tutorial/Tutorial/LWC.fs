namespace tutorial
    module LWC =
        open System.Windows.Forms
        open System.Drawing

        type LWC() = 
            let mutable parent : Control = null
            let mutable location = PointF()
            let mutable size = SizeF()

            abstract OnMouseDown : MouseEventArgs -> unit
            default this.OnMouseDown _ = ()

            abstract OnMouseMove : MouseEventArgs -> unit
            default this.OnMouseMove _ = ()

            abstract OnMouseUp : MouseEventArgs -> unit
            default this.OnMouseUp _ = ()

            abstract OnPaint : PaintEventArgs -> unit
            default this.OnPaint _ = ()

            abstract HitTest : PointF -> bool
            default this.HitTest p = (RectangleF(PointF(), size)).Contains(p)

            (*
                semplice funzione che aggiorna la posizione dell'oggetto
            *)
            abstract UpdatePosition : PointF -> unit
            default this.UpdatePosition p =
                location <- p

            member this.Invalidate() = 
                if parent <> null then parent.Invalidate()

            member this.Location with get() = location and set(v) = location <- v
            member this.Size with get() = size and set(v) = size <- v
            member this.Parent with get() = parent and set(v) = parent <- v
