Imports System.Diagnostics
Imports System.IO
Module Killer_Module

    Sub Main()
        Dim processInfo As New ProcessStartInfo()
        processInfo.FileName = "cmd.exe" ' Nama file CMD
        processInfo.RedirectStandardInput = True
        processInfo.RedirectStandardOutput = True
        processInfo.UseShellExecute = False

        Dim process As Process = Process.Start(processInfo)

        ' Mengirimkan perintah ke CMD
        Dim cmdWriter As StreamWriter = process.StandardInput
        cmdWriter.WriteLine("echo Hello, World!")
        cmdWriter.WriteLine("dir") ' Contoh perintah lain

        ' Membaca output dari CMD
        Dim cmdReader As StreamReader = process.StandardOutput
        Dim output As String = cmdReader.ReadToEnd()
        Console.WriteLine(output)

        process.WaitForExit()
        process.Close()
    End Sub

End Module
