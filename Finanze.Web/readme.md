    public partial class PiaCategorieChart : IDisposable
    {

        private CancellationTokenSource _cts;

        [Inject]
        public required FinanzeAccessDatabaseService serviceDb { get; set; }


        [Parameter]
        public int SelectedContoId { get; set; }

        [Parameter]
        public DateTime SelectedDatetime { get; set; }

        protected override void OnInitialized()
        {
            _cts = new CancellationTokenSource();
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

    }