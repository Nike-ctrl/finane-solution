using Finanze.Web.Models;

namespace Finanze.Web.Components.Pages.OverviewCategorie
{
    public partial class OverviewCategorieMainPage
    {

        public ConteEDateModel ContoDate { get; set; } = new ConteEDateModel();



        public OverviewCategorieMainPage()
        {
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;
            ContoDate = new ConteEDateModel()
            {
                ContoId = 1,
                DataInzio = new DateTime(year, 1, 1),
                //DataFine = new DateTime(tmpData.Year, tmpData.Month, 1)
                DataFine = new DateTime(year, month, DateTime.DaysInMonth(year, month))
                
            };

        }


        public void HandleSelectInfo(ConteEDateModel info)
        {
            ContoDate = info;   
        }

    }
}
