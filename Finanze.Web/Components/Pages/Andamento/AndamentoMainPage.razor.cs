namespace Finanze.Web.Components.Pages.Andamento
{
    public partial class AndamentoMainPage
    {

        private int raggruppamentoId = 2;


        private int _anno = DateTime.Now.Year;





        public void HandleSelectedRaggruppamento(int id)
        {
            raggruppamentoId = id;
        }

        public void HandleSelectedYear(int id)
        {
            _anno = id;
        }


    }
}
