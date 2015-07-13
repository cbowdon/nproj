namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("NProj")>]
[<assembly: AssemblyProductAttribute("NProj")>]
[<assembly: AssemblyDescriptionAttribute("NProj")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
[<assembly: CLSCompliant()>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"
