#r "System.Xml.Linq"
#I "packages/FSharp.Data.2.2.0/lib/net40/"
#r "FSharp.Data.dll"

open System
open System.IO
open System.Xml.Linq
open FSharp.Data

// Use an example to quickly generate schema
[<Literal>]
let fsprojFile = "NProj/NProj.fsproj"

type FsProj = XmlProvider<fsprojFile>
type OutputType = Exe | Library
type FileType = Source | ProjectRef | Dll | Data

// IO helpers
let fileType (filename:string): FileType = failwith "TODO"
let getProjName (dir:string): string = dir |> Path.GetFullPath |> Path.GetFileName
let readProj (target:string): FsProj.Project = FsProj.Parse target
let writeProj (target:string) (proj:FsProj.Project): unit =
    proj.XElement.ToString()
    |>  (fun p -> File.WriteAllText(target, p))

// FsProj manipulation
let createProj (outputType:OutputType) (name:string): FsProj.Project =
    let toolsVer = 4.0m
    let defaultTargets = "Build"
    let imports = [|
        FsProj.Import("$(FSharpTargetsPath)","Exists('$(FSharpTargetsPath)')")
        FsProj.Import(@"$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props", @"Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')")
    |]
    let sample = FsProj.GetSample()
    let sampleMainProp =
        sample.PropertyGroups
        |> Seq.filter (fun p -> p.OutputType.IsSome)
        |> Seq.head
    let mainProp = FsProj.PropertyGroup(
        sampleMainProp.Condition,
        sampleMainProp.Configuration,
        sampleMainProp.Platform,
        sampleMainProp.SchemaVersion,
        Guid.NewGuid() |> Some, // project guid
        Some (sprintf "%A" outputType), // output type
        Some name, // root namespace
        Some name, // assembly name
        sampleMainProp.TargetFrameworkVersion,
        sampleMainProp.TargetFSharpCoreVersion,
        Some name,
        sampleMainProp.TargetFrameworkProfile,
        sampleMainProp.DebugSymbols,
        sampleMainProp.DebugType,
        sampleMainProp.Optimize,
        sampleMainProp.Tailcalls,
        sampleMainProp.OutputPath,
        sampleMainProp.DefineConstants,
        sampleMainProp.WarningLevel,
        sampleMainProp.DocumentationFile,
        sampleMainProp.MinimumVisualStudioVersion,
        sampleMainProp.FSharpTargetsPath)
    let sampleOtherProps =
        sample.PropertyGroups
        |> Array.filter (fun p -> p.OutputType.IsNone)
    let properties = Array.concat [ [| mainProp |]; sampleOtherProps ]
    let refs = [|
        FsProj.Reference("mscorlib", None)
        FsProj.Reference("FSharp.Core, Version=$(TargetFSharpCoreVersion), Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", Some false)
        FsProj.Reference("System", None)
        FsProj.Reference("System.Core", None)
        FsProj.Reference("System.Numerics", None)
    |]
    let items = [|
        FsProj.ItemGroup(refs, None, None)
        FsProj.ItemGroup([||], Some (FsProj.Compile "Library.fs"), Some (FsProj.None "Script.fsx"))
    |]
    // TODO this line prints 5...
    properties |> Seq.length |> printfn "%i properties"
    let res = FsProj.Project(toolsVer, defaultTargets, imports, properties, items)
    // TODO - but this prints 1!
    res.PropertyGroups |> Seq.length |> printfn "%i properties"
    // TODO apparently there is a bug around the FsProj.Project constructor
    // Might not be able to do this with XmlProvider,
    // could try mutating the underlying XElement
    res

let addFile (filename:string) (filetype:FileType) (project:FsProj.Project): FsProj.Project = failwith "TODO"

let removeFile (filename:string) (filetype:FileType) (project:FsProj.Project): FsProj.Project = failwith "TODO"

// External API
let init (dir:string) (outputType:OutputType): unit =
    let projName = getProjName dir
    projName
    |> createProj outputType
    |> writeProj (sprintf "%s.fsproj" projName)

let add (filename:string) (projectFile:string): unit =
    projectFile
    |> readProj
    |> addFile filename (fileType filename)
    |> writeProj projectFile

let remove (filename:string) (projectFile:string): unit =
    projectFile
    |> readProj
    |> removeFile filename (fileType filename)
    |> writeProj projectFile

let move (oldFilename:string) (newFilename:string) (projectFile:string): unit = failwith "TODO"

[<EntryPoint>]
let main (args:string[]): int = failwith "TODO"
