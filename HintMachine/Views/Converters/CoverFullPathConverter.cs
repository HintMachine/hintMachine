using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace HintMachine.Views.Converters
{
    internal class CoverFullPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string coverFileName))
            {
                throw new ArgumentException($"Argument is not of type {typeof(string)}");
            }

            string coverFullPath = Path.GetFullPath($"./Assets/covers/{coverFileName}");
            if (!File.Exists(coverFullPath))
            {
                coverFullPath = $"./Assets/covers/unknown.png";
            }

            return coverFullPath;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
