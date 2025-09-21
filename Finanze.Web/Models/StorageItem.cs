namespace Finanze.Web.Models
{
    /// <summary>
    /// Classe di riferimento per il salvataggio delle info nel localstorage
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StorageItem<T>
    {
        /// <summary>
        /// Dati
        /// </summary>
        public required T Data { get; set; }


        /// <summary>
        /// Data di scadenza
        /// </summary>
        public DateTime Expiry { get; set; }

    }
}
