using CSharp_Interactive_Learning_App.Models;
using System.Globalization;

namespace CSharp_Interactive_Learning_App.Convertors
{
    public class VictoryConvertor : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is BattleResult result)
                return result == BattleResult.Victory;

            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
