Imports System.Net.NetworkInformation, System.Net
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Devices
Imports System.Diagnostics
Imports System.IO

Public Class Form1

    Private keyPressCount As Integer = 0
    Private enterKeyPressCount As Integer = 0


    Private Sub TextBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBox1.KeyDown
        If e.KeyCode = Keys.Enter Then
            enterKeyPressCount += 1

            If enterKeyPressCount >= 3 Then
                ExecuteCommand()

            Else
                Timer1.Start()
            End If
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Timer1.Stop()

        If enterKeyPressCount <= 2 Then
            'DO NEW LINE
        End If

        enterKeyPressCount = 0
    End Sub




    Private Sub ExecuteCommand()
        Button1.PerformClick()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim nonEmptyLines() As String = TextBox1.Lines.Where(Function(line) Not String.IsNullOrWhiteSpace(line)).ToArray()

        If nonEmptyLines.Length > 0 Then
            Dim processInfo As New ProcessStartInfo()
            processInfo.FileName = "cmd.exe"
            processInfo.RedirectStandardInput = True
            processInfo.RedirectStandardOutput = True
            processInfo.UseShellExecute = False
            processInfo.CreateNoWindow = True

            Dim process As Process = Process.Start(processInfo)

            ' Send commands to CMD
            Dim cmdWriter As StreamWriter = process.StandardInput
            For Each line As String In nonEmptyLines
                cmdWriter.WriteLine(line)
            Next
            cmdWriter.Close()

            ' Clear TextBox1
            TextBox1.Clear()

            ' Read output from CMD
            Dim cmdReader As StreamReader = process.StandardOutput
            Dim output As String = cmdReader.ReadToEnd()



            ' Mendapatkan versi Windows
            Dim windowsVersion As String = GetWindowsVersion(output)

            ' Menghilangkan kalimat output awal
            Dim startPhrase As String = "Microsoft Windows [Version " & windowsVersion & " Microsoft Corporation. All rights reserved."
            Dim startIndex As Integer = output.IndexOf(startPhrase)
            If startIndex >= 0 Then
                output = output.Substring(startIndex + startPhrase.Length)
            End If

            TextBox2.AppendText(output)
            cmdReader.Close()

            process.WaitForExit()
            process.Close()
        Else
            MessageBox.Show("All lines are empty.")
        End If


    End Sub


 Private Function IsTextBoxEmptyInAllLines() As Boolean
    Dim lines() As String = TextBox1.Lines

    For Each line As String In lines
        If Not String.IsNullOrWhiteSpace(line) Then
            Return False
        End If
    Next

    Return True
