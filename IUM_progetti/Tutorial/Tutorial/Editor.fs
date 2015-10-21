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
                test gestore evento del bottone: FAIL. Perche?
                perche per adesso l'evento viene catturato dal LWContainer. LWContainer non gestisce l'evento
                e quindi non ci sara' alcun modo per mandare l'evento al bottone cliccato
            *)
            do circleButtons |> Seq.iter (fun button -> button.MouseDown.Add(fun _ -> printfn "mi hai cliccato"))

            member this.Form with get() = form and set(v) = form <- v

            
                