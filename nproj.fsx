#!/usr/bin/fsharpi --exec
#r "System.Xml.Linq"

open System
open System.IO
open System.Linq
open System.Text
open System.Xml
open System.Xml.Linq

[<Literal>]
let fsprojFile = "NProj/NProj.fsproj"

[<Literal>]
let schema = "http://schemas.microsoft.com/developer/msbuild/2003"

type OutputType = Exe | Library

type FileType = Source | Reference | Data

let sample(): XDocument = XDocument.Load fsprojFile

let xname (path: string): XName = XName.Get(path, schema)

let createProj (output: OutputType) (name: string): XDocument =

    let setOutputType (out: string) (xdoc: XDocument): unit =
        seq {
            for proj in xname "Project" |> xdoc.Elements do
                for prop in xname "PropertyGroup" |> proj.Elements do
                    yield! xname "OutputType" |> prop.Elements
        } |> Seq.iter (fun o -> o.SetValue out)

    let removeDefaults (xdoc: XDocument): unit =
        seq {
            for proj in xname "Project" |> xdoc.Elements do
                for item in xname "ItemGroup" |> proj.Elements do
                    yield! xname "Compile" |> item.Elements
                    yield! xname "None" |> item.Elements
        } |> Seq.iter (fun r -> r.Remove())

    let out = sprintf "%A" output

    let proj = sample()
    setOutputType out proj
    removeDefaults proj
    proj

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

let add (name: string) (dir: string): unit =

    let itemGroups (proj: XDocument): XElement seq =
        seq {
            for p in xname "Project" |> proj.Elements do
                yield! xname "ItemGroup" |> p.Elements
        }

    let refGroups (itemGroups: XElement seq): XElement seq =
      itemGroups
      |> Seq.filter (fun i -> i.Elements(xname "Reference").Any())

    let otherGroups (itemGroups: XElement seq): XElement seq =
      itemGroups
      |> Seq.filter (fun i -> i.Elements(xname "Reference").Any() |> not)

    let addItemGroup (proj: XDocument): unit =
        xname "Project"
        |> proj.Elements
        |> Seq.iter (fun p -> p.Add(xname "ItemGroup"))

    let projFile = getProjName dir
    let proj = projFile |> XDocument.Load

    let fileType =
        match name |> Path.GetExtension with
        | ".dll"    -> Reference
        | ".fsproj" -> Reference
        | ".fs"     -> Source
        | _        -> Data

    // 2 item groups defined in template
    // one with only references
    // one with no refs, only compiles/includes
    let targetItemGroup =
        match fileType with
        | Reference -> proj |> itemGroups |> refGroups |> Seq.head
        | _         -> proj |> itemGroups |> otherGroups |> Seq.head

    let xel =
        match fileType with
        | Reference -> XElement(xname "Reference")
        | Source -> XElement(xname "Compile")
        | Data -> XElement(xname "None")

    // No namespace on attribute
    xel.SetAttributeValue(XName.Get "Include", name)
    targetItemGroup.Add(xel)

    if File.Exists(name) then () else File.WriteAllLines(name, [])

    writeProj projFile proj

let parseInit (args: string seq): unit = failwith "TODO init"
let parseAdd (args: string seq): unit = failwith "TODO add"
let printUsage (): unit = failwith "TODO help"

let cliArgs =
    fsi.CommandLineArgs
    |> Seq.skip 1
    |> Seq.map (fun a -> a.ToLowerInvariant())
    |> List.ofSeq

printfn "%A" cliArgs
match cliArgs with
| "init"::rest -> parseInit rest
| "add"::rest -> parseAdd rest
| _ -> printUsage ()
