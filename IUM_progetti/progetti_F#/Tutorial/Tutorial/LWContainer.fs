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
        open CircleButton

        type LWContainer() = 
            inherit UserControl()

            let controls = ResizeArray<LWC>()
            (*
                questa variabile mi serve per capire quando ho catturato un controller 
                e quando no. 
                nota: è essenziale per la gestione grafica in se? assolutamente no.
                a cosa serve? mi occorre per effettuare delle gestioni piu' complicate
                del semplice "clicca / rilascia" come ad esempio il drag and drop.
                
                esempio drag and drop:
                    L'idea di base è che io non sono interessato a "onMouseMove" finchè
                    1) non ho selezionato un controller
                    2) sto tenendo premuto il controller

                    se queste 2 condizioni sono soddisfatte allora io voglio spostare il
                    controller selezionato in modo tale che segua lo spostamento del mouse.

                    per fare ciò ho bisogno:
                    1) di avere un riferimento al controller selezionato
                    2) avere un flag che mi dice "onMouseDown non è ancora stato rilasciato"

                captured ha il compito di "memorizzare" il controller premuto.
                
                esempio d'uso:
                    (fun c ev -> captured <- Some(c); c.OnMouseDown(ev))

                    captured assume il riferimento a "c" che è un qualche(some) elemento di LWC

            *)
            let mutable captured : LWC option = None
            let mutable downIsPressed = false
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
                correlate è una funzione che ci permette di verificare se il click interessa
                un controller oppure no.

                per ogni controller contenuto all'interno del resizeArray controls andiamo 
                ad effettuare "hitTest" e se ritorna TRUE (l'evento ricade all'interno dello
                specifico controller):
                1) blocco il ciclo (found <- true)
                2) eseguo una funzione f:
                    LWC->MouseEventArgs->unit
                    il controller cliccato -> l'evento scatenato e da gestire -> void
            *)
            let correlate (evt : MouseEventArgs) (f : LWC->MouseEventArgs->unit) = 
                let mutable found = false
                for i in { (controls.Count - 1) .. -1 .. 0 } do
                    if not found then
                        let control = controls.[i]
                        if control.HitTest(PointF(single(evt.X) - control.Location.X, single(evt.Y) - control.Location.Y)) then
                            found <- true
                            f control (cloneMouseEvent control evt)
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
                printfn "onmousedown X = %d Y = %d" e.X e.Y 
                (*
                    per ipotesi dico che tengo premuto fin dal primo momento
                    in cui faccio onMouseDown
                *)
                downIsPressed <- true
                correlate e (fun control ev -> captured <- Some(control); control.OnMouseDown(ev))
                base.OnMouseDown e

            override this.OnMouseUp e = 
                printfn "onmouseup X = %d Y = %d" e.X e.Y
                (*
                    ho rilasciato il tasto => non lo sto piu' tenendo premuto
                *)
                downIsPressed <- false
                correlate e (fun control ev -> control.OnMouseUp(ev))
                (*
                    se captured <> None allora devo settarlo a None
                    in sostanza: non voglio piu' interagire con il controller
                    selezionato
                *)
                match captured with
                | Some control -> control.OnMouseUp(cloneMouseEvent control e); captured <- None
                | None -> ()

                base.OnMouseUp e

            override this.OnMouseMove e = 
                (*
                    test: la condizione dovrebbe essere:
                        downIsPressed AND captured <> None
                *)
                if downIsPressed && captured <> None then
                    //printfn "--- onmousemove X = %d Y = %d" e.X e.Y

                    match captured with
                    | Some control -> 
                        (*
                            il controller catturato è un bottone circolare?
                        *)
                        if control :? CircleButton then
                            let newPoint = PointF(single(e.X), single(e.Y))
                            control.UpdatePosition newPoint
                            this.Invalidate()
                    | None -> ()
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
                //printfn "onpaint"
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