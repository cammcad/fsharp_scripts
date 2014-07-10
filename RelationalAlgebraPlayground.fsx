open System

(* SELECT Operator *)
let (@-) cond  = List.filter(cond)
(* SELECT Combine with AND Operator *)
let (^) a b = (a |> List.filter) >> (b |> List.filter)
(* PROJECT Operator *)
let (!*) c  = List.map(c) 
(* UNION Operator U *)
let (^~) a b = List.concat(seq { yield a; yield b; })

type student = { studentid: Int32; fname: string; gpa: float;}
let students = [{studentid = 1; fname = "cameron"; gpa = 4.0};{studentid = 2; fname = "rachel"; gpa = 3.5}]

let rachel = (List.filter(fun (x: student) -> x.gpa < 4.0) 
             >>  !* (fun (x: student) -> x.fname) ) students  

let results = (fun (x: student) -> x.fname.StartsWith("c")) ^ (fun (x: student) -> x.gpa > 3.0) <| students

let r = ((@-) (fun x -> x.fname.StartsWith("c"))
        >> (!* (fun x -> x.studentid))) students

let all = [1;2;3] ^~ [4;5;6;]


