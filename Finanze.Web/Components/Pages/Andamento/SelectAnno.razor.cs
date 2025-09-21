using Finanza.Database.Models;
using Finanze.AccessDatabase;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Finanze.Web.Components.Pages.Andamento
{
    public partial class SelectAnno 
    {
        /// <summary>
        /// Evento per il passaggio delle date calcolate, anche se singola data, viene passata in questo modo 
        /// </summary>
        [Parameter]
        public EventCallback<int> OnSelectYear{ get; set; }

        [Parameter]
        public int StartYear { get; set; }


        private DateRange _dateRange = new DateRange(DateTime.Now.Date, DateTime.Now.AddDays(5).Date);

        private int _selectedYear;

        private int[] Years = Enumerable.Range(2020, DateTime.Now.Year - 2020 + 2).ToArray();

        public int SelectedYear
        {
            get
            {
                return _selectedYear;
            }
            set
            {

                _selectedYear = value;
                SetValues();
            }
        }

        protected override void OnParametersSet()
        {
            if (_selectedYear != StartYear)
            {
                _selectedYear = StartYear;
            }
            base.OnParametersSet();
        }


        private async Task SetValues()
        {
            await OnSelectYear.InvokeAsync(_selectedYear);

        }
    }
}
