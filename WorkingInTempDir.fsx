#I @"C:\Project Dragnet\7-ZipSharpLib"
#r "7zSharp.dll"

open System
open System.IO
open SevenZSharp


do File.Delete(Path.Combine(Path.GetTempPath(),"doc.dat"))
let f = File.Exists(Path.Combine(Path.GetTempPath(),"doc.dat"))

let unpackToTempDir =
    let s = @"C:\Project Dragnet\data\myhouse\myhouse.zip"
    let tp = Path.GetTempPath() //use local user temp dir
    let ntp = Directory.CreateDirectory(Path.Combine(tp,"ITOOutput"))
    do CompressionEngine.Current.Decoder.DecodeIntoDirectory(s,ntp.FullName)
    
    