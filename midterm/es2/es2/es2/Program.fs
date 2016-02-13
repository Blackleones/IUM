namespace Start
module Start = 
    open System.Windows.Forms
    open System.Drawing
    open mainControls

    [<EntryPoint>]
    let main argv = 
        let f = new Form(TopMost=false, Size=Size(500,500))
        let c = new mainControls(Dock=DockStyle.Fill)
        f.Controls.Add(c)
        f.Show()
        Application.Run(f)
        0
