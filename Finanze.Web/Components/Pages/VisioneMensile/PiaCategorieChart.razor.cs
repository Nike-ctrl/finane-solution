using Finanza.Database.CustomModel;
using Finanza.Database.Models;
using Finanze.AccessDatabase;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Drawing;

namespace Finanze.Web.Components.Pages.VisioneMensile
{
    public partial class PiaCategorieChart : IDisposable
    {

        private CancellationTokenSource _cts;

        [Inject]
        public required FinanzeAccessDatabaseService serviceDb { get; set; }

        [Inject]
        public required IJSRuntime JSRuntime { get; set; }

        [Parameter]
        public List<SommarioCategoria> Values { get; set; }

        //[Parameter]
        //public int SelectedRaggruppamentoContoId { get; set; }

        //[Parameter]
        //public DateTime SelectedDatetime { get; set; }

        [Parameter]
        public bool Entrate { get; set; }

        public string DivName { get {

                return Entrate ? "plot-pie-entrate" : "plot-pie-uscite";
            }
        }



        //public List<SommarioCategoria> Values { get; set; } = new List<SommarioCategoria>();

        protected override void OnInitialized()
        {
            _cts = new CancellationTokenSource();
        }

        protected async override Task OnParametersSetAsync()
        {


            //Values = Entrate ? await serviceDb.GetViewSommaCategoriaEntrateAsync(SelectedRaggruppamentoContoId, SelectedDatetime) : await serviceDb.GetViewSommaCategoriaUsciteAsync(SelectedRaggruppamentoContoId, SelectedDatetime);

            if (!Values.Any())
            {
                try
                {
                    await JSRuntime.InvokeVoidAsync("purgeChart", DivName);
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
            if (Entrate)
            {
                var colors = GenerateGreenScale(Values.Count());
                var data = new
                {
                    values = Values.Select(x => x.sum_valore),
                    labels = Values.Select(x => x.categoria_nome == null ? "No cat" : x.categoria_nome),
                    type = "pie",
                    marker = new {
                        colors = colors
                    }
            };

                dateForPlotly.Add(data);

            }
            else
            {
                var data = new
                {
                    values = Values.Select(x => x.sum_valore),
                    labels = Values.Select(x => x.categoria_nome == null ? "No cat" : x.categoria_nome),
                    type = "pie"
                };

                dateForPlotly.Add(data);
            }


            //var data2 = new
            //{
            //    values = Values.Select(x => x.sum_valore),
            //    labels = Values.Select(x => x.categoria_nome == null ? "no cat" : x.categoria_nome),
            //    type = "pie"
            //};

            //dateForPlotly.Add(data2);

            var layout = new
            {
                title = new
                {
                    text = Entrate ? "Entrate" : "Uscite"
                },
                showlegend = true,
            };

            var config = new { responsive = true, displayModeBar = false };

            try
            {
                await JSRuntime.InvokeVoidAsync("generateGenericChart", DivName, dateForPlotly.ToArray(), layout, config);
            }
            catch (Exception e)
            {
                //TODO Loggare correttamente l errore
                Console.WriteLine(e.Message);

            }

        }

        //protected override async Task OnAfterRenderAsync(bool firstRender)
        //{

        //    Values = Entrate ?  await serviceDb.GetViewSommaCategoriaEntrateAsync(SelectedRaggruppamentoContoId, SelectedDatetime) : await serviceDb.GetViewSommaCategoriaUsciteAsync(SelectedRaggruppamentoContoId, SelectedDatetime);

        //    if (!Values.Any())
        //    {
        //        try
        //        {
        //            await JSRuntime.InvokeVoidAsync("purgeChart", DivName);
        //        }
        //        catch (Exception e)
        //        {
        //            //TODO Loggare correttamente l errore
        //            Console.WriteLine(e.Message);

        //        }

        //        return;
        //    }

        //    //dati in base all'anno
        //    List<object> dateForPlotly = new List<object>();

        //    var data = new
        //    {
        //        values = Values.Select(x => x.sum_valore),
        //        labels = Values.Select(x => x.categoria_nome == null ? "no cat" : x.categoria_nome),
        //        type = "pie"
        //    };

        //    dateForPlotly.Add(data);

        //    //var data2 = new
        //    //{
        //    //    values = Values.Select(x => x.sum_valore),
        //    //    labels = Values.Select(x => x.categoria_nome == null ? "no cat" : x.categoria_nome),
        //    //    type = "pie"
        //    //};

        //    //dateForPlotly.Add(data2);

        //    var layout = new
        //    {
        //        title = Entrate ? "Entrate" : "Uscite",
        //        showlegend = true,
        //    };

        //    var config = new { responsive = true, displayModeBar = false };

        //    try
        //    {
        //        await JSRuntime.InvokeVoidAsync("generateGenericChart", DivName, dateForPlotly.ToArray(), layout, config);
        //    }
        //    catch (Exception e)
        //    {
        //        //TODO Loggare correttamente l errore
        //        Console.WriteLine(e.Message);

        //    }

        //    await base.OnAfterRenderAsync(firstRender);

        //}


        public static List<string> GenerateGreenScale(int count)
        {
            var colors = new List<string>();

            for (int i = 0; i < count; i++)
            {
                // Varia da un verde scuro (#004400) a un verde chiaro (#88FF88)
                float t = (float)i / Math.Max(count - 1, 1);

                // Interpolazione lineare tra due valori RGB
                int r = (int)(0 + t * (136 - 0));   // da 0 a 136
                int g = (int)(68 + t * (255 - 68));  // da 68 a 255
                int b = (int)(0 + t * (136 - 0));   // da 0 a 136

                System.Drawing.Color color = System.Drawing.Color.FromArgb(r, g, b);
                colors.Add($"#{color.R:X2}{color.G:X2}{color.B:X2}");
            }

            return colors;
        }



        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

    }
}
