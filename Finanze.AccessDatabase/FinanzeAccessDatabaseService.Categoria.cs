using CSharpFunctionalExtensions;
using Dapper;
using Finanza.Database.CustomModel;
using Finanza.Database.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Finanze.AccessDatabase
{
    public partial class FinanzeAccessDatabaseService
    {

        /// <summary>
        /// Ottiene tutte le categorie
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<categoria>> GetAllCategorieAsync(CancellationToken token = default)
        {
            try
            {

                using IDbConnection db = new NpgsqlConnection(_conn);

                //selezione di tutti i valori forecast_month_value in base all 'id
                string query = @"select * from categoria order by categoria_nome;";

                var command = new CommandDefinition(query,  cancellationToken: token);
                IEnumerable<categoria> listValuesDb = await db.QueryAsync<categoria>(command);
                return listValuesDb.AsList();

            }
            catch (Exception e)
            {
                return new();
            }


        }


        /// <summary>
        /// Metodo per il raggruppamento delle categorie in base all anno mese e conti presenti nel raggruppamento conti
        /// </summary>
        /// <param name="SelectedRaggruppamentoContoId"></param>
        /// <param name="dt"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<SommarioCategoria>> GetViewSommaCategoriaEntrateAsync(int SelectedRaggruppamentoContoId, DateTime dt, CancellationToken token = default)
        {
            try
            {

                using IDbConnection db = new NpgsqlConnection(_conn);

                //selezione di tutti i valori forecast_month_value in base all 'id
                string query = @"SELECT c.categoria_nome,
    t.mese,
    t.anno,
    sum(t.valore) AS sum_valore,
    sum(t.valore_reale) AS sum_valore_reale
   FROM transazione t
     LEFT JOIN categoria c ON t.categoria_id = c.categoria_id
   where t.tipo_transazione_id = 1 and t.conto_id in (select vg.conto_id  from vincolo_gruppo vg where vg.raggruppamento_conto_id = @ID)
         and t.anno = @ANNO and t.mese = @MESE
        GROUP BY t.categoria_id, t.anno, t.mese, t.conto_id, c.categoria_id
        ORDER BY c.categoria_id, t.anno, t.mese;";

                var parameters = new { ID = SelectedRaggruppamentoContoId, ANNO = dt.Year, MESE = dt.Month };
                var command = new CommandDefinition(query, parameters: parameters, cancellationToken: token);
                IEnumerable<SommarioCategoria> listValuesDb = await db.QueryAsync<SommarioCategoria>(command);
                return listValuesDb.AsList();

            }
            catch (Exception e)
            {
                return new();
            }


        }


        /// <summary>
        /// Metodo per il raggruppamento delle categorie in base all anno mese e conti presenti nel raggruppamento conti
        /// </summary>
        /// <param name="SelectedRaggruppamentoContoId"></param>
        /// <param name="dt"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<SommarioCategoria>> GetViewSommaCategoriaUsciteAsync(int SelectedRaggruppamentoContoId, DateTime dt, CancellationToken token = default)
        {
            try
            {

                using IDbConnection db = new NpgsqlConnection(_conn);

                //selezione di tutti i valori forecast_month_value in base all 'id
                string query = @"SELECT c.categoria_nome,
    t.mese,
    t.anno,
    sum(t.valore) AS sum_valore,
    sum(t.valore_reale) AS sum_valore_reale
   FROM transazione t
     LEFT JOIN categoria c ON t.categoria_id = c.categoria_id
   where t.tipo_transazione_id = 2 and t.conto_id in (select vg.conto_id  from vincolo_gruppo vg where vg.raggruppamento_conto_id = @ID)
         and t.anno = @ANNO and t.mese = @MESE
        GROUP BY t.categoria_id, t.anno, t.mese, t.conto_id, c.categoria_id
        ORDER BY c.categoria_id, t.anno, t.mese;";

                var parameters = new { ID = SelectedRaggruppamentoContoId, ANNO = dt.Year, MESE = dt.Month };
                var command = new CommandDefinition(query, parameters: parameters, cancellationToken: token);
                IEnumerable<SommarioCategoria> listValuesDb = await db.QueryAsync<SommarioCategoria>(command);
                return listValuesDb.AsList();

            }
            catch (Exception e)
            {
                return new();
            }


        }


        public async Task<Result> AddCategoriaAsync(string categoria, CancellationToken token = default)
        {
            categoria = categoria.ToLower().Trim();
            try
            {
                await using var db = await _dbContextFactory.CreateDbContextAsync(token);
                await using var transaction = await db.Database.BeginTransactionAsync(token);
                await db.categoria.AddAsync(new Finanza.Database.Models.categoria() { categoria_nome = categoria });
                await db.SaveChangesAsync(token); // Salva tutti insieme
                await transaction.CommitAsync(token);

                return Result.Success();
            }
            catch (DbUpdateException dbEx) when (dbEx.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                return Result.Failure("Categoria già esistente (violazione vincolo UNIQUE).");
            }
            catch (Exception ex)
            {
                return Result.Failure($"Errore EF: {ex.Message}");
            }

        }

        


        /// <summary>
        /// Metodo che ritorna la somma in valore assoluto  delle uscite o entrate del conto 
        /// </summary>
        /// <param name="idconto"></param>
        /// <param name="dtStart"></param>
        /// <param name="dtEND"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<SommarioCategoriaPerConto>> GetSommaCategoriaByIdContoAsync(int idconto, DateTime dtStart, DateTime dtEND, CancellationToken token = default)
        {
            try
            {

                using IDbConnection db = new NpgsqlConnection(_conn);

                //selezione di tutti i valori forecast_month_value in base all 'id
                string query = @"SELECT 
    c.conto_id,
    c.conto_nome,
    tt.tipo_transazione_id,
    tt.tipo_transazione_nome,
    COALESCE(c2.categoria_id, 0) AS categoria_id,
    COALESCE(c2.categoria_nome, 'NO_CATEGORIA') AS categoria_nome,
    ABS(SUM(t.valore_reale)) as sum_valore_reale_abs
FROM 
    transazione t
JOIN 
    conto c ON t.conto_id = c.conto_id
LEFT JOIN 
    categoria c2 ON t.categoria_id = c2.categoria_id
JOIN 
    tipo_transazione tt ON t.tipo_transazione_id = tt.tipo_transazione_id
WHERE 
    t.conto_id = @IDCONTO
    AND t.data >= @START
    AND t.data <= @END
GROUP BY 
    c.conto_id, 
    c.conto_nome,
    tt.tipo_transazione_id, 
    tt.tipo_transazione_nome,
    COALESCE(c2.categoria_id, 0),
    COALESCE(c2.categoria_nome, 'NO_CATEGORIA')
ORDER BY  tt.tipo_transazione_id, categoria_id;";

                var parameters = new { IDCONTO = idconto, START = dtStart, END = dtEND };
                var command = new CommandDefinition(query, parameters: parameters, cancellationToken: token);
                IEnumerable<SommarioCategoriaPerConto> listValuesDb = await db.QueryAsync<SommarioCategoriaPerConto>(command);
                return listValuesDb.AsList();

            }
            catch (Exception e)
            {
                return new();
            }


        }


    }
}
