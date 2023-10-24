using System;
using System.Globalization;
using System.Windows.Data;
using System.Linq;
using System.Collections.Generic;

namespace HintMachine.Views.Converters
{
    internal class GamePropertiesListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is List<string> stringList))
                throw new ArgumentException($"Argument is not of type {typeof(List<string>)}");

            if(stringList.Count == 1)
                return stringList.First();

            string fullText = string.Empty;
            foreach (string item in stringList)
                fullText += $"\n  • {item}";
            return fullText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
