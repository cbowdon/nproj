namespace NProj.Test

module Common =
    open System
    open Xunit
    open NProj
    open NProj.Common

    let rawCommandParsing: Object[] seq = Seq.empty

(*
   Examples from REPL testing
let initTest0 = collectArgs [ "." ]
let initTest1 = collectArgs [ "."; "--type"; "lib" ]
let initTest2 = collectArgs [ "."; "--lang"; "fsharp"; "--type"; "lib" ]
let addTest0 = collectArgs [ "one.fs" ]
let addTest1 = collectArgs [ "one.fs"; "two.fs" ]
let addTest2 = collectArgs [ "one.fs"; "two.fs"; "--project"; "numbers.fsproj" ]
let addTest3 = collectArgs [ "one.fs"; "two.fs"; "--link" ]
let addTest4 = collectArgs [ "one.fs"; "two.fs"; "--link"; "--project"; "numbers.fsproj" ]

val initTest0 : Command = {Arguments = ["."];
                           Options = map [];}
val initTest1 : Command = {Arguments = ["."];
                           Options = map [("--type", Some "lib")];}
val initTest2 : Command =
  {Arguments = ["."];
   Options = map [("--lang", Some "fsharp"); ("--type", Some "lib")];}
val addTest0 : Command = {Arguments = ["one.fs"];
                          Options = map [];}
val addTest1 : Command = {Arguments = ["two.fs"; "one.fs"];
                          Options = map [];}
val addTest2 : Command =
  {Arguments = ["two.fs"; "one.fs"];
   Options = map [("--project", Some "numbers.fsproj")];}
val addTest3 : Command = {Arguments = ["two.fs"; "one.fs"];
                          Options = map [("--link", null)];}
val addTest4 : Command =
  {Arguments = ["two.fs"; "one.fs"];
   Options = map [("--link", null); ("--project", Some "numbers.fsproj")];}
*)

    [<Theory>]
    [<MemberData("rawCommandParsing")>]
    let ``Parse raw commands - expect correct`` (raw: string seq) (expected: Command) = failwith "undefined"
