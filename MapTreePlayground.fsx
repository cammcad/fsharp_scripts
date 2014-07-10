open System

type 'a Tree = Node of 'a * 'a Tree list
let mytree = Node(1 , [Node(2,[]);Node(3,[]);Node(4,[])])
let rec addOne tree = 
    match tree with
    | Node(x, ts) ->
        Node(x + 1, List.map(addOne) ts)

let rec mapTree g tree =
    match tree with
    | Node(x, ts) -> Node(g x, (List.map( mapTree g) ts))

let r = mapTree ((+) 1) mytree  



let f = fun x -> x * x
let g = fun y -> y * y * y

(g >> f) 3




