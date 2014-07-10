open System

type 'a FamilyTree = 
    | Child of 'a
    | Node of 'a * FamilyTree<'a> list


module playground = 

    let t = 
        Node("Ransom",[Node("Mum",[Child("Kim");Child("Scott");Child("Jessica");Node("Cameron",[Child("Nicole");Child("Katheryn");Child("Charlotte")])])])

    let rec print_family = function
        | Child(v) -> printf "Child - %s \n" v
        | Node(p,clist) ->
            printf "Parent - %s \n" p 
            List.iter(fun m -> print_family m) clist


    let mapfamily f tree = 
        let rec apply = function
            | Child(v) -> printf "Child - %s \n" (f v); [v]
            | Node(p,clist) ->
                printf "Parent - %s \n" (f p );
                List.collect(fun m -> apply m) clist
        apply tree
        
