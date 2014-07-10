open System

(* Host CirculationSystem logging prototype of capture to-host value and from-host value *)
type HostWorkFlow() =
    let trace_host = printf "v: %s %A \n"
    let hostval = ref ""

    member x.Bind(result: string, cont: string->string) = 
          if !hostval <> "" then trace_host "from-host" result
          else 
            hostval := result
            trace_host "to-host" result
          cont result
            
    member x.Return e = e

let host = new HostWorkFlow()

host {
    let! to_host = "cameron"
    let! from_host = "frederick"
    return from_host
}


type ConverterMonad() =    
    member self.Bind ((x : 'a), (converter : 'a -> 'a)) = 
        (*  Trivial convert function to convert the type from string to int *)
        converter x

    member self.Return (x : 'a) = x

let where_filter = ConverterMonad()

let converted_res v f =
        where_filter {
                    let! cv = v
                    return List.filter(fun x -> f x) cv
                } 

(* True Monad with Ma being returned *)
type TrafficLight = | Red of string | Green of string | Yellow of string

type TrafficMonad() =    
    member self.Bind ((x : TrafficLight), (trafficController : TrafficLight -> TrafficLight)) = 
        (* Traffic controller computation to generate or change (abstract mutation) the traffic light*)
        trafficController x

    member self.Return (x : 'a) = x


let traffic = TrafficMonad()

let light_changer light = 
    match light with
    | Red(v) -> Green("Best be on yo way!")
    | Green(v) -> Yellow("Slow yo role!")
    | Yellow(v) -> Red("Hold up fool!")

let changelight light = 
     traffic {
                 let! tlight = light
                 return  light_changer tlight |> light_changer
             }


let trafficController light =  light |> changelight




(* Playing around with Haskells IO Monad -> specifically IO<char> *)
open System

type 'a IO  = Value of 'a


type HaskellIOMonad() =
    
    member self.Bind ((x : IO<char>), (f : char -> IO<char>)) = 
        match x with
        | Value(c) ->  f c
        | _ -> x

    member self.Return (x : 'a) = x

let IO_char = HaskellIOMonad()

let charval v = 
        IO_char {
                   (* The bind member -> executes the function (applies the function defined in the monad against the first argument) and stores the resut in the monad *)
                   let! charv = v
                   (* The return member -> re-wraps the value in it's monadic form (or specific algebraic / union type) and returns it *)
                   return Value('b')
                }
                
(* Thus restoring referential transparency (which is an overall goal of functional programs) *)


            


([1..5] |> List.filter(fun x -> not (x % 2 = 0)))


let (|||) a b = List.map(fun x -> a x) b
let odd x = not <| (x % 2 = 0)
let flter a b = List.filter a b
let result = [  (fun x -> x * x) ||| ([1..5] |> flter(fun x -> odd x))] 
