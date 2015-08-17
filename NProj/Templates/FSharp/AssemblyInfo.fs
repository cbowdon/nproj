namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("{0}")>]
[<assembly: AssemblyProductAttribute("{0}")>]
[<assembly: AssemblyDescriptionAttribute("{0}")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
[<assembly: CLSCompliant(true)>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"
