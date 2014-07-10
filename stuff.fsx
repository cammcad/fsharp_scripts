#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.0\PresentationFramework.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.0\WindowsBase.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.0\PresentationCore.dll"

open System.Windows
open System.Windows.Media



//example imperative loop in a functional style
let fetch_children_upto num f = 
    Seq.fold(f) [] [1..num] |> List.rev

let rec FindByType<'a> (control: DependencyObject) = 
        fetch_children_upto 
            (VisualTreeHelper.GetChildrenCount(control))
            (fun (accum: DependencyObject list) (elem: int) -> 
                let child = VisualTreeHelper.GetChild(control,elem)
                match child with 
                | c when c.GetType() = typeof<'a> -> child::accum
                | _ -> FindByType<'a>(child))






