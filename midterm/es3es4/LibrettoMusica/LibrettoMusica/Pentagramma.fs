module Pentagramma
open System.Windows.Forms
open System.Drawing
open LWC

type Nota() = 
    inherit LWC()

    do
        base.Size <- SizeF(14.f, 14.f)

    override this.OnPaint e =
        let rect = RectangleF(this.Location.X, this.Location.Y, base.Size.Width, base.Size.Height)
        e.Graphics.FillEllipse(Brushes.Black, rect)
(*---------------------------------------*)
type Retta() = 
    inherit LWC()
    
    let mutable selected = false
  
    do base.Size <- SizeF(2.f, 2.f)

    override this.OnPaint e = 
        let offsetY = 20.f
        e.Graphics.DrawLine(Pens.Black, this.Location, PointF(this.Location.X + 600.f, this.Location.Y))
        e.Graphics.DrawLine(Pens.Black, PointF(this.Location.X, this.Location.Y+offsetY * 1.f), PointF(this.Location.X + 600.f, this.Location.Y+offsetY * 1.f))
        e.Graphics.DrawLine(Pens.Black, PointF(this.Location.X, this.Location.Y+offsetY * 2.f), PointF(this.Location.X + 600.f, this.Location.Y+offsetY * 2.f))
        e.Graphics.DrawLine(Pens.Black, PointF(this.Location.X, this.Location.Y+offsetY * 3.f), PointF(this.Location.X + 600.f, this.Location.Y+offsetY * 3.f))
        e.Graphics.DrawLine(Pens.Black, PointF(this.Location.X, this.Location.Y+offsetY * 4.f), PointF(this.Location.X + 600.f, this.Location.Y+offsetY * 4.f))
(*---------------------------------------*)
type Pentagramma() as this = 
    inherit LWC()

    let mutable note = ResizeArray<Nota>()
    let mutable rette = ResizeArray<Retta>()
    let mutable selected = false
    let mutable background = Brushes.Red

    member this.Note 
        with get() = note
        and set(v) = note <- v

    member this.Selected
        with get() = selected
        and set(v) = selected <- v ;  this.Invalidate()

    member this.Addline p = 
        rette.Add(new Retta(Location=this.Location))
        this.Invalidate()

    member this.AddNote p = 
        note.Add(new Nota(Location=p))

    member this.RemoveNote p =
        let mutable index = -1
        for i in 0 .. +1 .. note.Count - 1 do
            if note.[i].HitTest p then
                index <- i

        if index <> -1 then
            note.RemoveAt(index)

    member this.SetParent p =
        base.Parent <- p
        note |> Seq.iter(fun n -> n.Parent <- p)

    override this.OnPaint e = 
        if selected then
            background <- Brushes.Blue
        else
            background <- Brushes.Red

        let rect = RectangleF(this.Location, this.Size)
        e.Graphics.FillRectangle(background, rect)
        rette |> Seq.iter(fun r -> r.OnPaint e)
        note |> Seq.iter(fun n -> n.OnPaint e)       
(*----------------------------------------*)
type Pentagrammi() as this = 
    inherit LWC()

    let mutable penta = ResizeArray<Pentagramma>()
    let iPPoint = PointF(0.f, 70.f) //posizione iniziale del primo pentagramma
    let Poffset = 150.f//offset fra un pentagramma e l'altro 
    let mutable pentaCPselected = new Pentagramma()
    let mutable w2v = new Drawing2D.Matrix()
    let mutable v2w = new Drawing2D.Matrix()
    let timer = new Timer(Interval=1000)

    member this.W2v
        with get() = w2v
        and set(v) = w2v <- v

    member this.V2w
        with get() = v2w
        and set(v) = v2w <- v

    member this.SetParent p =
        base.Parent <- p
        penta |> Seq.iter(fun pent -> pent.Parent <- p)

    member this.AddPentagram =
        let cPenta = penta.Count
        let p = new Pentagramma(Location=PointF(iPPoint.X, (iPPoint.Y + Poffset * float32 cPenta)), Size=SizeF(600.f, 80.f))
        p.SetParent(this.Parent)
        p.Addline(p)
        penta.Add(p)

    member this.checkHitTestPent (p:PointF) =
        let mutable result = false
        let tp = [|p|]
        v2w.TransformPoints(tp)
        penta |> Seq.iter(fun pent ->
            if pent.HitTest tp.[0] then
                result <- true
        )
        result

    member this.addNoteIfClick (p:PointF) = 
        let tp = [|p|]
        v2w.TransformPoints(tp)
        penta |> Seq.iter(fun pent -> 
            if pent.HitTest tp.[0] then
                
                pent.AddNote tp.[0]
        ) 

    member this.removeNoteIfClick (p:PointF) = 
        let tp = [|p|]
        v2w.TransformPoints(tp)
        penta |> Seq.iter(fun pent ->
            if pent.HitTest tp.[0] then
                pent.RemoveNote tp.[0]
        )
    
    member this.copy (p:PointF) = 
        let mutable note = null
        let tp = [|p|]
        v2w.TransformPoints(tp)
        penta |> Seq.iter(fun pent ->
            if pent.HitTest tp.[0] then
                pent.Selected <- true
                pentaCPselected <- pent
                note <- pent.Note
        )
        
        note
    
    member this.paste (noteC:ResizeArray<Nota>) (p:PointF) =
        pentaCPselected.Selected <- false
        let mutable note = null
        let tp = [|p|]
        v2w.TransformPoints(tp)
        penta |> Seq.iter(fun pent ->
            if pent.HitTest tp.[0] then
                timer.Tick.Add(fun _ ->
                noteC |> Seq.iter(fun n ->
                    pent.AddNote(PointF(n.Location.X, n.Location.Y + pent.Location.Y - pentaCPselected.Location.Y))
                    this.Invalidate()
                    )
                )
                timer.Start()
        )


    override this.OnPaint e = 
        let s = e.Graphics.Save()
        e.Graphics.Transform <- w2v
        penta |> Seq.iter(fun pent -> pent.OnPaint e)
        e.Graphics.Restore(s)