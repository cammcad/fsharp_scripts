(* Haskell Operator Module *)
(* Application opeator *)
let ($) a b = a b
(* List concat operator *)
let (++) a b = List.append a b
(* Raise-to-the-power operators *)
let replicate n x = Seq.take n (seq {})
let (^) (a: int) (b: int) = [1..b] |> List.map(fun _ -> a) |> List.reduce((*))
let (^^) (a: int) (b: int) = [1..b] |> List.map(fun _ -> a) |> List.reduce((*))
(* Force evaluation (strictness flag) *) 
let (!) (a: Lazy<'a>) = a.Force()





let sum z =   ((fun x -> x + 4) >> (fun y -> y + 5)) z
let concat = [1;2;3] ++ [4;5]
let application = (fun x -> x + 6) $ 6
let r = 3 ^^ 5

let x = 10
let r3 = lazy (x + 10)
let final = ! r3