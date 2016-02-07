﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace Anathema
{
    /// <summary>
    /// A textbox that allows for hex and dec values to be stored, validated, and colored. Extends WatermarkTextBox.
    /// </summary>
    public class HexDecTextBox : WatermarkTextBox
    {
        protected Boolean _IsHex;
        public Boolean IsHex
        {
            get { return this._IsHex; }
            set
            {
                this._IsHex = value;
                UpdateValidity();
            }
        }

        private Boolean _TextValid;
        private Boolean TextValid
        {
            get { return this._TextValid; }
            set
            {
                this._TextValid = value;
                UpdateColor();
            }
        }

        private Type ElementType { get; set; }

        private MenuItem DecimalMenuItem;
        private MenuItem HexidecimalMenuItem;
        private MenuItem ConvertToDecMenuItem;
        private MenuItem ConvertToHexMenuItem;
        private ContextMenu RightClickMenu;

        public HexDecTextBox()
        {
            DecimalMenuItem = new MenuItem("Decimal");
            HexidecimalMenuItem = new MenuItem("Hexidecimal");
            ConvertToDecMenuItem = new MenuItem("Convert to Dec");
            ConvertToHexMenuItem = new MenuItem("Convert to Hex");

            RightClickMenu = new ContextMenu(new MenuItem[] { DecimalMenuItem, HexidecimalMenuItem, new MenuItem("-"), ConvertToDecMenuItem, ConvertToHexMenuItem });

            RightClickMenu.Popup += RightClickMenu_Popup;
            DecimalMenuItem.Click += DecimalMenuItem_Click;
            HexidecimalMenuItem.Click += HexidecimalMenuItem_Click;

            ConvertToDecMenuItem.Click += ConvertToDecMenuItem_Click;
            ConvertToHexMenuItem.Click += ConvertToHexMenuItem_Click;

            this.ContextMenu = RightClickMenu;
            this.TextChanged += HexDecTextChanged;

            this.ElementType = typeof(UInt64);
        }

        public void SetElementType(Type ElementType)
        {
            this.ElementType = ElementType;
            UpdateValidity();
        }

        public String GetValueAsDecimal()
        {
            if (!TextValid)
                return null;

            if (IsHex)
                return Conversions.ParseValue(ElementType, Conversions.ParseHexAsValue(ElementType, this.Text)).ToString();
            else
                return Conversions.ParseValue(ElementType, this.Text).ToString();
        }

        public String GetValueAsHexidecimal()
        {
            if (!TextValid)
                return null;

            if (IsHex)
                return Conversions.ParseValue(ElementType, Conversions.ParseHexAsValue(ElementType, this.Text)).ToString("X");
            else
                return Conversions.ParseValueAsHex(ElementType, Conversions.ParseValue(ElementType, this.Text).ToString());
        }

        public Boolean IsValid()
        {
            return TextValid;
        }
        
        private void HexidecimalMenuItem_Click(Object Sender, EventArgs E)
        {
            this.IsHex = true;
        }

        private void DecimalMenuItem_Click(Object Sender, EventArgs E)
        {
            this.IsHex = false;
        }

        private void ConvertToHexMenuItem_Click(Object Sender, EventArgs E)
        {
            if (CheckSyntax.CanParseValue(ElementType, this.Text))
                this.Text = Conversions.ParseValueAsHex(ElementType, this.Text);

            this.IsHex = true;
        }

        private void ConvertToDecMenuItem_Click(Object Sender, EventArgs E)
        {
            if (CheckSyntax.CanParseHex(ElementType, this.Text))
                this.Text = Conversions.ParseHexAsValue(ElementType, this.Text).ToString();

            this.IsHex = false;
        }
        
        private void RightClickMenu_Popup(Object Sender, EventArgs E)
        {
            DecimalMenuItem.Checked = IsHex ? false : true;
            HexidecimalMenuItem.Checked = IsHex ? true : false;
        }

        private void HexDecTextChanged(Object Sender, EventArgs E)
        {
            UpdateValidity();
        }

        private void UpdateValidity()
        {
            if (IsHex)
            {
                if ((CheckSyntax.CanParseHex(ElementType, this.Text)))
                {
                    TextValid = true;
                    return;
                }
            }
            else
            {
                if (CheckSyntax.CanParseValue(ElementType, this.Text))
                {
                    TextValid = true;
                    return;
                }
            }

            TextValid = false;
            return;
        }

        private void UpdateColor()
        {
            if (!TextValid)
                this.ForeColor = Color.Red;
            else if (IsHex)
                this.ForeColor = Color.Green;
            else
                this.ForeColor = SystemColors.ControlText;

            this.Invalidate();
        }

    } // End class

} // End namespace