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
        | _ -> lang |> sprintf "Language not recognised: %s" |> failwith

    let outputType (ot: string): OutputType =
        match ot.ToLowerInvariant() with
        | "library" -> Library
        | "lib" -> Library
        | "executable" -> Exe
        | "exe" -> Exe
        | _ -> ot |> sprintf "Output type not recognised: %s" |> failwith

module Project =
  open Microsoft.Build.Evaluation
  open Microsoft.Build.Framework

  let init (args: Init): int = failwith "undefined"
  let add (args: Add): int = failwith "undefined"
  let remove (args: Remove): int = failwith "undefined"
  let move (args: Move): int = failwith "undefined"

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
        let acc' = {
            Path = Folder "."
            Lang = FSharp
            Type = Library }
        args |> List.ofSeq |> init' acc'

    let add (args: string seq): Add = failwith "undefined"
    let remove (args: string seq): Remove = failwith "undefined"
    let move (args: string seq): Move = failwith "undefined"

let help (): int = failwith "undefined"

let main (args: string[]): int =
    match List.ofSeq args with
    | "init"::rest -> rest |> Parsers.init |> Project.init
    | "add"::rest -> rest |> Parsers.add |> Project.add
    | "remove"::rest -> rest |> Parsers.remove |> Project.remove
    | "rm"::rest -> rest |> Parsers.remove |> Project.remove
    | "move"::rest -> rest |> Parsers.move |> Project.move
    | "mv"::rest -> rest |> Parsers.move |> Project.move
    | _ -> help()
