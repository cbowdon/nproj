namespace NProj.Test

module Parse =
    open System
    open Xunit
    open NProj
    open NProj.Common

    let initArgs = [ [| String.Empty |]]

    [<Theory>]
    [<MemberData("initArgs")>]
    let ``Parse init arguments expect success`` (flags: string) =
        // Fixture setup
        let args = flags.Split(',') |> Seq.map (fun f -> f.Trim())
        let expected: Init.InitCommand = { ProjectFile = { Location = Uri(".") }; Lang = FSharp; Type = Library }
        // Exercise system
        let actual = Init.parse args
        // Verify
        Assert.Equal(expected, actual)
