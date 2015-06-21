#!/usr/bin/fsharpi --exec
// Mono's implementation is incomplete, so built from Microsoft's open source.
// Build steps:
//   git clone https://github.com/Microsoft/msbuild.git
//   git checkout xplat
//   ./build.pl
// Must reference all the custom assemblies or will fall back to GAC version.
// TODO make msbuild a submodule
// TODO doesn't fucking work... can't load a project (;_;)
#r "../msbuild/bin/Unix/Debug-MONO/Microsoft.Build.dll"
#r "../msbuild/bin/Unix/Debug-MONO/Microsoft.Build.Framework.dll"
#r "../msbuild/bin/Unix/Debug-MONO/Microsoft.Build.Tasks.Core.dll"
#r "../msbuild/bin/Unix/Debug-MONO/Microsoft.Build.Utilities.Core.dll"

open System

type ProjectPath =
    | Folder of string
    | ProjectFile of string

type Language =
    | FSharp
    | CSharp

type OutputType =
    | Library
    | Exe

type Init = {
    Path: ProjectPath
    Lang: Language
    Type: OutputType }

type Add = {
    Paths: Uri seq
    Project: ProjectPath }

type Remove = {
    Paths: Uri seq
    Project: ProjectPath }

type Move = {
    From: Uri
    To: Uri
    Project: ProjectPath }

module Read =
    open System.IO

    let projectPath (path: string): ProjectPath =
        let path'= Path.GetFullPath(path)
        if Directory.Exists(path')
        then Folder path'
        else ProjectFile path' // TODO validate that projectFile is a *proj

    let language (lang: string): Language =
        match lang.ToLowerInvariant() with
        | "fsharp" -> FSharp
        | "f#" -> FSharp
        | "csharp" -> CSharp
        | "c#" -> CSharp
        | _ -> failwith "Language not recognised: %s" lang

    let outputType (ot: string): OutputType =
        match ot.ToLowerInvariant() with
        | "library" -> Library
        | "lib" -> Library
        | "executable" -> Exe
        | "exe" -> Exe
        | _ -> failwith "Output type not recognised: %s" ot

module NProj =

    open System.IO
    open Microsoft.Build.Evaluation
    open Microsoft.Build.Framework

    let uri (path: string): Uri = Uri(path)

    let findProjectsInDir (lang: Language) (path: string): string list =
        let pattern =
            match lang with
            | FSharp -> "*.fsproj"
            | CSharp -> "*.csproj"
        Directory.EnumerateFiles(path, pattern) |> List.ofSeq

    let projectFile (lang: Language) (path: string): string =
        let name = path |> Path.GetFullPath |> Path.GetFileNameWithoutExtension
        let extension =
            match lang with
            | FSharp -> ".fsproj"
            | CSharp -> ".csproj"
        Path.Combine(path, sprintf "%s%s" name extension)

    let init (args: Init): unit =
        let projFile =
            match args.Path with
            | ProjectFile path -> path
            | Folder path ->
                match findProjectsInDir args.Lang path with
                | [] -> projectFile args.Lang path
                | x::_ -> x
        let name = Path.GetFileNameWithoutExtension(projFile)
        let proj = Project()
        proj.Xml.DefaultTargets <- "Build"
        // Imports
        let commonPropImport = proj.Xml.AddImport("$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props")
        commonPropImport.Condition <- "Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')"
        // Properties
        proj.SetProperty("Language", sprintf "%A" args.Lang) |> ignore
        proj.SetProperty("OutputType", sprintf "%A" args.Type) |> ignore
        proj.SetProperty("Name", name) |> ignore
        proj.SetProperty("RootNamespace", name) |> ignore
        proj.SetProperty("AssemblyName", name) |> ignore
        proj.SetProperty("TargetFrameworkVersion", "v4.0") |> ignore
        proj.SetProperty("TargetFSharpCoreVersion", "4.3.0.0") |> ignore
        proj.SetProperty("SchemaVersion", "2.0") |> ignore
        proj.SetProperty("ProjectGuid", Guid.NewGuid() |> sprintf "%A") |> ignore
        // Default references
        proj.AddItem("Reference", "mscorlib") |> ignore
        proj.AddItem("Reference", "System") |> ignore
        proj.AddItem("Reference", "System.Core") |> ignore
        proj.AddItem("Reference", "System.Numerics") |> ignore
        proj.AddItem("Reference", "FSharp.Core, Version=$(TargetFSharpCoreVersion), Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a") |> ignore
        // FSharp targets
        let pg = proj.Xml.AddPropertyGroup()
        pg.Condition <- @"Exists('$(MSBuildExtensionsPath32)\..\Microsoft SDKs\F#\3.0\Framework\v4.0\Microsoft.FSharp.Targets')"
        pg.AddProperty("FSharpTargetsPath", @"$(MSBuildExtensionsPath32)\..\Microsoft SDKs\F#\3.0\Framework\v4.0\Microsoft.FSharp.Targets") |> ignore
        let fsTargetsImport = proj.Xml.AddImport("$(FSharpTargetsPath)")
        fsTargetsImport.Condition <- "Exists('$(FSharpTargetsPath)')"
        // Write
        proj.Save(projFile)

    let add (args: Add): unit = failwith "undefined"
    let remove (args: Remove): unit = failwith "undefined"
    let move (args: Move): unit = failwith "undefined"

    let load (path: string): Project = Project(path)

  (*
  // Testy test
  let sample = "Sample/Sample.fsproj"

  let proj = Project(sample)

  proj.AddItem("Compile", "Test.fs")

  proj.Save()
    *)

module Parsers =

    let init (args: string seq): Init =
        let rec init' acc args =
            match args with
            | "--lang"::lang::rest -> init' { acc with Lang = Read.language lang } rest
            | "--type"::ot::rest -> init' { acc with Type = Read.outputType ot } rest
            | dir::rest -> init' { acc with Path = Read.projectPath dir } rest
            | [] -> acc
        let acc' = {
            Path = Folder "."
            Lang = FSharp
            Type = Library }
        args |> List.ofSeq |> init' acc'

    let add (args: string seq): Add = failwith "undefined"
    let remove (args: string seq): Remove = failwith "undefined"
    let move (args: string seq): Move = failwith "undefined"

let help (): unit = failwith "undefined"

let main (args: string[]): unit =
    match args |> Seq.skip 1 |> List.ofSeq with
    | "init"::rest -> rest |> Parsers.init |> NProj.init
    | "add"::rest -> rest |> Parsers.add |> NProj.add
    | "remove"::rest -> rest |> Parsers.remove |> NProj.remove
    | "rm"::rest -> rest |> Parsers.remove |> NProj.remove
    | "move"::rest -> rest |> Parsers.move |> NProj.move
    | "mv"::rest -> rest |> Parsers.move |> NProj.move
    | _ -> help()

// TEST!
//let args = Parsers.init [ "."; "--lang"; "fsharp"; "--type"; "exe" ]
//NProj.init args
main [| "nproj"; "init";|]
// xbuild whatever_was_just_produced.fsproj
