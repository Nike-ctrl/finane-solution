using Dapper;
using Finanza.Database.Context;
using Finanza.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanze.AccessDatabase
{
    public partial class FinanzeAccessDatabaseService
    {
        /// <summary>
        /// Database Context Factory
        /// </summary>
        private readonly IDbContextFactory<FinanzeContext> _dbContextFactory;


        /// <summary>
        /// Logger 
        /// </summary>
        private readonly ILogger<FinanzeAccessDatabaseService>? _logger;

        /// <summary>
        /// stringa di connessione al db per connessione con dapper 
        /// </summary>
        private readonly string _conn;





        /// <summary>
        /// Costruttore 
        /// </summary>
        /// <param name="dbContextFactory"></param>
        /// <param name="configuration">Iconfiguratione per la presa della stringa di connessione DefaultConnectionFinanze</param>
        /// <param name="logger">Logger</param>
        public FinanzeAccessDatabaseService(IDbContextFactory<FinanzeContext> dbContextFactory, IConfiguration configuration, ILogger<FinanzeAccessDatabaseService>? logger = null)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;

            _conn = GetConnectionStringFromConfig(configuration);
        }





        /// <summary>
        /// Metodo per la prese della stringa di connessione
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <exception cref="Exception">In caso che non venga trovata la stringa di connessione DefaultConnectionFinanze lancia un eccezione </exception>
        private string GetConnectionStringFromConfig(IConfiguration configuration)
        {

            string? conn = configuration.GetConnectionString("DefaultConnectionFinanze");

            if (conn == null)
            {
                _logger?.LogError("Not found connection string DefaultConnectionFinanze inside appsettings.json");
                throw new Exception("Not found connection string DefaultConnectionFinanze inside appsettings.json");
            }
            _logger?.LogInformation("Connection string found");
            return conn;
        }



        public async Task<List<view_somma_conto_anno_mese>> TestDapper(CancellationToken? token = null)
        {
            try
            {
                // Se il token è null, viene impostato a None
                token ??= CancellationToken.None;

                using IDbConnection db = new NpgsqlConnection(_conn);

                //selezione di tutti i valori forecast_month_value in base all 'id
                string query = @"select * from view_somma_conto_anno_mese;";


                var command = new CommandDefinition(query,  cancellationToken: token.Value);
                IEnumerable<view_somma_conto_anno_mese> listValuesDb  = await db.QueryAsync<view_somma_conto_anno_mese>(command);
                return listValuesDb.AsList();

            }
            catch (Exception e)
            {
                return new();
            }


        }

        /// <summary>
        /// test con ef
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<view_somma_conto_anno_mese>> TestEF(CancellationToken? token = null)
        {
            try
            {
                // Se il token è null, viene impostato a None
                token ??= CancellationToken.None;
                await using var db = await _dbContextFactory.CreateDbContextAsync(token.Value);

                return await (from x in db.view_somma_conto_anno_mese select x).ToListAsync();
            }
            catch (Exception e)
            {

                return new();
            }

        }



    }
}
