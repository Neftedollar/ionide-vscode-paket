[<ReflectedDefinition; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Ionide.VSCode

open System
open System.Text.RegularExpressions
open FunScript
open FunScript.TypeScript
open FunScript.TypeScript.fs
open FunScript.TypeScript.child_process
open FunScript.TypeScript.vscode

open Ionide
open Ionide.VSCode

module PaketService =
    let private location =
        if Process.isWin () then
            (VSCode.getPluginPath "Ionide.Ionide-Paket") + @"\bin\paket.exe"
        else
            (VSCode.getPluginPath "Ionide.Ionide-Paket") + @"/bin/paket.exe"


    let private bootstrapperLocation =
        if Process.isWin () then
            (VSCode.getPluginPath "Ionide.Ionide-Paket") + @"\bin\paket.bootstrapper.exe"
        else
            (VSCode.getPluginPath "Ionide.Ionide-Paket")+ @"/bin/paket.bootstrapper.exe"

    let private spawnPaket cmd =
        let outputChannel = window.Globals.createOutputChannel "Paket"
        outputChannel.clear ()
        window.Globals.showInformationMessageOverload2 ("Paket started", "Open")
        |> Promise.toPromise
        |> Promise.success(fun n -> if n = "Open" then outputChannel.show (2 |> unbox) )
        |> ignore

        Process.spawnWithNotification location "mono" cmd outputChannel
        |> Process.onExit(fun (code) ->
            if code.ToString() ="0" then
                window.Globals.showInformationMessage "Paket completed" |> ignore
            else
                window.Globals.showErrorMessage "Paket failed" |> ignore)
        |> ignore
    let private execPaket cmd = Process.exec location "mono" cmd

    let private handlePaketList (error : Error, stdout : Buffer, stderr : Buffer) =
        if(stdout.toString() = "") then
            [||]
        else
            stdout.toString().Split('\n')
            |> Array.filter((<>) "" )

    let UpdatePaketSilent () = Process.spawn bootstrapperLocation "mono" ""
    let Init () = "init" |> spawnPaket
    let Update () = "update" |> spawnPaket
    let Install () = "install" |> spawnPaket
    let Outdated () = "outdated" |> spawnPaket
    let Restore () = "restore" |> spawnPaket
    let AutoRestoreOn () = "auto-restore on" |> spawnPaket
    let AutoRestoreOff () = "auto-restore off" |> spawnPaket
    let ConvertFromNuget () = "convert-from-nuget" |> spawnPaket
    let Simplify () = "simplify" |> spawnPaket

    let Add () =
        (window.Globals.showInputBox ())
        |> Promise.toPromise
        |> Promise.success (fun n -> if JS.isDefined n then  sprintf "add nuget %s" n  |> spawnPaket)
        |> ignore

    let AddToCurrent () =
        let fn = window.Globals.activeTextEditor.document.fileName
        if fn.EndsWith(".fsproj") then
            (window.Globals.showInputBox ())
            |> Promise.toPromise
            |> Promise.success (fun n -> if JS.isDefined n then sprintf "add nuget %s project \"%s\"" n fn |> spawnPaket)
            |> ignore
        else
            window.Globals.showErrorMessage "fsproj file needs to be opened" |> ignore

    let UpdateGroup () =
        "show-groups -s"
        |> execPaket
        |> Promise.success (handlePaketList)
        |> window.Globals.showQuickPick
        |> Promise.toPromise
        |> Promise.success (fun n -> if JS.isDefined n then sprintf "update group %s" n |> spawnPaket)
        |> ignore

    let UpdatePackage () =
        "show-installed-packages -s"
        |> execPaket
        |> Promise.success (handlePaketList)
        |> window.Globals.showQuickPick
        |> Promise.toPromise
        |> Promise.success (fun n -> if JS.isDefined n then sprintf "update nuget %s" n |> spawnPaket)
        |> ignore

    let UpdatePackageCurrent () =
        let fn = window.Globals.activeTextEditor.document.fileName
        if fn.EndsWith(".fsproj") then
            "show-installed-packages -s"
            |> execPaket
            |> Promise.success (handlePaketList)
            |> window.Globals.showQuickPick
            |> Promise.toPromise
            |> Promise.success (fun n -> if JS.isDefined n then sprintf "update nuget %s project \"%s\"" fn n |> spawnPaket)
            |> ignore
        else
            window.Globals.showErrorMessage "fsproj file needs to be opened" |> ignore

    let RemovePackage () =
        "show-installed-packages -s"
        |> execPaket
        |> Promise.success (handlePaketList)
        |> window.Globals.showQuickPick
        |> Promise.toPromise
        |> Promise.success (fun n -> if JS.isDefined n then sprintf "remove nuget %s" n |> spawnPaket)
        |> ignore

    let RemovePackageCurrent () =
        let fn = window.Globals.activeTextEditor.document.fileName
        if fn.EndsWith(".fsproj") then
            "show-installed-packages -s"
            |> execPaket
            |> Promise.success (handlePaketList)
            |> window.Globals.showQuickPick
            |> Promise.toPromise
            |> Promise.success (fun n -> if JS.isDefined n then sprintf "remove nuget %s project \"%s\"" fn n |> spawnPaket)
            |> ignore
        else
            window.Globals.showErrorMessage "fsproj file needs to be opened" |> ignore

type Paket() =
    member x.activate(state:obj) =
        PaketService.UpdatePaketSilent () |> ignore
        commands.Globals.registerCommand("paket.Init", PaketService.Init |> unbox) |> ignore
        commands.Globals.registerCommand("paket.Install", PaketService.Install |> unbox) |> ignore
        commands.Globals.registerCommand("paket.Update", PaketService.Update |> unbox) |> ignore
        commands.Globals.registerCommand("paket.Outdated", PaketService.Outdated |> unbox) |> ignore
        commands.Globals.registerCommand("paket.Restore", PaketService.Restore |> unbox) |> ignore
        commands.Globals.registerCommand("paket.AutoRestoreOn", PaketService.AutoRestoreOn |> unbox) |> ignore
        commands.Globals.registerCommand("paket.AutoRestoreOff", PaketService.AutoRestoreOff |> unbox) |> ignore
        commands.Globals.registerCommand("paket.ConvertFromNuget", PaketService.ConvertFromNuget |> unbox) |> ignore
        commands.Globals.registerCommand("paket.Simplify", PaketService.Simplify |> unbox) |> ignore
        commands.Globals.registerCommand("paket.Add", PaketService.Add |> unbox) |> ignore
        commands.Globals.registerCommand("paket.AddToCurrent", PaketService.AddToCurrent |> unbox) |> ignore
        commands.Globals.registerCommand("paket.UpdateGroup", PaketService.UpdateGroup |> unbox) |> ignore
        commands.Globals.registerCommand("paket.UpdatePackage", PaketService.UpdatePackage |> unbox) |> ignore
        commands.Globals.registerCommand("paket.UpdatePackageCurrent", PaketService.UpdatePackageCurrent |> unbox) |> ignore
        commands.Globals.registerCommand("paket.RemovePackage", PaketService.RemovePackage |> unbox) |> ignore
        commands.Globals.registerCommand("paket.RemovePackageCurrent", PaketService.RemovePackageCurrent |> unbox) |> ignore
        ()
