(* Streams in F# inspired by SICP (Structure and Interpretation of Computer Programs) *)
open System

(* Data Abstraction over Primitive Lists (with Lazy | thunk | suspension built into the type *)
type 'a Stream = Empty | Cons of 'a * (unit-> 'a Stream)

(* Now we can define an inifite streams *)
let rec ones = Cons(1,fun _ -> ones)

(* Fetch the head from the Stream *)
let rec hd s : 'a = 
    match s with
    | Empty -> failwith "hd"
    | Cons(h,_) -> h

(* Fetch the tail from the Stream *)
let rec tl (s: 'a Stream) : 'a Stream = 
    match s with
    | Empty -> failwith "tl"
    | Cons(_,t) -> t() //fetch the tail by evaluating the Lazy | thunk | suspension

(* Transform a list into a Stream *)

let rec toStream (lst: 'a list) : 'a Stream =
        (* foldr in F# *) 
        List.foldBack(fun elem acc  -> Cons(elem, fun _ -> acc)) lst Empty

toStream [1;2;3;4;5]

