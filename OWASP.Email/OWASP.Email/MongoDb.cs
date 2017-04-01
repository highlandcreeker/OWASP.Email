using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace OWASP.Email
{
	public class MongoDb
	{
		private static string listData;
		private static List<Tester> staff;

		public async static Task<List<Tester>> AsyncFind()
		{

			string uri = System.Configuration.ConfigurationManager.ConnectionStrings["Mongodb"].ConnectionString;

			var client = new MongoClient(uri);

			var owasp = client.GetDatabase("OTGDb");

			var users = owasp.GetCollection<BsonDocument>("users");


			var filter = Builders<BsonDocument>.Filter.Gt("__v", 20);
			var sort = Builders<BsonDocument>.Sort.Ascending("lastName");

			var cnt = users.Count(filter);
			staff = new List<Tester>();

			await users.Find(filter).Sort(sort).ForEachAsync(user =>
			staff.Add(new Tester() { username = user["username"].ToString(), lastname = user["lastName"].ToString(), firstname = user["firstName"].ToString() })
			);

			return staff;
		}
	}
}
