#r @"C:\Program Files (x86)\ComponentOne\Studio for WPF\bin\Design\C1.WPF.C1Chart.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.0\PresentationFramework.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.0\WindowsBase.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.0\PresentationCore.dll"
open System

(* ==== Charting references ==== *)
open System.Windows
open System.Windows.Controls
open C1.WPF.C1Chart

(* create a panel (or blank charting control) *)
let Panel = new C1Chart()
let legend = new C1ChartLegend()
Panel.Children.Add legend
(* Panel Width *)
let width (w: float) (p: C1Chart) = 
    p.Width <- w 
    p.View.AxisX.Title <- "Time"
    p.View.AxisY.Title <- "Count"
    p

(* Panel Height *)
let height (h: float) (p: C1Chart)  = 
    p.Height <- h 
    p



(* Chart Type *)
let add  (ct: ChartType) (p: C1Chart) = 
    p.ChartType <- ct
    p

(* Data *)
let data  (d: float list) label (p: C1Chart) =
    let ds = new DataSeries()
    ds.Label <- label
    ds.Values <- new Media.DoubleCollection(Seq.ofList d)
    p.Data.Children.Add ds
    p.Data.ItemNames <- seq { yield "Revision 1" 
                              yield "Revision 2"
                              yield "Revision 3"
                              yield "Revision 4"
                              yield "Revision 5"
                              yield "Revision 6" }
    p

(* Render *)
let render (c: C1Chart) = 
    let content = c
    let grd = new Grid()
    grd.Children.Add(content) |> ignore
    new Window(Title = "C1 Chart Playground Via F#", Content = grd)
    
let vis = 
    Panel
        |> width 640.0
        |> height 480.0
        |> add ChartType.ColumnStacked
        |> data [1.0; 1.2; 1.7; 1.5; 0.7; 0.3] "Precast"
        |> data [1.5; 1.8; 1.7; 1.6; 0.9; 0.2] "Precast 2"
        |> data [2.0; 1.5; 1.8; 1.9; 0.5; 0.1] "Precast 3"
        |> data [9.2; 10.5; 1.8; 1.9; 25.0; 30.0] "Some Other Wall"
        |> render

[<STAThread>] ignore <| (new Application()).Run vis