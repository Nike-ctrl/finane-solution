using Finanza.Database.Models;
using Finanze.AccessDatabase;
using Finanze.Web.Components.Pages.DialogGestioneInserimenti;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Globalization;
using System.Net.Http;
using System.Text;
using static MudBlazor.CategoryTypes;

namespace Finanze.Web.Components.Pages.LastInsert
{
    public partial class TransazioniTable
    {
        [Inject]
        public required FinanzeAccessDatabaseService serviceDb { get; set; }

        [Inject]
        public required IDialogService Dialog { get; set; }

        [Inject]
        public required IJSRuntime JSRuntime { get; set; }

        private int? lastId = null; 

        private IEnumerable<view_transazione_completa> pagedData;
        private MudTable<view_transazione_completa> table;

        private int totalItems = 0;
        private string searchString = null;

        private bool _firtLoad = true;

        private async Task<TableData<view_transazione_completa>> ServerReload(TableState state, CancellationToken token)
        {

            if (state.Page == 0)
            {
                var data = await serviceDb.FirstPageAllTransactionAsync(state.PageSize, token);
                //lastId = data.Min(x => x.transazione_id);
                var valueMax = totalItems == 0 ? data.Max(x => x.transazione_id) : totalItems;
                totalItems = valueMax.Value;
                _firtLoad = false;
                return new TableData<view_transazione_completa>() { Items = data, TotalItems = totalItems };
            }

            var data2 = await serviceDb.OtherPageAllTransactionAsync((totalItems - ( state.Page * state.PageSize)), state.PageSize, token);
            //lastId = data2.Min(x => x.transazione_id);

            return new TableData<view_transazione_completa>() { Items = data2, TotalItems = totalItems };

        }


        public async Task EditContentAsync(int? transazione_id)
        {
            if (transazione_id == null)
            {
                return;
            }

            var parameters = new DialogParameters<EditDialog>
                {
                    { x => x.IdTransazione, (int)transazione_id}
                };

            DialogOptions options = new() { MaxWidth = MaxWidth.Medium, FullWidth = true, NoHeader = true, BackgroundClass = "blur-all" };
            var dialog = await Dialog.ShowAsync<EditDialog>("", parameters, options);
            var result = await dialog.GetReturnValueAsync<bool?>();
            if (result == null)
            {
                return;
            }
          await table.ReloadServerData();



        }

        private async Task DonwlaodAll()
        {
            List<view_transazione_completa> plotModels = new();
            //scelta se prendere istrom dati o controparte dati
            plotModels = await serviceDb.GetAllTransactionAsync();
            if (!plotModels.Any())
            {
                return;
            }

            var fileName = $"transazioni_{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss", CultureInfo.InvariantCulture)}.csv";
            Stream fileStream = GetStream(plotModels);
            using var streamRef = new DotNetStreamReference(stream: fileStream);

            await JSRuntime.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
        }



        private Stream GetStream(List<view_transazione_completa> value)
        {
            StringBuilder sb = new StringBuilder();
            string sep = ";";
            sb.AppendLine($"ID{sep}Data{sep}Creazione{sep}Conto{sep}Direzione{sep}Categoria{sep}Valore{sep}Descrizione{sep}");

            foreach (var item in value)
            {
                sb.Append($"{item.transazione_id};");
                sb.Append($"{item.data?.ToString("dd.MM.yyyy")};");
                sb.Append($"{item.creazione?.ToString("dd.MM.yyyy")};");
                sb.Append($"{item.conto_nome};");
                sb.Append($"{item.tipo_transazione_nome};");
                sb.Append($"{item.categoria_nome};");
                sb.Append($"{item.valore_reale};");
                sb.Append($"{item.descrizione};");
                sb.AppendLine();
            }


            var utf8Bom = Encoding.UTF8.GetPreamble();
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());

            var result = new byte[utf8Bom.Length + bytes.Length];
            Buffer.BlockCopy(utf8Bom, 0, result, 0, utf8Bom.Length);
            Buffer.BlockCopy(bytes, 0, result, utf8Bom.Length, bytes.Length);

            return new MemoryStream(result);

            //return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(sb.ToString()));
        }

        private void OnSearch(string text)
        {
            searchString = text;
            table.ReloadServerData();
        }
    }
}
