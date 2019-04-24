using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Traducir.Core.Models;

namespace Traducir.Web.Net.ViewModels.Users
{
    public class NotificationsViewModel
    {
        public NotificationSettings NotificationSettings { get; set; }

        public string VapidPublic { get; set; }

        public NotificationProperty[] NotificationProperties(int group)
        {
            var minOrder = group == 0 ? 0 : 4;
            var maxOrder = minOrder + 3;
            return typeof(NotificationSettings)
                .GetProperties()
                .Select(p =>
                {
                    var attribute = p.GetCustomAttributes<DisplayAttribute>().SingleOrDefault();
                    if(attribute == null || attribute.Order < minOrder || attribute.Order > maxOrder)
                    {
                        return null;
                    }
                    return new NotificationProperty
                    {
                        DataName = char.ToLower(p.Name[0]) + p.Name.Substring(1),
                        DisplayName = attribute.Name,
                        Value = (bool)p.GetValue(NotificationSettings, null),
                        Order = attribute.Order
                    };
                })
                .Where(n => n != null)
                .OrderBy(n => n.Order)
                .ToArray();
        }

        public string ClassFor(bool value) =>
            $"notification-type list-group-item {(value ? "active" : null)}";

        public class NotificationProperty
        {
            public string DataName { get; set; }

            public string DisplayName { get; set; }

            public bool Value { get; set; }

            public int Order { get; set; }
        }
    }
}
