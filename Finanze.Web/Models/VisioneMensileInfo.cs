namespace Finanze.Web.Models
{
    public class VisioneMensileInfo
    {
        /// <summary>
        /// Identificativo per il cookie
        /// </summary>
        public static readonly string CookieName = "vmensile";

        public int IdRaggruppamento { get; set; }

        public DateTime SelectedDate { get; set; }

    }
}
