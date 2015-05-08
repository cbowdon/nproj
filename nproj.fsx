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
let fsprojFile = "Sample/Sample.fsproj"

[<Literal>]
let assemblyInfoFile = "Sample/AssemblyInfo.fs"

type OutputType = Exe | Library

type FileType = Source | Reference | Data | Project

type Library = { Include: Uri; Private: bool option }

type Include = SourceFile of Uri | DataFile of Uri

type ItemType = Reference | File | ProjectReference

type Import = { Project: string; Condition: string }

type Properties = { Guid: Guid; OutputType: OutputType; Name: string }

type Configuration = {
    Optimize: bool
    Tailcalls: bool
    OutputPath: Uri
    DefineConstants: string list
    WarningLevel: int
    DocumentationFile: Uri
    DebugType: bool
    DebugSymbols: bool
    Condition: string
}

type Project = {
    File: Uri
    Properties: Properties
    Configurations: Configuration list
    References: Library list
    Includes: Include list
    ProjectReferences: Project list
}

// Serialization
module Xml =

    let schema = "http://schemas.microsoft.com/developer/msbuild/2003"

    let xname (name: string): XName = XName.Get(name, schema)

    module Write =
        let project (proj: Project): XDocument = failwith "TODO"
        let configuration (conf: Configuration): XElement = failwith "TODO"
        let properties (prop: Properties): XElement = failwith "TODO"
        let import (imp: Import): XElement = failwith "TODO"
        let reference (lib: Library): XElement = failwith "TODO"
        let projectReference (lib: Library): XElement = failwith "TODO"
        let sourceFile (file: Include): XElement = failwith "TODO"
        let toDisk (file: Uri) (xdoc: XDocument): unit = failwith "TODO"

    module Read =

        let configuration (conf: XElement): Configuration = failwith "TODO"
        let import (imp: XElement): Import = failwith "TODO"

        let inclusion (file: XElement): Uri =
            let inc = file.Attribute(xname "Include")
            Uri(inc.Value)

        let reference (lib: XElement): Library =
            let privs = lib.Elements(xname "Private")
            let priv =
                if Seq.isEmpty privs
                then None
                else privs.First().Value |> bool.Parse |> Some
            { Include = inclusion lib; Private = priv }

        let projectReference (lib: XElement): Project = failwith "TODO"

        let sourceFile (file: XElement): Include =
            let inc = file.Attribute(xname "Include")
            let uri = Uri(inc.Value)
            match file.Name.ToString() with
            | "Compile" -> SourceFile uri
            | "None" -> DataFile uri
            | _ -> failwith "Is not a source file or data file"

        let properties (prop: XElement): Properties =
            let name = prop.Element(xname "Name").Value
            let guid = prop.Element(xname "ProjectGuid").Value |> Guid.Parse
            let output =
                match prop.Element(xname "OutputType").Value with
                | "Exe" -> Exe
                | "Library" -> Library
                | _ -> failwith "Invalid output type in proj file"
            { Name = name; Guid = guid; OutputType = output }

        let project (file: Uri) (proj: XDocument): Project =
            let propertyGroups = proj.Descendants(xname "PropertyGroup")
            let isConf (xel: XElement) = xel.Elements(xname "OutputPath").Any()
            let props = propertyGroups.First(fun p -> p |> isConf |> not) |> properties
            let confs = propertyGroups.SkipWhile(fun p -> p |> isConf) |> Seq.map configuration

            let itemGroups = proj.Descendants(xname "ItemGroup")
            let refs = itemGroups.Descendants(xname "Reference") |> Seq.map reference
            let compiles = itemGroups.Descendants(xname "Compile") |>  Seq.map sourceFile
            let nones = itemGroups.Descendants(xname "None") |>  Seq.map sourceFile
            {
                File = file
                Properties = props
                Configurations = [] // TODO
                References = List.ofSeq refs
                Includes = Seq.append compiles nones |> List.ofSeq
                ProjectReferences = [] // TODO
            }

// Parsing user commands
module Read =
    let projectFile (input: string): Uri option =
        let projFileForDir dirPath =
            if Directory.Exists(dirPath)
            then
                let uri = Uri(dirPath)
                let projFilename = uri.Segments.Last() |> sprintf "%s.fsproj"
                Uri(uri, projFilename) |> Some
            else
                None
        match Path.GetExtension(input).ToLowerInvariant() with
        | "fsproj" -> Uri(input) |> Some
        | _ -> try projFileForDir input with | _ -> None

    let outputType (input: string): OutputType option =
        match input.ToLowerInvariant() with
        | "exe" -> Some Exe
        | "lib" -> Some Library
        | "library" -> Some Library
        | _ -> None

    let filename (input: string): ItemType option =
        match Path.GetExtension(input).ToLowerInvariant() with
        | "fs" -> Some File
        | "fsproj" -> Some ProjectReference
        | "dll" -> Some Reference
        | _ -> Some File

    let file (input: string): Uri option = try Uri(input) |> Some with | _ -> None

    let project (input: string): Project option =
        match projectFile input with
        | None -> None
        | Some p -> p.AbsolutePath |> XDocument.Load |> Xml.Read.project p |> Some


// Main operations
module NProj =
    let init (directory: Uri) (outputType: OutputType): Project = failwith "TODO"
    let add (project: Project) (filename: Uri): Project = failwith "TODO"
    let remove (project: Project) (filename: Uri): Project = failwith "TODO"
    let move (project: Project) (source: Uri) (target: Uri): Project = failwith "TODO"

// Utility funcs
let assemblyInfo (project: Project): string = failwith "TODO"

let settings: XmlWriterSettings =
    let settings = XmlWriterSettings()
    settings.Encoding <- Encoding.UTF8
    settings.Indent <- true
    settings.IndentChars <- "  "
    settings.OmitXmlDeclaration <- false
    settings.NewLineOnAttributes <- false
    settings.NewLineChars <- Environment.NewLine
    settings

let printUsage (): unit =
    File.ReadAllLines("README.org") // temporary solution
    |> Seq.iter Console.WriteLine

let main (args: string list): int =
    match args with
    | "init"::rest -> failwith "TODO init"
    | "add"::rest -> failwith "TODO add"
    | "remove"::rest -> failwith "TODO remove"
    | "rm"::rest -> failwith "TODO remove"
    | "move"::rest -> failwith "TODO move"
    | "mv"::rest -> failwith "TODO move"
    | _ -> failwith "TODO no args specified - usage statement"

//fsi.CommandLineArgs |> Seq.skip 1 |> List.ofSeq |> main
