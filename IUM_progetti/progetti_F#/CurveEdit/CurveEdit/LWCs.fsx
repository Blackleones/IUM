open System.Windows.Forms
open System.Drawing

// Lightweight controls: astrazione programmativa che imita i controlli grafici
type LWC() =
  let mutable parent : Control = null
  let mutable location = PointF()
  let mutable size = SizeF()
  
  abstract OnMouseDown : MouseEventArgs -> unit
  default this.OnMouseDown _ = ()

  abstract OnMouseMove : MouseEventArgs -> unit
  default this.OnMouseMove _ = ()

  abstract OnMouseUp : MouseEventArgs -> unit
  default this.OnMouseUp _ = ()

  abstract OnPaint : PaintEventArgs -> unit
  default this.OnPaint _ = ()

  abstract HitTest : PointF -> bool
  default this.HitTest p =
    (RectangleF(PointF(), size)).Contains(p)

  member this.Invalidate() =
    if parent <> null then parent.Invalidate()

  member this.Location
    with get() = location
    and set(v) = location <- v; this.Invalidate()

  member this.Size
    with get() = size
    and set(v) = size <- v; this.Invalidate()

  member this.Parent
    with get() = parent
    and set(v) = parent <- v

type LWContainer() =
  inherit UserControl()

  let controls = ResizeArray<LWC>()

  let cloneMouseEvent (c:LWC) (e:MouseEventArgs) =
    new MouseEventArgs(e.Button, e.Clicks, e.X - int(c.Location.X), e.Y - int(c.Location.Y), e.Delta)

  let correlate (e:MouseEventArgs) (f:LWC->MouseEventArgs->unit) =
    let mutable found = false
    for i in { (controls.Count - 1) .. -1 .. 0 } do
      if not found then
        let c = controls.[i]
        if c.HitTest(PointF(single(e.X) - c.Location.X, single(e.Y) - c.Location.Y)) then
          found <- true
          f c (cloneMouseEvent c e)

  let mutable captured : LWC option = None

  member this.LWControls = controls

  override this.OnMouseDown e =
    printfn "DA LWCONTAINER-DOWN"
    correlate e (fun c ev -> captured <- Some(c); c.OnMouseDown(ev))
    printfn "DA LWCONTAINER-DOWN - PRIMA SUPER"
    base.OnMouseDown e
    printfn "DA LWCONTAINER-DOWN - DOPO SUPER"

  override this.OnMouseUp e =
    printfn "DA LWCONTAINER-UP"
    correlate e (fun c ev -> c.OnMouseUp(ev))
    match captured with
    | Some c -> c.OnMouseUp(cloneMouseEvent c e); captured <- None
    | None  -> ()
    base.OnMouseUp e

  override this.OnMouseMove e =
    printfn "DA LWCONTAINER-MOVE"
    correlate e (fun c ev -> c.OnMouseMove(ev))
    match captured with
    | Some c -> c.OnMouseMove(cloneMouseEvent c e)
    | None  -> ()
    base.OnMouseMove e

  override this.OnPaint e =
    controls |> Seq.iter (fun c ->
      let s = e.Graphics.Save()
      e.Graphics.TranslateTransform(c.Location.X, c.Location.Y)
      e.Graphics.Clip <- new Region(RectangleF(0.f, 0.f, c.Size.Width, c.Size.Height))
      let r = e.Graphics.ClipBounds
      let evt = new PaintEventArgs(e.Graphics, new Rectangle(int(r.Left), int(r.Top), int(r.Width), int(r.Height)))
      c.OnPaint evt
      e.Graphics.Restore(s)
    )
    base.OnPaint(e)

