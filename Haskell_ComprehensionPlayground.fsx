#I @"C:\Functional Programming Scripts\F# scripts\Open Source Code\FSharpPowerPack-1.9.9.9\FSharpPowerPack-1.9.9.9\bin"
#r @"FSharp.PowerPack.Linq"
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Linq
open Microsoft.FSharp.Linq.QuotationEvaluation

let rec unite lst = 
   match lst with
   | [] -> []
   | x :: xs -> x @ unite xs
      

let (|||) v b =
    let value = Expr.Value(v)
    let r = QuotationEvaluator.EvaluateUntyped(value) :?> ('a -> 'b)
    List.map r b
    
    

let r: int list = (fun x -> x * x) |||  [1..5] |> List.filter(fun y -> not(y % 2 = 0)) 



