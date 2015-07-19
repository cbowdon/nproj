namespace NProj.Test

module Add
    open System
    open Xunit
    open NProj
    open NProj.Common

    let parseAdd = [ [| String.Empty |] ]
 (*
    Tests performed in REPL

let addTest0 = Add.parse [ "one.fs" ]
let addTest1 = Add.parse [ "one.fs"; "two.fs" ]
let addTest2 = Add.parse [ "one.fs"; "two.fs"; "--project"; "." ]
let addTest3 = Add.parse [ "one.fs"; "two.fs"; "--project"; "NProj.fsproj" ]

val addTest0 : Add.AddCommand =
  {SourceFiles = [{Location = file:///home/vagrant/Working/NProj/one.fs;
                   Exists = false;}];
   ProjectFile = Directory file:///home/vagrant/Working/NProj;}
val addTest1 : Add.AddCommand =
  {SourceFiles =
    [{Location = file:///home/vagrant/Working/NProj/two.fs;
      Exists = false;}; {Location = file:///home/vagrant/Working/NProj/one.fs;
                         Exists = false;}];
   ProjectFile = Directory file:///home/vagrant/Working/NProj;}
val addTest2 : Add.AddCommand =
  {SourceFiles =
    [{Location = file:///home/vagrant/Working/NProj/two.fs;
      Exists = false;}; {Location = file:///home/vagrant/Working/NProj/one.fs;
                         Exists = false;}];
   ProjectFile = Directory file:///home/vagrant/Working/NProj;}
val addTest3 : Add.AddCommand =
  {SourceFiles =
    [{Location = file:///home/vagrant/Working/NProj/two.fs;
      Exists = false;}; {Location = file:///home/vagrant/Working/NProj/one.fs;
                         Exists = false;}];
   ProjectFile = File file:///home/vagrant/Working/NProj/NProj.fsproj;}

*)

    [<Theory>]
    [<MemberData("parseAdd")>]
    let ``Parse add arguments expect success`` (args: string seq) (expected: Add.AddCommand) = failwith "undefined"
