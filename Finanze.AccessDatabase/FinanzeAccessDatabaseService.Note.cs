using CSharpFunctionalExtensions;
using Dapper;
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

    /// <summary>
    /// Parte della classe che si occupa di gestire le note
    /// </summary>
    public partial class FinanzeAccessDatabaseService
    {


        public async Task<List<nota>> GetAllNoteForAPeriodAsync(DateTime dt, CancellationToken token = default)
        {
            try
            {
                using IDbConnection db = new NpgsqlConnection(_conn);

                //presa di tutte le note per il periodo specifico 
                string query = @"select * from nota n where n.anno = @ANNO and n.mese = @MESE order by creation desc;;";

                var parameters = new { MESE = dt.Month, ANNO = dt.Year };
                var command = new CommandDefinition(query, parameters: parameters, cancellationToken: token);
                IEnumerable<nota> listValuesDb = await db.QueryAsync<nota>(command);
                return listValuesDb.AsList();

            }
            catch (Exception e)
            {
                return new();
            }


        }



        /// <summary>
        /// Metodo per la cancellazione della nota 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<Result> DeleteNoteByIdAsync(int id, CancellationToken token = default)
        {

            try
            {
                await using var db = await _dbContextFactory.CreateDbContextAsync(token);
                await using var transaction = await db.Database.BeginTransactionAsync(token);
                // controllo presenza 

                nota? n = await db.nota.Where(x => x.nota_id == id).SingleOrDefaultAsync();
                if (n == null)
                {
                    return Result.Failure("Non trovato");
                }

                db.nota.Remove(n);


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
        /// Metodo per l'aggiunta della nota
        /// Viene impostata la data di creazione ad now e impostato l id a 0
        /// </summary>
        /// <param name="n"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<Result> AddNoteAsync(nota n, CancellationToken token = default)
        {

            try
            {
                await using var db = await _dbContextFactory.CreateDbContextAsync(token);
                await using var transaction = await db.Database.BeginTransactionAsync(token);


                n.creation = DateTime.Now;
                n.nota_id = 0;
                await db.nota.AddAsync(n);

                await db.SaveChangesAsync(token); // Salva tutti insieme
                await transaction.CommitAsync(token);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Errore EF: {ex.Message}");
            }
        }

    }
}
