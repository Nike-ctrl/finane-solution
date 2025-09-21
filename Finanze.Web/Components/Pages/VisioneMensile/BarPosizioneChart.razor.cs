using Finanza.Database.Models;
using Finanze.AccessDatabase;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Finanze.Web.Components.Pages.VisioneMensile
{
    public partial class BarPosizioneChart : IDisposable
    {

        private CancellationTokenSource _cts;


        [Inject]
        public required IJSRuntime JSRuntime { get; set; }




        [Parameter]
        public decimal Entrate { get; set; }

        [Parameter]
        public decimal Uscite { get; set; }

        [Parameter]
        public decimal Risultato { get; set; }

        [Parameter]
        public bool PresentData { get; set; }

        protected override void OnInitialized()
        {
            _cts = new CancellationTokenSource();
        }


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!PresentData)
            {
                try
                {
                    await JSRuntime.InvokeVoidAsync("purgeChart", "plotbar");
                }
                catch (Exception e)
                {
                    //TODO Loggare correttamente l errore
                    Console.WriteLine(e.Message);

                }

                return;
            }

            //dati in base all'anno
            List<object> dateForPlotly = new List<object>();

            var coloreRisultato = Risultato < 0 ? "#F44336" : "#2196F3";
            var data = new
            {
                y = new decimal[] {Entrate,Uscite,Risultato},
                x = new string[] { "Entrate", "Uscite", "Risultato" },
                type = "bar",
                marker = new
                {
                    color = new string[] { "#4CAF50", "#F44336", coloreRisultato } // Verde, Rosso, Blu
                }
            };



            dateForPlotly.Add(data);
            var layout = new
            {
                title = new
                {
                    text = $"Distribuzione"
                },
                showlegend = false,
            };

            var config = new { responsive = true, displayModeBar = false };

            try
            {
                await JSRuntime.InvokeVoidAsync("generateGenericChart", "plotbar", dateForPlotly.ToArray(), layout, config);
            }
            catch (Exception e)
            {
                //TODO Loggare correttamente l errore
                Console.WriteLine(e.Message);

            }

            await base.OnAfterRenderAsync(firstRender);

        }




        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
