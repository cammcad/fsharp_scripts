
let a = ["ccc";"aaaz";"d";"bb"]
let b = ["Cameron",31; "Rachel",29; "Nicole",10; "Katheryn",8; "Charlotte",1]


let keyfunc (arg: (string * int)) = arg |> fun (name:string,age:int) -> name.[0..2]
    


let sorted lst key = 
    List.map(fun v -> key v, v) lst
    |> List.sortBy(fun (sv,v) -> sv)
    |> List.map(fun (sv,v) -> v)

sorted a (fun s -> s.Length)
sorted b (fun (name,age) -> age)
sorted b keyfunc
//let a = [| 1..5 |] in printfn "%A" a.[2..4]
let name = "cameron"
let nicknames = b |> List.map(fun (name,age) -> name.[0..2]) 
