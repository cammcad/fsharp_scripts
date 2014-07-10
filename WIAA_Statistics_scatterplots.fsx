#load @"C:\Functional Programming Scripts\F# scripts\Open Source Code\FSharpChart-0.2\FSharpChart-0.2\FSharpChart.fsx"
open System
open System.IO
open Samples.Charting

let xvalues = [|14;9;12;7;10;5;13;20;16;10;15;31;18;12;13;6;20;15;31;17;9;15;7|]
//let yvalues = [| 0 .. Array.length data - 1 |]
let points2 = [|2,5;4,2;6,8;8,1;10,10|]

FSharpChart.Point(xvalues)

(* 2012 Team Sizes *)
let data = 
    File.ReadAllLines(@"C:\WIAA Data Analytics\2012_teamSizes.txt")
    |> Seq.map(fun x -> 
                    let ts = x.Split('_')
                    (ts.[1] |> int))

let yvalues = [| 0 .. Seq.length data - 1 |]

Seq.length data - 1

FSharpChart.Point(Array.ofSeq data,yvalues)




let rnd = new Random()
FSharpChart.FastPoint
  ( [ for i in 0 .. 1000 -> rnd.NextDouble() ],
    [ for i in 0 .. 1000 -> rnd.NextDouble() ] )