End Function


    Private Function GetWindowsVersion(output As String) As String
        Dim startPhrase As String = "Version "
        Dim startIndex As Integer = output.IndexOf(startPhrase)
        If startIndex >= 0 Then
            Dim endIndex As Integer = output.IndexOf(" ", startIndex + startPhrase.Length)
            If endIndex >= 0 Then
                Return output.Substring(startIndex + startPhrase.Length, endIndex - startIndex - startPhrase.Length)
            End If
        End If
        Return ""
    End Function

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        TextBox1.ResetText()

    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        init_cbox()
        init_sbox()
    End Sub

    Private Sub Form1_Click(sender As Object, e As EventArgs) Handles MyBase.Click
        Button5.Focus()
    End Sub



    Private portsItems As List(Of String)
    Private Sub init_cbox()
        ' Clear existing items in CheckedListBox
        CheckedListBox1.Items.Clear()

        ' Execute netstat command to retrieve used ports
        Dim processInfo As New ProcessStartInfo()
        processInfo.FileName = "netstat"
        processInfo.Arguments = "-ano" ' Include process ID information
        processInfo.RedirectStandardOutput = True
        processInfo.UseShellExecute = False
        processInfo.CreateNoWindow = True

        Dim process As Process = Process.Start(processInfo)

        ' Read netstat output and add used ports with local address to CheckedListBox
        Dim output As String = process.StandardOutput.ReadToEnd()

        Dim portRegex As New Regex("(?<=:)\d+(?=\s)")
        Dim addressRegex As New Regex("\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")

        Dim portMatches As MatchCollection = portRegex.Matches(output)
        Dim addressMatches As MatchCollection = addressRegex.Matches(output)

        Dim minCount As Integer = Math.Min(portMatches.Count, addressMatches.Count)

        ' Initialize the portsItems variable with original items
        portsItems = New List(Of String)()

        For i As Integer = 0 To minCount - 1
            Dim port As String = portMatches(i).Value
            Dim address As String = addressMatches(i).Value

            Dim item As String = address & ":" & port

            If (Not (CheckedListBox1.Items.Contains(item))) Then
                CheckedListBox1.Items.Add(item)
                portsItems.Add(item)
            End If

        Next

        process.WaitForExit()
        process.Close()

        CheckedListBox1.Tag = portsItems
    End Sub

    Private Sub init_sbox()
        TextBox3.ForeColor = Color.Gray
        TextBox3.Text = "Search port here"
    End Sub

    Private Sub TextBox3_GotFocus(sender As Object, e As EventArgs) Handles TextBox3.GotFocus
        If TextBox3.Text = "Search port here" Then
            TextBox3.Text = ""
            TextBox3.ForeColor = Color.Black
        End If
    End Sub

    Private Sub TextBox3_LostFocus(sender As Object, e As EventArgs) Handles TextBox3.LostFocus
        If TextBox3.Text = "" Then
            TextBox3.Text = "Search port here"
            TextBox3.ForeColor = Color.Gray
        End If
    End Sub



    Private Sub TextBox3_KeyUp(sender As Object, e As KeyEventArgs) Handles TextBox3.KeyUp
        Dim searchText As String = TextBox3.Text.ToLower()

        ''' WAY2:
        CheckedListBox1.BeginUpdate() ' Disable UI updates while modifying the CheckedListBox

        ' Clear the CheckedListBox items
        CheckedListBox1.Items.Clear()

        ' Loop through each item in the original items
        For Each item As String In portsItems
            Dim itemText As String = item.ToLower()

            ' Check if item contains search text
            If itemText.Contains(searchText) Then
                CheckedListBox1.Items.Add(item) ' Add matching item to the CheckedListBox

                ' Check the searched item
                If itemText = searchText Then
                    CheckedListBox1.SetItemChecked(CheckedListBox1.Items.Count - 1, True)
                End If
            End If
        Next

        CheckedListBox1.EndUpdate() ' Enable UI updates after modifying the CheckedListBox



        ''' WAY1:
        '' Check if TextBox is empty
        'If String.IsNullOrEmpty(searchText) Then
        '    ' Uncheck all items in CheckedListBox
        '    For i As Integer = 0 To CheckedListBox1.Items.Count - 1
        '        CheckedListBox1.SetItemChecked(i, False)
        '    Next
        '    Return ' Exit the method
        'End If

        '' Filtering
        'For i As Integer = 0 To CheckedListBox1.Items.Count - 1
        '    Dim itemText As String = CheckedListBox1.Items(i).ToString().ToLower()

        '    ' Check if item contains search text
        '    If itemText.Contains(searchText) Then
        '        CheckedListBox1.SetItemChecked(i, True)
        '    Else
        '        CheckedListBox1.SetItemChecked(i, False)
        '    End If
        'Next
    End Sub




    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        If CheckedListBox1.CheckedItems.Count > 0 Then
            ' Kill selected ports
            For Each item As String In CheckedListBox1.CheckedItems
                Dim port As String = item.Split(":"c)(1)
                Dim result As Boolean = KillProcessByPort(port)

                ' Display status in TextBox2
                If result Then
                    TextBox2.AppendText("Port " & port & " killed successfully." & Environment.NewLine)
                Else
                    TextBox2.AppendText("Failed to kill port " & port & "." & Environment.NewLine)
                End If
            Next

            ' Refresh the CheckedListBox after killing the ports
            init_cbox()
            init_sbox()


        Else
            ' No items are checked
            MessageBox.Show("No ports are checked :)")
        End If

    End Sub



    Private Function KillProcessByPort(port As String) As Boolean
        Dim processId As String = GetProcessIdByPort(port)

        If Not String.IsNullOrEmpty(processId) Then
            Dim processInfo As New ProcessStartInfo()
            processInfo.FileName = "taskkill"
            processInfo.Arguments = "/F /PID " & processId
            processInfo.RedirectStandardOutput = True
            processInfo.UseShellExecute = False
            processInfo.CreateNoWindow = True

            Dim process As Process = Process.Start(processInfo)
            process.WaitForExit()
            process.Close()

            Return True ' Process killed successfully
        Else
            Return False ' Failed to kill the process
        End If
    End Function


    Private Function GetProcessIdByPort(port As String) As String
        Dim processInfo As New ProcessStartInfo()
        processInfo.FileName = "netstat"
        processInfo.Arguments = "-ano" ' Include process ID information
        processInfo.RedirectStandardOutput = True
        processInfo.UseShellExecute = False
        processInfo.CreateNoWindow = True

        Dim process As Process = Process.Start(processInfo)

        Dim output As String = process.StandardOutput.ReadToEnd()
        Dim lines() As String = output.Split({Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)

        For Each line As String In lines
            If line.Contains(":" & port) Then
                Dim parts() As String = line.Split({" "}, StringSplitOptions.RemoveEmptyEntries)
                If parts.Length >= 2 Then
                    Return parts(parts.Length - 1)
                End If
            End If
        Next

        process.WaitForExit()
        process.Close()

        Return String.Empty
    End Function

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        TextBox2.Clear()

    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        init_cbox()
    End Sub

    Private Sub TableLayoutPanel4_Click(sender As Object, e As EventArgs) Handles TableLayoutPanel4.Click
        Button5.Focus()
    End Sub

    Private Sub Panel3_Click(sender As Object, e As EventArgs) Handles Panel3.Click
        Dim urlFB As String = "https://www.facebook.com/profile.php?id=100025068874578"
        Process.Start(urlFB)
    End Sub

    Private Sub Panel2_Click(sender As Object, e As EventArgs) Handles Panel2.Click
        Dim urlIG As String = "https://www.instagram.com/henrykim119"
        Process.Start(urlIG)
    End Sub
End Class