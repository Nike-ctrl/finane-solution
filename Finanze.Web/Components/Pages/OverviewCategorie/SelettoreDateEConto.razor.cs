using Finanza.Database.Models;
using Finanze.AccessDatabase;
using Finanze.Web.Models;
using Microsoft.AspNetCore.Components;

namespace Finanze.Web.Components.Pages.OverviewCategorie
{
    public partial class SelettoreDateEConto : IDisposable
    {
        private CancellationTokenSource _cts;

        [Inject]
        public required FinanzeAccessDatabaseService serviceDb { get; set; }

        [Parameter]
        public ConteEDateModel Info { get; set; }


        [Parameter]
        public EventCallback<ConteEDateModel> OnSelectInfo { get; set; }


        public List<conto> Conti { get; set; } = new List<conto>();

        public conto SelectedConto
        {
            get;
            set;
        } = new conto();

        public string? SelectedContoNome
        {
            get
            {
                return SelectedConto.conto_nome;
            }
            set
            {
                SelectedConto = new conto() { conto_nome = value };
                Info.ContoId = (from x in Conti where x.conto_nome == value select x.conto_id).First();
                Update();
            }
        }


        private DateTime? _dateStart { get; set; }

        private DateTime? _dateEnd { get; set; }


        protected override void OnInitialized()
        {
            _cts = new CancellationTokenSource();
        }

        protected override async Task OnParametersSetAsync()
        {
            if (!Conti.Any())
            {
                Conti = await serviceDb.GetAllContoAsync();
            }
            SelectedConto = (from x in Conti where x.conto_id == Info.ContoId  select x).First();
            _dateEnd = Info.DataFine;
            _dateStart = Info.DataInzio;

            await base.OnParametersSetAsync();
        }


        private async Task SetStart()
        {
            if(_dateStart == null)
            {
                return;

            }
            Info.DataInzio = _dateStart.Value;
            await Update();

        }

        private async Task SetEnd()
        {
            if (_dateEnd == null)
            {
                return;

            }
            Info.DataFine = _dateEnd.Value;
            await Update();

        }

        async Task Update()
        {
            await OnSelectInfo.InvokeAsync(Info);

        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

    }
}
