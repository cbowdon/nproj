#!/usr/bin/fsharpi --exec
// Mono's implementation is incomplete, so built from Microsoft's open source.
// Build steps:
//   git clone https://github.com/Microsoft/msbuild.git
//   git checkout xplat
//   ./build.pl
// Must reference all the custom assemblies or will fall back to GAC version.
#r "../msbuild/bin/Unix/Debug-MONO/Microsoft.Build.dll"
#r "../msbuild/bin/Unix/Debug-MONO/Microsoft.Build.Framework.dll"
#r "../msbuild/bin/Unix/Debug-MONO/Microsoft.Build.Tasks.Core.dll"
#r "../msbuild/bin/Unix/Debug-MONO/Microsoft.Build.Utilities.Core.dll"

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

open System

type ProjectPath = Directory | ProjectFile
type Language = CSharp | FSharp
type OutputType = Library | Exe

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

module Parsers =

    let init (args: string seq): Init = failwith "undefined"
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
