#I @"C:\Program Files (x86)\Microsoft Research KinectSDK"
#r @"Microsoft.Research.Kinect.dll"

open Microsoft.Research.Kinect

let runtime = Nui.Runtime()
do runtime.Initialize(Nui.RuntimeOptions.UseSkeletalTracking) |> ignore
runtime.NuiCamera.ElevationAngle <- 10
