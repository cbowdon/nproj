namespace NProj

module Add =
    open System
    open System.Linq
    open NProj.Common
    open NProj.IO
    open NProj.Project
    open NProj.Language

    type AddCommand = { SourceFiles: SourceFile seq
                        ProjectFile: Directory }

    let defaultAdd =
        disk { let! pf = directory "."
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
                    let! pfl = directory x
                    return { cmd' with ProjectFile = pfl } }

    let parse (args: string seq): FreeDisk<AddCommand> =
        foldParsers [ parseSourceFiles; parseProjectFile ] defaultAdd args

    let projectFileInDir (dir: string): FreeDisk<string> =
        disk { let! files = listFiles dir (Some "*proj")
               match List.ofSeq files with
               | [] -> return failwith "No project file in directory %s" dir
               | [x] -> return x
               | _ -> return failwith "Multiple project files in directory %s" dir }

    let execute (cmd: AddCommand): FreeDisk<unit> =
        disk { let (Directory dir) = cmd.ProjectFile
               let! projectFiles = listFiles dir (Some "*proj")
               let proj =
                   match List.ofSeq projectFiles with
                   | [x] -> x
                   | [] -> failwith "No project file in directory"
                   | _ -> failwith "Multiple project files in directory. What are you doing?"

               do! add { ProjectFilePath = proj; Items = cmd.SourceFiles; PropertyGroups = [] } }
