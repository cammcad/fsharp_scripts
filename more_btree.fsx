open System

type 'a Broot = 
| RBottom of 'a 
| Node of 'a * 'a Broot * 'a Broot

let ss = Node("Cameron",Node("",RBottom("Andy"),RBottom("Barry")),Node("",RBottom("Daniel"),RBottom("Eric")))




let rec find searchSpace friend = 
    match searchSpace with
    | RBottom(n) -> if friend = n then n else "no match"
    | Node(v,l,r) ->
        match v with
        | v when v = "" ->
            let left,right =  (find l friend), (find r friend) in
            if left = "" || left = "no match" then right else left
        | _ ->
            if v = friend then v 
            else 
            if friend < v then find l friend else find r friend