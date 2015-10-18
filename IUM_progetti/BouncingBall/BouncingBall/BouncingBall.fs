namespace bouncingballs
    module BouncingBall =
        open System.Windows.Forms
        open System.Drawing

        type BouncingBall() =
            let mutable location = PointF() //posizione dell'oggetto
            let mutable speed = SizeF(105.f, 105.f)
            let mutable size = SizeF(25.f, 25.f)
            let mutable lastTick = System.DateTime.Now
            let mutable borderColor = Pens.DarkBlue
            let mutable color = Brushes.Blue

            //definizione dei getters and setters
            member this.Location with get() = location and set(v) = location <- v
            member this.Speed with get() = speed and set(v) = speed <- v
            member this.Size with get() = size and set(v) = size <- v
            member this.Bounds with get() = new RectangleF(location, size)
            member this.BorderColor with get() = borderColor and set(v) = borderColor <- v
            member this.Color with get() = color and set(v) = color <- v
            (*
                funzione che aggiorna lo stato dell'oggetto.
                la velocità è espressa in pixel * millisecondo
            *)
            member this.UpdatePosition() = 
                let t = System.DateTime.Now
                let dt = t - lastTick
                let vx = speed.Width / 1000.f
                let vy = speed.Height / 1000.f
                let dx = vx * single(dt.TotalMilliseconds)
                let dy = vy * single(dt.TotalMilliseconds)
                location <- PointF(location.X + dx, location.Y + dy)
                lastTick <- t