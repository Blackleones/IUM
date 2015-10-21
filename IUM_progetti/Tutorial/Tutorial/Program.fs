namespace tutorial
    module Main = 
        open System.Windows.Forms
        open Editor

        [<EntryPoint>]
        let main argv = 
            let form = new Form(TopMost=true)
            let editor = new Editor(Form=form)
            editor.Dock <- DockStyle.Fill
            form.Controls.Add(editor)
            form.Show()

            Application.Run(form)
            0 // restituisci un intero come codice di uscita
