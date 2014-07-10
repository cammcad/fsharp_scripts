#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.0\PresentationFramework.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.0\WindowsBase.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.0\PresentationCore.dll"

open System
open System.Globalization
open System.IO
open System.Windows
open System.Windows.Shapes
open System.Windows.Media
open System.Windows.Controls
open System.Windows.Markup
open System.Xml

(* Add shape to canvas at specific location *)
let addShapeAndLabel_to_coordinate (label: string) (coordinate: float * float) (c:    Canvas) = 
  let btn = Button(Content=label,Foreground=SolidColorBrush(Colors.White))
  let template = 
     "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
        TargetType=\"Button\">" +
        "<Grid>" +
        " <Ellipse Width=\"15\" Height=\"15\" Fill=\"Orange\" HorizontalAlignment=\"Center\"/>" +
        " <ContentPresenter HorizontalAlignment=\"Center\" " + "VerticalAlignment=\"Center\"/> " + 
        "</Grid>" +
        "</ControlTemplate>"

  btn.Template <- XamlReader.Parse(template) :?> ControlTemplate
  c.Children.Add(btn) |> ignore
  let textsize =  
    FormattedText(label,CultureInfo.GetCultureInfo("en-us"),FlowDirection.LeftToRight,Typeface("Verdana"),32.0,Brushes.White)
    |> fun x -> x.MinWidth, x.LineHeight
  let left,top = coordinate
  let middle_point_width = fst(textsize) / 2.0
  let middle_point_height = snd(textsize) / 2.0
  MessageBox.Show(fst(textsize).ToString()) |> ignore
  Canvas.SetLeft(btn,left - middle_point_width)
  Canvas.SetTop(btn,top - middle_point_height)

let shell = new Window(Width=300.0,Height=300.0)
let canvas = new   Canvas(Width=300.0,Height=300.0,Background=SolidColorBrush(Colors.Green))

addShapeAndLabel_to_coordinate "Tree Node 1" (0.0 - 7.0,0.0) canvas
addShapeAndLabel_to_coordinate "TreeNode 2" (150.0 - 7.0, 75.) canvas
shell.Content <- canvas

[<STAThread>] ignore <| (new Application()).Run shell