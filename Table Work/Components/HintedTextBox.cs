using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StationManager.Components
{
    public class HintedTextBox : TextBox
    {
        private Brush defaultBrush;
        
        private string _placeHolderText;

        private bool PlaceholderEnabled {
            get => defaultBrush != Foreground;
            set
            {
                if (value)
                {
                    this.Foreground = new SolidColorBrush(Colors.Gray);
                    base.Text = PlaceholderText;
                }
                else 
                {
                    this.Foreground = defaultBrush;
                    base.Text = (string)GetValue(ContentProperty);   
                }
            }
        }

        public string PlaceholderText
        {
            get => _placeHolderText;
            set
            {
                _placeHolderText = value;
                PlaceholderEnabled = this.PlaceholderEnabled || (!this.IsFocused && String.IsNullOrEmpty(this.Text) && !String.IsNullOrEmpty(PlaceholderText));
            }
        }

        public new string Text
        {
            get => (string)GetValue(ContentProperty);
            set {
                if (Text != value)
                    SetValue(ContentProperty, value);
            } 
        }

        public event EventHandler<EventArgs> ContentChanged;

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Text",
            typeof(string),
            typeof(HintedTextBox),
            new PropertyMetadata("", new PropertyChangedCallback(PropertyChanged)));

        private static void PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var htb = (HintedTextBox)d;
            var args = new ContentChangedEventArgs(e.NewValue, e.OldValue);
            htb.ContentChanged?.Invoke(htb, args);
            if (!args.Handled)
                ((TextBox)d).Text = (string)e.NewValue;
            else
                htb.Text = (string)args.PrevContent;
        }

        public HintedTextBox()
        {
            
            defaultBrush = this.Foreground;
            this.TextChanged += (s, e) => {
                string text = base.Text;
                if (e.UndoAction == UndoAction.Undo && text == PlaceholderText)
                    base.Text = text = "";

                if (PlaceholderEnabled && String.IsNullOrEmpty(text))
                    e.Handled = false;
                else
                {
                    if (text != PlaceholderText && PlaceholderEnabled)
                        PlaceholderEnabled = false;

                    if (!PlaceholderEnabled)
                    {
                        Text = text;
                    }
                }
            };

            this.GotFocus += (object sender, RoutedEventArgs e) =>
            {
                if (PlaceholderEnabled)
                {
                    Console.WriteLine($"Text before focus: {Text}, {base.Text}");
                    PlaceholderEnabled = false;
                }
            };

            this.LostFocus += (Object sender, RoutedEventArgs e) => {
                if (String.IsNullOrEmpty(this.Text))
                {
                    PlaceholderEnabled = true;
                }
            };
        }
    }

    public class ContentChangedEventArgs : EventArgs
    {
        public object NewContent { get; }
        public object PrevContent { get; }

        [DefaultValue(false)]
        public bool Handled { get; set; }

        public ContentChangedEventArgs(object newContent, object prevContent)
        {
            NewContent = newContent;
            PrevContent = prevContent;
        }
    }
}
