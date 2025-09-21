using Finanza.Database.CustomModel;
using Finanza.Database.Models;
using Finanze.AccessDatabase;
using Finanze.Web.Models;
using Microsoft.AspNetCore.Components;

namespace Finanze.Web.Components.Pages.OverviewCategorie
{
    public partial class SommarioCategorieContent : IDisposable
    {
        private CancellationTokenSource _cts;

        [Inject]
        public required FinanzeAccessDatabaseService serviceDb { get; set; }

        [Parameter]
        public ConteEDateModel Info { get; set; }

        public List<SommarioCategoriaPerConto> Sommario { get; set; } = new List<SommarioCategoriaPerConto>();
        protected override void OnInitialized()
        {
            _cts = new CancellationTokenSource();
        }

        protected override async Task OnParametersSetAsync()
        {
            Sommario = await serviceDb.GetSommaCategoriaByIdContoAsync(Info.ContoId, Info.DataInzio, Info.DataFine);
            await base.OnParametersSetAsync();
        }
        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
