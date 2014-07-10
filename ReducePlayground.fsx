(* e.g. list_add [(1,3);(4,2);(3,0)] =[4;6;3] *)
let list_add = List.map(fun (x,y) -> x+y)
let list_add1 = List.fold(fun accum (x,y) ->  x+y::accum) [] >> List.rev
let sum_squares = List.reduce(fun accum elem -> accum + elem) 
