using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Chatty.Maui.ChatModel;

namespace Chatty.Maui.Converters
{
    public class SideColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is User userType && userType == User.Receiver)
                return Color.Parse("#005c4b");
            else
                return Color.Parse("#353535");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
