namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("NProj.Test")>]
[<assembly: AssemblyProductAttribute("NProj.Test")>]
[<assembly: AssemblyDescriptionAttribute("NProj.Test")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"
