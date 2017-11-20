﻿Imports System.Text.RegularExpressions

Public Class CreateObjects

    'TODO: Update comments
    Public Property CourseList As New List(Of Course)
    Public Property WeightList As New List(Of Weight)
    Public Sections() As String
    Public CourseOfferings(,) As Integer
    Public Property PeriodCount = 839

    Public Database As New Database
    Public ConnectionString As String = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=|DataDirectory|\Fall2017Classes.accdb"
    Public ProjSQL As String
    Public DataSet As New DataSet

    Public Sub CreateObjects()
        'First, we get the table in the dataset updated
        Dim tableName As String = "Classes"
        ProjSQL = "SELECT * FROM " & tableName & " WHERE Days = 'T R' OR Days = 'M W' OR Days = 'M W F'"
        Database.RunSql(ConnectionString, ProjSQL, DataSet, tableName)

        'Next we transfer data from the table to objects
        For rowNum As Integer = 0 To DataSet.Tables(tableName).Rows.Count - 1
            Dim Course As New Course With {
                .CRN = DataSet.Tables(tableName).Rows(rowNum)("CRN ?"),
                .Department = DataSet.Tables(tableName).Rows(rowNum)("Department"),
                .Title = DataSet.Tables(tableName).Rows(rowNum)("Title"),
                .Instructor = DataSet.Tables(tableName).Rows(rowNum)("Instructor"),
                .Days = DataSet.Tables(tableName).Rows(rowNum)("Days"),
                .Begin = DataSet.Tables(tableName).Rows(rowNum)("Begin"),
                .EndInst = DataSet.Tables(tableName).Rows(rowNum)("End"),
                .Location = DataSet.Tables(tableName).Rows(rowNum)("Location ?"),
                .CourseNumber = DataSet.Tables(tableName).Rows(rowNum)("Course Number")
            }

            CourseList.Add(Course)
            rowNum = rowNum + 1
        Next

        'Creates Sections
        'TODO change to enum implementation
        Sections = New String() {"Evening", "Morning", "TuesThurs", "MonWedFri", "MonWed"}

        'Initializes Course Offerings Paramater 2D array
        CourseOfferings = New Integer(PeriodCount - 1, CourseList.Count - 1) {}

        Dim PM As Regex
        Dim AM As Regex
        Dim startIndex As Integer
        Dim endIndex As Integer
        Dim time As String()
        For Each course As Course In CourseList
            PM = New Regex("([PM])\w+")
            AM = New Regex("([AM])\w+")
            If PM.Matches(course.Begin).Count > 0 Then
                MessageBox.Show("begin: " & course.Begin)
                time = PM.Replace(course.Begin, "").Split(":")
                startIndex = (time(0) * 20 + time(1) * 5) - 1
                MessageBox.Show("startIndex: " & startIndex)
                'MessageBox.Show("result" & course.Begin)
            ElseIf AM.Matches(course.Begin).Count > 0 Then
                'MessageBox.Show("AM")
            Else
                Throw New Exception()
            End If

        Next
        MessageBox.Show("Success")


    End Sub

    Public Sub Query()
        'TODO This is for creating queries for certian criteria on the client

    End Sub

End Class


