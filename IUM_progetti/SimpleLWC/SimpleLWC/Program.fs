//Altre informazioni su F# all'indirizzo http://fsharp.org
// Per ulteriori informazioni, vedere il progetto 'Esercitazione su F#'.
namespace Main
module Main =
    open System.Windows.Forms
    open System.Drawing
    open CustomButton.CustomButton
    [<EntryPoint>]
    let main argv = 
        let f = new Form(Text="MyForm", TopMost=true, Dock=DockStyle.Fill)
        f.Show()

        let cb = new CustomButton(Text="ELEONORA")

        f.Paint.Add(fun e ->
            cb.OnPaint(e)
        )

        f.Resize.Add(fun e ->
            cb.Size <- SizeF(float32(f.Width / 2), float32(f.Height / 2))
            f.Invalidate()
        )

        f.Invalidate()
        Application.Run(f)
        0 // restituisci un intero come codice di uscita
