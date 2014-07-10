#load @"FSharpChart-0.2\FSharpChart.fsx"

open System
open System.Drawing
open Samples.Charting
open Samples.Charting.ChartStyles
open System.Windows.Forms.DataVisualization.Charting


(* Data *)
let data = 
 FSharpChart.Columns [
                       FSharpChart.Column [30.0;35.0;15.0;10.0;8.0]
                       FSharpChart.Doughnut [30.0;35.0;15.0;10.0;8.0]
                       FSharpChart.Doughnut [30.0;35.0;15.0;10.0;8.0]
                     ]




    


(*
type conversion_info = {startAngle: float; endAngle: float; dX: float; dY: float}

let to_double (v: int) = Convert.ToDouble(v)
let to_byte (d: Double) = Convert.ToByte(d)

let rec convert_to_conversioninfo (data: float list) (explode: float) (sum: float) (start: float) (sweep: float) : conversion_info list =
    match data with 
    | [] -> []
    | h :: t -> 
            match sum with 
            | sum when sum < 1.0 ->
                    let newStartAngle = start + sweep
                    let newSweepAngle = 2.0 * Math.PI * h

                    {startAngle = newStartAngle; 
                     endAngle = newStartAngle + newSweepAngle; 
                     dX = explode * Math.Cos(newStartAngle + newSweepAngle / 2.0); 
                     dY = explode * Math.Sin(newStartAngle + newSweepAngle / 2.0);} 
                     ::
                     convert_to_conversioninfo t explode sum newStartAngle newSweepAngle
            | sum when sum > 1.0 -> 
                    let newStartAngle = start + sweep
                    let newSweepAngle = 2.0 * Math.PI * h / sum

                    {startAngle = newStartAngle; 
                     endAngle = newStartAngle + newSweepAngle; 
                     dX = explode * Math.Cos(newStartAngle + newSweepAngle / 2.0); 
                     dY = explode * Math.Sin(newStartAngle + newSweepAngle / 2.0);} 
                     ::
                     convert_to_conversioninfo t explode sum newStartAngle newSweepAngle

            | _ -> raise(ApplicationException("sum case: convert_to_conversioninfo function"))
    

let DrawVisual (c: Canvas) (info: conversion_info) : unit = 
    let p = new Path()
    let random = new Random()
    let r = random.Next(20,200) |> to_double |> to_byte
    let g = random.Next(30,150) |> to_double |> to_byte
    let b = random.Next(10,100) |> to_double |> to_byte
    p.Stroke <- new SolidColorBrush(Colors.Black)
    p.StrokeThickness <- 1.0
    p.Fill <- new SolidColorBrush(Color.FromRgb(r,g,b))
    let pg = new PathGeometry()
    let pf = new PathFigure()
    let ls1 = new LineSegment()
    let ls2 = new LineSegment()
    let arc = new ArcSegment()
    let xc = c.Width / 2.0 + info.dX
    let yc = c.Height / 2.0 + info.dY
    let r = 0.8 * xc

    pf.IsClosed <- true
    pf.StartPoint <- new Point(xc,yc)
    pf.Segments.Add ls1 
    pf.Segments.Add arc
    pf.Segments.Add ls2
    pg.Figures.Add pf
    p.Data <- pg

    ls1.Point <- new Point(xc + r * Math.Cos(info.startAngle), yc + r * (Math.Sin(info.startAngle)))
    arc.SweepDirection <- SweepDirection.Clockwise
    arc.Point <- new Point(xc + r * Math.Cos(info.endAngle), yc + r * (Math.Sin(info.endAngle)))
    arc.Size <- new Size(r, r)
    ls2.Point <- new Point(xc + r * Math.Cos(info.endAngle), yc + r * (Math.Sin(info.endAngle)))
    c.Children.Add(p) |> ignore

let conversion_data =  
   let sum = List.sum data
   convert_to_conversioninfo data 0.0 sum 0.0 0.0

do conversion_data |> List.iter(DrawVisual plotArea) *)


//let shell = new Window(Width=800.0,Height=640.0)




//[<STAThread>] ignore <| (new Application()).Run shell