using CSharpFunctionalExtensions;
using Finanza.Database.CustomModel;
using Finanza.Database.Models;
using Finanze.AccessDatabase;

namespace Finanze.Web.Models
{
    public class TrapassoViewModel
    {

        /// <summary>
        /// Indica se è inizializzato
        /// </summary>
        public bool IsInitialized { get; private set; }


        /// <summary>
        /// Lista di tutti i conti disponibili
        /// </summary>
        public List<conto> conti { get; set; } = [];


        /// <summary>
        /// Lista di tutte le categorie
        /// </summary>
        public List<categoria> categorie { get; set; } = [];

        /// <summary>
        /// conto sorgente
        /// </summary>
        private conto _contoSorgente = new();
        
        /// <summary>
        /// Conto destinazione
        /// </summary>
        private conto _contoDestinazione = new();



        public string? SelectedContoSorgente
        {
            get => _contoSorgente.conto_nome;
            set
            {
                var match = conti.FirstOrDefault(x => x.conto_nome == value);
                if (match != null) _contoSorgente = match;
            }
        }

        public string? SelectedContoDestinazione
        {
            get => _contoDestinazione.conto_nome;
            set
            {
                var match = conti.FirstOrDefault(x => x.conto_nome == value);
                if (match != null) _contoDestinazione = match;
            }
        }

        /// <summary>
        /// Indica un problema sui conti, non possono essere uguali
        /// </summary>
        public bool StessiContiProblema => _contoDestinazione.conto_id == _contoSorgente.conto_id;


        private categoria _categoria = new();

        public string? SelectedCategoria
        {
            get => _categoria.categoria_nome;
            set
            {
                var match = categorie.FirstOrDefault(x => x.categoria_nome == value);
                if (match != null) _categoria = match;
            }
        }


        //private tipo_transazione _tipo = new();

        //public string? SelectedTipo
        //{
        //    get => _tipo.tipo_transazione_nome;
        //    set
        //    {
        //        var match = tipologie.FirstOrDefault(x => x.tipo_transazione_nome == value);
        //        if (match != null) _tipo = match;
        //    }
        //}


        public string Descrizione { get; set; } = string.Empty;
         
        public decimal Value { get; set; }


        public void Initialize(List<conto> Inconti, List<categoria> cat)
        {
            conti = Inconti ?? throw new ArgumentNullException(nameof(Inconti));
            categorie = cat ?? throw new ArgumentNullException(nameof(cat));
            _contoSorgente = new();
            _contoDestinazione = new();
            IsInitialized = true;
        }


        public void Clear()
        {
            _contoDestinazione = new();
            _contoSorgente = new();
            _categoria = new();
            Descrizione = string.Empty;
            Value = 0;
        }

        public Result<TrapassoModel[]> ToModelArray(List<DateTime> dt)
        {
            if (!dt.Any())
            {
                return Result.Failure<TrapassoModel[]>("Selezionare almeno una data");
            }

            Result ris = CheckValues();
            if (ris.IsFailure)
            {
                return Result.Failure<TrapassoModel[]>(ris.Error);
            }
            var arrayValue = (from x in dt select new TrapassoModel()
            {
                DestContoId = _contoDestinazione.conto_id,
                ScrContoId = _contoSorgente.conto_id,
                CategoriaId = _categoria.categoria_id == 0 ? null : _categoria.categoria_id,
                Descrizione = Descrizione,
                Valore = Value,
                Dt = x,
                
                
            }).ToArray();

            return Result.Success<TrapassoModel[]>(arrayValue);
        }

        public static async Task<TrapassoViewModel> CreateAsync(FinanzeAccessDatabaseService db, CancellationToken token = default)
        {
            var conti = await db.GetAllContoAsync(token);
            var cat = await db.GetAllCategorieAsync(token);
            cat.Add(new categoria() { categoria_id = 0, categoria_nome = "NULL" }); // aggiunta per null
            var vm = new TrapassoViewModel();
            vm.Initialize(conti,cat);
            return vm;
        }

        /// <summary>
        /// Metodo che controlla che i valori siano corretti
        /// </summary>
        /// <returns></returns>
        private Result CheckValues()
        {
            if(SelectedContoSorgente == null)
            {
                return Result.Failure("Selezionare il conto sorgente");
            }


            if (SelectedContoDestinazione == null)
            {
                return Result.Failure("Selezionare il conto destinazione");
            }

            if (StessiContiProblema)
            {
                return Result.Failure("Il trapasso non puo essere fatto fra conti uguali");
            }
            return Result.Success();

        }

        internal async Task ReloadCategorie(FinanzeAccessDatabaseService db, CancellationToken token)
        {
            var cat = await db.GetAllCategorieAsync(token);
            cat.Add(new categoria() { categoria_id = 0, categoria_nome = "NULL" }); // aggiunta per null
            categorie = cat;
        }

        //internal static async Task<TrapassoViewModel> CreateAndSetAsync(FinanzeAccessDatabaseService db CancellationToken token)
        //{

        //    var conti = await db.GetAllContoAsync(token);
        //    var cat = await db.GetAllCategorieAsync(token);
        //    cat.Add(new categoria() { categoria_id = 0, categoria_nome = "NULL" }); // aggiunta per null
        //    var vm = new TrapassoViewModel();
        //    vm.Initialize(conti, cat);
        //    return vm;
        //}
    }
}
