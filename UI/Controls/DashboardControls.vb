Imports System
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports EnterpriseCRM.UI.Theme

Namespace EnterpriseCRM.UI.Controls
    Public Class StatCardControl
        Inherits UserControl

        Private _title As String = "Metric"
        Private _value As String = "0"
        Private _subtext As String = ""
        Private _accentColor As Color = ThemeManager.BrandPrimary

        Public Property Title As String
            Get
                Return _title
            End Get
            Set(value As String)
                _title = value
                Invalidate()
            End Set
        End Property

        Public Property Value As String
            Get
                Return _value
            End Get
            Set(value As String)
                _value = value
                Invalidate()
            End Set
        End Property

        Public Property Subtext As String
            Get
                Return _subtext
            End Get
            Set(value As String)
                _subtext = value
                Invalidate()
            End Set
        End Property

        Public Property AccentColor As Color
            Get
                Return _accentColor
            End Get
            Set(value As Color)
                _accentColor = value
                Invalidate()
            End Set
        End Property

        Public Sub New()
            DoubleBuffered = True
            Size = New Size(220, 105)
            Margin = New Padding(8)
            Font = New Font("Segoe UI", 9.0F)
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            MyBase.OnPaint(e)
            Dim g = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit

            Dim rect = New Rectangle(0, 0, Width - 1, Height - 1)

            Using bgBrush As New SolidBrush(ThemeManager.CardBackgroundColor)
                g.FillRectangle(bgBrush, rect)
            End Using

            Using borderPen As New Pen(ThemeManager.BorderColor, 1)
                g.DrawRectangle(borderPen, rect)
            End Using

            Using accentBrush As New SolidBrush(_accentColor)
                g.FillRectangle(accentBrush, 0, 0, 4, Height)
            End Using

            Using titleFont As New Font("Segoe UI", 9.5F, FontStyle.Regular)
                Using titleBrush As New SolidBrush(ThemeManager.TextSecondaryColor)
                    g.DrawString(_title.ToUpper(), titleFont, titleBrush, New PointF(18, 14))
                End Using
            End Using

            Using valFont As New Font("Segoe UI", 18.0F, FontStyle.Bold)
                Using valBrush As New SolidBrush(ThemeManager.TextPrimaryColor)
                    g.DrawString(_value, valFont, valBrush, New PointF(16, 36))
                End Using
            End Using

            If Not String.IsNullOrEmpty(_subtext) Then
                Using subFont As New Font("Segoe UI", 8.5F, FontStyle.Regular)
                    Using subBrush As New SolidBrush(ThemeManager.TextSecondaryColor)
                        g.DrawString(_subtext, subFont, subBrush, New PointF(18, 76))
                    End Using
                End Using
            End If
        End Sub
    End Class

    Public Class SimpleBarChartControl
        Inherits UserControl

        Public Class ChartItem
            Public Property Label As String
            Public Property Value As Decimal
            Public Property Color As Color
        End Class

        Private _items As New List(Of ChartItem)()
        Private _title As String = "Pipeline Breakdown"

        Public Property Title As String
            Get
                Return _title
            End Get
            Set(value As String)
                _title = value
                Invalidate()
            End Set
        End Property

        Public Sub New()
            DoubleBuffered = True
            Size = New Size(400, 260)
            Font = New Font("Segoe UI", 9.0F)
        End Sub

        Public Sub SetData(items As List(Of ChartItem))
            _items = If(items, New List(Of ChartItem)())
            Invalidate()
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            MyBase.OnPaint(e)
            Dim g = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit

            Dim rect = New Rectangle(0, 0, Width - 1, Height - 1)

            Using bgBrush As New SolidBrush(ThemeManager.CardBackgroundColor)
                g.FillRectangle(bgBrush, rect)
            End Using

            Using borderPen As New Pen(ThemeManager.BorderColor, 1)
                g.DrawRectangle(borderPen, rect)
            End Using

            Using titleFont As New Font("Segoe UI", 11.0F, FontStyle.Bold)
                Using titleBrush As New SolidBrush(ThemeManager.TextPrimaryColor)
                    g.DrawString(_title, titleFont, titleBrush, New PointF(18, 16))
                End Using
            End Using

            If _items.Count = 0 Then
                Using emptyFont As New Font("Segoe UI", 9.5F, FontStyle.Italic)
                    Using emptyBrush As New SolidBrush(ThemeManager.TextSecondaryColor)
                        g.DrawString("No data available to display.", emptyFont, emptyBrush, New PointF(18, 60))
                    End Using
                End Using
                Return
            End If

            Dim maxVal As Decimal = 0
            For Each itm In _items
                If itm.Value > maxVal Then maxVal = itm.Value
            Next
            If maxVal = 0 Then maxVal = 1

            Dim chartTop = 65
            Dim rowHeight = 34
            Dim startY = chartTop
            Dim barMaxW = Width - 200

            For Each itm In _items
                Using lblFont As New Font("Segoe UI", 9.0F, FontStyle.Regular)
                    Using lblBrush As New SolidBrush(ThemeManager.TextPrimaryColor)
                        g.DrawString(itm.Label, lblFont, lblBrush, New RectangleF(18, startY + 2, 100, rowHeight))
                    End Using
                End Using

                Dim trackRect = New Rectangle(120, startY + 5, barMaxW, 14)
                Using trackBrush As New SolidBrush(If(ThemeManager.CurrentTheme = AppThemeMode.Light, Color.FromArgb(241, 245, 249), Color.FromArgb(51, 65, 85)))
                    g.FillRectangle(trackBrush, trackRect)
                End Using

                Dim fillW = CInt(Math.Max(4, (itm.Value / maxVal) * barMaxW))
                Dim fillRect = New Rectangle(120, startY + 5, fillW, 14)
                Using fillBrush As New SolidBrush(itm.Color)
                    g.FillRectangle(fillBrush, fillRect)
                End Using

                Using valFont As New Font("Segoe UI", 9.0F, FontStyle.Bold)
                    Using valBrush As New SolidBrush(ThemeManager.TextPrimaryColor)
                        Dim valStr = If(itm.Value >= 1000, $"${itm.Value:N0}", itm.Value.ToString("N0"))
                        g.DrawString(valStr, valFont, valBrush, New PointF(120 + barMaxW + 10, startY + 2))
                    End Using
                End Using

                startY += rowHeight
            Next
        End Sub
    End Class
End Namespace
