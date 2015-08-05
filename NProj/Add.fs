namespace NProj

module Add =
    open System
    open System.Linq
    open Microsoft.Build.Evaluation
    open NProj.Common
    open NProj.IO

    type AddCommand = { SourceFiles: SourceFile seq
                        ProjectFile: ProjectFileLocation }

    let defaultAdd =
        disk { let! pf = projectFileLocation "."
               return { SourceFiles = []; ProjectFile = pf } }

    let parseSourceFiles (raw: Command) (cmd: FreeDisk<AddCommand>) =
        disk { let! cmd' = cmd
               let! sourceFiles =
                   raw.Arguments
                   |> List.map sourceFile
                   |> FreeDisk.sequence
               return { cmd' with SourceFiles = sourceFiles } }

    let parseProjectFile (raw: Command) (cmd: FreeDisk<AddCommand>) =
        disk { let! cmd' = cmd
               match Map.tryFind "--project" raw.Options with
               | None -> return cmd'
               | Some None -> return failwith "The flag \"--project\" requires an argument"
               | Some (Some x) ->
                    let! pfl = projectFileLocation x
                    return { cmd' with ProjectFile = pfl } }

    let parse (args: string seq): FreeDisk<AddCommand> =
        foldParsers [ parseSourceFiles; parseProjectFile ] defaultAdd args

    let projectFileInDir (dir: string): FreeDisk<string> =
        disk { let! files = listFiles dir (Some "*proj")
               match List.ofSeq files with
               | [] -> return failwith "No project file in directory %s" dir
               | [x] -> return x
               | _ -> return failwith "Multiple project files in directory %s" dir }

    let relativePath (project: Project) (path: string): string =
        let pathUri = Uri(path)
        let projUri = Uri(project.FullPath)
        let relUri = projUri.MakeRelativeUri(pathUri)
        relUri.ToString()

    let isProgramFs (item: ProjectItem): bool =
        item.EvaluatedInclude
        |> System.IO.Path.GetFileName
        |> (=) "Program.fs"

    let addSource (project: Project) (source: SourceFile): unit =
        // TODO Create non-existent item from template?
        let items =
            match source with
            | Compile x -> project.AddItem("Compile", relativePath project x)
            | Content x -> project.AddItem("Content", relativePath project x)
            | Reference x -> project.AddItem("Reference", relativePath project x)
            | ProjectReference x -> project.AddItem("ProjectReference", relativePath project x)
            | Import x -> failwith "Adding imports is not yet supported."
        items |> ignore

    let moveProgramFsToEnd (project: Project): unit =
        let programFs = project.GetItems("Compile") |> Seq.tryFind isProgramFs
        match programFs with
        | None -> ()
        | Some p ->
            project.RemoveItem(p) |> ignore
            project.AddItemFast("Compile", p.EvaluatedInclude) |> ignore

    let execute (cmd: AddCommand): FreeDisk<unit> =
        disk { let! projectFile =
                   match cmd.ProjectFile with
                   | Directory path -> projectFileInDir path
                   | File path -> Pure path
               let project = new Project(projectFile)
               cmd.SourceFiles |> Seq.iter (addSource project)
               moveProgramFsToEnd project
               project.Save() }
