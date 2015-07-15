namespace NProj

type Add = { SourceFiles: SourceFile seq
             ProjectFile: ProjectFile } with

  static member Parse (args: string seq): Add = failwith "undefined"
