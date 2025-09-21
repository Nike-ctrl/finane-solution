using CSharpFunctionalExtensions;
using Finanza.Database.CustomModel;
using Finanza.Database.Models;
using Finanze.AccessDatabase;
using Finanze.Web.Components.Pages.DialogGestioneInserimenti;
using Finanze.Web.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Finanze.Web.Components.Pages.GestioneTransazioni
{
    public partial class GestioneTransazioneContent : IDisposable
    {

        private CancellationTokenSource _cts;

        [Inject]
        public required FinanzeAccessDatabaseService serviceDb { get; set; }

        [Inject]
        public required IDialogService Dialog { get; set; }


        [Parameter]
        public bool Trapasso { get; set; } = false;

        [Parameter]
        public EventCallback<int> OnEndInsert { get; set; }


        /// <summary>
        /// Indica che la transazione va editata invece che aggiunta
        /// </summary>
        [Parameter]
        public int IdTransazione { get; set; } = 0;



        /// <summary>
        /// Date selezionate
        /// </summary>
        public List<DateTime> SelectedDate { get; set; } = new List<DateTime>();

        /// <summary>
        /// Indica che è possibile mostrare solo la selezione di una data singola
        /// </summary>
        public bool OnlySingleDate { get; set; } = false;


        public DateTime StartDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Blocca il cambio della tipologia in caso che la transazione da modificare è vincolata
        /// </summary>
        public bool BlockType { get; set; } = false;


        public List<view_sommario_transazioni> LinkedTransaction { get; set; } = new List<view_sommario_transazioni>();

        /// <summary>
        /// Oggetto di nuovo trapasso
        /// </summary>
        public TrapassoViewModel NuovoTrapasso { get; set; } = new TrapassoViewModel();

        /// <summary>
        /// Oggetto per mostrare errore o successo operazione 
        /// </summary>
        public InfoActionDBModel Info { get; set; } = new InfoActionDBModel();
        public TransazioneModelView NuovaTransazione { get; set; } = new TransazioneModelView();


        protected override void OnInitialized()
        {
            _cts = new CancellationTokenSource();
        }

        protected override async Task OnParametersSetAsync()
        {
            NuovoTrapasso = await TrapassoViewModel.CreateAsync(serviceDb, _cts.Token);
            NuovaTransazione = await TransazioneModelView.CreateAsync(serviceDb, _cts.Token); // TODO: aggiungere la parte di id da passare per modifica

            //var conti = await serviceDb.GetAllContoAsync();
            if (IdTransazione > 0)
            {


                Result<transazione> resulttransazioneInfo = await serviceDb.GetTransazioneByIdAsync(IdTransazione, _cts.Token);
                if (resulttransazioneInfo.IsFailure)
                {
                    return;
                }
                transazione transDB = resulttransazioneInfo.Value;
                NuovaTransazione.InitializeById(transDB);

                LinkedTransaction = await serviceDb.GetAllTransactionLinkedByTrapassoId(IdTransazione, _cts.Token);
                BlockType = LinkedTransaction.Any();
                OnlySingleDate = true;
                SelectedDate.Add(transDB.data);
                StartDate = transDB.data;
                // assegnare data

            }
            await base.OnParametersSetAsync();
        }



        private async Task InsertTrapasso()
        {
            Info.ShowMessage = false;

            Result<TrapassoModel[]> ris = NuovoTrapasso.ToModelArray(SelectedDate);
            if (ris.IsFailure)
            {
                Info.SetError(ris.Error);
                return;
            }

            Result resultDb = await serviceDb.InsertTrapassiMultipliAsync(ris.Value, _cts.Token);
            if (resultDb.IsFailure)
            {
                Info.SetError(ris.Error);
                return;
            }
            Info.SetSuccess("Inseriti");
            NuovoTrapasso.Clear();
            
            await OnEndInsert.InvokeAsync(1);

        }

        private async Task DeleteTransazione()
        {
            if (NuovaTransazione.IdDb < 1)
            {
                return;
            }
                var parameters = new DialogParameters<ConfermaEliminazioneDialog>
        {
            { x => x.ContentText, "Vuoi davvero cancellare questa transazione ? " },
            { x => x.ButtonText, "Delete" },
            { x => x.Color, Color.Error }
        };

            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall, BackgroundClass = "blur-all" };

            var dialog = await Dialog.ShowAsync<ConfermaEliminazioneDialog>("Delete", parameters, options);

            var result = await dialog.GetReturnValueAsync<bool?>();
            if (result == null)
            {
                return;
            }
            if (result.Value)
            {

               await serviceDb.DeleteTransazioniByIdAsync(NuovaTransazione.IdDb, _cts.Token);

            }

            await OnEndInsert.InvokeAsync(1);

        }

        private async Task AddCategorie()
        {
            DialogOptions options = new() { MaxWidth = MaxWidth.Medium, FullWidth = true, NoHeader = true, BackgroundClass = "blur-all" };
            var dialog = await Dialog.ShowAsync<AddCategorieDialog>("", options);
            var result = await dialog.GetReturnValueAsync<bool?>();

            await NuovoTrapasso.ReloadCategorie(serviceDb, _cts.Token);
            await NuovaTransazione.ReloadCategorie(serviceDb, _cts.Token);


        }


        private async Task EditTransazione()
        {
            Info.ShowMessage = false;

            //EDIT
            if (NuovaTransazione.IdDb > 0)
            {
                if (LinkedTransaction.Any())
                {

                    ///Qui si controlla che l altra transazione vincolata non abbia lo stesso conto id
                    var valoreUgualeTipo = (from x in LinkedTransaction where x.transazione_id != NuovaTransazione.IdDb && x.conto_id == NuovaTransazione.conto_id select x).Any();
                    if (valoreUgualeTipo)
                    {
                        Info.SetError("I conti non possono essere gli stessi");
                        return;
                    }


                }



                Result<transazione[]> ris = NuovaTransazione.ToModelArray(SelectedDate);
                if (ris.IsFailure)
                {
                    Info.SetError(ris.Error);
                    return;
                }

                var diversoDaUno = !(ris.Value.Count() == 1);
                if (diversoDaUno)
                {
                    Info.SetError("Puo esserci un solo valore da inserire");
                    return;
                }

                Result resultDb = await serviceDb.UpdateTransazioneAsync(ris.Value[0], _cts.Token);
                if (resultDb.IsFailure)
                {
                    Info.SetError(ris.Error);
                    return;
                }
                Info.SetSuccess("Aggionrato");


            }
            else
            {
                //INSERT
                Info.SetError("Non è presente l id della transazione");
                return;

            }



            NuovoTrapasso.Clear();
            await OnEndInsert.InvokeAsync(1);

        }


        private async Task InsertTransazione()
        {

            Info.ShowMessage = false;

            Result<transazione[]> ris = NuovaTransazione.ToModelArray(SelectedDate);
            if (ris.IsFailure)
            {
                Info.SetError(ris.Error);
                return;
            }

            Result resultDb = await serviceDb.InsertTransazioniMultipliAsync(ris.Value, _cts.Token);
            if (resultDb.IsFailure)
            {
                Info.SetError(ris.Error);
                return;
            }
            Info.SetSuccess("Inseriti");
            NuovoTrapasso.Clear();
            await OnEndInsert.InvokeAsync(1);
        }





        public void HandleListDates(List<DateTime> dates)
        {
            SelectedDate = dates;
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

    }
}
