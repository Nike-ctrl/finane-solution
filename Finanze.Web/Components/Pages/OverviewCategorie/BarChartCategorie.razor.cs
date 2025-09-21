using Finanza.Database.CustomModel;
using Finanza.Database.Models;
using Finanze.AccessDatabase;
using Finanze.Web.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.ComponentModel;
using System.Globalization;

namespace Finanze.Web.Components.Pages.OverviewCategorie
{
    public partial class BarChartCategorie : IDisposable
    {
        private CancellationTokenSource _cts;


        [Inject]
        public required IJSRuntime JSRuntime { get; set; }

        [Parameter]
        public ConteEDateModel Info { get; set; } = new ConteEDateModel();

        [Parameter]
        public List<SommarioCategoriaPerConto> Sommario { get; set; } = new List<SommarioCategoriaPerConto>();

        private static CultureInfo _culture = new CultureInfo("de-CH");


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

            if (!Sommario.Any())
            {
                try
                {
                    await JSRuntime.InvokeVoidAsync("purgeChart", "barchartcat");
                }
                catch (Exception e)
                {
                    //TODO Loggare correttamente l errore
                    Console.WriteLine(e.Message);

                }
                return;
            }

            var entrate = Sommario.Where(x => x.tipo_transazione_id == 1).ToList();
            var uscite = Sommario.Where(x => x.tipo_transazione_id == 2).ToList();


            //dati in base all'anno
            List<object> dateForPlotly = new List<object>();
            var data = new
            {
                y = entrate.Select(x => x.sum_valore_reale_abs),
                x = entrate.Select(x => x.categoria_nome),
                type = "bar",
                name = "Entrate",
                marker = new { color = "#2ecc71" },
                hovertemplate = "<b>%{customdata[1]}</b><br>%{customdata[0]}<extra></extra>", // sistema di formattazione del mouse sopra il punto
                customdata = entrate.Select(val => new object[] {
                        val.categoria_nome,  // Data originale
                        val.sum_valore_reale_abs.ToString("N0",  _culture) // Valore formattato
                    }).ToArray(),
            };

            dateForPlotly.Add(data);

            var data2 = new
            {
                y = uscite.Select(x => x.sum_valore_reale_abs),
                x = uscite.Select(x => x.categoria_nome),
                type = "bar",
                name = "Uscite",
                marker = new { color = "#e74c3c" },
                hovertemplate = "<b>%{customdata[1]}</b><br>%{customdata[0]}<extra></extra>", // sistema di formattazione del mouse sopra il punto
                customdata = uscite.Select(val => new object[] {
                        val.categoria_nome,  // Data originale
                        val.sum_valore_reale_abs.ToString("N0",  _culture) // Valore formattato
                    }).ToArray(),
            };

            dateForPlotly.Add(data2);

            var totEntrate = entrate.Sum(x => x.sum_valore_reale_abs);
            var totUscite = uscite.Sum(x => x.sum_valore_reale_abs);
            var diff = (totEntrate - totUscite);
            var data3 = new
            {
                y = new decimal[] { totEntrate, totUscite, diff },
                x = new string[] { "Entrate", "Uscite", "Risultato" },
                type = "bar",
                name = "Risultato ",
                marker = new { color = new string[] { "#4CAF50", "#F44336", "#2196F3" } },
                hovertemplate = "<br>%{customdata}<extra></extra>", // sistema di formattazione del mouse sopra il punto
                customdata = new decimal[] { totEntrate, totUscite, diff },
                Visible = "legendonly",
            };

            dateForPlotly.Add(data3);

            var layout = new
            {
                title = new
                {
                    text = $"Categorie [{Info.DataInzio.ToString("MM.yyyy")} - {Info.DataFine.ToString("MM.yyyy")}]"
                },
                showlegend = true,
                //barmode = "group",
                legend = new
                {
                    orientation = "h",
                    x = 0,        // 1 = bordo destro, 0 = bordo sinistro
                    y = 1,        // 1 = in alto, 0 = in basso
                    xanchor = "left",  // l’ancoraggio orizzontale della legenda
                    yanchor = "top"   // l’ancoraggio verticale della legenda
                },
                xaxis = new
                {
                    title = new
                    {
                        text = "Categorie"
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
            };


            var config = new { responsive = true, displayModeBar = false };

            try
            {
                await JSRuntime.InvokeVoidAsync("generateGenericChart", "barchartcat", dateForPlotly.ToArray(), layout, config);
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
