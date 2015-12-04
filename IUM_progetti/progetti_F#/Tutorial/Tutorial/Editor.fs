namespace tutorial
    module Editor = 
        open System.Windows.Forms
        open System.Drawing
        open CircleButton
        open LWContainer

        type Editor() as this =
            inherit LWContainer() 

            let mutable form : Form = null
            (*
                dico al sistema grafico che sono interessato a usare il doublebuffer
            *)
            do this.SetStyle(ControlStyles.OptimizedDoubleBuffer ||| ControlStyles.AllPaintingInWmPaint, true)
            (*
                creo un array di bottoni circolari
            *)
            let circleButtons = [|
             new CircleButton(Location=PointF(32.f, 0.f));
             new CircleButton(Location=PointF(32.f, 64.f));
            |]

            (*
                itero su ogni bottone una funzione che associa questo Editor come parente del bottone
                e poi aggiunge il bottone all interno dell' array dei controlli
            *)
            do circleButtons |> Seq.iter (fun button -> button.Parent <- this; this.LWControls.Add(button))
            (*
                funzioni da eseguire quando clicco i bottoni circolari
                funzioni TEST per vedere l'andamento degli eventi
            *)
            do circleButtons |> Seq.iter (fun button -> button.MouseDown.Add(fun e -> printfn "mi hai cliccato"))
            do circleButtons |> Seq.iter (fun button -> button.MouseUp.Add(fun _ -> printfn "mi hai rilasciato"))

            member this.Form with get() = form and set(v) = form <- v

            
                