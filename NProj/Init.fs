namespace NProj

module Init =

    open Common

    type InitCommand = { ProjectFile: ProjectFile
                         Lang: Language
                         Type: AssemblyType }

    let defaultInit = { ProjectFile = projectFile "."
                        Lang = FSharp
                        Type = Library }

    let parse (args: string seq): InitCommand = failwith "undefined"

    let execute (cmd: InitCommand): unit = failwith "undefined"
