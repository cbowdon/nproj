namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("NProjPlaceholder")>]
[<assembly: AssemblyProductAttribute("NProjPlaceholder")>]
[<assembly: AssemblyDescriptionAttribute("NProjPlaceholder")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
[<assembly: CLSCompliant()>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"
