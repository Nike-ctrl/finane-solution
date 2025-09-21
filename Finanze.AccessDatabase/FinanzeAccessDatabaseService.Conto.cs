using Dapper;
using Finanza.Database.CustomModel;
using Finanza.Database.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanze.AccessDatabase
{

    /// <summary>
    /// Parte che si occupa della gestione dei conti
    /// </summary>
    public partial class FinanzeAccessDatabaseService
    {
        /// <summary>
        /// Metodo che ritorna tutti i conti disponibili 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<conto>> GetAllContoAsync(CancellationToken token = default)
        {
            try
            {


                using IDbConnection db = new NpgsqlConnection(_conn);

                //selezione di tutti i valori forecast_month_value in base all 'id
                string query = @"select * from conto c order by c.conto_id ;";


                var command = new CommandDefinition(query, cancellationToken: token);
                IEnumerable<conto> listValuesDb = await db.QueryAsync<conto>(command);
                return listValuesDb.AsList();

            }
            catch (Exception e)
            {
                return new();
            }


        }


        /// <summary>
        /// Metodo che ritorna tutti i raggruppamenti dei conti
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<raggruppamento_conto>> GetAllRaggrupamentoContoAsync(CancellationToken token = default)
        {
            try
            {
                using IDbConnection db = new NpgsqlConnection(_conn);

                //selezione di tutti i valori forecast_month_value in base all 'id
                string query = @"select * from raggruppamento_conto c order by c.raggruppamento_conto_id ;";


                var command = new CommandDefinition(query, cancellationToken: token);
                IEnumerable<raggruppamento_conto> listValuesDb = await db.QueryAsync<raggruppamento_conto>(command);
                return listValuesDb.AsList();

            }
            catch (Exception e)
            {
                return new();
            }


        }

        /// <summary>
        /// Metodo che chiama la view view_somma_raggruppamenti_mese_anno, sono le posizioni per ogni mese dei conti vincolati al raggruppamento
        /// </summary>
        /// <param name="idraggr"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<view_valore_cumulativo_mensile>> GetValoreCumulativoAsync(int idraggr,int year, CancellationToken token = default)
        {
            try
            {
                using IDbConnection db = new NpgsqlConnection(_conn);

                //selezione di tutti i valori forecast_month_value in base all 'id
                string query = @"select * from view_valore_cumulativo_mensile c where c.raggruppamento_conto_id = @ID and c.anno = @ANNO order by c.anno, c.mese;";


                var command = new CommandDefinition(query, parameters: new { ID = idraggr,ANNO = year } , cancellationToken: token);
                IEnumerable<view_valore_cumulativo_mensile> listValuesDb = await db.QueryAsync<view_valore_cumulativo_mensile>(command);
                return listValuesDb.AsList();

            }
            catch (Exception e)
            {
                return new();
            }


        }



        public async Task<List<ValoreCumulativoMensileConDifferenza>> GetValoreCumulativoConDifferenzaPrimoValoreAsync(int idraggr, int year, CancellationToken token = default)
        {
            try
            {
                using IDbConnection db = new NpgsqlConnection(_conn);

                //selezione di tutti i valori forecast_month_value in base all 'id
                string query = @"SELECT 
    t.anno,
    t.mese,
    t.data,
    t.valore_cumulativo,
t.valore_mensile ,
    (t.valore_cumulativo - d.valore_cumulativo) AS differenza_col_primo
FROM view_valore_cumulativo_mensile t
CROSS JOIN (
    SELECT t2.valore_cumulativo
    FROM view_valore_cumulativo_mensile t2
    WHERE t2.raggruppamento_conto_id = @ID AND t2.anno = @ANNO
    ORDER BY t2.data
    LIMIT 1
) d
WHERE t.raggruppamento_conto_id = @ID AND t.anno =@ANNO;";


                var command = new CommandDefinition(query, parameters: new { ID = idraggr, ANNO = year }, cancellationToken: token);
                IEnumerable<ValoreCumulativoMensileConDifferenza> listValuesDb = await db.QueryAsync<ValoreCumulativoMensileConDifferenza>(command);
                return listValuesDb.AsList();

            }
            catch (Exception e)
            {
                return new();
            }


        }

    }
}
