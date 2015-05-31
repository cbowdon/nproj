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
let fsprojFile = "./Sample/Sample.fsproj"

[<Literal>]
let assemblyInfoFile = "./Sample/AssemblyInfo.fs"

type OutputType = Exe | Library

type FileType = Source | Reference | Data | Project

type Library = { Include: string; Private: bool option }

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

type Command =
    Init of Uri * OutputType
    | Add of Project * Uri

// Serialization
module Xml =

    let schema = "http://schemas.microsoft.com/developer/msbuild/2003"

    let xname (name: string): XName = XName.Get(name, schema)
    let xnons (name: string): XName = XName.Get name // XName with no namespace, for attributes

    module Marshal =
        let configuration (conf: Configuration): XElement = failwith "TODO"

        let properties (prop: Properties): XElement =
            XElement(xname "PropertyGroup",
                XElement(xname "OutputType", sprintf "%A" prop.OutputType),
                XElement(xname "Name", sprintf "%s" prop.Name),
                XElement(xname "ProjectGuid", sprintf "%A" prop.Guid))

        let import (imp: Import): XElement = failwith "TODO"

        let priv (p: bool option): XElement list =
            match p with
            | Some p -> [ XElement(xname "Private", p) ]
            | None -> []

        let reference (lib: Library): XElement =
            XElement(xname "Reference",
                     XAttribute(xnons "Include", lib.Include),
                     priv lib.Private)

        let inclusion (file: Include): XElement =
            // TODO would prefer relative paths here
            match file with
            | SourceFile uri -> XElement(xname "Compile", XAttribute(xnons "Include", uri.AbsolutePath))
            | DataFile uri -> XElement(xname "Content", XAttribute(xnons "Include", uri.AbsolutePath))

        let projectReference (lib: Library): XElement = failwith "TODO"

        let project (proj: Project): XDocument =
            XDocument(
                XElement(xname "Project",
                    List.concat [
                        [ properties proj.Properties ];
                        List.map reference proj.References;
                        List.map inclusion proj.Includes;
                        // List.map projectReference proj.ProjectReferences, // TODO
                    ]))

    module IO =
        let writeToDisk (project: Project): unit = failwith "TODO"

        let settings: XmlWriterSettings =
            let settings = XmlWriterSettings()
            settings.Encoding <- Encoding.UTF8
            settings.Indent <- true
            settings.IndentChars <- "  "
            settings.OmitXmlDeclaration <- false
            settings.NewLineOnAttributes <- false
            settings.NewLineChars <- Environment.NewLine
            settings

    module Unmarshal =

        let configuration (conf: XElement): Configuration = failwith "TODO"
        let import (imp: XElement): Import = failwith "TODO"

        let reference (lib: XElement): Library =
            let privs = lib.Elements(xname "Private")
            let priv =
                if Seq.isEmpty privs
                then None
                else privs.First().Value |> bool.Parse |> Some
            let lib = lib.Attribute(xnons "Include").Value
            { Include = lib; Private = priv }

        let projectReference (lib: XElement): Project = failwith "TODO"

        let inclusion (file: XElement): Include =
            let inc = file.Attribute(xnons "Include")
            let uri = Uri(inc.Value)
            match file.Name.ToString() with
            | "Compile" -> SourceFile uri
            | "Content" -> DataFile uri
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
            let compiles = itemGroups.Descendants(xname "Compile") |>  Seq.map inclusion
            let nones = itemGroups.Descendants(xname "Content") |>  Seq.map inclusion
            {
                File = file
                Properties = props
                Configurations = [] // TODO
                References = List.ofSeq refs
                Includes = Seq.append compiles nones |> List.ofSeq
                ProjectReferences = [] // TODO
            }

module Util =

    let assemblyInfo (project: Project): string = failwith "TODO"

    let printUsage (): unit =
        File.ReadAllLines("README.org") // temporary solution
        |> Seq.iter Console.WriteLine

    let item (input: string): ItemType option =
        if not(File.Exists(input))
        then None
        else
            match Path.GetExtension(input).ToLowerInvariant() with
            | ".fs" -> Some File
            | ".fsproj" -> Some ProjectReference
            | ".dll" -> Some Reference
            | _ -> Some File

    let relatedProjectFile (uri: Uri): Uri =
        let ext = Path.GetExtension(uri.AbsolutePath)
        match ext.ToLowerInvariant() with
        | ".fsproj" -> uri
        | _ -> let projFilename = uri.Segments.Last() |> sprintf "%s.fsproj"
               Uri(uri, projFilename)

// Parsing user commands
module Parse =
    let location (input: string): Uri option =
        try
            input |> Path.GetFullPath |> (fun u -> Uri(u)) |> Some
        with
            | _ -> None

    let outputType (input: string): OutputType option =
        match input.ToLowerInvariant() with
        | "exe" -> Some Exe
        | "lib" -> Some Library
        | "library" -> Some Library
        | _ -> None

    let project (input: string): Project option =
        match location input with
        | None -> None
        | Some p -> p.AbsolutePath |> XDocument.Load |> Xml.Unmarshal.project p |> Some

    // yo F# this is where a built-in Maybe computation expression would be helpful...
    // I don't want to roll my own or include a lib just for this!
    let bind (opt: 'a option) (f: 'a -> 'b option): 'b option =
        match opt with
        | None -> None
        | Some x -> f x

    module Command =
        let init (uri: string) (output: string): Command option =
            let uriOpt = location uri
            let outputOpt = outputType output
            bind uriOpt (fun uri ->
            bind outputOpt (fun output ->
            Init(uri, output) |> Some))

        let add (uri: string) (proj: string): Command option =
            let uriOpt = location uri
            let projOpt = project proj
            bind uriOpt (fun uri ->
            bind projOpt (fun proj ->
            Add(proj, uri) |> Some))

    let command (args: string seq): Command option =
        match List.ofSeq args with
        // TODO support optional and default arguments
        | ["init"; uri; "--outputType"; outputType ] -> Command.init uri outputType
        // TODO support multiple filenames
        | ["add"; fileUri; "--project"; projUri ] -> Command.add fileUri projUri
        // TODO support aliases
        | ["remove"; fileUri; "--project"; projUri ] -> failwith "TODO"
        | ["move"; sourceUri; targetUri; "--project"; projUri ] -> failwith "TODO"
        | _ -> None


// Main operations
module NProj =
    let init (directory: Uri) (outputType: OutputType): Project =
        {
            File = Util.relatedProjectFile directory
            Properties = { Name = ""; Guid = Guid.NewGuid(); OutputType = outputType }
            Configurations = [] // TODO defaults
            References = [] // TODO defaults
            Includes = [] // TODO defaults
            ProjectReferences = [] // TODO defaults
        }

    let add (project: Project) (file: Uri): Project = failwith "TODO"
    let remove (project: Project) (filename: Uri): Project = failwith "TODO"
    let move (project: Project) (source: Uri) (target: Uri): Project = failwith "TODO"

let main (args: string seq): int =
    match Parse.command args with
    | None -> Util.printUsage(); 1
    | Some command ->
        let result =
            match command with
            | Init (directory, outputType) -> NProj.init directory outputType
            | Add (project, file) -> NProj.add project file
        Xml.IO.writeToDisk result
        0

//fsi.CommandLineArgs |> Seq.skip 1 |> main

let proj = Parse.project fsprojFile
let xml =
    match proj with
    | Some p -> Xml.Marshal.project p
    | None -> failwith "None!"
