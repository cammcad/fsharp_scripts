open System.Collections.Generic
open Microsoft.FSharp.Core.Operators

(* Script Inspired By Clojure Reducers Library *)

(* reduce & fold are the same *)
(* What is a reducing function? 
   It's a function that you'd pass to reduce
   e.g. (accum input) -> input + accum *)

(* Reduction Transformers *)
// Instead of making a new concrete collection, change what 
// reduce means for collection by modifiying the supplied reducing function and return the function

//Simple F# reduce definition (the first element is Identity or the base case) 
List.reduce(fun acc input -> input + acc) [1;2;3]

//Simple transformer definition (modify the reducing function by supplying a mapping function e.g. -> tf)
//tf -> ((*) 3) -> the mapping function
let transformer_map = fun tf -> fun acc input ->  tf input :: acc 
transformer_map ((*) 3) [1;2;3] 

//Simple transformer definition (modify the reducing function by supplying a predicate function e.g. -> pf)
//pf -> ((>) 3) -> the predicate function
let transformer_filter = fun pf -> fun acc input ->  if pf input then input :: acc else acc
transformer_filter ((>) 3) [1;2;3]


(* Lift the essense of transformer_map and transformer_filter into a reducer *)
let reducer = fun f coll -> List.fold(f) [] coll
let rmap = fun f coll -> reducer (transformer_map f) coll
let rfilter = fun f coll -> reducer (transformer_filter f) coll


let mapit = rmap ((+) 3) 
let filterit = rfilter ((<=)3)  


let map_and_filter1 = rmap ((+)3) << rfilter ((>)3)
let map_and_filter2 = rmap ((+)3) >> rfilter ((<)4)


let result combine computations =    
    let asyncComputations = seq { for computation in computations do yield async { return computation }}
    (Async.Parallel >> Async.RunSynchronously >> combine) asyncComputations
    

let computations = 
        seq { yield fun _ -> map_and_filter1 [1;2;3]
              yield fun _ -> map_and_filter2 [1;2;3] }



result (fun ar -> List.ofArray ar |> List.collect(fun l -> l) |> List.fold((+)) 0) computations




