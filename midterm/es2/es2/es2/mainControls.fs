module mainControls
open System.Drawing
open System.Windows.Forms
open LWC
open Button

(*
    realizzazione:
        ogni animazione è descritta da un "oggetto" che contiene le seguenti informazioni:
        -start: dopo quanti millisecondi dev'essere attivata l'animazione
        -stop: a quale millisecondo deve terminare l'animazione
        -move: funzione d'animazione
        -<attributo da animare>
        
        la funzione move, oltre ad animare l'oggetto, verifica la variabile "mills"
        per avviare/terminare l'animazione una volta raggiunto il valore delle variabili start/stop
        
        l'esempio seguente consiste nell'utilizzare un singolo timer per animare 2 rettangoli
        ms      0           2000        4000        5000      
                |              |           |           |          
                v              v           v           v      
                ----------------------------------------
                b1  ----------------------------------->
                                b2--------------------->

        Dunque è possibile realizzare un motore di animazioni che utilizza un solo timer tramite l'utilizzo
        di un array che andrà a contenere tutti gli oggetti da animare.
        Per ogni oggetto contenuto nell'array andiamo ad invocare la funzione "move(mills)" che si occupa di 
        aggiornare la posizione dell'oggetto se e solo se vengono rispettati i vincoli "start & stop"


    <vantaggi? (supposizione)
        liv. sistema operativo
        la funzione timer rimane in attesa di un interrupt =>
            1 timer => 1interrupt
            n timer => gestire n interrupt
        liv. sw
            se ho delle animazioni che si svolgono con il solito ritmo:
                1 timer
            se ho delle animazioni che si svolgono a ritmi diversi:
                n timer
    >
        
*)

type mainControls() as this = 
    inherit LWContainer()

    let timer = new Timer(Interval=30)
    let mutable currentMills = 0 //variabile per sommare i mills passati
    let mutable stop = 0 //variabile per terminare il timer

    do
        let b1 = new Button(Parent=this, Location=PointF(0.f,0.f), Start=0,Stop=5000)
        let b2 = new Button(Parent=this, Location=PointF(0.f,60.f), Color=Brushes.Black, Start = 2000, Stop = 5000)
        this.LWControls.Add(b1)
        this.LWControls.Add(b2)
     
        (*
            il timer deve terminare quando l'animazione piu' lunga è stata eseguita
            "piu' lunga" => valore stop maggiore rispetto a tutte le altre animazioni
        *)
        if b1.Stop > b2.Stop then
            stop <- b1.Stop
        else 
            stop <- b2.Stop

        this.Invalidate()

        timer.Tick.Add(fun _ ->
            b1.Move currentMills
            b2.Move currentMills
            this.Invalidate()
            currentMills <- currentMills + 30
            
            if currentMills > stop then
                timer.Stop()
        )
        timer.Start()

    override this.OnPaint e =  
        this.LWControls |> Seq.iter( fun c ->
            let s = e.Graphics.Save()
            e.Graphics.TranslateTransform(c.Location.X, c.Location.Y)
            c.OnPaint e
            e.Graphics.Restore(s)
        )
        base.OnPaint(e)