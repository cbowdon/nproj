namespace NProj

module Add =
    open System
    open System.IO
    open System.Linq
    open Microsoft.Build.Evaluation
    open Common

    type AddCommand = { SourceFiles: SourceFile seq
                        ProjectFile: ProjectFileLocation }

    let defaultAdd = { SourceFiles = []
                       ProjectFile = projectFileLocation "." }

    let parseSourceFiles (raw: Command) (cmd: AddCommand) =
        { cmd with SourceFiles = raw.Arguments |> List.map sourceFile }

    let parseProjectFile (raw: Command) (cmd: AddCommand) =
        match Map.tryFind "--project" raw.Options with
        | None -> cmd
        | Some None -> failwith "The flag \"--project\" requires an argument"
        | Some (Some x) -> { cmd with ProjectFile = projectFileLocation x }

    let parse (args: string seq): AddCommand =
        let raw = collectArgs args
        [ parseSourceFiles; parseProjectFile ]
        |> Seq.fold (fun acc p -> p raw acc) defaultAdd

    let projectFileInDir (dir: string): string =
        match Directory.EnumerateFiles(dir,"*proj") |> List.ofSeq with
        | [] -> failwith "No project file in directory %s" dir
        | [x] -> x
        | _ -> failwith "Multiple project files in directory %s" dir

    let relativePath (project: Project) (path: Uri): string =
        let projUri = uri project.FullPath
        let relUri = projUri.MakeRelativeUri(path)
        relUri.ToString()

    let addSource (project: Project) (source: SourceFile): unit =
        // Todo create no existent item
        let items =
            match source with
            | Compile x -> project.AddItem("Compile", relativePath project x)
            | Content x -> project.AddItem("Content", relativePath project x)
            | Reference x -> project.AddItem("Reference", relativePath project x)
            | ProjectReference x -> failwith "Adding project references is not yet supported"
            | Import x -> failwith "Adding imports is not yet supported."
        // TODO enforce putting Program.fs at end of items
        items |> ignore

    let execute (cmd: AddCommand): unit =
        let projectFile =
            match cmd.ProjectFile with
            | Directory path -> projectFileInDir path.AbsolutePath
            | File path -> path.AbsolutePath
        let project = new Project(projectFile)
        cmd.SourceFiles |> Seq.iter (addSource project)
        project.Save()



