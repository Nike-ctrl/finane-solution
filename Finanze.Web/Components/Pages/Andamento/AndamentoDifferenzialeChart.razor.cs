using Finanza.Database.Models;
using Finanze.AccessDatabase;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;

namespace Finanze.Web.Components.Pages.Andamento
{
    public partial class AndamentoDifferenzialeChart : IDisposable
    {
        private CancellationTokenSource _cts;

        [Inject]
        public required FinanzeAccessDatabaseService serviceDb { get; set; }


        [Inject]
        public required IJSRuntime JSRuntime { get; set; }

        [Parameter]
        public int SelectedRaggruppamento { get; set; }


        [Parameter]
        public int Year { get; set; }

        private static CultureInfo _culture = new CultureInfo("de-CH");

        private List<view_valore_cumulativo_mensile> _values = new List<view_valore_cumulativo_mensile>();


        protected override void OnInitialized()
        {
            _cts = new CancellationTokenSource();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            // Annulla e ricrea il token
            _cts.Cancel();
            _cts.Dispose();
            _cts = new CancellationTokenSource();

            _values = await serviceDb.GetValoreCumulativoAsync(SelectedRaggruppamento, Year, _cts.Token);
            if (!_values.Any())
            {
                try
                {
                    await JSRuntime.InvokeVoidAsync("purgeChart", "andamentodiffplot");
                }
                catch (Exception e)
                {
                    //TODO Loggare correttamente l errore
                    Console.WriteLine(e.Message);

                }
                return;
            }
            var oggi = DateTime.Now;
            var valoriPrimaDiOggi = from x in _values where x.anno < oggi.Year || (x.anno == oggi.Year && x.mese <= oggi.Month) select x;
            var valoriDopoOggi = from x in _values where x.anno > oggi.Year || (x.anno == oggi.Year && x.mese >= oggi.Month) select x;


            //dati in base all'anno
            List<object> dateForPlotly = new List<object>();
            var data = new
            {
                x = valoriPrimaDiOggi.Select(x => x.data),
                y = valoriPrimaDiOggi.Select(x => x.valore_cumulativo),
                hovertemplate = "<b>%{customdata[1]}</b><br>%{customdata[0]}<extra></extra>", // sistema di formattazione del mouse sopra il punto
                customdata = valoriPrimaDiOggi.Select(val => new object[] {
                        val.data?.ToString("MM.yyyy"),  // Data originale
                        val.valore_cumulativo?.ToString("N2",  _culture) // Valore formattato
                    }).ToArray(),
                name = "Valori effettivi",
                Type = "scatter",
                line = new { shape = "spline" },
                mode = "lines+markers"
            };
            dateForPlotly.Add(data);

            var data2 = new
            {
                x = valoriDopoOggi.Select(x => x.data),
                y = valoriDopoOggi.Select(x => x.valore_cumulativo),
                hovertemplate = "<b>%{customdata[1]}</b><br>%{customdata[0]}<extra></extra>", // sistema di formattazione del mouse sopra il punto
                customdata = valoriDopoOggi.Select(val => new object[] {
                        val.data?.ToString("MM.yyyy"),  // Data originale
                        val.valore_cumulativo?.ToString("N2",  _culture) // Valore formattato
                    }).ToArray(),
                name = "Valori previsionali",
                Type = "scatter",
                mode = "lines+markers",
                line = new { color = "#e8255c", shape = "spline" },
                //mode = "lines",
                //line = new
                //{
                //    dash = "dot",
                //    width = 4
                //}
            };
            dateForPlotly.Add(data2);
            var layout = new
            {
                title = $"Andamento <br>[Stato mese in corso : {Math.Round(valoriPrimaDiOggi.Sum(x => (decimal)x.valore_mensile), 0)} CHF]",
                showlegend = true,
                legend = new
                {
                    orientation = "h",
                },
                xaxis = new
                {
                    title = new
                    {
                        text = "Periodo"
                    }
                },
                yaxis = new
                {
                    title = new
                    {
                        text = "CHF"
                    }
                },
                displayModeBar = false,
                //margin = new
                //{
                //    l = 100,
                //    r = 40,
                //    b = 40,
                //    t = 50,
                //    pad = 3
                //}
            };


            var config = new { responsive = true, displayModeBar = false };

            try
            {
                await JSRuntime.InvokeVoidAsync("generateGenericChart", "andamentodiffplot", dateForPlotly.ToArray(), layout, config);
            }
            catch (Exception e)
            {
                //TODO Loggare correttamente l errore
                Console.WriteLine(e.Message);
            }

        }



        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
