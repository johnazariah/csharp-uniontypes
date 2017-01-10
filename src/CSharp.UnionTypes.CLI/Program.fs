open System.IO
open System.Text.RegularExpressions

open CSharp.UnionTypes.CodeGenerator

type CompilerParameters = {
    HelpRequested : bool
    VerboseRequested : bool
    InputFile : string option
    OutputFile : string option
} with
    static member apply argv =
        let split_arg_value (s: System.String) =
            let parts = s.Split ('=')
            match parts.Length with
            | l when l = 1 -> Some (parts.[0].TrimStart('-'), "")
            | l when l = 2 -> Some (parts.[0].TrimStart('-'), parts.[1])
            | _ -> None

        let arg_value_map = argv |> Array.choose split_arg_value |> Map.ofSeq
        {
            InputFile  = arg_value_map.TryFind "input-file"
            OutputFile = arg_value_map.TryFind "output-file"
            HelpRequested = arg_value_map.ContainsKey "help"
            VerboseRequested = true
        }

let validate cp =
    let get_error_message message o =
        o |> Option.fold (fun _ _ -> None) (Some message)

    let inputFileSpecified fOpt =
        fOpt |> get_error_message "<--input-file> not specified"

    let outputFileSpecified fOpt =
        fOpt |> get_error_message "<--output-file> not specified"

    let validateInputFileExists fOpt =
        fOpt |> Option.filter (fun f -> File.Exists f) |> get_error_message "Input File Does Not Exist"

    let ensureOutputDirExists fOpt =
        fOpt |> Option.filter (fun f -> (FileInfo f).Directory.Create(); true) |> get_error_message "Output Directory Does Not Exist"

    [
        inputFileSpecified cp.InputFile
        outputFileSpecified cp.OutputFile
        validateInputFileExists cp.InputFile
        ensureOutputDirExists cp.OutputFile
    ] |> List.choose id

let printUsage errors cp =
    printfn "   %s : Parse and Generate Code for C# Union Types" System.AppDomain.CurrentDomain.FriendlyName
    printfn ""
    printfn "Arguments:"
    printfn "   [--help] : Optional. Print this message and exit"
    printfn "   --input-file : Full path to the '.csunion' file containing the Union Type declarations. File must exist."
    printfn "   --output-file : Full path to the '.cs' file into which the C# code will be written. The path to this file will be created if it doesn't exist. The file will be overwritten if it exists."
    printfn ""
    printfn "Specified Values"
    printfn "   --input-file currently has value [%s]" (match cp.InputFile with | Some f -> f | None -> "")
    printfn "   --output-file currently has value [%s]" (match cp.OutputFile with | Some f -> f | None -> "")
    printfn ""
    errors |> List.iter (fun e -> printfn "ERROR : %s" e)

let generateCode cp =
    generate_code_for_csunion_file (cp.InputFile, cp.OutputFile)
    |> ignore

[<EntryPoint>]
let main argv =
    let cp = CompilerParameters.apply argv
    let errors = validate cp

    if (cp.HelpRequested || (errors <> [])) then
        printUsage errors cp
    else
        generateCode cp

    0 // return an integer exit code
