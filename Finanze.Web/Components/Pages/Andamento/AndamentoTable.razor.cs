using Finanza.Database.CustomModel;
using Finanza.Database.Models;
using Finanze.AccessDatabase;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;

namespace Finanze.Web.Components.Pages.Andamento
{
    public partial class AndamentoTable : IDisposable
    {
        private CancellationTokenSource _cts;

        [Inject]
        public required FinanzeAccessDatabaseService serviceDb { get; set; }


        [Parameter]
        public int SelectedRaggruppamento { get; set; }


        [Parameter]
        public int Year { get; set; }

        private List<ValoreCumulativoMensileConDifferenza> _values = new List<ValoreCumulativoMensileConDifferenza>();

        private static CultureInfo _culture = new CultureInfo("de-CH");
        protected override void OnInitialized()
        {
            _cts = new CancellationTokenSource();
        }


        protected override async Task OnParametersSetAsync()
        {
            _values = await serviceDb.GetValoreCumulativoConDifferenzaPrimoValoreAsync(SelectedRaggruppamento, Year, _cts.Token);
            if (!_values.Any())
            {
                return;
            }
            _values.MinBy(x => x.data).valore_mensile = 0; 
        }

        private string GetRowClass(ValoreCumulativoMensileConDifferenza item, int s)
        {
            if (item.data < DateTime.Today)
            {
                //return "riga-vecchia"; // Classe CSS personalizzata
                return string.Empty; // Classe CSS personalizzata
            }
            return "riga-nuova";
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
