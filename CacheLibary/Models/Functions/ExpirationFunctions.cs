using CacheLibary.DAOs;
using CacheLibary.Interfaces;
using CacheLibary.Interfaces.Options;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Models.Functions
{
  internal class ExpirationFunctions
  {
    private static SQLiteAsyncConnection _db = PersistentCacheManager.Instance.GetDatabase();

    internal static Expiration GetExpiration(IOptions options)
    {
      Expiration expiration;
      if (options is IExpires expires)
      {
        DateTime? totalExpiration = null;
        if (expires.PersitentExperation.HasValue)
          totalExpiration = DateTime.UtcNow.Add(expires.PersitentExperation.Value);
        expiration = new Expiration()
        {
          TotalExpiration = totalExpiration,
          SlidingExpiration = options.Expires.PersistenSlidingExpiration,
          LastAccess = DateTime.UtcNow,
        };
      }
      else
        expiration = null;
      return expiration;
    }
    private const string UpdateExpirationQuery = "update Expiration set LastAccess = {0} where Id = (select ExpirationId from Key where ObjectKeyBlob = '{1}')";
    private const string UpdateExpirationsQuery = "update Expiration set LastAccess = {0} where Id in (select ExpirationId from Key where ObjectKeyBlob in ( {1} ))";
    internal static async Task UpdateExpiration<K>(IKey<K> key)
    {
      await UpdateExpiration(key.KeyBlob);
    }
    internal static async Task UpdateExpiration(Key key, SQLiteConnection transaction)
    {
      await UpdateExpiration(key.ObjectKeyBlob, transaction);
    }
    private static async Task UpdateExpiration(string keyBlob, SQLiteConnection transaction = null)
    {
      string updateQuery = string.Format(UpdateExpirationQuery, DateTime.UtcNow.Ticks, keyBlob);
      if (transaction == null)
        _ = await _db.ExecuteAsync(updateQuery);
      else
        _ = transaction.Execute(updateQuery);
    }

    private static async Task UpdateExpirations(string keyBlobs, SQLiteConnection transaction = null)
    {
      string updateQuery = string.Format(UpdateExpirationsQuery, DateTime.UtcNow.Ticks, keyBlobs);
      if (transaction == null)
        _ = await _db.ExecuteAsync(updateQuery);
      else
        _ = transaction.Execute(updateQuery);
    }

    internal static async Task UpdateExpiration<K>(IEnumerable<IKey<K>> keys)
    {
      string keyBlobs = "'" + string.Join("', '", keys.Select(k => k.KeyBlob)) + "'";
      await UpdateExpirations(keyBlobs);
    }
    internal static async Task UpdateExpirations(IEnumerable<Key> keys, SQLiteConnection transaction)
    {
      string keyBlobs = "'" + string.Join("', '", keys.Select(k => k.ObjectKeyBlob)) + "'";
      await UpdateExpirations(keyBlobs, transaction);
    }
    private const string GetExpired = "select Id from Expiration where TotalExpiration != 0 and TotalExpiration < {0} UNION select Id from Expiration where SlidingExpiration != 0 and SlidingExpiration + LastAccess < {0}";
    private const string SetKeysDeleted = "update Key set Deleted = true where ExpirationId in ( {0} )";
    private const string DeleteExpirations = "delete from Expiration where Id in ( {0} )";
    internal static async Task<int> DeleteKeyAndExpiration()
    {
      string query = string.Format(GetExpired, DateTime.UtcNow.Ticks);
      IEnumerable<int> expiredIds = await _db.QueryScalarsAsync<int>(query);
      if (!expiredIds.Any())
        return 0;
      string expiredString = string.Join(", ", expiredIds);
      string updateQuery = string.Format(SetKeysDeleted, expiredString);
      string deleteQuery = string.Format(DeleteExpirations, expiredString);
      await _db.RunInTransactionAsync(t =>
      {
        _ = t.Execute(updateQuery);
        _ = t.Execute(deleteQuery);
      });
      return expiredIds.ToList().Count;
    }

    private const string GetDeletedKeyIds = "select Id from Key where Deleted = true and ObjectKeyBlob Like '%{0}%'";
    private const string GetToDeleteValueIds = "select d.Id from Key as k Join (select * from KeyValue join (select Id from {0}) on Id = ValueId) as d on k.Id = KeyId where k.Id in ( {1} )";
    private const string DeleteKeys = "delete from Key where Id in ( {0} )";
    private const string DeleteKeyValues = "delete from KeyValue where KeyId in ( {0} )";
    private const string DeleteDaoValues = "delete from {0} where Id in (select Id from {0} where Id in ( {1} ) and Id not in (select distinct Id from (((select Id from MaterialDAO where Id in ( {1} )) join (select * from KeyValue) on Id = KeyId) join (Select Id as kId from Key where ObjectKeyBlob like '%{2}%') on kId = KeyId)))";
    internal static async Task DeleteExpired<D>(Type objectType) where D : ICustomOptionDAO, new()
    {
      string s = objectType.AssemblyQualifiedName;
      string deletedKeysQuery = string.Format(GetDeletedKeyIds, s);
      IEnumerable<int> deletedKeyIds = await _db.QueryScalarsAsync<int>(deletedKeysQuery);
      if (!deletedKeyIds.Any())
        return;
      string deletedIds = string.Join(", ", deletedKeyIds);
      string deleteKeys = string.Format(DeleteKeys, deletedIds);
      string deleteKeyValues = string.Format(DeleteKeyValues, deletedIds);
      TableMapping mapping = await _db.GetMappingAsync<D>();
      string getToDeletedValues = string.Format(GetToDeleteValueIds, mapping.TableName, deletedIds);
      IEnumerable<int> toDeleteValueIds = await _db.QueryScalarsAsync<int>(getToDeletedValues);
      string toDeleteValues = string.Join(", ", toDeleteValueIds);
      string deleteValues = string.Format(DeleteDaoValues, mapping.TableName, toDeleteValues, s);
      await _db.RunInTransactionAsync(t =>
      {
        int i = t.Execute(deleteKeys);
        i = t.Execute(deleteKeyValues);
        i = t.Execute(deleteValues);
      });
    }

    internal static async Task DeleteExpired(Type objectType)
    {
      throw new NotImplementedException();
    }
  }
}
