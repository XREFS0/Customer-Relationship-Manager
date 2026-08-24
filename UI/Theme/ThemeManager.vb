Imports System.Drawing
Imports System.Windows.Forms

Namespace EnterpriseCRM.UI.Theme
    Public Enum AppThemeMode
        Light = 0
        Dark = 1
    End Enum

    Public Class ThemeManager
        Public Shared Property CurrentTheme As AppThemeMode = AppThemeMode.Light

        Public Shared ReadOnly LightBackground As Color = Color.FromArgb(248, 250, 252)
        Public Shared ReadOnly LightCardBackground As Color = Color.White
        Public Shared ReadOnly LightSidebarBackground As Color = Color.FromArgb(15, 23, 42)
        Public Shared ReadOnly LightSidebarText As Color = Color.FromArgb(226, 232, 240)
        Public Shared ReadOnly LightSidebarHover As Color = Color.FromArgb(30, 41, 59)
        Public Shared ReadOnly LightSidebarActive As Color = Color.FromArgb(37, 99, 235)
        Public Shared ReadOnly LightHeaderBackground As Color = Color.White
        Public Shared ReadOnly LightTextPrimary As Color = Color.FromArgb(15, 23, 42)
        Public Shared ReadOnly LightTextSecondary As Color = Color.FromArgb(100, 116, 139)
        Public Shared ReadOnly LightBorder As Color = Color.FromArgb(226, 232, 240)
        Public Shared ReadOnly LightGridHeader As Color = Color.FromArgb(241, 245, 249)
        Public Shared ReadOnly LightGridRowAlt As Color = Color.FromArgb(248, 250, 252)

        Public Shared ReadOnly DarkBackground As Color = Color.FromArgb(15, 23, 42)
        Public Shared ReadOnly DarkCardBackground As Color = Color.FromArgb(30, 41, 59)
        Public Shared ReadOnly DarkSidebarBackground As Color = Color.FromArgb(10, 15, 29)
        Public Shared ReadOnly DarkSidebarText As Color = Color.FromArgb(203, 213, 225)
        Public Shared ReadOnly DarkSidebarHover As Color = Color.FromArgb(30, 41, 59)
        Public Shared ReadOnly DarkSidebarActive As Color = Color.FromArgb(59, 130, 246)
        Public Shared ReadOnly DarkHeaderBackground As Color = Color.FromArgb(30, 41, 59)
        Public Shared ReadOnly DarkTextPrimary As Color = Color.FromArgb(241, 245, 249)
        Public Shared ReadOnly DarkTextSecondary As Color = Color.FromArgb(148, 163, 184)
        Public Shared ReadOnly DarkBorder As Color = Color.FromArgb(51, 65, 85)
        Public Shared ReadOnly DarkGridHeader As Color = Color.FromArgb(30, 41, 59)
        Public Shared ReadOnly DarkGridRowAlt As Color = Color.FromArgb(24, 34, 49)

        Public Shared ReadOnly BrandPrimary As Color = Color.FromArgb(37, 99, 235)
        Public Shared ReadOnly BrandPrimaryHover As Color = Color.FromArgb(29, 78, 216)
        Public Shared ReadOnly BrandSuccess As Color = Color.FromArgb(22, 163, 74)
        Public Shared ReadOnly BrandWarning As Color = Color.FromArgb(217, 119, 6)
        Public Shared ReadOnly BrandDanger As Color = Color.FromArgb(220, 38, 38)
        Public Shared ReadOnly BrandInfo As Color = Color.FromArgb(8, 145, 178)

        Public Shared ReadOnly Property BackgroundColor As Color
            Get
                Return If(CurrentTheme = AppThemeMode.Light, LightBackground, DarkBackground)
            End Get
        End Property

        Public Shared ReadOnly Property CardBackgroundColor As Color
            Get
                Return If(CurrentTheme = AppThemeMode.Light, LightCardBackground, DarkCardBackground)
            End Get
        End Property

        Public Shared ReadOnly Property TextPrimaryColor As Color
            Get
                Return If(CurrentTheme = AppThemeMode.Light, LightTextPrimary, DarkTextPrimary)
            End Get
        End Property

        Public Shared ReadOnly Property TextSecondaryColor As Color
            Get
                Return If(CurrentTheme = AppThemeMode.Light, LightTextSecondary, DarkTextSecondary)
            End Get
        End Property

        Public Shared ReadOnly Property BorderColor As Color
            Get
                Return If(CurrentTheme = AppThemeMode.Light, LightBorder, DarkBorder)
            End Get
        End Property

        Public Shared ReadOnly Property HeaderBackgroundColor As Color
            Get
                Return If(CurrentTheme = AppThemeMode.Light, LightHeaderBackground, DarkHeaderBackground)
            End Get
        End Property

        Public Shared Sub StyleGridView(grid As DataGridView)
            grid.BackgroundColor = CardBackgroundColor
            grid.BorderStyle = BorderStyle.None
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            grid.GridColor = BorderColor
            grid.EnableHeadersVisualStyles = False
            grid.RowHeadersVisible = False
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
            grid.MultiSelect = False
            grid.AllowUserToAddRows = False
            grid.AllowUserToDeleteRows = False
            grid.AllowUserToResizeRows = False
            grid.RowTemplate.Height = 36
            grid.Font = New Font("Segoe UI", 9.5F, FontStyle.Regular)

            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            grid.ColumnHeadersHeight = 40
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None
            grid.ColumnHeadersDefaultCellStyle.BackColor = If(CurrentTheme = AppThemeMode.Light, LightGridHeader, DarkGridHeader)
            grid.ColumnHeadersDefaultCellStyle.ForeColor = If(CurrentTheme = AppThemeMode.Light, Color.FromArgb(51, 65, 85), Color.FromArgb(203, 213, 225))
            grid.ColumnHeadersDefaultCellStyle.Font = New Font("Segoe UI", 9.5F, FontStyle.Bold)
            grid.ColumnHeadersDefaultCellStyle.Padding = New Padding(8, 0, 8, 0)

            grid.DefaultCellStyle.BackColor = CardBackgroundColor
            grid.DefaultCellStyle.ForeColor = TextPrimaryColor
            grid.DefaultCellStyle.SelectionBackColor = If(CurrentTheme = AppThemeMode.Light, Color.FromArgb(219, 234, 254), Color.FromArgb(30, 58, 138))
            grid.DefaultCellStyle.SelectionForeColor = If(CurrentTheme = AppThemeMode.Light, Color.FromArgb(30, 58, 138), Color.White)
            grid.DefaultCellStyle.Padding = New Padding(8, 0, 8, 0)

            grid.AlternatingRowsDefaultCellStyle.BackColor = If(CurrentTheme = AppThemeMode.Light, LightGridRowAlt, DarkGridRowAlt)
            grid.AlternatingRowsDefaultCellStyle.ForeColor = TextPrimaryColor
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = grid.DefaultCellStyle.SelectionBackColor
            grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = grid.DefaultCellStyle.SelectionForeColor
            grid.AlternatingRowsDefaultCellStyle.Padding = New Padding(8, 0, 8, 0)
        End Sub

        Public Shared Sub StyleButton(btn As Button, isPrimary As Boolean)
            btn.FlatStyle = FlatStyle.Flat
            btn.FlatAppearance.BorderSize = If(isPrimary, 0, 1)
            btn.Font = New Font("Segoe UI", 9.5F, FontStyle.Bold)
            btn.Cursor = Cursors.Hand

            If isPrimary Then
                btn.BackColor = BrandPrimary
                btn.ForeColor = Color.White
                btn.FlatAppearance.MouseOverBackColor = BrandPrimaryHover
            Else
                btn.BackColor = CardBackgroundColor
                btn.ForeColor = TextPrimaryColor
                btn.FlatAppearance.BorderColor = BorderColor
                btn.FlatAppearance.MouseOverBackColor = If(CurrentTheme = AppThemeMode.Light, Color.FromArgb(241, 245, 249), Color.FromArgb(51, 65, 85))
            End If
        End Sub
    End Class
End Namespace
