#I @"C:\mercurial\opensource\WSDDJA"
#r @"Microsoft.Office.Interop.Excel.dll"


open System.IO
open System
open Microsoft.Office.Interop.Excel
open System.Runtime.InteropServices

// Create new Excel.Application
let app = ApplicationClass(Visible = true) 
//Create the workbook path
let workbookPath = @"C:\mercurial\opensource\WSDDJA\FINAL- Kentlake Competition 2012 Tally Sheet.xls"
// Open the workbook from the workbook path
let workbook = app.Workbooks.Open(workbookPath)
// Get the worksheets collection
let sheets = workbook.Worksheets 
// Grab the worksheet we need to pull data from
let worksheet = (sheets.[box 1] :?> _Worksheet) 
let r1 = (worksheet.Range("A1", "IV1").Value2 :?> obj[,])
seq {
        for value in r1 do
            match value <> null with
            | true -> yield Some(value)
            | false -> yield None
    }
    |> Seq.choose(fun x -> x)