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
let fsprojFile = "Sample/Sample.fsproj"

[<Literal>]
let assemblyInfoFile = "Sample/AssemblyInfo.fs"

type OutputType = Exe | Library

type FileType = Source | Reference | Data | Project

type Library = { Include: string; Private: bool option }

type SourceFile = SourceFile of Uri | DataFile of Uri

type ItemType = Library | SourceFile | Project

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
    Name: string
    File: Uri
    References: Library list
    ProjectReferences: Project list
    Files: SourceFile list
    Properties: Properties
    Configurations: Configuration list
}

// Serialization
module Xml =
    module Write =
        let project (proj: Project): XDocument = failwith "TODO"
        let configuration (conf: Configuration): XElement = failwith "TODO"
        let properties (prop: Properties): XElement = failwith "TODO"
        let import (imp: Import): XElement = failwith "TODO"
        let reference (lib: Library): XElement = failwith "TODO"
        let projectReference (lib: Library): XElement = failwith "TODO"
        let sourceFile (file: SourceFile): XElement = failwith "TODO"
        let toDisk (file: Uri) (xdoc: XDocument): unit = failwith "TODO"
    module Read =
        let project (proj: XDocument): Project = failwith "TODO"
        let configuration (conf: XElement): Configuration = failwith "TODO"
        let properties (prop: XElement): Properties = failwith "TODO"
        let import (imp: XElement): Import = failwith "TODO"
        let reference (lib: XElement): Library = failwith "TODO"
        let projectReference (lib: XElement): Library = failwith "TODO"
        let sourceFile (file: XElement): SourceFile = failwith "TODO"

// Parsing user commands
module Read =
    let directory (input: string): Uri option = failwith "TODO"
    let outputType (input: string): OutputType option = failwith "TODO"
    let project (input: string): Project option = failwith "TODO"
    let filename (input: string): ItemType option = failwith "TODO"
    let file (input: string): Uri option = failwith "TODO"

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

fsi.CommandLineArgs |> Seq.skip 1 |> List.ofSeq |> main
