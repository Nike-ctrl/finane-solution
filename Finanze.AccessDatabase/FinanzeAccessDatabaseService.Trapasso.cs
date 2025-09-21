using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using Dapper;
using Finanza.Database.Models;
using Npgsql;
using System.Data;
using Finanza.Database.CustomModel;

namespace Finanze.AccessDatabase
{
    public partial class FinanzeAccessDatabaseService
    {
        /// <summary>
        /// Metodo che si occupa di inserire il trapasso di conto
        /// Questo metodo chiama la procedura esegui_trapasso
        /// </summary>
        /// <param name="scrContoId">id conto sorgente</param>
        /// <param name="destContoId">id conto destinazione</param>
        /// <param name="valore">Valore da trapassare</param>
        /// <param name="dt">Data del trapasso</param>
        /// <param name="categoria_id">id categoria, puo essere null</param>
        /// <param name="descrizione">descrizione, puo essere null</param>
        /// <param name="token"></param>
        /// <returns>Ritorna lo stato del trapasso</returns>
        public async Task<Result> InsertTrapassoAsync(int scrContoId, int destContoId, decimal valore, DateTime dt, int? categoria_id, string? descrizione, CancellationToken token = default)
        {
            try
            {
                using IDbConnection db = new NpgsqlConnection(_conn);

                //selezione di tutti i valori forecast_month_value in base all 'id
                string query = @"select esegui_trapasso(@IDSCR,@IDDEST,@VALORE,@DATA,@IDCAT,@DESC);";

                var parameters = new { IDSCR = scrContoId, IDDEST = destContoId, VALORE = valore, DATA = dt, IDCAT = categoria_id, DESC = descrizione, };
                var command = new CommandDefinition(query, parameters: parameters, cancellationToken: token);
                var ris = await db.QueryAsync(command);
                return Result.Success("Insert");

            }
            catch (Exception e)
            {
                return Result.Failure($"Error: {e.Message}");
            }
        }


        private async Task<Result> InsertTrapassoInternalAsync(IDbConnection db,IDbTransaction transaction,int scrContoId,int destContoId,decimal valore,DateTime dt,int? categoria_id,string? descrizione, CancellationToken token = default)
        {
            try
            {
                string query = @"select esegui_trapasso(@IDSCR,@IDDEST,@VALORE,@DATA,@IDCAT,@DESC);";

                var parameters = new
                {
                    IDSCR = scrContoId,
                    IDDEST = destContoId,
                    VALORE = valore,
                    DATA = dt,
                    IDCAT = categoria_id,
                    DESC = descrizione
                };

                var command = new CommandDefinition(query, parameters: parameters, transaction:transaction, cancellationToken: token);
                await db.QueryAsync(command);

                return Result.Success();
            }
            catch (Exception e)
            {
                return Result.Failure($"Errore inserimento trapasso: {e.Message}");
            }
        }


        //public async Task<Result> InsertTrapassiMultipliAsync(TrapassoModel[] trapassi, CancellationToken token = default)
        public async Task<Result> InsertTrapassiMultipliAsync(IEnumerable<TrapassoModel> trapassi, CancellationToken token = default)
        {

            using var db = new NpgsqlConnection(_conn);
            await db.OpenAsync(token);
            using var transaction = db.BeginTransaction();

            try
            {
                foreach (var t in trapassi)
                {
                    var res = await InsertTrapassoInternalAsync(
                        db,
                        transaction,
                        t.ScrContoId,
                        t.DestContoId,
                        t.Valore,
                        t.Dt,
                        t.CategoriaId,
                        t.Descrizione,
                        token);

                    if (!res.IsSuccess)
                    {
                        await transaction.RollbackAsync(token);
                        return Result.Failure($"Errore su uno dei trapassi: {res.Error}");
                    }
                }

                await transaction.CommitAsync(token);
                return Result.Success("Tutti i trapassi inseriti correttamente.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(token);
                return Result.Failure($"Errore durante il ciclo: {ex.Equals}");
            }
        }


    }
}
