﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using WpfApp1.Properties;

namespace WpfApp1 {
    public sealed class EnumToStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) { return null; }

            return Resources.ResourceManager.GetString(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            string str = (string)value;

            foreach (object enumValue in Enum.GetValues(targetType)) {
                if (str == Resources.ResourceManager.GetString(enumValue.ToString(), Resources.Culture)) { return enumValue; }
            }

            throw new ArgumentException(null, "value");
        }
    }

    public sealed class EnumerateExtension : MarkupExtension {
        public Type Type { get; set; }

        public EnumerateExtension(Type type) {
            this.Type = type;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            string[] names = Enum.GetNames(Type);
            string[] values = new string[names.Length];

            for (int i = 0; i < names.Length; i++) {
                values[i] = Resources.ResourceManager.GetString(names[i], Resources.Culture);
            }

            return values;
        }
    }

    public sealed class ColorHelp {
        public static Dictionary<string, Color> Colors = new Dictionary<string, Color>();
        static ColorHelp() {
            var prs = typeof(Colors).GetProperties();
            foreach (var p in prs) {
                Colors.Add(p.Name, (Color)p.GetValue(null, null));
            }
        }

        public static Color GetColor(string color) {
            if (Colors.ContainsKey(color))
                return Colors[color];
            var c = Color.FromArgb(
                Convert.ToByte(color.Substring(1, 2), 16),
                Convert.ToByte(color.Substring(3, 2), 16),
                Convert.ToByte(color.Substring(5, 2), 16),
                Convert.ToByte(color.Substring(7, 2), 16));

            Colors.Add(color, c);

            return c;
        }
    }
}
