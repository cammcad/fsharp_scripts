#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.0\PresentationFramework.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.0\WindowsBase.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.0\PresentationCore.dll"

open System
open System.IO
open System.Windows
open System.Windows.Shapes
open System.Windows.Media
open System.Windows.Controls
open System.Windows.Markup
open System.Xml


(* Retrieve the center pixel of a given framework element *)
let find_center_pixel (elem: FrameworkElement): (float * float) = (elem.ActualHeight / 2.0) , (elem.ActualWidth / 2.0)
(* Node label maker *)
let makeLabel (name: string) = 
    let lbl = TextBlock(Text=name,Foreground=SolidColorBrush(Colors.White))
    let tg = TransformGroup() 
    tg.Children.Add(TranslateTransform())
    lbl.RenderTransform <- tg
    lbl
(* Add shape to canvas at specific location *)
let addShapeAndLabel_to_coordinate (label: string) (coordinate: float * float) (c: Canvas) = 
    let btn = Button(Content=label,Foreground=SolidColorBrush(Colors.White))
    let template = 
         "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
            TargetType=\"Button\">" +
            "<Grid>" +
            " <Ellipse Fill=\"Orange\" HorizontalAlignment=\"Center\"/>" +
            " <ContentPresenter HorizontalAlignment=\"Center\" " + "VerticalAlignment=\"Center\"/> " + 
            "</Grid>" +
            "</ControlTemplate>"

          
        
    //let stringReader = new StringReader(template);
    //let xmlReader = XmlReader.Create(stringReader);
    btn.Template <- XamlReader.Parse(template) :?> ControlTemplate
    c.Children.Add(btn) |> ignore
    let left,top = coordinate
    Canvas.SetLeft(btn,left)
    Canvas.SetTop(btn,top)

let shell = new Window(Width=300.0,Height=300.0)
let canvas = new Canvas(Width=300.0,Height=300.0,Background=SolidColorBrush(Colors.Green))
//let node = new Ellipse(Width=15.0,Height=15.0,Fill=SolidColorBrush(Colors.Orange))
//let node_label = makeLabel "Tree Node 1" |> fun x -> x.Visibility <- Visibility.Collapsed x
//node.MouseLeftButtonDown.Add( fun _ -> 
//                                    find_center_pixel node |> fun centerpixel -> 
//                                                                  let left,t = centerpixel
//                                                                  (* add label for shape *)
//                                                                  let lblcenterX = node_label
//                                                                  MessageBox.Show(lblcenterX.ToString()) |> ignore
//                                                                  //lbl.Margin <- new Thickness(Left= -lblcenterX,Top=0.0,Right=0.0,Bottom=0.0)
//                                                                  //addShape_to_coordinate (100.0 + left, 50.0 + t) lbl canvas
//                                                                  )
                                                                  
addShapeAndLabel_to_coordinate "Tree Node 1" (100.0,50.0) canvas
//canvas.Children.Add(node_label)
shell.Content <- canvas





[<STAThread>] ignore <| (new Application()).Run shell