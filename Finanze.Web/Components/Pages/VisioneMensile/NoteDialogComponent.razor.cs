using CSharpFunctionalExtensions;
using Finanza.Database.Models;
using Finanze.AccessDatabase;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Finanze.Web.Components.Pages.VisioneMensile
{
    public partial class NoteDialogComponent : IDisposable
    {

        private CancellationTokenSource _cts;
        [Inject]
        public required FinanzeAccessDatabaseService serviceDb { get; set; }


        /// <summary>
        /// Periodo selezionato dall utente
        /// </summary>
        [Parameter]
        public required DateTime Period { get; set; }


        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }


        /// <summary>
        /// Lista di tutte le note trovate nel db
        /// </summary>
        public List<nota> AllNote { get; set; } = new List<nota>();


        private nota nuovaNota = new nota();




        private bool showAddNew = false;

        private bool isError = false;

        private string errorMessage = string.Empty;


        protected override void OnInitialized()
        {
            _cts = new CancellationTokenSource();
            InitNota();
        }

        protected override async Task OnParametersSetAsync()
        {
            AllNote = await serviceDb.GetAllNoteForAPeriodAsync(Period, _cts.Token);
        }


        /// <summary>
        /// Metodo che cambia la variabile per mostrare la parte di aggiunta nota e resetta il testo della nota
        /// </summary>
        void ShowAddNote()
        {
            nuovaNota.testo_nota = string.Empty;
            showAddNew = !showAddNew;

        }


        async Task InsertNote()
        {
            isError = false;
            errorMessage = string.Empty;

            if (string.IsNullOrEmpty(nuovaNota.testo_nota))
            {
                errorMessage = "Inserire il testo della nota";
                isError = true;
                return;
            }

            Result ris = await serviceDb.AddNoteAsync(nuovaNota, _cts.Token);

            if (ris.IsFailure)
            {
                errorMessage = ris.Error;
                isError = true;
                return;
            }

            InitNota();
            AllNote = await serviceDb.GetAllNoteForAPeriodAsync(Period, _cts.Token);

        }

        private void InitNota()
        {
            nuovaNota = new nota();
            nuovaNota.mese = Period.Month;
            nuovaNota.anno = Period.Year;
        }


        async Task DeleteNoteById(int id)
        {

            await serviceDb.DeleteNoteByIdAsync(id, _cts.Token);
            AllNote = await serviceDb.GetAllNoteForAPeriodAsync(Period, _cts.Token);

        }

        private void Cancel() => MudDialog.Cancel();

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
