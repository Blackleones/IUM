module MenuButtons
open System.Windows.Forms
open System.Drawing
open LWC

type Menu = 
    | AddNote = 0
    | AddPentagram = 1
    | none = 2
    | scroll = 3
    | delete = 4
    | copyAndPaste = 5

type MenuButton() = 
    inherit LWC()

    let mutable text = ""
    let mutable menu = Menu.none
    let mutable selected = false

    do
        base.Size <- SizeF(50.f, 50.f)

    member this.Text 
       with get() = text
       and set(v) = text <- v

    member this.Menu
        with get() = menu
        and set(v) = menu <- v

    member this.Selected 
        with get() = selected
        and set(v) = selected <- v

    override this.OnPaint e =
        let rect = RectangleF(this.Location.X, this.Location.Y, base.Size.Width, base.Size.Height)

        if selected then
            e.Graphics.FillRectangle(Brushes.Red, rect)
        else
            e.Graphics.FillRectangle(Brushes.Yellow, rect)

        let sSize = e.Graphics.MeasureString(text, base.Parent.Font)
        let stringPoint = PointF(this.Location.X + (this.Size.Width - sSize.Width)/2.f, this.Location.Y + (this.Size.Height - sSize.Height)/2.f)
        e.Graphics.DrawString(text, base.Parent.Font, Brushes.Black, stringPoint)
(*---------------------------------------*)
type MenuButtons() =
    inherit LWC()

    let mutable menu = ResizeArray<MenuButton>()
    let mutable optionSelected = Menu.none

    do
        menu.Add(new MenuButton(Text="N+", Menu=Menu.AddNote, Size=SizeF(40.f, 40.f), Location=PointF(0.f, 0.f)))
        menu.Add(new MenuButton(Text="P+", Menu=Menu.AddPentagram, Size=SizeF(40.f, 40.f), Location=PointF(545.f, 0.f)))
        menu.Add(new MenuButton(Text="V", Menu=Menu.scroll, Size=SizeF(40.f, 40.f), Location=PointF(250.f, 0.f))) //in realtà è un "scrollview"
        menu.Add(new MenuButton(Text="N-", Menu=Menu.delete, Size=SizeF(40.f, 40.f), Location=PointF(40.f, 0.f)))
        menu.Add(new MenuButton(Text="C", Menu=Menu.copyAndPaste, Size=SizeF(40.f, 40.f), Location=PointF(290.f, 0.f)))
       
    member this.Menu 
        with get() = menu
        and set(v) = menu <- v

    member this.OptionSelected 
        with get() = optionSelected
        and set(v) = optionSelected <- v

    member this.SetParent p =
        base.Parent <- p
        menu |> Seq.iter(fun m -> m.Parent <- p)

    override this.OnPaint e =
        menu |> Seq.iter(fun m -> m.OnPaint e)

    override this.OnMouseDown e = 
        let clickedPoint = PointF(float32 e.Location.X, float32 e.Location.Y)

        menu |> Seq.iter(fun m -> 
            if m.HitTest clickedPoint then
                if optionSelected = m.Menu then
                    m.Selected <- false 
                    optionSelected <- Menu.none
                else
                    m.Selected <- true
                    optionSelected <- m.Menu
            else
                m.Selected <- false

            if optionSelected = Menu.AddPentagram then
                m.Selected <- false
        )