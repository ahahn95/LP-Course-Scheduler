﻿Imports Microsoft.SolverFoundation.Common
Imports Microsoft.SolverFoundation.Services
Imports Microsoft.SolverFoundation.Solvers

'Used to find the ideal schedule
Public Class Optimization

    'The optimal course list
    Public OptimalCourseList As New List(Of DiscreteCourse)

    'Solver fields
    Public ObjectCreator As New ObjectCreator
    Public Solver As SimplexSolver

    'Output data from solver
    Public ObjectiveFunctionValue As Integer
    Public DecisionVariableValues(,) As Integer

    'Variables used for solver
    Private DecisionVariableKey As String
    Private DecisionVariableIndex As Integer

    Private ConstraintCoefficient As Single
    Private ConstraintKey As String
    Private ConstraintIndex As Integer

    Private ObjectiveIndex As Integer

    'In order: Evening, Morning, TR, MW, MWF 
    Public GoalAmounts = New Integer() {50, 30, 5, 70, 10}
    Public SectionPerformance = New Integer() {0, 0, 0, 0, 0}
    Public SlackSurplus = New Integer() {0, 0, 0, 0, 0}

    'These need to change to be set by the user and maybe change locations
    Public Property AmountRequestedCourses As Integer

    'Creates the optimization
    Public Sub BuildModel()
        Solver = New SimplexSolver

        ObjectCreator.CreateObjects()

        AddDecisionVariables()
        AddOverlapConstraints()
        AddEnrollmentConstraints()
        AddDuplicateCourseConstraint()
        AddObjectiveFunction()

        Solve() 'And calculate slack/surplus
    End Sub

    'Adds decision variables
    Private Sub AddDecisionVariables()
        For courseIndex As Integer = 0 To ObjectCreator.ReferenceList.Count - 1
            DecisionVariableKey = ObjectCreator.ReferenceList.ElementAt(courseIndex).CRN
            Solver.AddVariable(DecisionVariableKey, DecisionVariableIndex)
            Solver.SetIntegrality(DecisionVariableIndex, True)
            Solver.SetBounds(DecisionVariableIndex, 0, 1)
        Next
    End Sub

    'Adds overlap constraints
    Private Sub AddOverlapConstraints()
        For period = 0 To ObjectCreator.PeriodCount - 1
            ConstraintKey = "Overlap Constraint: " & period
            Solver.AddRow(ConstraintKey, ConstraintIndex)
            Dim iter As Integer = 0
            For Each course As DiscreteCourse In ObjectCreator.ReferenceList
                ConstraintCoefficient = ObjectCreator.DiscreteCourseOfferings(iter, period)
                iter = iter + 1
                DecisionVariableKey = course.CRN
                DecisionVariableIndex = Solver.GetIndexFromKey(DecisionVariableKey)
                Solver.SetCoefficient(ConstraintIndex, DecisionVariableIndex, ConstraintCoefficient)
            Next
            Solver.SetBounds(ConstraintIndex, 0, 1)
        Next
    End Sub

    'Adds enrollment constraints
    Private Sub AddEnrollmentConstraints()
        ConstraintKey = "Enrollment Constraint"
        Solver.AddRow(ConstraintKey, ConstraintIndex)
        For Each DiscreteCourse As DiscreteCourse In ObjectCreator.ReferenceList
            ConstraintCoefficient = 1
            DecisionVariableKey = DiscreteCourse.CRN
            DecisionVariableIndex = Solver.GetIndexFromKey(DecisionVariableKey)
            Solver.SetCoefficient(ConstraintIndex, DecisionVariableIndex, ConstraintCoefficient)
        Next
        Solver.SetBounds(ConstraintIndex, AmountRequestedCourses, AmountRequestedCourses)
    End Sub

    'Adds duplicate course constraint
    Private Sub AddDuplicateCourseConstraint()
        For Each AbstractCourse As AbstractCourse In ObjectCreator.AbstractCourseList
            ConstraintKey = "Duplicate Course Constraint: " + AbstractCourse.Department + " " + AbstractCourse.CourseNumber.ToString
            Solver.AddRow(ConstraintKey, ConstraintIndex)
            For Each DiscreteCourse As DiscreteCourse In AbstractCourse.DiscreteCourseList
                ConstraintCoefficient = 1
                DecisionVariableKey = DiscreteCourse.CRN
                DecisionVariableIndex = Solver.GetIndexFromKey(DecisionVariableKey)
                Solver.SetCoefficient(ConstraintIndex, DecisionVariableIndex, ConstraintCoefficient)
            Next
            Solver.SetBounds(ConstraintIndex, 0, 1)
        Next
    End Sub

    'Adds objective function
    Private Sub AddObjectiveFunction()
        Dim objKey As String = "Objective Function"
        Solver.AddRow(objKey, ObjectiveIndex)
        For section = 0 To ObjectCreator.Sections.Count - 1
            For Each course As DiscreteCourse In ObjectCreator.ReferenceList
                ConstraintCoefficient = Math.Abs(course.Totals(section) - GoalAmounts(section))
                DecisionVariableKey = course.CRN
                DecisionVariableIndex = Solver.GetIndexFromKey(DecisionVariableKey)
                Solver.SetCoefficient(ObjectiveIndex, DecisionVariableIndex, ConstraintCoefficient)
            Next
        Next
    End Sub

    'Solves
    Private Sub Solve()
        Solver.AddGoal(ObjectiveIndex, 0, True)

        Dim mySolverParms As New SimplexSolverParams

        Solver.Solve(mySolverParms)

        Try
            ObjectiveFunctionValue = Solver.GetValue(ObjectiveIndex).ToString
        Catch ex As Exception
            MessageBox.Show("Indeterminate")
        End Try


        DecisionVariableValues = New Integer(ObjectCreator.ReferenceList.Count - 1, 1) {}

        For i = 0 To ObjectCreator.ReferenceList.Count - 1
            DecisionVariableValues(i, 0) = Solver.GetKeyFromIndex(i)
            DecisionVariableValues(i, 1) = Solver.GetValue(i).ToDouble
        Next

        For i = 0 To ObjectCreator.ReferenceList.Count - 1
            If DecisionVariableValues(i, 1) = 1 Then
                For Each course As DiscreteCourse In ObjectCreator.ReferenceList
                    If DecisionVariableValues(i, 0).Equals(course.CRN) Then
                        OptimalCourseList.Add(course)
                        Dim k As Object = 3
                        For totals = 0 To course.Totals.Count - 1
                            SectionPerformance(totals) = SectionPerformance(totals) + course.Totals(totals)
                        Next
                    End If
                Next
            End If
        Next

        For i = 0 To 4
            SlackSurplus(i) = SectionPerformance(i) - GoalAmounts(i)
        Next
    End Sub

End Class