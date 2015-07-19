namespace NProj

module Add =
    open Common

    type AddCommand = { SourceFiles: SourceFile seq
                        ProjectFile: ProjectFileLocation }

    let defaultAdd = { SourceFiles = []
                       ProjectFile = projectFileLocation "." }

    let parse (args: string seq): AddCommand = failwith "undefined"

    let execute (cmd: AddCommand): unit = failwith "undefined"
