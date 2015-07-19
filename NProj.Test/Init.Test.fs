namespace NProj.Test

module Parse =
    open System
    open Xunit
    open NProj
    open NProj.Common

    let parseInit = [ [| String.Empty |]]
(*
   Tests performed in REPL
TODO implement proper tests after mono upgrade

let init0 = Init.parse [ "."; "--lang"; "fsharp"; "--type"; "exe" ]
let init1 = Init.parse [ "./NProj.fsproj"; "--lang"; "fsharp"; "--type"; "lib" ]
let init2 = Init.parse [ "./NProj.fsproj" ]
let init3 = Init.parse [ "--type"; "lib" ]
let init4 = Init.parse [ "--type"; "lib"; "." ]
let init5 = Init.parse []
val init0 : Init.InitCommand =
  {ProjectFile = Directory file:///home/vagrant/Working/NProj;
   Lang = FSharp;
   Type = Exe;}

> >
val init1 : Init.InitCommand =
  {ProjectFile = File file:///home/vagrant/Working/NProj/NProj.fsproj;
   Lang = FSharp;
   Type = Library;}

> >
val init2 : Init.InitCommand =
  {ProjectFile = File file:///home/vagrant/Working/NProj/NProj.fsproj;
   Lang = FSharp;
   Type = Library;}

> >
val init3 : Init.InitCommand =
  {ProjectFile = Directory file:///home/vagrant/Working/NProj;
   Lang = FSharp;
   Type = Library;}

> >
val init4 : Init.InitCommand =
  {ProjectFile = Directory file:///home/vagrant/Working/NProj;
   Lang = FSharp;
   Type = Library;}

> >
val init5 : Init.InitCommand =
  {ProjectFile = Directory file:///home/vagrant/Working/NProj;
   Lang = FSharp;
   Type = Library;}
*)

    [<Theory>]
    [<MemberData("parseInit")>]
    let ``Parse init arguments expect success`` (flags: string) = failwith "undefined"
