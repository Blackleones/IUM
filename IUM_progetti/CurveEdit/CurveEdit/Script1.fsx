//LETTORE DI CURVE

open System.Windows.Forms
open System.Drawing

let f = new Form(Text = "CurveEdit", BackColor = Color.Bisque, TopMost = true)
f.Show()

(* quando creo un bottone, in realtà alloco un po' di risorse di sistema (finestre). Il Framework cosa fa? Fa quello che fa il file
system. Questo ha un overhead e dei comportamenti magari non desiderati. Per esempio nei sistemi grafici tradizionali, le
finestre sono opache.
Definisco una nuova classe che deriva direttamente da object con il sistema LIGHTWEIGHT CONTROLS
In cosa consiste? Nel rifare un pezzettino di sistema grafico a piacimento. Ottengo quindi più praticità e più efficienza.
il Lithtweight controls è un'astrazione programmativa che imita i controlli grafici.
*)
type LWC() = 
    let mutable parent : Control = null //quello che ho creato apparterrà ad un controllo
    let mutable location = PointF()
    let mutable size = SizeF()

    abstract (*indica un tipo astratto, cioè un'interfaccia *) OnPaint : (PaintEventArgs) -> unit
    (*cioè OnPaint prende un tipo evento paint e non restituisce nulla (unit)*)
    default this.OnPaint e = () //di default non fa nulla. 
    //potevo anche lasciarlo astratto anche se così non avrei potuto istanziarlo

    abstract OnMouseDown : MouseEventArgs -> unit
    default this.OnMouseDown _ = ()
    
    abstract OnMouseMove : MouseEventArgs -> unit
    default this.OnMouseDown _ = ()

    abstract OnMouseUp : MouseEventArgs -> unit
    default this.OnMouseDown _ = ()

    member this.Invalidate() = 
        if parent <> null then parent.Invalidate()

    member this.Location
        with get() = location
        and set(v) = location <- v; if parent <> null then parent.Invalidate()

    member this.Parent
        with get() = parent
        and set(v) = parent <- v

    member this.Size
        with get() = size
        and set(v) = size <- v; if parent <> null then parent.Invalidate()

//Adesso creo il button:
type IumButton() as this =
    inherit LWC() //eredita da LWC
    do this.Size <- SizeF(32.f, 32.f) //richiama il Size di LWC

    let mutable text = ""

    member this.Text
        with get() = text
        and set(v) = text <- v; if (this.Parent <> null) then this.Parent.Invalidate()

    override this.OnMouseMove e = ()

    override this.OnPaint e = 
        let g = e.Graphics
        //il bottone vive in coordinate VISTA
        g.FillEllipse(Brushes.Red, new Rectangle(int(this.Location.X), int(this.Location.Y), int(this.Size.Width), int(this.Size.Height)))
        let sz = g.MeasureString(text,this.Parent.Font) //calcolo quanto occupa la stringa UP con il font standard
        g.DrawString (text, this.Parent.Font (*font di default*), Brushes.White, (*dove lo metto? in posizione A*) PointF((this.Location.X - sz.Width)/2.f, (this.Location.Y - sz.Height)/2.f))
        //A=( (larghezzatot-larghezzafont) /2 , (altezzatot - altezzafont)/2)
        
//Introduciamo un controllo grafico che si comporta come la form (che deriva dalla classe controllo che è la radice dei controlli
//grafici)
//FINESTRA: rettangolo di pixel gestito dal sistema grafico
//CONTROLLO GRAFICO: finestra dove al suo interno si possono ricevere eventi

type LWContainer() =
    inherit UserControl()

    let controls = ResizeArray<LWC>() //i controlli sono un array ridimensionabile di controlli

    override this.OnPaint e =
        controls |> Seq.iter(fun c -> c.OnPaint e)
        

//type si usa per introdurre un nuovo tipo
//inherit costruttore :per ereditare il costruttore di un oggetto
type Editor() =
    inherit UserControl()

    //creo un array di 4 punti per la curva: 0,0 20,20 50,50 e 100,100. Point() è il punto 0,0 cioè il costruttore vuoto di point
    let pts = [| PointF(); PointF(20.f,20.f); PointF(50.f,50.f); PointF(50.f,100.f) |]
    
    //definisco una funzione che per il clic dell'utente, mi sa dire o meno se sono in uno dei cerchietti
    let handleSize = 5.f //raggio di un cerchietto
    
    let mutable selected = None
    let mutable offsetDrag = PointF()
    
    let mutable tension = 1.f //stabilisco una tensione

    let mutable w2v = new Drawing2D.Matrix() //creo un nuovo oggetto matrice di trasformazione w2v (world to view) assumendo che
    //la matrice appena creata sia quella identica
    let mutable v2w = new Drawing2D.Matrix() //creo un nuovo oggetto matrice di trasformazione v2w (view to world) assumendo che
    //la matrice appena creata sia quella identica
    
    let translateW (tx, ty) = 
        w2v.Translate(tx,ty)
        v2w.Translate(-tx, -ty, Drawing2D.MatrixOrder.Append)

    let rotateW a =
        w2v.Rotate(a)
        v2w.Rotate(-a, Drawing2D.MatrixOrder.Append)

    let rotateAtW p a =
        w2v.RotateAt(a, p)
        v2w.RotateAt(-a, p, Drawing2D.MatrixOrder.Append)
        
    let scaleW (sx, sy) =
        w2v.Scale(sx,sy)
        v2w.Scale(1.f/sx, 1.f/sy, Drawing2D.MatrixOrder.Append)

    let transformP (m:Drawing2D.Matrix) (p:Point) = //trasforma punti in "coppie" del piano
        let a = [| PointF(single p.X, single p.Y) |]
        m.TransformPoints(a)
        a.[0] // restituisce a[0]

    let handleHitTest (p:PointF) (h:PointF) = // funzione che restituisce se un punto è contenuto nel cerchietto o no
        let x = p.X - h.X
        let y = p.Y - h.Y
        x * x + y * y < handleSize * handleSize

    //VOGLIO AGGIUNGERE DEI BOTTONI PER FARE LO SCROLLING  
    // li metto qui dopo la transform perchè le interfacce si disegnano dopo 
    (*let b = new IumButton(Parent = this, Size = SizeF(64.f, 64.f))//definisco un nuovo
    b.OnPaint(e)*)
    let controls = [| new IumButton(Parent=this, Text = "UP"); new IumButton(Parent=this, Location=PointF(32.f,32.f), Text="R") |]

    member this.Tension //Tension è una proprietà che quando la leggo chiama il Get. Se la assegno chiama invece il Set
        //il vantaggio è che non espone i campi della classe e che è modificabile facilmente
        with get () = tension
        and set (v) = tension <- v; this.Invalidate(); //metto qui l'invalidate in modo che ogni volta che la setto non devo
        //ricordarmi di chiamarla

//quando si usa inherit si usa un OVERRIDE cioè si riscrivono dei metodi. In questo caso riscrivo il metodo OnPaint
    override this.OnPaint e =
        let g = e.Graphics //recupero il contesto grafico

        let drawHandle (p:PointF) = //funzione che dato un punto crea un'ellisse 10x10 centrata nel punto
            g.DrawEllipse(Pens.Black, p.X - 5.f, p.Y - 5.f, 10.f, 10.f)

        //salvo il contesto grafico
        let ctx = g.Save()

        g.Transform <- w2v //ogni volta che faccio la paint mi metto nel sistema di coordinate che mi interessa
        g.DrawBezier(Pens.Black, pts.[0], pts.[1], pts.[2], pts.[3]) //disegno una curva di Bezier che vuole una penna e 4 punti di controllo
        g.DrawLine(Pens.Red, pts.[0], pts.[1]) //disegno due linee
        g.DrawLine(Pens.Red, pts.[2], pts.[3])
        g.DrawCurve(Pens.Blue, pts, tension) //disegno una curva
        //disegno le ellissi nei punti ciclando sull'array di punti:
        pts |> Array.iter drawHandle //Array: nome del modulo che contiene le utilità degli array; iter è la funzionalità
        //che mi permette di iterare sugli elementi di un array
(*      in f# è definito quest'operatore
    let (|>) x f = f x
    che permette di scambare funzione ed argomento. Ma allora iter drawHandle non è una funzione?
    In F# tutte le funzioni hanno esattamente un solo argomento: SOLO UNO!! Il linguaggio prevede però che tra i propri tipi ci siano tuple, cioè
    //strutture dati che hanno più campi.
*)
        //Avrei potuto scrivere la stessa cosa così:
        //Array.iter drawHandle p

        g.Restore(ctx)
        base.OnPaint(e)

    //Adesso se voglio muovere la mia curva? come faccio?
    override this.OnMouseDown e =(
        let l = transformP v2w e.Location
        //printfn "%d, %d" e.Location.X e.Location.Y //ogni volta che clicco nella form stampa il valore delle coordinate
        let ht = handleHitTest l //ht è la funzione che dato un punto mi restituisce un boolean
        //let idx = pts |> Array.tryFindIndex ht //se scrivo questo il let mi dà errore, perchè idx è di tipo int option cioè
        //serve anche per rappresentare il caso in cui non ci sia nessun elemento che soddisfa il predicato
        //Quindi devo fare:
        (*let idx = pts |> Array.tryFindIndex ht
        match idx  with
        | Some i ->
        | None _ -> ()*)
        //In realtà semplificando introduco selected:
        selected <- pts |> Array.tryFindIndex ht
        match selected with
            | Some(idx) -> 
                let p = pts.[idx]
                offsetDrag <- PointF(p.X - l.X, p.Y - l.Y)
            | None -> ()
    )
    
    override this.OnMouseUp e =(
        selected <- None
    )
    
    override this.OnMouseMove e =( //allo spostamento del mouse fai..
        let l = transformP v2w e.Location
        controls |> Array.iter(fun b -> b.OnMouseMove e)
        match selected  with
            | Some idx -> 
                pts.[idx] <- l //aggiorna le coordinate del punto
                this.Invalidate()
            | None _ -> ()
    )

    //adesso faccio lo scrolling: come? con la traslazione
    //poi aggiungo q ed e per ruotare con la rotazione
    override this.OnKeyDown e =
        let translate (x,y) = 
            let t = [| PointF(0.f, 0.f); PointF(x,y) |]
            v2w.TransformPoints(t)
            translateW ((t.[1].X - t.[0].X), (t.[1].Y - t.[0].Y))

        match e.KeyCode with
            | Keys.W ->  //cioè quando premi il tasto w sposta la vista verso l'alto --> sposto le coordinate mondo
              translate(0.f, -10.f)
              this.Invalidate()  
            | Keys.A ->  //cioè quando premi il tasto a sposta la vista verso sinistra --> sposto le coordinate mondo
              translate(-10.f,0.f)
              this.Invalidate()  
            | Keys.D ->  //cioè quando premi il tasto d sposta la vista verso destra --> sposto le coordinate mondo
              translate(10.f, 0.f)
              this.Invalidate()  
            | Keys.S ->  //cioè quando premi il tasto s sposta la vista verso il basso --> sposto le coordinate mondo
              translate(0.f,10.f)
              this.Invalidate()
            | Keys.E ->
              let p = transformP v2w (Point(this.Width / 2, this.Height / 2))
              rotateAtW p -10.f
              this.Invalidate()
            | Keys.Q ->
              let p = transformP v2w (Point(this.Width / 2, this.Height / 2))
              rotateAtW p 10.f
              this.Invalidate()
            | Keys.Z ->
              let p = transformP v2w (Point(this.Width / 2, this.Height / 2))
              //prendo il punto p cioè il punto in coordinate mondo che corrisponde al centro della form, effettuo la trasformazione
              scaleW(1.1f, 1.1f)//per lo zoom
              let p1 = transformP v2w (Point(this.Width / 2, this.Height / 2))
              translateW(p1.X - p.X, p1.Y - p.Y)
              this.Invalidate()
            | Keys.X ->
              let p = transformP v2w (Point(this.Width / 2, this.Height / 2))
              //prendo il punto p cioè il punto in coordinate mondo che corrisponde al centro della form, effettuo la trasformazione
              scaleW(1.f/1.1f, 1.f/1.1f)//per lo zoom
              let p1 = transformP v2w (Point(this.Width / 2, this.Height / 2))
              translateW(p1.X - p.X, p1.Y - p.Y)
              this.Invalidate()
            | _ -> () //per tutti gli altri tasti non fare nulla
            

let e = new Editor(Dock=DockStyle.Fill)//definisco e come editor
f.Controls.Add(e)//lo aggiungo alla form
e.Tension <- 1.f //la tensione è un parametro extra che consente di regolare la "curvosità" della curva
e.Focus()

//No nel midterm
//let b = new Button (Text="OK")
//e.Controls.Add(b)

//se allineo i 4 punti sovrapponendoli, succede che la prima volta che ne tolgo uno prendo il primo, poi il secondo,
//il terzo e poi il quarto: è un meccanismo di ordinamento

