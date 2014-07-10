open System

let (>>>) (f1: 'a->'b) (f2: 'b->'c) = 
    Async.RunSynchronously(async { return f1 }) >> fun x -> Async.RunSynchronously( async{ return f2 x})


((fun x -> List.rev x) >>> (fun y -> List.fold(fun acc elem -> acc + elem) "" y ) >>> (fun z -> String.length z )) ["c";"a";"m";"e";"r";"o";"n"] 
