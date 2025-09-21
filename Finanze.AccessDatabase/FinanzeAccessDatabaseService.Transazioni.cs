using CSharpFunctionalExtensions;
using Dapper;
using Finanza.Database.CustomModel;
using Finanza.Database.Context;
using Finanza.Database.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanze.AccessDatabase
{
    public partial class FinanzeAccessDatabaseService
    {
        /// <summary>
        /// Metodo che ritorna il risultato della view GetViewSommarioTransazioniAsync
        /// </summary>
        /// <param name="idRaggruppamentoConto">id del raggruppamento del conto, qui si andranno a prendere i conti vincolati</param>
        /// <param name="dt">data di riferimento, si prende anno e mese</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<view_sommario_transazioni>> GetViewSommarioTransazioniAsync(int idRaggruppamentoConto, DateTime dt, CancellationToken token = default)
        {
            try
            {

                using IDbConnection db = new NpgsqlConnection(_conn);

                //selezione di tutti i valori forecast_month_value in base all 'id
                string query = @"select * from view_sommario_transazioni t
                    where t.conto_id in (select vg.conto_id  from vincolo_gruppo vg where vg.raggruppamento_conto_id = @ID) and t.anno = @ANNO and t.mese = @MESE
                    order by t.tipo_transazione_id, t.data;";

                var parameters = new { ID = idRaggruppamentoConto, ANNO = dt.Year, MESE = dt.Month };
                var command = new CommandDefinition(query, parameters: parameters, cancellationToken: token);
                IEnumerable<view_sommario_transazioni> listValuesDb = await db.QueryAsync<view_sommario_transazioni>(command);
                return listValuesDb.AsList();

            }
            catch (Exception e)
            {
                return new();
            }


        }


        /// <summary>
        /// Metodo che prende il valore della transazione
        /// </summary>
        /// <param name="idTransazione"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<Result<transazione>> GetTransazioneByIdAsync(int idTransazione, CancellationToken token = default)
        {
            try
            {

                using IDbConnection db = new NpgsqlConnection(_conn);

                string query = @"select * from transazione t where t.transazione_id =@ID;";

                var parameters = new { ID = idTransazione };
                var command = new CommandDefinition(query, parameters: parameters, cancellationToken: token);
                transazione? trans = await db.QuerySingleOrDefaultAsync<transazione>(command);
                return trans == null ? Result.Failure<transazione>("Non trovato") : Result.Success<transazione>(trans);

            }
            catch (Exception e)
            {
                return Result.Failure<transazione>(e.Message);
            }


        }


        /// <summary>
        /// Ritorna una lista di valori che sono vincolati per il trapasso ID
        /// </summary>
        /// <param name="idTransazione"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<view_sommario_transazioni>> GetAllTransactionLinkedByTrapassoId(int idTransazione, CancellationToken token = default)
        {
            try
            {

                using IDbConnection db = new NpgsqlConnection(_conn);

                //selezione di tutti i valori forecast_month_value in base all 'id
                string query = @"select* from view_sommario_transazioni t
                                where t.trapasso_id in (
                                    select t.trapasso_id from transazione t
                                    where t.transazione_id = @ID);";

                var parameters = new { ID = idTransazione };
                var command = new CommandDefinition(query, parameters: parameters, cancellationToken: token);
                IEnumerable<view_sommario_transazioni> listValuesDb = await db.QueryAsync<view_sommario_transazioni>(command);
                return listValuesDb.AsList();

            }
            catch (Exception e)
            {
                return new();
            }


        }


        public async Task<Result> DeleteTransazioniByIdAsync(int id, CancellationToken token = default)
        {

            try
            {
                await using var db = await _dbContextFactory.CreateDbContextAsync(token);
                await using var transaction = await db.Database.BeginTransactionAsync(token);
                // controllo presenza 

                transazione? transazione = await db.transazione.Where(x => x.transazione_id == id).SingleOrDefaultAsync();
                if (transazione == null)
                {
                    return Result.Failure("Non trovato");
                }

                db.transazione.Remove(transazione);


                await db.SaveChangesAsync(token); // Salva tutti insieme
                await transaction.CommitAsync(token);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Errore EF: {ex.Message}");
            }
        }
        public async Task<Result> UpdateTransazioneAsync(transazione transazioneToEdit, CancellationToken token = default)
        {

            try
            {
                await using var db = await _dbContextFactory.CreateDbContextAsync(token);
                await using var transaction = await db.Database.BeginTransactionAsync(token);
                // controllo presenza 

                transazione? transazione = await db.transazione.Where(x => x.transazione_id == transazioneToEdit.transazione_id).SingleOrDefaultAsync();
                if (transazione == null)
                {
                    return Result.Failure("Non trovato");
                }

                transazione.descrizione = transazioneToEdit.descrizione;
                transazione.valore = transazioneToEdit.valore;
                transazione.conto_id = transazioneToEdit.conto_id;
                transazione.tipo_transazione_id = transazioneToEdit.tipo_transazione_id;
                transazione.data = transazioneToEdit.data;
                transazione.categoria_id = transazioneToEdit?.categoria_id;



                // modifica
                db.Entry(transazione).State = EntityState.Modified;



                await db.SaveChangesAsync(token); // Salva tutti insieme
                await transaction.CommitAsync(token);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Errore EF: {ex.Message}");
            }
        }

        public async Task<List<tipo_transazione>> GetAllTipoTransazioneAsync(CancellationToken token = default)
        {
            try
            {
                using IDbConnection db = new NpgsqlConnection(_conn);

                string query = @"select * from tipo_transazione";
                var command = new CommandDefinition(query, cancellationToken: token);
                IEnumerable<tipo_transazione> listValuesDb = await db.QueryAsync<tipo_transazione>(command);
                return listValuesDb.AsList();

            }
            catch (Exception e)
            {
                return new();
            }


        }


        public async Task<Result> InsertTransazioniMultipliAsync(IEnumerable<transazione> transazioni, CancellationToken token = default)
        {

            try
            {
                await using var db = await _dbContextFactory.CreateDbContextAsync(token);
                await using var transaction = await db.Database.BeginTransactionAsync(token);

                await db.transazione.AddRangeAsync(transazioni, token); // Inserisce tutto in memoria EF

                await db.SaveChangesAsync(token); // Salva tutti insieme
                await transaction.CommitAsync(token);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Errore EF: {ex.Message}");
            }
        }



        /// <summary>
        /// Questa è la prima pagina che viene caricata
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<view_transazione_completa>> FirstPageAllTransactionAsync(int pagesize, CancellationToken token = default)
        {
            try
            {

                using IDbConnection db = new NpgsqlConnection(_conn);

                //selezione di tutti i valori forecast_month_value in base all 'id
                string query = @"select * from view_transazione_completa limit @PAGE;";

                var param = new { PAGE = pagesize};
                var command = new CommandDefinition(query, parameters: param, cancellationToken: token);
                IEnumerable<view_transazione_completa> listValuesDb = await db.QueryAsync<view_transazione_completa>(command);
                return listValuesDb.AsList();

            }
            catch (Exception e)
            {
                return new();
            }


        }
        /// <summary>
        /// Queste sono le altre pagine, dove è necesasrio passare l'id 
        /// </summary>
        /// <param name="lastid"></param>
        /// <param name="token"></param>
        /// <returns></returns>

        public async Task<List<view_transazione_completa>> OtherPageAllTransactionAsync(int lastid, int pagesize, CancellationToken token = default)
        {
            try
            {

                using IDbConnection db = new NpgsqlConnection(_conn);

                //selezione di tutti i valori forecast_month_value in base all 'id
                string query = @"select * from view_transazione_completa t where t.transazione_id < @ID limit  @PAGE;";

                var parameter = new { ID = lastid, PAGE = pagesize };
                var command = new CommandDefinition(query, parameters:parameter, cancellationToken: token);
                IEnumerable<view_transazione_completa> listValuesDb = await db.QueryAsync<view_transazione_completa>(command);
                return listValuesDb.AsList();

            }
            catch (Exception e)
            {
                return new();
            }


        }


        public async Task<List<view_transazione_completa>> GetAllTransactionAsync( CancellationToken token = default)
        {
            try
            {

                using IDbConnection db = new NpgsqlConnection(_conn);

                //selezione di tutti i valori forecast_month_value in base all 'id
                string query = @"select * from view_transazione_completa t order by t.data;";

                var command = new CommandDefinition(query, cancellationToken: token);
                IEnumerable<view_transazione_completa> listValuesDb = await db.QueryAsync<view_transazione_completa>(command);
                return listValuesDb.AsList();

            }
            catch (Exception e)
            {
                return new();
            }


        }

    }
}
