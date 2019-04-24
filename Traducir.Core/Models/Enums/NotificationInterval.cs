using System.ComponentModel.DataAnnotations;

namespace Traducir.Core.Models.Enums
{
    public enum NotificationInterval
    {
        [Display(Name="minutes", Order = 2)]
        Minutes = 1,

        [Display(Name = "hours", Order = 1)]
        Hours = 60,

        [Display(Name = "days", Order = 0)]
        Days = 1440
    }
}