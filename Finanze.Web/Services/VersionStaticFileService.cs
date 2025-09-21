namespace Finanze.Web.Services
{
    /// <summary>
    /// Problema : nel momento in cui si pubblica una nuova versione i vari browser mantengono i file statici precedenti
    /// con js e css basta aggiungere ?version= cambiando il numero della versione per forzare il caricamento del nuovo file
    /// Questa classe si occupa semplicemente di questo
    /// Viene inizializzato il servizio come Singleton cosi da durare per tutto il tempo di vita dell'app
    /// </summary>
    public class VersionStaticFileService
    {

        /// <summary>
        /// Versione generata
        /// </summary>
        private string SiteVersionGiud;


        public VersionStaticFileService()
        {
            SiteVersionGiud = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Viene chiamato nell app.razor
        /// </summary>
        /// <returns></returns>
        public string GetVersion()
        {
            return SiteVersionGiud;
        }

    }
}
