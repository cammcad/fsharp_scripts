
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.0\PresentationFramework.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.0\WindowsBase.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.0\PresentationCore.dll"

open System
open System.Windows
open System.Windows.Shapes
open System.Windows.Media
open System.Windows.Controls

let r = new Random()
let total_count_of_all_objects = 4556

(* Helper functions - remembe the (: byte) & (: float)
   is just annotation for the return type of the function *)
let to_byte (v: int) : byte = Convert.ToByte(v)
let to_float (v: int) : float = Convert.ToDouble(v)
let to_color (r: int) (g: int) (b: int) : Color = 
    Color.FromRgb(r |> to_byte, g |> to_byte, b |> to_byte)

(* Tuple to hold the total number of objects used in the 3D Model 
   (random) and total number of objects in the 3D Model *)
let used_total = (r.Next(2000,4556),total_count_of_all_objects)

(* Calculate the number of objects used relative to the total *)
let simulation_result (used_total: int * int): float = 
    let used,total = used_total
    used * 100 / total |> to_float



let construct_visualization (used_total: int * int): Grid = 
    (* Get simulation result and convert to a value between 0 and 1
       so that we can represent a value that the gradient stops
       understand *)
    let v = simulation_result used_total / 100.0 
    let actual_color = to_color 3 129 51 
    let total_color = to_color 255 5 5 
    let start_point = new Point(0.0, -0.925) //gradient start point
    let end_point = new Point(1.0, - 0.893) // gradient end point
    let brush = new LinearGradientBrush(actual_color, total_color, start_point, end_point)
    let gradientStopColor1 = new GradientStop(actual_color, v)
    let gradientStopColor2 = new GradientStop(total_color, v)
    brush.GradientStops.Add gradientStopColor1
    brush.GradientStops.Add gradientStopColor2
    
    let rect = new Rectangle(Fill=brush)
    let grd = new Grid(Width=260.0, Height=30.0,
                       HorizontalAlignment=HorizontalAlignment.Center,
                       VerticalAlignment=VerticalAlignment.Center)

    grd.Children.Add rect |> ignore
    grd
                        


let shell = new Window(Width=300.0,Height=300.0)
let viz = construct_visualization used_total
shell.Content <- viz

[<STAThread>] ignore <| (new Application()).Run shell