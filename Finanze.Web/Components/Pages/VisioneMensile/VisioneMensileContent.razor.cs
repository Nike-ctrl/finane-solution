using CSharpFunctionalExtensions;
using Finanza.Database.CustomModel;
using Finanza.Database.Models;
using Finanze.AccessDatabase;
using Finanze.Web.Components.Pages.DialogGestioneInserimenti;
using Finanze.Web.Components.Pages.GestioneTransazioni;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Globalization;
using System.Text;
using static MudBlazor.CategoryTypes;

namespace Finanze.Web.Components.Pages.VisioneMensile
{
    public partial class VisioneMensileContent : IDisposable
    {

        private CancellationTokenSource _cts;

        [Inject]
        public required FinanzeAccessDatabaseService serviceDb { get; set; }

        [Inject]
        public required IJSRuntime JSRuntime { get; set; }

        [Inject]
        public required IDialogService Dialog { get; set; }


        [Parameter]
        public int SelectedRaggruppamentoContoId { get; set; }

        [Parameter]
        public DateTime SelectedDatetime { get; set; }


        public List<SommarioCategoria> EntrateSomma { get; set; } = new List<SommarioCategoria>();

        public List<SommarioCategoria> UsciteSomma { get; set; } = new List<SommarioCategoria>();


        


        /// <summary>
        /// Lista di tutte le transazioni 
        /// </summary>
        private List<view_sommario_transazioni> transazioni { get; set; } = new List<view_sommario_transazioni>();


        /// <summary>
        /// Elementi selezionati dalla tabella 
        /// </summary>
        private HashSet<view_sommario_transazioni> selectedItems = new HashSet<view_sommario_transazioni>();



        /// <summary>
        /// Testo che indica la somma dei valori reali degli elementi selezionati 
        /// </summary>
        private string SumSelectedItem { get {
                if (!selectedItems.Any())
                {
                    return "Somma elementi selezionati 0 CHF"; 
                }

                return $"Somma elementi selezionati {(decimal)selectedItems.Sum(x => x.valore_reale)} CHF";
            
            }
        }


        /// <summary>
        /// totale delle entrate
        /// </summary>
        public decimal Entrate { get { return transazioni.Where(x => x.tipo_transazione_id == 1).Sum(x => (decimal)x.valore); } }



        /// <summary>
        /// totale delle uscite
        /// </summary>
        public decimal Uscite { get { return transazioni.Where(x => x.tipo_transazione_id == 2).Sum(x => (decimal)x.valore); } }


        /// <summary>
        /// diff tra entrate e uscite
        /// </summary>
        public decimal Diff
        {
            get
            {
                return Entrate - Uscite;
            }
        }

        public bool DatiPresenti { get; set; } = true;

        /// <summary>
        /// Colore per indicare se è un problema 
        /// </summary>
        public string ColorDiff
        {
            get
            {
                return Diff >= 0 ? "#4CAF50": "#F44336";
            }
        }

        protected override void OnInitialized()
        {
            _cts = new CancellationTokenSource();
        }

        protected override async Task OnParametersSetAsync()
        {
            await GetTransactionAsync();

            await base.OnParametersSetAsync();
        }

        private async Task GetTransactionAsync(bool forcestate = false)
        {
            transazioni = await serviceDb.GetViewSommarioTransazioniAsync(SelectedRaggruppamentoContoId, SelectedDatetime, _cts.Token);
            if (!transazioni.Any())
            {
                DatiPresenti = false;
                //Entrate = 0;
                //Uscite = 0;
                EntrateSomma = new List<SommarioCategoria>();
                UsciteSomma = new List<SommarioCategoria>(); 
                return;
            }
            EntrateSomma = await serviceDb.GetViewSommaCategoriaEntrateAsync(SelectedRaggruppamentoContoId, SelectedDatetime);
            UsciteSomma = await serviceDb.GetViewSommaCategoriaUsciteAsync(SelectedRaggruppamentoContoId, SelectedDatetime);
            //Entrate = transazioni.Where(x => x.tipo_transazione_id == 1).Sum(x => (decimal)x.valore);
            //Uscite = transazioni.Where(x => x.tipo_transazione_id == 2).Sum(x => (decimal)x.valore);
            DatiPresenti = true;
            if (forcestate)
            {
                StateHasChanged();
            }
        }


        /// <summary>
        /// Metodo per scaricare i dati 
        /// </summary>
        /// <returns></returns>
        async Task DonwlaodAll()
        {
            if (!transazioni.Any())
            {
                return;
            }
            var fileName = $"transazioni_mese_{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss", CultureInfo.InvariantCulture)}.csv";
            Stream fileStream = GetStream(transazioni);
            using var streamRef = new DotNetStreamReference(stream: fileStream);

            await JSRuntime.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);

        }


        /// <summary>
        /// Metodo che apre il dialog delle note 
        /// </summary>
        /// <returns></returns>
        private async Task OpenNote()
        {

            DialogOptions options = new() { MaxWidth = MaxWidth.Medium, FullWidth = true, NoHeader = true, BackgroundClass = "blur-all" };
            DialogParameters param = new DialogParameters();
            param.Add("Period", SelectedDatetime);

            var dialog = await Dialog.ShowAsync<NoteDialogComponent>("", parameters: param, options);
            var result = await dialog.GetReturnValueAsync<bool?>();

        }


        /// <summary>
        /// Al click dell elemento della tabella, esso viene aggiunto alla lista che si occupa di sommare i valori 
        /// </summary>
        /// <param name="args"></param>
        void OnRowClick(TableRowClickEventArgs<view_sommario_transazioni> args)
        {
            if(args.Item == null)
            {
                return;
            }
            selectedItems.Add(args.Item);
           // TODO : aggiungere la somma 
        }


        private Stream GetStream(IEnumerable<view_sommario_transazioni> value)
        {
            StringBuilder sb = new StringBuilder();
            string sep = ";";
            sb.AppendLine($"ID{sep}Data{sep}Conto{sep}Direzione{sep}Categoria{sep}Valore{sep}Descrizione{sep}");

            foreach (var item in value)
            {
                sb.Append($"{item.transazione_id};");
                sb.Append($"{item.data?.ToString("dd.MM.yyyy")};");
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




        async Task OpenDialog()
        {
            DialogOptions options = new() { MaxWidth = MaxWidth.Medium, FullWidth = true, NoHeader = true, BackgroundClass = "blur-all" };
            var dialog = await Dialog.ShowAsync<InsertDialog>("", options);
            var result = await dialog.GetReturnValueAsync<bool?>();
            if (result == null)
            {
                return;
            }
            if (result.Value)
            {
                // Ricarica la pagina forzando il reload completo
                await GetTransactionAsync(true);
            }

        }

        public async Task EditContentAsync(int? transazione_id)
        {
            if(transazione_id == null)
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
            if (result.Value)
            {
                // Ricarica la pagina forzando il reload completo
                await GetTransactionAsync(true);
                //NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
            }



        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
