#r "System.Xml.Linq"
#I "packages/FSharp.Data.2.2.0/lib/net40/"
#r "FSharp.Data.dll"

open System.IO
open System.Xml.Linq
open FSharp.Data

// Use an example to quickly generate schema
[<Literal>]
let fsprojFile = "NProj/NProj.fsproj"

type FsProj = XmlProvider<fsprojFile>
type OutputType = Exe | Library
type FileType = Source | ProjectRef | Dll | Data

// External API
let init (dir:string) (outputType:OutputType): unit =
    let projName = getProjName dir
    projName
    |> createProj outputType
    |> writeProj projName

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

// Internal helpers
let createProject (outputType:OutputType) (name:string): FsProj.Project =
    let toolsVer = 4.0m
    let defaultTargets = "Build"
    let imports = [|
        FsProj.Import("$(FSharpTargetsPath)","Exists('$(FSharpTargetsPath)')")
        FsProj.Import(@"$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props", @"Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')")
    |]
    let sample = FsProj.GetSample()
    let mainProp =
        sample.PropertyGroups
        |> Seq.filter (fun p -> p.OutputType.IsSome)
        |> Seq.head
    let conf = mainProp.Configuration
    let mp = FsProj.PropertyGroup(None, )
    Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
    <SchemaVersion>2.0</SchemaVersion>
    <ProjectGuid>33b41d1c-7d30-491a-ba81-ba62e1629a13</ProjectGuid>
    <!-- OutputType = Library OR Exe (see MSBuild docs) -->
    <OutputType>Library</OutputType>
    <RootNamespace>NProj</RootNamespace>
    <AssemblyName>NProj</AssemblyName>
    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>
    <TargetFSharpCoreVersion>4.3.0.0</TargetFSharpCoreVersion>
    <Name>NProj</Name>
    <TargetFrameworkProfile />

    let confProps =
        sample.PropertyGroups
        |> Array.filter (fun p -> p.OutputType.IsNone)
    let properties = Array.concat [ [| mainProp |]; confProps ]
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
    FsProj.Project(toolsVer, defaultTargets, imports, properties, items)

let addFile (filename:string) (filetype:FileType) (project:FsProj.Project): FsProj.Project = failwith "TODO"

let removeFile (filename:string) (filetype:FileType) (project:FsProj.Project): FsProj.Project = failwith "TODO"

let fileType (filename:string): FileType = failwith "TODO"

let getProjName (dir:string): string = Path.GetDirectoryName dir
let readProj (target:string): FsProj.Project = FsProj.Parse target
let writeProj (target:string) (proj:FsProj.Project): unit = proj |> sprintf "%A" |>  (fun p -> File.WriteAllText(target, p))

[<EntryPoint>]
let main (args:string[]): int = failwith "TODO"
