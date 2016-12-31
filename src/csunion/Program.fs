open System.IO
open System.Text.RegularExpressions

open BrightSword.CSharpExtensions.DiscriminatedUnion.CodeGenerator

(*
type CompilerParameters = {
    InputFile : FileInfo
    OutputFile : FileInfo
} with
    static member apply(input, output) =
        let i = input  |> Option.filter (fun f -> File.Exists f) |> Option.map FileInfo
        let o = output |> Option.map (FileInfo >> (fun fi -> fi.Directory.Create (); fi))

        match (i, o) with
        | Some i', Some o'-> Some {InputFile = i'; OutputFile = o'}
        | _, _ -> None
*)

let private parseCompilerParameters argv = 
    let split_arg_value (s: System.String) = 
        let parts = s.Split ('=')
        in 
        if (parts.Length <> 2) then None else Some (parts.[0].TrimStart('-'), parts.[1])

    let arg_value_map = argv |> Array.choose split_arg_value |> Map.ofSeq
    in
    (arg_value_map.TryFind "input-file", arg_value_map.TryFind "output-file")

[<EntryPoint>]
let main argv = 
    parseCompilerParameters argv
    |> generate_code_for_csunion_file
    |> ignore

    0 // return an integer exit code
