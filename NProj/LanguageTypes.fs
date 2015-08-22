namespace NProj

module LanguageTypes =
    type LanguageName =
    | CSharp
    | FSharp

    type Language = { Name: LanguageName
                      ProjectExtension: string
                      SourceExtension: string
                      AssemblyInfoTemplate: string
                      SourceTemplate: string }
