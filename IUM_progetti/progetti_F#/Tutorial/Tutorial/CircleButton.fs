namespace tutorial
    module CircleButton = 
        open System.Windows.Forms
        open System.Drawing
        open LWC

        type CircleButton() as this = 
            inherit LWC()

            let clickevt = new Event<System.EventArgs>()
            let downevt = new Event<System.EventArgs>()
            let upevt = new Event<System.EventArgs>()
            let moveevt = new Event<System.EventArgs>()

            //imposto la grandezza
            do this.Size <- SizeF(32.f, 32.f)

            (*
                creo le code per gli eventi.
                ogni coda contiene le funzioni che dovrà
                invocare ogni volta che avviene uno specifico
                evento
            *)
            member this.Click = clickevt.Publish
            member this.MouseDown = downevt.Publish
            member this.MouseUp = upevt.Publish
            member this.MouseMove = moveevt.Publish

            (*
                avvia gli eventi della coda relativa
                all'evento scatenato
            *)
            override this.OnMouseUp e = upevt.Trigger(e)
            override this.OnMouseDown e = downevt.Trigger(e)
            override this.OnMouseMove e = moveevt.Trigger(e)
            override this.OnPaint e = 
                let g = e.Graphics
                g.FillEllipse(Brushes.Red, new Rectangle(0, 0, int(this.Size.Width), int(this.Size.Height)))
