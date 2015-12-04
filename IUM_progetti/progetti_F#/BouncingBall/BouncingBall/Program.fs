namespace bouncingballs
module Main = 
    open System
    open System.Windows.Forms
    open System.Drawing
    open BouncingBalls

    [<EntryPoint>]
    let main argv = 
        let f = new Form(TopMost=true)
        let bouncingBalls = new BouncingBalls(Form=f)
        bouncingBalls.Dock <- DockStyle.Fill
        f.Controls.Add(bouncingBalls)
        f.Show()

        Application.Run(f)
        0