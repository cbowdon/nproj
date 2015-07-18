namespace NProj.Test

module Common =
    open System
    open Xunit
    open NProj
    open NProj.Common

    let rawCommandParsing: Object[] seq = Seq.empty
      // [| ["."; "--type"; "Exe" ]; { Arguments = [ "." ]; Options = Map.add "--type" (Some "Exe") Map.empty } |]

    [<Theory>]
    [<MemberData("rawCommandParsing")>]
    let ``Parse raw commands - expect correct`` (raw: string seq) (expected: Command) = failwith "undefined"
