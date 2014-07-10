type check = { thing : string; mutable signed : bool;} 
let checkstosign = [{thing = "check"; signed = false;};{thing = "check"; signed = false;};{thing = "check"; signed = false;}]


let rec signchecks (checks: check list) (accum: check list) =     
    match accum with 
    | [] -> checks
    | h::t ->  
            h.signed <- true 
            signchecks (h::checks) t  

let signedchecks = signchecks [] checkstosign