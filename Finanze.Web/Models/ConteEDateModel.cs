namespace Finanze.Web.Models
{


    /// <summary>
    /// Modello per la comunicazione fra componente selettore e visualizzazione grafici
    /// </summary>
    public class ConteEDateModel
    {
        public int ContoId { get; set; }

        public DateTime DataInzio { get; set; }


        public DateTime DataFine { get; set; }

    }
}
