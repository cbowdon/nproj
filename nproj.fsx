#!/usr/bin/fsharpi --exec
// Mono's implementation is incomplete, so built from Microsoft's open source.
// Build steps:
//   git clone https://github.com/Microsoft/msbuild.git
//   git checkout xplat
//   ./build.pl
// Must reference all the custom assemblies or will fall back to GAC version.
// TODO make msbuild a submodule
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

module Project =

    open System.IO
    open Microsoft.Build.Evaluation
    open Microsoft.Build.Framework

    let uri (path: string): Uri = Uri(path)

    let findProjectsInDir (lang: Language) (path: string): string list =
        let pattern = match lang with
        | FSharp -> "*.fsproj"
        | CSharp -> "*.csproj"
        Directory.EnumerateFiles(path, pattern) |> List.ofSeq

    let projectFile (lang: Language) (path: string): string =
        let name = Path.GetFileNameWithoutExtension path
        let extension = match lang with
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
        let proj = Project()
        proj.SetProperty("OutputType", sprintf "%A" args.Type) |> ignore
        proj.SetProperty("Language", sprintf "%A" args.Lang) |> ignore
        proj.Save(projFile)

    let add (args: Add): unit = failwith "undefined"
    let remove (args: Remove): unit = failwith "undefined"
    let move (args: Move): unit = failwith "undefined"

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
    | "init"::rest -> rest |> Parsers.init |> Project.init
    | "add"::rest -> rest |> Parsers.add |> Project.add
    | "remove"::rest -> rest |> Parsers.remove |> Project.remove
    | "rm"::rest -> rest |> Parsers.remove |> Project.remove
    | "move"::rest -> rest |> Parsers.move |> Project.move
    | "mv"::rest -> rest |> Parsers.move |> Project.move
    | _ -> help()

// TEST!
//let args = Parsers.init [ "."; "--lang"; "fsharp"; "--type"; "exe" ]
//Project.init args
main [| "nproj"; "init";|]
