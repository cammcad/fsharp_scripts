open System

let rec charCount (s: string) = 
    match s with
    | str when str.Length = 0 -> 0
    | _ -> 1 + charCount(s.Substring(1,s.Length - 1))

let reverse (str: string) =
    let rec rev (r: string*string) = 
        match fst(r) with
        | s when s.Length = 0 -> snd(r)
        | _ ->
            let rest = fst(r)
            let state = snd(r)
            let nextchar = rest.Substring(0,1)
            rev(rest.Substring(1,rest.Length - 1),
                    state.Insert(0,nextchar))
    rev (str,"")