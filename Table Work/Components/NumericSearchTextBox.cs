using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;

namespace StationManager.Components
{
    class NumericSearchTextBox : HintedTextBox
    {
        [DefaultValue(true)]
        public bool IsInteger { get; set; }

        public bool AllOccurrences { get => (bool)GetValue(AllOccurrencesProperty); set => SetValue(AllOccurrencesProperty, value); }

        public static readonly DependencyProperty AllOccurrencesProperty = DependencyProperty.Register("AllOccurrences", typeof(bool), typeof(NumericSearchTextBox), new PropertyMetadata(true));

        public NumericSearchTextBox()
        {
            this.PreviewTextInput += OnPreviewTextInput;
        }

        public string GetSQLQuery(string name)
        {
            string nullQuery = $"(({name}.end is null or {name}.end = 0) and {name}.start = {{0}})";
            string noneNullQuery = $"({name}.end is not null and {name}.end != 0 and {name}.start <= {{0}} and {name}.end >= {{1}})";
            var queryList = new List<string>();
            foreach(var part in Text.Split(';'))
            {
                var numbers = part.Split('-');
                if (numbers.Length == 1)
                {
                    decimal value;
                    if (decimal.TryParse(numbers[0].Replace(" ", ""), IsInteger ? NumberStyles.Integer : NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value)) {
                        string string_value = value.ToString().Replace(",", ".");
                        queryList.Add("(" + string.Format(nullQuery, string_value) + " OR " + string.Format(noneNullQuery, string_value, string_value) + ")");
                    }
                } 
                else if (numbers.Length == 2)
                {
                    var values = new decimal[2];
                    if (decimal.TryParse(numbers[0].Replace(" ", ""), IsInteger ? NumberStyles.Integer : NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out values[0]) &&
                        decimal.TryParse(numbers[1].Replace(" ", ""), IsInteger ? NumberStyles.Integer : NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out values[1]))
                    {
                        queryList.Add(string.Format(noneNullQuery, values[0].ToString().Replace(",","."), values[1].ToString().Replace(",", ".")));
                    }
                }
            }
            if (AllOccurrences)
            {
                var newQueryList = queryList.Select((content) => $"SELECT StationID FROM {name} WHERE {content} GROUP BY StationID");
                return $"{string.Join(" INTERSECT  ", newQueryList)}";
            }
            return $"SELECT StationID FROM {name} WHERE {string.Join(" OR ", queryList)} GROUP BY StationID";
        }

        private void OnPreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (e.Text.Length == 0)
                return;
            var tb = (HintedTextBox)sender;
            if (IsInteger && e.Text == ".")
            {
                e.Handled = true;
            }
            else
                e.Handled = !new Regex(@"[0-9.\-;]+", RegexOptions.IgnoreCase).IsMatch(e.Text) || ".;-".Any(x => x == e.Text[0] && ((tb.Text.Length == 0) || (tb.Text.Length > 0 && ".;-".Any(y => y == tb.Text[tb.Text.Length - 1]))));
        }
    }
}
