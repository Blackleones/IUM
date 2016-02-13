namespace Start
module Start = 
    open System.Windows.Forms
    open System.Drawing
    open MainControls
    (*
        COMANDI:
            N+- = aggiungi / togli nota
            P+ = aggiungi pentagramma
            V = scorrimento verticale
            C = copia&incolla
    *)
    [<EntryPoint>]
    let main argv = 
        let f = new Form(Size=Size(600, 800), MaximumSize=Size(600,800), MinimumSize=Size(600,800), TopMost=true)
        let c = new MainControls(Dock=DockStyle.Fill)
        f.Controls.Add(c)
        f.Show()
        Application.Run(f)
        0
