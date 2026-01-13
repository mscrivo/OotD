// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Drawing;
using System.Windows.Forms;
using OotD.Utility;

namespace OotD.Forms;

/// <summary>
///     Dialog for selecting a virtual desktop to assign an instance to.
/// </summary>
public sealed class VirtualDesktopSelectionDialog : Form
{
    private readonly ListBox _desktopListBox;
    private readonly Button _okButton;
    private readonly Button _cancelButton;
    private readonly Button _clearButton;
    private readonly Label _instructionLabel;

    /// <summary>
    ///     Gets the selected desktop GUID, or null if none selected or cancelled.
    /// </summary>
    public Guid? SelectedDesktopId { get; private set; }

    /// <summary>
    ///     Gets whether the user chose to clear the desktop assignment.
    /// </summary>
    public bool ClearAssignment { get; private set; }

    public VirtualDesktopSelectionDialog()
    {
        // Enable DPI awareness
        AutoScaleMode = AutoScaleMode.Font;
        AutoScaleDimensions = new SizeF(7F, 15F);

        // Form properties
        Text = "Select Virtual Desktop";
        MinimumSize = new Size(450, 340);
        Size = new Size(450, 340);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowIcon = false;
        ShowInTaskbar = false;

        // Main layout panel
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 4,
            Padding = new Padding(12),
            AutoSize = true
        };

        // Define columns: [Clear Button] [Spacer] [OK + Cancel]
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        // Define rows: [Label] [ListBox] [Spacer] [Buttons]
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 12F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // Instruction label - spans all columns
        _instructionLabel = new Label
        {
            Text = "Select the virtual desktop where you want this instance to appear:",
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 8),
            MaximumSize = new Size(400, 0)
        };
        mainLayout.Controls.Add(_instructionLabel, 0, 0);
        mainLayout.SetColumnSpan(_instructionLabel, 3);

        // Desktop list box - spans all columns
        _desktopListBox = new ListBox
        {
            Dock = DockStyle.Fill,
            SelectionMode = SelectionMode.One,
            MinimumSize = new Size(200, 120)
        };
        _desktopListBox.DoubleClick += DesktopListBox_DoubleClick;
        mainLayout.Controls.Add(_desktopListBox, 0, 1);
        mainLayout.SetColumnSpan(_desktopListBox, 3);

        // Clear button - left side
        _clearButton = new Button
        {
            Text = "Show on All Desktops",
            AutoSize = true,
            MinimumSize = new Size(150, 30),
            Margin = new Padding(0, 0, 8, 0)
        };
        _clearButton.Click += ClearButton_Click;
        mainLayout.Controls.Add(_clearButton, 0, 3);

        // Button panel for OK and Cancel - right side
        var buttonPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Anchor = AnchorStyles.Right
        };

        // Cancel button
        _cancelButton = new Button
        {
            Text = "Cancel",
            AutoSize = true,
            MinimumSize = new Size(85, 30),
            DialogResult = DialogResult.Cancel,
            Margin = new Padding(0)
        };
        buttonPanel.Controls.Add(_cancelButton);

        // OK button
        _okButton = new Button
        {
            Text = "OK",
            AutoSize = true,
            MinimumSize = new Size(85, 30),
            DialogResult = DialogResult.OK,
            Margin = new Padding(0, 0, 8, 0)
        };
        _okButton.Click += OkButton_Click;
        buttonPanel.Controls.Add(_okButton);

        mainLayout.Controls.Add(buttonPanel, 2, 3);

        Controls.Add(mainLayout);

        AcceptButton = _okButton;
        CancelButton = _cancelButton;

        LoadVirtualDesktops();
    }

    private void LoadVirtualDesktops()
    {
        try
        {
            var desktops = VirtualDesktopManager.GetVirtualDesktops();
            var currentDesktopId = VirtualDesktopManager.GetCurrentDesktopId();

            if (desktops.Count == 0)
            {
                _instructionLabel.Text = "No virtual desktops found. Create additional desktops using Windows Task View (Win+Tab).";
                _desktopListBox.Enabled = false;
                _okButton.Enabled = false;
                return;
            }

            foreach (var desktop in desktops)
            {
                var displayName = desktop.Name;

                // Mark the current desktop
                if (currentDesktopId.HasValue && desktop.Id == currentDesktopId.Value)
                {
                    displayName += " (Current)";
                }

                _desktopListBox.Items.Add(new DesktopListItem(desktop.Id, displayName));

                // Pre-select the current desktop
                if (currentDesktopId.HasValue && desktop.Id == currentDesktopId.Value)
                {
                    _desktopListBox.SelectedIndex = _desktopListBox.Items.Count - 1;
                }
            }

            // If nothing is selected, select the first item
            if (_desktopListBox.SelectedIndex == -1 && _desktopListBox.Items.Count > 0)
            {
                _desktopListBox.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            _instructionLabel.Text = $"Error loading virtual desktops: {ex.Message}";
            _desktopListBox.Enabled = false;
            _okButton.Enabled = false;
        }
    }

    private void OkButton_Click(object? sender, EventArgs e)
    {
        if (_desktopListBox.SelectedItem is DesktopListItem selectedItem)
        {
            SelectedDesktopId = selectedItem.Id;
            ClearAssignment = false;
        }
    }

    private void ClearButton_Click(object? sender, EventArgs e)
    {
        ClearAssignment = true;
        SelectedDesktopId = null;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void DesktopListBox_DoubleClick(object? sender, EventArgs e)
    {
        if (_desktopListBox.SelectedItem != null)
        {
            OkButton_Click(sender, e);
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    /// <summary>
    ///     Item in the desktop list box.
    /// </summary>
    private sealed class DesktopListItem(Guid id, string displayName)
    {
        public Guid Id { get; } = id;
        private string DisplayName { get; } = displayName;

        public override string ToString() => DisplayName;
    }
}
