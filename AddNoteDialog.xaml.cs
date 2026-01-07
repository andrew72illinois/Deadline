using System;
using System.Windows;

namespace Deadline
{
    public partial class AddNoteDialog : Window
    {
        public string NoteContent { get; private set; } = string.Empty;
        public double? ProgressAmount { get; private set; }

        public AddNoteDialog()
        {
            InitializeComponent();
            NoteContentTextBox.Focus();
        }

        private void NoteContentTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            OkButton.IsEnabled = !string.IsNullOrWhiteSpace(NoteContentTextBox.Text);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            NoteContent = NoteContentTextBox.Text.Trim();
            
            if (double.TryParse(ProgressAmountTextBox.Text, out var amount))
            {
                ProgressAmount = amount;
            }
            else
            {
                ProgressAmount = null;
            }
            
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

