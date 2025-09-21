using CSharpFunctionalExtensions;
using Finanze.AccessDatabase;
using Finanze.Web.Models;
using Finanze.Web.Services;
using Microsoft.AspNetCore.Components;
using Telegram.Bot.Types;

namespace Finanze.Web.Components.Pages.VisioneMensile
{
    public partial class VisioneMensileMainPage : IDisposable
    {

        private CancellationTokenSource _cts;
        [Inject]
        public required FinanzeAccessDatabaseService serviceDb { get; set; }

        [Inject]
        public required StorageService LocalStorageService { get; set; }



        /// <summary>
        /// Selezione del primo conto, default 1
        /// </summary>
        public int IdRaggruppamentoConto { get; set; }


        /// <summary>
        /// Data di selezione del periodo, default oggi
        /// </summary>
        public DateTime DataSelezionePeriodo { get; set; }


        protected override void OnInitialized()
        {
            //IdRaggruppamentoConto = 1;
            //DataSelezionePeriodo = DateTime.Now.Date;
            _cts = new CancellationTokenSource();
        }

        protected override async Task OnParametersSetAsync()
        {
            Result<VisioneMensileInfo> resultLast = await LocalStorageService.GetItemAsyncWithExpiry<VisioneMensileInfo>(VisioneMensileInfo.CookieName);
            if (resultLast.IsFailure )
            {
                IdRaggruppamentoConto = 1;
                DataSelezionePeriodo = DateTime.Now.Date;
                return;
            }
            IdRaggruppamentoConto = resultLast.Value.IdRaggruppamento;
            DataSelezionePeriodo = resultLast.Value.SelectedDate;
        }


        /// <summary>
        /// Metodo per la gestione della presa del raggruppamento dei conti
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task HandleUpdateIdConto(int id)
        {

            IdRaggruppamentoConto = id;
            await SetLocalSotrage();

        }



        /// <summary>
        /// Metodo per il passaggio del id dell'analisi selezionata
        /// </summary>
        /// <param name="id"></param>
        private async Task HandleSelectedDatetime(DateTime dt)
        {
            DataSelezionePeriodo = dt;
            await SetLocalSotrage();
        }

        async Task SetLocalSotrage()
        {

            await LocalStorageService.SetItemAsyncWithExpiry<VisioneMensileInfo>(
                VisioneMensileInfo.CookieName,
                new VisioneMensileInfo()
                {
                    IdRaggruppamento = IdRaggruppamentoConto,
                    SelectedDate = DataSelezionePeriodo
                });

        }


        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

    }
}
