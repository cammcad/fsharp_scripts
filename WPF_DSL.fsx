#I @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\WPF"

#r "PresentationFramework.dll"
#r "WindowsBase.dll"
#r "PresentationCore.dll"
#r "System.Xaml.dll"

open System
open System.Windows
open System.Windows.Markup
open System.Windows.Shapes
open System.Windows.Media
open System.Windows.Controls
open System.IO




let ui = @"<Grid
  xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
  xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  
    <StackPanel Orientation=""Horizontal"">
      <Button x:Name=""green"" Width=""30"" Height=""30"" Background=""Green""></Button>
      <Button x:Name=""orange"" Width=""30"" Height=""30"" Background=""Orange""></Button>
      <Button x:Name=""red"" Width=""30"" Height=""30"" Background=""Red""></Button>
    </StackPanel>
    
    <!--<Button Width=""30"" Height=""30"" Margin=""10,5""></Button>-->
  </Grid>" 

//let sreader = new StringReader(ui)
let toplevel = XamlReader.Parse(ui) :?> Grid
toplevel.Children
                      
let shell = new Window(Width=300.0,Height=300.0)
ignore <| (new Application()).Run shell




let rec numstrings() = 
    seq {
            yield! ["one",500; "two",1000; "three",250]
            yield! numstrings()
        }



let runner() = 
    async {
                for numstr,interval in numstrings() do
                    printf "%s \n" numstr
                    do! Async.Sleep(interval)     
          }        


Async.Start(runner(), Async.DefaultCancellationToken)
Async.CancelDefaultToken()