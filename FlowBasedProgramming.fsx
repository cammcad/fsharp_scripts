(* Flow Based Programming Sample *)

open System

type Msg = | Data of string * Type  

type Component() =
    let trace = printf "v: %A \n"
    

    member x.Bind(result: Msg, cont: Msg->Msg) = 
        (* Some instrumentation and event based logic in here *)
        //trace result
        cont result
        (* Some instrumentation and event based logic in here *)
    member x.Return e = 
                trace e
                e

let program = new Component()


let input = Data("2.0",typedefof<float>)
let parser = fun (d: Msg) -> Data("some-json-here",typedefof<int>)
let producer = fun (d: Msg) -> Data("more-json-here",typedefof<char>)
let producer2 = fun (d: Msg) -> Data("even-more-json-here",typedefof<byte>)

let p = fun prgrm input -> program { 
                                    let! d = input
                                    return prgrm d }
let (|>>) p1 p2 inp = (p p1 inp) |> p2

(parser |>> producer |>> producer2) input
              
        
        
