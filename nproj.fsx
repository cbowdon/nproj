#!/usr/bin/fsharpi --exec
#r "System.Xml.Linq"

open System
open System.IO
open System.Linq
open System.Text
open System.Text.RegularExpressions
open System.Xml
open System.Xml.Linq

[<Literal>]
let schema = "http://schemas.microsoft.com/developer/msbuild/2003"

[<Literal>]
let fsprojFile = "NProj/NProj.fsproj"

[<Literal>]
let assemblyInfoFile = "NProj/AssemblyInfo.fs"

type OutputType = Exe | Library

type FileType = Source | Reference | Data

let sampleProj(): XDocument = XDocument.Load fsprojFile
let sampleAssemblyInfo(): string = File.ReadAllText assemblyInfoFile

let xname (path: string): XName = XName.Get(path, schema)

let createAssemblyInfo (name: string): string =
    let ai = sampleAssemblyInfo()
    Regex.Replace(ai, "NProjPlaceholder", name)

let createProj (output: OutputType) (name: string): XDocument =

    let setOutputType (out: string) (xdoc: XDocument): unit =
        seq {
            for proj in xname "Project" |> xdoc.Elements do
                for prop in xname "PropertyGroup" |> proj.Elements do
                    yield! xname "OutputType" |> prop.Elements
        } |> Seq.iter (fun o -> o.SetValue out)

    let out = sprintf "%A" output

    let proj = sampleProj()
    setOutputType out proj
    proj

let createSource (name: string): string =
    sprintf "namespace %s%s" name Environment.NewLine

let filename (parts: string seq): string =
      String.Join(Path.DirectorySeparatorChar.ToString(), parts)

let getProjName (dir:string): string =
      dir |> Path.GetFullPath |> Path.GetFileName

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

let init (dir: string) (output: OutputType): unit =
    let name = getProjName dir
    let projFile = filename [ dir; sprintf "%s.fsproj" name ]
    createProj output name |> writeProj projFile

    let aiFile = filename [ dir; "AssemblyInfo.fs" ]
    let ai = createAssemblyInfo name
    File.WriteAllText(aiFile, ai)

let add (name: string) (dir: string): unit =

    let itemGroups (proj: XDocument): XElement seq =
        seq {
            for p in xname "Project" |> proj.Elements do
                yield! xname "ItemGroup" |> p.Elements
        }

    // 2 item groups defined in template
    // one with only references
    // one with no refs, only compiles/includes

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

    let projFile = getProjName dir |> sprintf "%s.fsproj"
    let proj = projFile |> XDocument.Load

    let fileType =
        match name |> Path.GetExtension with
        | ".dll"    -> Reference
        | ".fsproj" -> Reference
        | ".fs"     -> Source
        | _        -> Data

    let addItem (tag: string): unit =
        let targetItemGroup =
            match fileType with
            | Reference -> proj |> itemGroups |> refGroups |> Seq.head
            | _         -> proj |> itemGroups |> otherGroups |> Seq.head

        let xel = XElement(xname tag)
        // No namespace on attribute
        xel.SetAttributeValue(XName.Get "Include", name)
        targetItemGroup.Add(xel)

    let addSource (name: string): unit =
        if File.Exists(name)
        then ()
        else File.WriteAllLines(name, [ createSource name ])

    match fileType with
    | Reference -> addItem "Reference"
    | Source -> addItem "Compile"; addSource name
    | Data -> addItem "None"

    writeProj projFile proj

let printUsage (): unit =
    File.ReadAllLines("README.org") // temporary solution
    |> Seq.iter Console.WriteLine

let outputType (ot: string) =
    match ot.ToLowerInvariant() with
    | "exe" -> Exe
    | "lib" -> Library
    | "library" -> Library
    | _ -> failwith "Output type not recognised - should be lib or exe"

let parseInit (args: string list): unit =
    match args with
    | [] -> init "." Library
    | [ "--directory"; dir ] -> init dir Library
    | [ "--directory"; dir; "--type"; ot ] -> outputType ot |> init dir
    | _ -> printUsage ()

let parseAdd (args: string list): unit =
    match args with
    | [ name; "--project"; dir ] -> add name dir
    | [ name ] -> add name "."
    | _ -> printUsage ()

let cliArgs = fsi.CommandLineArgs |> Seq.skip 1 |> List.ofSeq

match cliArgs with
| "init"::rest -> parseInit rest
| "add"::rest -> parseAdd rest
| _ -> printUsage ()
