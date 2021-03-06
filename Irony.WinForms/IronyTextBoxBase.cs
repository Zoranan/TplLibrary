﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using SWFBorderStyle = System.Windows.Forms.BorderStyle;

namespace Irony.WinForms {
  /// <summary>
  /// Common ancestor for Irony.WinForms text boxes.
  /// Wraps around FastColoredTextBox, adds VisualStyle border.
  /// Allows implicit conversion to the System.Windows.Forms.TextBox.
  /// </summary>
  [ToolboxItem(false)]
  public partial class IronyTextBoxBase : UserControl, ITextBox {
    BorderStyleEx _borderStyle;

    /// <summary>
    /// Initializes a new instance of the <see cref="IronyTextBoxBase" /> class.
    /// </summary>
    public IronyTextBoxBase() {
      InitializeComponent();
      InitializeFastColoredTextBox();
      BorderStyle = BorderStyleEx.VisualStyle;
    }

    private void InitializeFastColoredTextBox() {
      FastColoredTextBox = CreateFastColoredTextBox();
      FastColoredTextBox.Dock = DockStyle.Fill;
      FastColoredTextBox.Name = "FastColoredTextBox";
      FastColoredTextBox.TextChanged += FastColoredTextBox_TextChanged;
      FastColoredTextBox.WordWrap = false;
      Controls.Add(FastColoredTextBox);
    }

    /// <remarks>Override this method to create custom descendant of FastColoredTextBox.</remarks>
    protected virtual FastColoredTextBox CreateFastColoredTextBox() {
      var textBox = new FastColoredTextBox();
      textBox.AutoScrollMinSize = new System.Drawing.Size(25, 15);
      return textBox;
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FastColoredTextBox FastColoredTextBox { get; private set; }

    /// <summary>
    /// Gets or sets the text associated with this <see cref="IronyTextBoxBase"/>.
    /// </summary>
    [DefaultValue(""), Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Localizable(true)]
    [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
    public override string Text {
      get { return FastColoredTextBox.Text; }
      set {
        if (Text != value) {
          FastColoredTextBox.ClearUndo();
          FastColoredTextBox.ClearStylesBuffer();
          FastColoredTextBox.Text = value;
          FastColoredTextBox.SelectionStart = 0;
          FastColoredTextBox.SelectionLength = 0;
          FastColoredTextBox.SetVisibleState(0, FastColoredTextBoxNS.VisibleState.Visible);
        }
      }
    }

    private void FastColoredTextBox_TextChanged(object sender, TextChangedEventArgs args) {
      OnTextChanged(args);
    }

    /// <summary>
    /// Gets or sets the border style of the control.
    /// </summary>
    [DefaultValue(BorderStyleEx.VisualStyle), Browsable(true)]
    public new BorderStyleEx BorderStyle {
      get { return _borderStyle; }
      set {
        if (_borderStyle != value) {
          _borderStyle = value;
          if (_borderStyle != BorderStyleEx.VisualStyle) {
            base.BorderStyle = (SWFBorderStyle)value;
            base.Padding = new Padding(0);
          } else {
            base.BorderStyle = SWFBorderStyle.None;
            if (Application.RenderWithVisualStyles)
              base.Padding = new Padding(1);
            else
              base.Padding = new Padding(2);
          }
          Invalidate();
        }
      }
    }

    /// <summary>
    /// Hide the inherited Padding property.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new Padding Padding {
      get { return new Padding(0); }
      set { /* ignore */ }
    }

    /// <summary>
    /// Raises the <see cref="E:Paint" /> event.
    /// </summary>
    /// <param name="args">The <see cref="PaintEventArgs" /> instance containing the event data.</param>
    protected override void OnPaint(PaintEventArgs args) {
      base.OnPaint(args);
      // paint custom control borders
      if (BorderStyle == BorderStyleEx.VisualStyle) {
        if (Application.RenderWithVisualStyles)
          ControlPaint.DrawVisualStyleBorder(args.Graphics, new Rectangle(0, 0, Width - 1, Height - 1));
        else {
          ControlPaint.DrawBorder3D(args.Graphics, new Rectangle(0, 0, Width, Height), Border3DStyle.SunkenOuter);
          ControlPaint.DrawBorder3D(args.Graphics, new Rectangle(1, 1, Width - 2, Height - 2), Border3DStyle.SunkenInner);
        }
      }
    }

    [DefaultValue(false)]
    public bool ReadOnly {
      get { return FastColoredTextBox.ReadOnly; }
      set { FastColoredTextBox.ReadOnly = value; }
    }

    /// <summary>
    /// Selects a range of text in the text box.
    /// </summary>
    /// <param name="start">The starting position.</param>
    /// <param name="length">The length of the selection.</param>
    public void Select(int start, int length) {
      FastColoredTextBox.SelectionStart = start;
      FastColoredTextBox.SelectionLength = length;
      FastColoredTextBox.DoCaretVisible();
    }

    /// <summary>
    /// Gets or sets the starting point of the text selected in the text box.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public virtual int SelectionStart {
      get { return FastColoredTextBox.SelectionStart; }
      set { FastColoredTextBox.SelectionStart = value; }
    }

    /// <summary>
    /// Gets or sets the number of characters selected in the text box.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public virtual int SelectionLength {
      get { return FastColoredTextBox.SelectionLength; }
      set { FastColoredTextBox.SelectionLength = value; }
    }

    /// <summary>
    /// Scrolls the contents of the control to the current caret position.
    /// </summary>
    public void ScrollToCaret() {
      FastColoredTextBox.DoCaretVisible();
    }
  }
}
