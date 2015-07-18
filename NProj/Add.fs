namespace NProj

module Add =
    open Common

    type AddCommand = { SourceFiles: SourceFile seq
                        ProjectFile: ProjectFile }

    let defaultAdd = { SourceFiles = []
                       ProjectFile = projectFile "." }

    let parse (args: string seq): AddCommand = failwith "undefined"

    let execute (cmd: AddCommand): unit = failwith "undefined"
