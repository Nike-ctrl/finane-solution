using CSharpFunctionalExtensions;
using Finanza.Database.CustomModel;
using Finanza.Database.Models;
using Finanze.AccessDatabase;

namespace Finanze.Web.Models
{
    public class TransazioneModelView
    {

        public int IdDb { get; set; } = 0;

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


        public List<tipo_transazione> tipologie { get; set; } = [];


        /// <summary>
        /// conto sorgente
        /// </summary>
        private conto _contoSorgente = new();


        private categoria _categoria = new();

        private tipo_transazione _tipo = new();

        public int conto_id { get { return _contoSorgente.conto_id; } }


        public string? SelectedContoSorgente
        {
            get => _contoSorgente.conto_nome;
            set
            {
                var match = conti.FirstOrDefault(x => x.conto_nome == value);
                if (match != null) _contoSorgente = match;
            }
        }




        public string? SelectedCategoria
        {
            get => _categoria.categoria_nome;
            set
            {
                var match = categorie.FirstOrDefault(x => x.categoria_nome == value);
                if (match != null) _categoria = match;
            }
        }

        public string? SelectedTipo
        {
            get => _tipo.tipo_transazione_nome;
            set
            {
                var match = tipologie.FirstOrDefault(x => x.tipo_transazione_nome == value);
                if (match != null) _tipo = match;
            }
        }





        public string Descrizione { get; set; } = string.Empty;

        public decimal Value { get; set; }


        public void Initialize(List<conto> Inconti, List<categoria> cat, List<tipo_transazione> tipi)
        {
            conti = Inconti ?? throw new ArgumentNullException(nameof(Inconti));
            categorie = cat ?? throw new ArgumentNullException(nameof(cat));
            tipologie = tipi ?? throw new ArgumentNullException(nameof(tipi));
            _contoSorgente = new();
            IsInitialized = true;
        }


        public void Clear()
        {
            _contoSorgente = new();
            _categoria = new();
            _tipo = new();
            Descrizione = string.Empty;
            Value = 0;
        }

        public Result<transazione[]> ToModelArray(List<DateTime> dt)
        {
            if (!dt.Any())
            {
                return Result.Failure<transazione[]>("Selezionare almeno una data");
            }

            Result ris = CheckValues();
            if (ris.IsFailure)
            {
                return Result.Failure<transazione[]>(ris.Error);
            }
            var arrayValue = (from x in dt
                              select new transazione()
                              {
                                  transazione_id = IdDb,
                                  conto_id = _contoSorgente.conto_id,
                                  tipo_transazione_id = _tipo.tipo_transazione_id,
                                  categoria_id = _categoria.categoria_id == 0 ? null : _categoria.categoria_id,
                                  descrizione = Descrizione,
                                  valore = Value,
                                  data = x,


                              }).ToArray();

            return Result.Success<transazione[]>(arrayValue);
        }

        public static async Task<TransazioneModelView> CreateAsync(FinanzeAccessDatabaseService db, CancellationToken token = default)
        {
            var conti = await db.GetAllContoAsync(token);
            var tipi = await db.GetAllTipoTransazioneAsync(token);
            var cat = await db.GetAllCategorieAsync(token);
            cat.Add(new categoria() { categoria_id = 0, categoria_nome = "NULL" }); // aggiunta per null
            var vm = new TransazioneModelView();
            vm.Initialize(conti, cat,tipi);
            return vm;
        }


        /// <summary>
        /// Metodo che converte la transazione in TransazioneModelView
        /// </summary>
        /// <param name="transDB"></param>
        internal void InitializeById(transazione transDB)
        {
            IdDb = transDB.transazione_id;
            _contoSorgente = (from x in conti where x.conto_id == transDB.conto_id select x).First();
            _tipo = (from x in tipologie where x.tipo_transazione_id == transDB.tipo_transazione_id select x).First();
            _categoria = transDB.categoria_id != null ? (from x in categorie where x.categoria_id == transDB.categoria_id select x).First() : new();
            Descrizione = transDB.descrizione != null ? transDB.descrizione : string.Empty ;
            Value = transDB.valore;
        }


        /// <summary>
        /// Metodo che controlla che i valori siano corretti
        /// </summary>
        /// <returns></returns>
        private Result CheckValues()
        {
            if (SelectedContoSorgente == null)
            {
                return Result.Failure("Selezionare il conto sorgente");
            }


            if (SelectedTipo == null)
            {
                return Result.Failure("Selezionare il tipo di transazione");
            }

            return Result.Success();

        }

        internal async Task ReloadCategorie(FinanzeAccessDatabaseService db, CancellationToken token)
        {
            var cat = await db.GetAllCategorieAsync(token);
            cat.Add(new categoria() { categoria_id = 0, categoria_nome = "NULL" }); // aggiunta per null
            categorie = cat;
        }
    }
}
