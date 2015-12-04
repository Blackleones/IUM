namespace bouncingballs
    module BouncingBalls =
        open System.Windows.Forms
        open System.Drawing
        open BouncingBall

        (*
            vado a definire la form
        *)
        type BouncingBalls() as this = 
            inherit UserControl()

            (*
                uso implicito del double buffer: dico alla classe super d'implementare il disegno tramite
                doubleBuffer
            *)
            do this.SetStyle(ControlStyles.OptimizedDoubleBuffer ||| ControlStyles.AllPaintingInWmPaint, true)

            (*
                la form è "la form" su cui andiamo a disegnare. Questa ci verrà
                passata dall'esterno durante la creazione dell'oggetto BouncingBalls
            *)
            let mutable form = null
            let mutable buffer : Bitmap = null
            let mutable balls = new ResizeArray<BouncingBall>()
            let timer = new Timer(Interval=30) //30FPS

            (*
                aggiorno il buffer: se il buffer non esiste allora lo creo come nuova immagine BITMAP
                 altrimenti buffer.Dispose() <- IMPORTANTE = mi permette di collegare il buffer al Garbage
                        Collector cosi da poterlo eliminare appena non mi serve piu

            *)
            let updateBuffer() =
                if buffer = null || buffer.Width <> this.Width || buffer.Height <> this.Height then
                    if buffer <> null then buffer.Dispose()
                    buffer <- new Bitmap(this.Width, this.Height)
            
            (*
                se la palla picchia sulle pareti allora gli cambio la direzione della velocità
            *)
            let updateBall() = 
                balls |> Seq.iter (fun ball ->
                    ball.UpdatePosition()
                    
                    if ball.Location.X < 0.f || ball.Location.X + ball.Size.Width > single(this.Width) then
                        ball.Speed <- SizeF( - ball.Speed.Width, ball.Speed.Height)
                    if ball.Location.Y < 0.f || ball.Location.Y + ball.Size.Height > single(this.Height) then
                        ball.Speed <- SizeF(ball.Speed.Width, - ball.Speed.Height)
                )

            (*
                il costrutto "do" a quanto pare mi permette di scrivere il codice sequenziale
                che verrà eseguito dopo la creazione dell'oggetto.

                posso scrivere "do" dove mi pare, tanto verrà comunque eseguito sequenzialmente.
                (che senso ha?)
            *)
            do
               timer.Tick.Add(fun _ ->
                    updateBall()
                    this.Invalidate()
               )
               timer.Start()

            do balls.Add(new BouncingBall(Location=PointF(20.f, 20.f)))

            //getters and setters
            member this.Form with get() = form and set(v) = form <- v

            (*
                implementazione esplicita della tecnica DOUBLE BUFFER:
                    a cosa serve? permette di eliminare l'effetto flickering.
                    come? il double buffer ci permette di avere 2 buffer su cui andare
                    a disegnare:
                        screen -> buffer 1: disegno ONLINE
                        buffer 2: disegno OFFLINE

                        se noi andiamo a disegnare direttamente su screen otteniamo l'effetto
                        flickering in quanto la scheda video va a leggere il frame buffer con una 
                        frequenza diversa rispetto a quella con cui il gestore sta disegnando 
                        => notiamo l'effetto flickering "disegno mentre sto finendo il vecchio disegno"
                        
                        come possiamo risolvere questo problema? una possibile soluzione consiste
                        nel disegnare il prossimo frame su un buffer di supporto (buffer 2) e quindi
                        otteniamo un disegno OFFLINE: l'utente non si accorge della generazione del nuovo
                        frame in quanto questo è disegnato su un buffer in memoria.

                        Ma allora come avviene il passaggio dalla memoria RAM (credo) a screen? quando il
                        buffer 2 è stato completamente disegnato, avvisiamo il gestore grafico che il nuovo
                        frame lo trova direttamente in memoria all'interno del buffer 2. Il contesto grafico
                        non fara' altro che prelevare il buffer e stamparlo 1 VOLTA SOLA a video 
                        => "addio" effetto flickering.

                        maggiore è il numero di buffer di supporto, minore è percezione del flickering e
                        peggiore è l'uso dello spazio in memoria

                        nota: una immagine è sempre quadratica => elementi del buffer usati ordine O(n^2)
                        => 1 buffer comporta O(n^2) * 30 frame * secondo
            *)
            override this.OnPaint e = 

                updateBuffer()
                let vg = e.Graphics
                let g = Graphics.FromImage(buffer)
                use bg = new SolidBrush(this.BackColor) //use = mi creo e mi ammazzo subito
                //pulisco lo schermo
                g.FillRectangle(bg, 0, 0, buffer.Width, buffer.Height)

                balls |> Seq.iter (fun ball ->
                    let r = ball.Bounds
                    g.FillEllipse(ball.Color, r)
                    g.DrawEllipse(ball.BorderColor, r)
                )      

                //il buffer 2 è pronto! lo disegno a video
                vg.DrawImage(buffer, 0, 0)

            override this.OnMouseDown(e) = 
                balls.Add(new BouncingBall(Location=PointF(single(e.X), single(e.Y))))