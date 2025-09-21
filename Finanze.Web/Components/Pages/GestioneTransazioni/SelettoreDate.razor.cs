using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Finanze.Web.Components.Pages.GestioneTransazioni
{
    public partial class SelettoreDate
    {
        /// <summary>
        /// Evento per il passaggio delle date calcolate, anche se singola data, viene passata in questo modo 
        /// </summary>
        [Parameter]
        public EventCallback<List<DateTime>> OnSelectDateList { get; set; }


        /// <summary>
        /// Indica se va mostrata solo la parte di data singola
        /// </summary>
        [Parameter]
        public bool OnlySingleDate { get; set; }


        [Parameter]
        public DateTime StartDate { get; set; } = DateTime.Now;


        private bool multiDate = false;

        public bool MultiDate { get { 
                return multiDate;
            }
            set
            {
                multiDate = value;
                if (!value)
                {
                    SetSingleValue();
                }
            }
        }


        private DateTime? _dateStart;

        private DateRange _dateRange = new DateRange(DateTime.Now.Date, DateTime.Now.AddDays(5).Date);

        private int _selectedDay = DateTime.Today.Day;

        private int[] giorni = Enumerable.Range(1, 31).ToArray();

        public int SelectedDay
        {
            get
            {
                return _selectedDay;
            }
            set
            {

                _selectedDay = value;
                SetValues();
            }
        }

        protected override void OnParametersSet()
        {
            if (_dateStart != StartDate)
            {


                _dateStart = StartDate;
                _dateRange = new DateRange(StartDate, StartDate.AddDays(5).Date);
                _selectedDay = StartDate.Day;
            }
            base.OnParametersSet();
        }


        private async Task SetValues()
        {
            var datatmp = _dateRange.Start;

            var dataFinetmp = _dateRange.End;

            if(datatmp is null || dataFinetmp is null)
            {
                return;
            }

            List<DateTime> dates = GetDateRicorrenzeConFallback(datatmp.Value, dataFinetmp.Value, _selectedDay);
            await OnSelectDateList.InvokeAsync(dates);
            ;
        }
        private async Task SetSingleValue()
        {
            if (_dateStart is null)
            {
                return;
            }

            await OnSelectDateList.InvokeAsync(new List<DateTime> { _dateStart.Value });
        }


        public static List<DateTime> GetDateRicorrenzeConFallback(DateTime dataInizio, DateTime dataFine, int giornoDelMese)
        {
            var result = new List<DateTime>();

            // Parti dal primo giorno del mese della data di inizio
            var dataCorrente = new DateTime(dataInizio.Year, dataInizio.Month, 1);

            while (dataCorrente <= dataFine)
            {
                int ultimoGiornoDelMese = DateTime.DaysInMonth(dataCorrente.Year, dataCorrente.Month);
                int giornoValidato = Math.Min(giornoDelMese, ultimoGiornoDelMese);

                var ricorrenza = new DateTime(dataCorrente.Year, dataCorrente.Month, giornoValidato);

                if (ricorrenza >= dataInizio && ricorrenza <= dataFine)
                {
                    result.Add(ricorrenza);
                }

                // Vai al mese successivo
                dataCorrente = dataCorrente.AddMonths(1);
            }

            return result;
        }

    }
}
