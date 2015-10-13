open System.Windows.Forms
open System.Drawing

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
    default this.OnMouseMove _ = ()

    abstract OnMouseUp : MouseEventArgs -> unit
    default this.OnMouseUp _ = ()

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
     
//Introduciamo un controllo grafico che si comporta come la form (che deriva dalla classe controllo che è la radice dei controlli
//grafici)
//FINESTRA: rettangolo di pixel gestito dal sistema grafico
//CONTROLLO GRAFICO: finestra dove al suo interno si possono ricevere eventi

type LWContainer() =
    inherit UserControl()
   
    let controls = ResizeArray<LWC>() //i controlli sono un array ridimensionabile di controlli

    member this.LWControls = controls
   
    override this.OnPaint e =
        controls |> Seq.iter(fun c -> 
            let s = e.Graphics.Save()
            e.Graphics.TranslateTransform(-c.Location.X, -c.Location.Y)
            c.OnPaint e
            e.Graphics.Restore(s)
            )
        base.OnPaint(e)
        


