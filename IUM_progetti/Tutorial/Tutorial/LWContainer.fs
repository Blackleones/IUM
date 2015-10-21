(*
    come avviene il passaggio di un evento.

    FORM -> EDITOR -> LWCONTAINER.
        da qui sono io programmatore a esplicitare a chi
        voglio mandare l'evento. Questo avviene tramite
        override degli eventi (vedi sotto)

        esempio: la funzione paint passa l'evento "disegna" 
            a tutti i controlli presenti 
*)

namespace tutorial
    module LWContainer =
        open System.Drawing
        open System.Windows.Forms
        open LWC

        type LWContainer() = 
            inherit UserControl()

            let controls = ResizeArray<LWC>()

            (*
                copio l'evento del mouse generato dalla form
                per inviarlo al bottone interessato
            *)
            let cloneMouseEvent (control : LWC) (e : MouseEventArgs) = 
                new MouseEventArgs(e.Button,
                 e.Clicks,
                 e.X - int(control.Location.X),
                 e.Y - int(control.Location.Y),
                 e.Delta)

            (*
                member mi permette di vedere gli oggetti
                dall'esterno.

                qual è il problema? chiamando l'array di bottoni
                "controls" vado a creare ambiguità quando devo 
                aggiungere dei bottoni nell'array.

                dall'esterno Controls ha il significato di 
                "controllore/gestore" mentre io intendo l'array,
                allora dico che "LWControls" ritorna "controls"
                che è esattamente il resizeArray
            *)
            member this.LWControls = controls

            override this.OnMouseDown e = 
                printfn "onmousedown"
                base.OnMouseDown e

            override this.OnMouseUp e = 
                printfn "onmouseup"
                base.OnMouseUp e

            override this.OnMouseMove e = 
                base.OnMouseMove e

            (*
                questa funzione viene richiamata ogni volta che
                la form si ridisegna. cosa fa questo override?

                per ogni controllo andiamo a:
                    1) salvare l'ambiente grafico
                    2) trasliamo l'ambiente grafico al centro del controllo 
                        (o nell'angolo in alto a sinistra del controllo?)
                    3) diciamo al sistema grafico che la regione di clipping
                        messa a disposizione per disegnare è grossa tanto
                        quanto il bottone da disegnare

                        nota: parte dal punto (0, 0) perche abbiamo spostato
                            le coordinate del sistema grafico nel punto
                            dov'è situato il bottone (vedi punto 2)

                    4) prendo i bordi della regione di clipping
                    5) creo un nuovo paintEventArgs che servirà al controllo
                        in esame per disegnarsi
                    6) invoco la funzione OnPaint del controllo in esame
                        e gli passo l'evento appena creato
                    7) ripristino l'ambiente grafico com'era al punto 1

            *)
            override this.OnPaint e = 
                printfn "onpaint"
                controls |> Seq.iter (fun control -> 
                    let save = e.Graphics.Save()
                    e.Graphics.TranslateTransform(control.Location.X, control.Location.Y)
                    e.Graphics.Clip <- new Region(RectangleF(0.f, 0.f, control.Size.Width, control.Size.Height))
                    let border = e.Graphics.ClipBounds
                    let paintevt = new PaintEventArgs(e.Graphics, new Rectangle(int(border.Left), int(border.Top), int(border.Width), int(border.Height)))
                    control.OnPaint paintevt
                    e.Graphics.Restore(save)
                   )
                base.OnPaint e