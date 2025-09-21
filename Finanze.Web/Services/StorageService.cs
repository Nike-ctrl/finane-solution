using Blazored.LocalStorage;
using CSharpFunctionalExtensions;
using Finanze.Web.Models;

namespace Finanze.Web.Services
{
    public class StorageService
    {
        /// <summary>
        /// Local storage
        /// </summary>
        private readonly ILocalStorageService _localStorageService;


        /// <summary>
        /// Tempo di scadenza
        /// </summary>
        private readonly TimeSpan _expiry;



        public StorageService(ILocalStorageService localStorageService)
        {
            _localStorageService = localStorageService;
            _expiry = TimeSpan.FromMinutes(45);
        }


        /// <summary>
        /// Metodo per il settaggio della variabile
        /// </summary>
        /// <typeparam name="T">Classe riferimento</typeparam>
        /// <param name="key">Chiave di riferimento</param>
        /// <param name="data">Oggetto</param>
        /// <returns>Ritorna il risutlato di salvataggio del info</returns>
        public async Task<Result> SetItemAsyncWithExpiry<T>(string key, T data)
        {
            try
            {
                await _localStorageService.SetItemAsync(key, new StorageItem<T>
                {
                    Data = data,
                    Expiry = DateTime.UtcNow.Add(_expiry)
                });
                return Result.Success();
            }
            catch (Exception ex)
            {

                return Result.Failure(ex.Message);
            }



        }


        /// <summary>
        /// Metodo per l'ottenimento del valore salvato nello storage
        /// </summary>
        /// <typeparam name="T">Classe di riferimento</typeparam>
        /// <param name="key">Chiave di ricerca</param>
        /// <returns>Ritorna il risultato della presa dei dati</returns>
        public async Task<Result<T>> GetItemAsyncWithExpiry<T>(string key)
        {
            try
            {
                var storageItem = await _localStorageService.GetItemAsync<StorageItem<T>>(key);

                if (storageItem is null)
                {
                    return Result.Failure<T>("Empty");
                }

                if (storageItem.Expiry < DateTime.UtcNow)
                {
                    await _localStorageService.RemoveItemAsync(key);
                    return Result.Failure<T>("Expired");
                }
                return Result.Success<T>(storageItem.Data);
            }
            catch (Exception ex)
            {
                return Result.Failure<T>(ex.Message);
            }

        }


        /// <summary>
        /// Metodo che rimuove l oggetto salvato in base alla chiave passata
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<Result> ClearAsync(string key)
        {

            try
            {
                await _localStorageService.RemoveItemAsync(key);
                return Result.Success("Cleared");
            }
            catch (Exception ex)
            {

                return Result.Failure($"Error : {ex.Message}");
            }

        }
    }
}
