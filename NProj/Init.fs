namespace NProj

type Init = { ProjectFile: ProjectFile
              Lang: Language
              Type: AssemblyType } with

  static member Parse (args: string seq): Init = failwith "undefined"
