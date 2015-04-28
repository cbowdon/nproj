#!/usr/bin/fsharpi --exec
#r "System.Xml.Linq"

open System
open System.IO
open System.Text
open System.Xml
open System.Xml.Linq

[<Literal>]
let fsprojFile = "NProj/NProj.fsproj"

[<Literal>]
let schema = "http://schemas.microsoft.com/developer/msbuild/2003"

type OutputType = Exe | Library

let sample(): XDocument = XDocument.Load fsprojFile

let xname (path: string): XName = XName.Get(path, schema)

let createProj (output: OutputType) (name: string): XDocument =

    let setOutputType (out: string) (xdoc: XDocument): XDocument =
        seq {
            for proj in xname "Project" |> xdoc.Elements do
                for prop in xname "PropertyGroup" |> proj.Elements do
                    yield! xname "OutputType" |> prop.Elements
        } |> Seq.iter (fun o -> o.SetValue out)
        xdoc

    let removeDefaults (xdoc: XDocument): XDocument =
        seq {
            for proj in xname "Project" |> xdoc.Elements do
                for item in xname "ItemGroup" |> proj.Elements do
                    yield! xname "Compile" |> item.Elements
                    yield! xname "None" |> item.Elements
        } |> Seq.iter (fun r -> r.Remove())
        xdoc

    let removeEmpty (xdoc: XDocument): XDocument =
        seq {
            for proj in xname "Project" |> xdoc.Elements do
                for item in xname "ItemGroup" |> proj.Elements do
                    if not item.HasElements then yield item
        } |> Seq.iter (fun r -> r.Remove())
        xdoc

    let out = sprintf "%A" output

    sample()
    |> setOutputType out
    |> removeDefaults
    |> removeEmpty

let getProjName (dir:string): string = dir |> Path.GetFullPath |> Path.GetFileName

let writeProj (target: string) (proj: XDocument): unit =
    let settings = XmlWriterSettings()
    settings.Encoding <- Encoding.UTF8
    settings.Indent <- true
    settings.IndentChars <- "  "
    settings.OmitXmlDeclaration <- false
    settings.NewLineOnAttributes <- false
    settings.NewLineChars <- Environment.NewLine
    use writer = XmlWriter.Create(target, settings)
    proj.Save(writer)

// External API
let init (dir: string) (output: OutputType): unit =
    let name = getProjName dir
    createProj output name
    |> writeProj (sprintf "%s.fsproj" name)

[<EntryPoint>]
let main (args: string[]): int =
    match args |> Array.map (fun a -> a.ToLowerInvariant()) with
    | [| "init"; dir; |] -> init dir Library
    | [| "init"; dir; "--type"; "exe" |] -> init dir Exe
    | [| "init"; dir; "--type"; "library" |] -> init dir Library
    | _ -> failwith "Arguments not recognized"
    0
