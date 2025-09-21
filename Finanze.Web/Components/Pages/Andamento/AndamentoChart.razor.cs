using CSharpFunctionalExtensions;
using Finanza.Database.CustomModel;
using Finanza.Database.Models;
using Finanze.AccessDatabase;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor.Charts;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Finanze.Web.Components.Pages.Andamento
{
    public partial class AndamentoChart : IDisposable
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

        private List<ValoreCumulativoMensileConDifferenza> _values = new List<ValoreCumulativoMensileConDifferenza>();


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

            _values = await serviceDb.GetValoreCumulativoConDifferenzaPrimoValoreAsync(SelectedRaggruppamento, Year, _cts.Token);
            if (!_values.Any())
            {
                try
                {
                    await JSRuntime.InvokeVoidAsync("purgeChart", "andamentoplot");
                    await JSRuntime.InvokeVoidAsync("purgeChart", "andamentodiffplot");
                    await JSRuntime.InvokeVoidAsync("purgeChart", "andamentoeffettivo");
                    
                }
                catch (Exception e)
                {
                    //TODO Loggare correttamente l errore
                    Console.WriteLine(e.Message);

                }
                return;
            }

            await GenerateChartAndamento();
            await GenerateChartDiff();
            await GenerateChartAndamentoEffettivo();
        }

        private async Task GenerateChartDiff()
        {
            var oggi = DateTime.Now;
            var valoriPrimaDiOggi = from x in _values where x.anno < oggi.Year || (x.anno == oggi.Year && x.mese <= oggi.Month) select x;
            var valoriDopoOggi = from x in _values where x.anno > oggi.Year || (x.anno == oggi.Year && x.mese >= oggi.Month) select x;
            //var valoriPrimaDiOggi = from x in _values where x.data <= oggi select x;
            //var valoriDopoOggi = from x in _values where x.data >= oggi select x;


            //dati in base all'anno
            List<object> dateForPlotly = new List<object>();
            var data = new
            {
                x = valoriPrimaDiOggi.Select(x => x.data),
                y = valoriPrimaDiOggi.Select(x => x.differenza_col_primo),
                hovertemplate = "<b>%{customdata[1]}</b><br>%{customdata[0]}<extra></extra>", // sistema di formattazione del mouse sopra il punto
                customdata = valoriPrimaDiOggi.Select(val => new object[] {
                        val.data.ToString("MM.yyyy"),  // Data originale
                        val.differenza_col_primo.ToString("N2",  _culture) // Valore formattato
                    }).ToArray(),
                name = "Valori effettivi differenziale",
                Type = "scatter",
                line = new { shape = "spline" },
                mode = "lines+markers"
            };
            dateForPlotly.Add(data);

            var data2 = new
            {
                x = valoriDopoOggi.Select(x => x.data),
                y = valoriDopoOggi.Select(x => x.differenza_col_primo),
                hovertemplate = "<b>%{customdata[1]}</b><br>%{customdata[0]}<extra></extra>", // sistema di formattazione del mouse sopra il punto
                customdata = valoriDopoOggi.Select(val => new object[] {
                        val.data.ToString("MM.yyyy"),  // Data originale
                        val.differenza_col_primo.ToString("N2",  _culture) // Valore formattato
                    }).ToArray(),
                name = "Valori previsionali differenziale",
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
                title = new
                {
                   text = $"Andamento Differenziale <br>[Stato mese in corso : {Math.Round(valoriPrimaDiOggi.MaxBy(x => x.data).differenza_col_primo, 0)} CHF]",
                },
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


        private async Task GenerateChartAndamento()
        {
            var oggi = DateTime.Now;
            var valoriPrimaDiOggi = from x in _values where x.anno < oggi.Year || (x.anno == oggi.Year && x.mese <= oggi.Month) select x;
            var valoriDopoOggi = from x in _values where x.anno > oggi.Year || (x.anno == oggi.Year && x.mese >= oggi.Month) select x;

            //var valoriPrimaDiOggi = from x in _values where x.data <= oggi select x;
            //var valoriDopoOggi = from x in _values where x.data >= oggi select x;
            //dati in base all'anno
            List<object> dateForPlotly = new List<object>();
            var data = new
            {
                x = valoriPrimaDiOggi.Select(x => x.data),
                y = valoriPrimaDiOggi.Select(x => x.valore_cumulativo),
                hovertemplate = "<b>%{customdata[1]}</b><br>%{customdata[0]}<extra></extra>", // sistema di formattazione del mouse sopra il punto
                customdata = valoriPrimaDiOggi.Select(val => new object[] {
                        val.data.ToString("MM.yyyy"),  // Data originale
                        val.valore_cumulativo.ToString("N2",  _culture) // Valore formattato
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
                        val.data.ToString("MM.yyyy"),  // Data originale
                        val.valore_cumulativo.ToString("N2",  _culture) // Valore formattato
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
                title = new
                {
                    text = $"Andamento <br>[Stato mese in corso : {Math.Round(valoriPrimaDiOggi.MaxBy(x => x.data).valore_cumulativo, 0)} CHF]"
                },
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
            };


            var config = new { responsive = true, displayModeBar = false };

            try
            {
                await JSRuntime.InvokeVoidAsync("generateGenericChart", "andamentoplot", dateForPlotly.ToArray(), layout, config);
            }
            catch (Exception e)
            {
                //TODO Loggare correttamente l errore
                Console.WriteLine(e.Message);
            }
        }


        private async Task GenerateChartAndamentoEffettivo()
        {
            var oggi = DateTime.Now;
            var valoriPrimaDiOggi = from x in _values where x.anno < oggi.Year || (x.anno == oggi.Year && x.mese <= oggi.Month) select x;
            var valoriDopoOggi = from x in _values where x.anno > oggi.Year || (x.anno == oggi.Year && x.mese > oggi.Month) select x;

            valoriPrimaDiOggi.MinBy(x => x.data).valore_mensile = 0;

            //var valoriPrimaDiOggi = from x in _values where x.data <= oggi select x;
            //var valoriDopoOggi = from x in _values where x.data >= oggi select x;
            //dati in base all'anno
            List<object> dateForPlotly = new List<object>();
            var data = new
            {
                x = valoriPrimaDiOggi.Select(x => x.data),
                y = valoriPrimaDiOggi.Select(x => x.valore_mensile),
                hovertemplate = "<b>%{customdata[1]}</b><br>%{customdata[0]}<extra></extra>", // sistema di formattazione del mouse sopra il punto
                customdata = valoriPrimaDiOggi.Select(val => new object[] {
                        val.data.ToString("MM.yyyy"),  // Data originale
                        val.valore_mensile.ToString("N2",  _culture) // Valore formattato
                    }).ToArray(),
                name = "Valori effettivi",
                Type = "bar",
                line = new { shape = "spline" },

            };
            dateForPlotly.Add(data);

            var data2 = new
            {
                x = valoriDopoOggi.Select(x => x.data),
                y = valoriDopoOggi.Select(x => x.valore_mensile),
                hovertemplate = "<b>%{customdata[1]}</b><br>%{customdata[0]}<extra></extra>", // sistema di formattazione del mouse sopra il punto
                customdata = valoriDopoOggi.Select(val => new object[] {
                        val.data.ToString("MM.yyyy"),  // Data originale
                        val.valore_mensile.ToString("N2",  _culture) // Valore formattato
                    }).ToArray(),
                name = "Valori previsionali",
                Type = "bar",
                marker = new { color = "#e8255c" },
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
                title = new
                {
                    text = $"Versamento effettivo mensile <br>[Stato mese in corso : {Math.Round(valoriPrimaDiOggi.Sum(x => x.valore_mensile), 0)} CHF]"
                },
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
            };


            var config = new { responsive = true, displayModeBar = false };

            try
            {
                await JSRuntime.InvokeVoidAsync("generateGenericChart", "andamentoeffettivo", dateForPlotly.ToArray(), layout, config);
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
