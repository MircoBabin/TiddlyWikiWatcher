using System;
using System.Drawing;
using System.Windows.Forms;

namespace TiddlyWikiWatcher
{
    public class CustomDialogBox
    {
        public enum Result { Button1, Button2, Button3 }
        public static Result Show(string title, string promptText, 
            Result cancelButton,
            string button1, string button2 = null, string button3 = null)
        {
            const int labelMarginX = 9;
            const int buttonMarginX = 6;
            const int buttonMarginY = 6;

            // Form
            Form form = new Form();
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.Text = title;
            form.ClientSize = new Size(400, 200);

            // Label
            Label label = new Label();
            form.Controls.Add(label);
            label.SetBounds(labelMarginX, 20, form.ClientSize.Width - labelMarginX - labelMarginX, 1);
            label.AutoSize = true;
            label.Text = promptText;

            // Button 1
            Button button_1 = new Button();
            form.Controls.Add(button_1);
            button_1.AutoSize = true;
            button_1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            button_1.TextAlign = ContentAlignment.MiddleLeft;
            button_1.Text = button1;

            // Button 2
            Button button_2 = null;
            if (button2 != null)
            {
                button_2 = new Button();
                form.Controls.Add(button_2);
                button_2.AutoSize = true;
                button_2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                button_2.TextAlign = ContentAlignment.MiddleLeft;
                button_2.Text = button2;
            }

            // Button 3
            Button button_3 = null;
            if (button3 != null) 
            {
                button_3 = new Button();
                form.Controls.Add(button_3);
                button_3.AutoSize = true;
                button_3.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                button_3.TextAlign = ContentAlignment.MiddleLeft;
                button_3.Text = button3;
            }

            // Position buttons
            int buttonY = label.Top + label.Height + buttonMarginY;

            form.ClientSize = new Size(label.Width + labelMarginX, buttonY + button_1.Height + buttonMarginY);

            int buttonX = form.ClientSize.Width - buttonMarginX;
            if (button_3 != null)
            {
                buttonX -= button_3.Width;
                button_3.Location = new Point(buttonX, buttonY);
                buttonX -= buttonMarginX;
            }

            if (button_2 != null)
            {
                buttonX -= button_2.Width;
                button_2.Location = new Point(buttonX, buttonY);
                buttonX -= buttonMarginX;
            }

            buttonX -= button_1.Width;
            button_1.Location = new Point(buttonX, buttonY);

            // Show form
            switch (cancelButton)
            {
                case Result.Button1:
                    form.CancelButton = button_1;
                    button_1.TabIndex = 0;
                    if (button_2 != null) button_2.TabIndex = 1;
                    if (button_3 != null) button_3.TabIndex = 2;
                    break;

                case Result.Button2:
                    if (button_2 == null) throw new Exception("Button2 is not defined, it can't be the cancelButton.");
                    form.CancelButton = button_2;
                    button_2.TabIndex = 0;
                    if (button_3 != null) button_3.TabIndex = 1;
                    button_1.TabIndex = 2;
                    break;

                case Result.Button3:
                    if (button_3 == null) throw new Exception("Button3 is not defined, it can't be the cancelButton.");
                    form.CancelButton = button_3;
                    button_3.TabIndex = 0;
                    button_1.TabIndex = 1;
                    if (button_2 != null) button_2.TabIndex = 2;
                    break;

                default:
                    throw new Exception("Unknown cancelButton: " + cancelButton);
            }

            button_1.DialogResult = DialogResult.OK;
            if (button_2 != null) button_2.DialogResult = DialogResult.No;
            if (button_3 != null) button_3.DialogResult = DialogResult.Yes;

            DialogResult dialogResult = form.ShowDialog();

            switch (dialogResult)
            {
                case DialogResult.OK:
                    return Result.Button1;

                case DialogResult.No:
                    return Result.Button2;

                case DialogResult.Yes:
                    return Result.Button3;
            }

            return cancelButton; // Form closed, no button was choosen.
        }
    }
}
