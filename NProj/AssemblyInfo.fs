namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("MinimalFSharpProject")>]
[<assembly: AssemblyProductAttribute("MinimalFSharpProject")>]
[<assembly: AssemblyDescriptionAttribute("MinimalFSharpProject")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
[<assembly: CLSCompliant()>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"
