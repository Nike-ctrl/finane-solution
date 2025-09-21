using Finanza.Database.Models;
using Finanze.AccessDatabase;
using Microsoft.AspNetCore.Components;

namespace Finanze.Web.Components.Pages.VisioneMensile
{
    public partial class SelezionePeriodoEConto : IDisposable
    {

        private CancellationTokenSource _cts;

        [Inject]
        public required FinanzeAccessDatabaseService serviceDb { get; set; }

        [Parameter]
        public int SelectedContoId { get; set; }

        [Parameter]
        public DateTime SelectedDatetime { get; set; }


        /// <summary>
        /// Lista dei conti disponibili in db 
        /// </summary>
        public List<raggruppamento_conto> Conti { get; set; } = new List<raggruppamento_conto>();


        /// <summary>
        /// Evento per la selezione del conto
        /// </summary>
        [Parameter]
        public EventCallback<int> OnSelectConto { get; set; }


        /// <summary>
        /// Evento per il passaggio della selezione della nuova data
        /// </summary>
        [Parameter]
        public EventCallback<DateTime> OnSelectDate { get; set; }



        /// <summary>
        /// Conto di default
        /// </summary>
        public raggruppamento_conto SelectedConto
        {
            get;
            set;
        } = new raggruppamento_conto();

        public string? SelectedContoNome
        {
            get
            {
                return SelectedConto.raggruppamento_conto_nome;
            }
            set
            {
                SelectedConto = new raggruppamento_conto() { raggruppamento_conto_nome = value };
                UpdateConto();
            }
        }


        private async Task UpdateConto()
        {
            var ris = Conti.Where(x => x.raggruppamento_conto_nome == SelectedConto.raggruppamento_conto_nome).FirstOrDefault();
            if (ris == null)
            {
                return;
            }

            await OnSelectConto.InvokeAsync(ris.raggruppamento_conto_id);
        }






        protected override void OnInitialized()
        {
            _cts = new CancellationTokenSource();
        }


        protected override async Task OnParametersSetAsync()
        {
            if (!Conti.Any())
            {
                Conti = await serviceDb.GetAllRaggrupamentoContoAsync(_cts.Token);
            }
            var tmpConto = (from x in Conti where x.raggruppamento_conto_id == SelectedContoId select x).SingleOrDefault();
            if(tmpConto is not null)
            {
                SelectedConto = tmpConto;
            }
        }


        private async Task ChangeMonth(int m)
        {
            SelectedDatetime = SelectedDatetime.AddMonths(m);
           await OnSelectDate.InvokeAsync(SelectedDatetime);

        }


        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
