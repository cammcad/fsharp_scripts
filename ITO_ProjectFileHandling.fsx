#r @"C:\Project Dragnet\svn\ITO\Libraries\Ionic.Zip.dll"

open System
open System.IO
open Ionic.Zip



//grab the file and the path to the file
let file = @"C:\brian\bdog.ito"
let path = Path.GetDirectoryName(file)
//create working directory in file location
let workingdir = Directory.CreateDirectory(Path.Combine(path, Guid.NewGuid().ToString()))
let destinationdir = Path.Combine(path,workingdir.FullName)
//rename the ito file to zip
let zipfilename = Path.GetFileNameWithoutExtension(file) + ".zip"
let fullzipname = Path.Combine(path,zipfilename)
File.Move(file,Path.Combine(path,zipfilename))
//try and extract the file to workingdir
do     try
            use zip1 = ZipFile.Read(fullzipname)
            for e in zip1 do
                e.Extract(destinationdir, true)

       with _ -> Directory.Delete destinationdir
                 File.Move(Path.Combine(path,zipfilename), file)
