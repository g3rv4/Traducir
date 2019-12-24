using Traducir.Core.Helpers;

namespace Traducir.Core.Models
{
    public class SODBString
    {
        public int LCID { get; set; }

        public string Hash { get; set; }

        public string NormalizedHash => Hash.ToNormalizedKey();

        public string EffectiveTranslation { get; set; }
    }
}