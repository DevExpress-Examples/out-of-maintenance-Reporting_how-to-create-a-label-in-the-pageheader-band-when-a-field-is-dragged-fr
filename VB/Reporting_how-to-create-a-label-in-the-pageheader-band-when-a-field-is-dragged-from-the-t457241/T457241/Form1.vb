Imports DevExpress.XtraReports.Design
Imports DevExpress.XtraReports.UI
Imports DevExpress.XtraReports.UserDesigner
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.ComponentModel.Design
Imports System.Data
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Text
Imports System.Windows.Forms

Namespace T457241
	Partial Public Class Form1
		Inherits DevExpress.XtraEditors.XtraForm

		Private designer As ReportDesignTool
		Private host As IDesignerHost
		Public Sub New()
			InitializeComponent()
		End Sub

		Private Sub simpleButton1_Click(ByVal sender As Object, ByVal e As EventArgs) Handles simpleButton1.Click
			designer = New ReportDesignTool(New XtraReport1())
			AddHandler designer.DesignRibbonForm.DesignMdiController.DesignPanelLoaded, AddressOf DesignMdiController_DesignPanelLoaded
			designer.ShowRibbonDesignerDialog()
		End Sub

		Private Sub DesignMdiController_DesignPanelLoaded(ByVal sender As Object, ByVal e As DesignerLoadedEventArgs)
			If radioGroup1.SelectedIndex = 0 Then
				AddHandler TryCast(sender, XRDesignPanel).ComponentAdded, AddressOf DesignPanel_ComponentAdded
			Else
				e.DesignerHost.RemoveService(GetType(IFieldListDragDropService))
				e.DesignerHost.AddService(GetType(IFieldListDragDropService), New CustomFieldListDragDropService(e.DesignerHost, DirectCast(sender, XRDesignPanel)))
			End If
		End Sub

		Private Sub DesignPanel_ComponentAdded(ByVal sender As Object, ByVal e As System.ComponentModel.Design.ComponentEventArgs)
			Dim headerBand As PageHeaderBand
			host = DirectCast(designer.DesignRibbonForm.DesignMdiController.ActiveDesignPanel.Report.Site.GetService(GetType(IDesignerHost)), IDesignerHost)
			If Not(TypeOf e.Component Is XRLabel) AndAlso Not(TypeOf e.Component Is XRTable) Then
				Return
			End If
			If TypeOf e.Component Is XRLabel Then
				Dim label As XRLabel = TryCast(e.Component, XRLabel)
				If label.ExpressionBindings.Count = 0 Then
					Return
				End If
				If TypeOf label.Parent Is XRTableRow Then
					Return
				End If
				headerBand = CreatePageHeaderBand(designer.Report)
				Dim newLabel As XRLabel = CopyLabel(label, headerBand)
				headerBand.Controls.Add(newLabel)
				DesignToolHelper.AddToContainer(host, newLabel)
			End If
			If TypeOf e.Component Is XRTable Then
				headerBand = CreatePageHeaderBand(designer.Report)
				Dim sourceRow As XRTableRow = (TryCast(e.Component, XRTable)).Rows(0)
				Dim headerTable As XRTable = CreateTable(sourceRow, headerBand.HeightF)
				headerBand.Controls.Add(headerTable)
				DesignToolHelper.AddToContainer(host, headerTable)
			End If
		End Sub

		Private Function CreatePageHeaderBand(ByVal report As XtraReport) As PageHeaderBand
			Dim headerBand As PageHeaderBand = TryCast(report.Bands(BandKind.PageHeader), PageHeaderBand)
			' Remove the comments if you need to recreate the PageHeader band
			'if(headerBand != null) {
			'    for(int i = headerBand.Controls.Count - 1; i >= 0; i--)
			'        DesignToolHelper.RemoveFromContainer(host, headerBand.Controls[i]);
			'    headerBand.Controls.Clear();
			'    DesignToolHelper.RemoveFromContainer(host, headerBand);
			If headerBand Is Nothing Then
				headerBand = New PageHeaderBand()
				headerBand.HeightF = 0
				headerBand.Visible = True
				DesignToolHelper.AddToContainer(host, headerBand)
			End If
			Return headerBand
		End Function

		Private Function CreateTable(ByVal sourceRow As XRTableRow, ByVal height As Single) As XRTable
			Dim table As New XRTable()
			Dim headerTableRow As New XRTableRow()
			table.BeginInit()
			table.Rows.Add(headerTableRow)
			For Each cell As XRTableCell In sourceRow.Cells
				CopyCell(cell, headerTableRow)
			Next cell
			table.Borders = DevExpress.XtraPrinting.BorderSide.All
			table.LocationF = New PointF(0, height)
			table.AdjustSize()
			table.EndInit()
			Return table
		End Function

		Private Function CopyLabel(ByVal label As XRLabel, ByVal band As PageHeaderBand) As XRLabel
            Dim labelText As String = "XRLabel"
            Dim report As XtraReport = TryCast(band.Report, XtraReport)
			Dim newLabel As New XRLabel()
			newLabel.LocationF = label.LocationF
			newLabel.WidthF = label.WidthF
            newLabel.HeightF = label.HeightF
            newLabel.BackColor = Color.Green
            newLabel.ForeColor = Color.Yellow
            newLabel.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter
            newLabel.Font = New Font("Calibry", 11, FontStyle.Bold)
			newLabel.Borders = DevExpress.XtraPrinting.BorderSide.All
			If label.ExpressionBindings.Count > 0 Then
				labelText = label.ExpressionBindings(0).Expression
				Dim index As Integer = labelText.LastIndexOf(".")
				If index > 0 Then
					labelText = labelText.Substring(index + 1)
				End If
			End If
			newLabel.Text = labelText
			Return newLabel
		End Function

		Private Sub CopyCell(ByVal source As XRTableCell, ByVal row As XRTableRow)
			Dim cell As New XRTableCell()
			cell.WidthF = source.WidthF
			cell.HeightF = source.HeightF
			cell.BackColor = Color.Green
			cell.ForeColor = Color.Yellow
			cell.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter
			cell.Font = New Font("Calibry", 11, FontStyle.Bold)
			Dim cellText As String = source.ExpressionBindings(0).Expression
			Dim index As Integer = cellText.LastIndexOf(".")
            If index > 0 Then
                cellText = cellText.Substring(index + 1)
            End If
            cell.Text = cellText
			row.Cells.Add(cell)
			DesignToolHelper.AddToContainer(host, cell)
		End Sub
	End Class
End Namespace
