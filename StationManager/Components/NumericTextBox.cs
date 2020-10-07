using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace StationManager.Components
{
    class NumericTextBox : HintedTextBox
    {

        [Description("Minimum value of number."), Category("Data")]
        public decimal MinValue { get => (decimal)GetValue(MinValueProperty); set => SetValue(MinValueProperty, value); }
        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register("MinValue", typeof(decimal), typeof(NumericTextBox),
            new PropertyMetadata((decimal)0, new PropertyChangedCallback(PropertyChanged)));


        [Description("Maximum value of number."), Category("Data")]
        public decimal MaxValue { get => (decimal)GetValue(MaxValueProperty); set => SetValue(MaxValueProperty, value); }
        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(decimal), typeof(NumericTextBox),
            new PropertyMetadata((decimal)999999, new PropertyChangedCallback(PropertyChanged)));

        private static void PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (NumericTextBox)d;
            sender.PlaceholderText = $"{sender.MinValue}-{sender.MaxValue}";
        }

        [DefaultValue(true)]
        [Description("Is value Integer.")]
        public bool IsInteger { get => (bool)GetValue(IsIntegerProperty); set => SetValue(IsIntegerProperty, value); }
        public static readonly DependencyProperty IsIntegerProperty = DependencyProperty.Register("IsInteger", typeof(bool), typeof(NumericTextBox));

        public NumericTextBox()
        {
            this.PreviewTextInput += OnPreviewTextInput;
            this.ContentChanged += OnTextChange;
        }

        
        private void OnTextChange(object sender, EventArgs e)
        {
            var args = (ContentChangedEventArgs)e;
            if (!IsTextCorrect((string)args.NewContent))
            {
                args.Handled = true;
            }
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string str = GetEditedText(e.Text);
            e.Handled = !IsTextCorrect(str);
        }

        private bool IsTextCorrect(string text)
        {
            if (text == "-")
                return false;

            var parts = text.Split('-');
            if (parts.Length > 2)
                return false;
            foreach (var part in parts)
            {
                if (!IsStringNumberCorrect(part))
                    return false;
            }
            return true;
        }

        private bool IsStringNumberCorrect(string number)
        {
            decimal i;
            if (string.IsNullOrEmpty(number))
                return true;
            if (IsInteger)
            {
                if (decimal.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out i) && i >= MinValue && i <= MaxValue)
                    return true;
            }
            else
            {
                if (!decimal.TryParse(number, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out i))
                    return false;

                if ((int)MinValue == (int)i)
                {
                    var minValueZeroes = GetDecimalZeroes(MinValue);
                    var newDecimalZeroes = GetDecimalZeroes(i);
                    if (minValueZeroes[0] == newDecimalZeroes[0] && minValueZeroes[0] == newDecimalZeroes[1])
                        return true;
                    else if (minValueZeroes[0] > newDecimalZeroes[0])
                        return i <= MaxValue;
                }
                return i >= MinValue && i <= MaxValue;
            }
            return true;
        }
        
        private int[] GetDecimalZeroes(decimal d)
        {
            if (d.ToString().Contains(','))
            {
                string secondPart = d.ToString().Split(',')[1];
                return new int[] { CountFirst(secondPart, '0'), secondPart.Length };
            }
            return new int[] {0, 0};
        }
        
        private int CountFirst(string content, char element)
        {
            int i = 0;
            foreach (char ch in content) {
                if (ch != element)
                    break;
                i++;
            }
            return i;
        }
        private string GetEditedText(string editPart)
        {
            return Text.Substring(0, SelectionStart) + editPart + Text.Substring(SelectionStart + SelectionLength);
        }
    }
}
