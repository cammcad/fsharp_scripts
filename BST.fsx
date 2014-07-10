open System


type BTree = | Node of int ref * BTree * BTree | Leaf of int ref


let values = [3;1;2]


let rec insert n ct bt = 
    match ct with
    | Leaf(v) ->
        printf "Leaf call: %A \n" ct
        v := n
        bt
    | Node(v,lt,rt) ->
        let currentval = !v
        printf "v: %A \n" currentval 
        if currentval = -1 then 
            v := n
            ct
        elif n < v.Value then 
            printf "recursive call on left node: %A \n" lt
            insert n lt bt
        else
            printf "recursive call on right node: %A \n" rt 
            insert n rt bt
    

let initTree =  (Node(ref -1,Node(ref -1,Leaf(ref -1),Leaf(ref -1)),Leaf(ref -1)))

let r = 
    insert 1 initTree initTree

let r2 = insert 2 r r
