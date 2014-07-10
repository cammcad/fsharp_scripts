#I @"C:\Program Files (x86)\Sho 2.0 for .NET 4\bin"
#r "ShoArray.dll"
#r "ShoViz.dll"
#r "MathFunc.dll"
#r "MatrixInterf.dll"
#r "IronPython.dll"


open ShoNS.Array
open ShoNS.MathFunc
open ShoNS.Visualization



let x = ArrayRandom.RandomDoubleArray(10,10) 
in x * x.T

let figure = ShoPlotHelper.Figure() in figure.Bar(x)

let result x = 
    match x with
    | x when x = 1 -> 1 * 100
    | otherwise -> 5 * 10

do printf "%i "(result 1)

