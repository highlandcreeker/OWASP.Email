using System;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Data.SqlClient;
using System.IO;
using System.Collections.Generic;
using System.Xml;

using MongoDB.Driver.Core;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OWASP.Email
{
	public class OWASPData
	{
		public List<Manager> Managers;
		public OWASPData(List<Manager> mgrs)
		{
			this.Managers = mgrs;
		}
	}
	public class Manager : ICloneable
	{
		public string Name;
		public string Email;
		public List<Tester> Staff;

		public Manager(string name, string email)
		{
			Staff = new List<Tester>();
			this.Name = name;
			this.Email = email;
		}

		public object Clone()
		{
			return new Manager(this.Name, this.Email) { Staff = new List<Tester>(this.Staff) };
		}
	}
	public class Tester
	{
		public string username;
		public string lastname;
		public string firstname;
	}
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				string rows = null;
				TestResultParser x = new TestResultParser();
				OWASPData d = x.ParseJson();
				//OWASPData d = x.ParseCsv();

				foreach (Manager m in d.Managers)
				{
					using (StringWriter sw = new StringWriter(new System.Text.StringBuilder()))
					{
						foreach (Tester t in m.Staff)
						{
							string row = MergeControl.MergeRow(t.firstname, t.lastname, t.username);
							sw.WriteLine(row);
						}

						rows = sw.GetStringBuilder().ToString();
					}

					string body = MergeControl.MergeBody(rows);

					if (string.IsNullOrWhiteSpace(m.Email))
					{
						using (var f = System.IO.File.AppendText(System.IO.Directory.GetCurrentDirectory() + @"\InvalidManagers.log"))
						{
							f.WriteLine(m.Name);
						}
					}
					else
					{
						Communications.SendMessage(m.Email, body);
					}
				}
			}
			catch (Exception ex)
			{
				using (var f = System.IO.File.AppendText(System.IO.Directory.GetCurrentDirectory() + @"\Error.log"))
				{
					f.Write(ex.ToString().ToCharArray());
				}
			}
		}
	}

	public class TestResultParser
	{

		private string csvPath = @"D:\users\scottca\Documents\OWASP\OWASP_Staff_Completed.txt";
		private string jsonPath = @"D:\users\scottca\Documents\OWASP\output.current.json";
		private List<Tester> staff = null;
		private string xmlStaff = null;

		public OWASPData ParseCsv()
		{
			using (var f = File.OpenText(csvPath))
			{
				staff = new List<Tester>();

				while (!f.EndOfStream)
				{
					string[] record = f.ReadLine().Split(',');
					staff.Add(new Tester() { lastname = record[0], firstname = record[1], username = record[2] });
				}
			}

			Convert();
			return MapManagerStaff();

		}

		public OWASPData ParseJson()
		{

			using (var rdr = new StreamReader(jsonPath))
			{
				string json = rdr.ReadToEnd();
				staff = JsonConvert.DeserializeObject<List<Tester>>(json);
			}

			Convert();
			return MapManagerStaff();
		}

		public OWASPData GetMongoData()
		{
			staff = MongoData.AsyncFind().Result;
			this.Convert();

			return MapManagerStaff();

		}

		private OWASPData MapManagerStaff()
		{
			List<Manager> mgrList = new List<Manager>();
			Manager mgr = null;
			string conString = System.Configuration.ConfigurationManager.ConnectionStrings["SQL"].ConnectionString;

			using (var conn = new SqlConnection(conString))
			{
				conn.Open();

				using (var cmd = new SqlCommand())
				{
					cmd.Connection = conn;
					cmd.CommandType = System.Data.CommandType.StoredProcedure;
					cmd.CommandText = "GetStaffNotTested";
					cmd.Parameters.Add(new SqlParameter("@StaffTested", xmlStaff));

					using (var rdr = cmd.ExecuteReader())
					{
						while (rdr.Read())
						{
							string nextMgr = rdr["Manager"].ToString();
							string nextMgrEmail = rdr["managerEmail"].ToString();

							if (mgr == null)
							{
								mgr = new Manager(nextMgr, nextMgrEmail);
							}
							else if (mgr.Name != nextMgr)
							{
								mgrList.Add((Manager)mgr.Clone());
								mgr.Name = nextMgr;
								mgr.Email = nextMgrEmail;
								mgr.Staff.Clear();
							}

							mgr.Staff.Add(new Tester() { username = rdr["EmpEmail"].ToString(), lastname = rdr["Last"].ToString(), firstname = rdr["First"].ToString() });
						}
					}

				}
			}

			return new OWASPData(mgrList);
		}

		private void Convert()
		{
			var builder = new System.Text.StringBuilder();

			using (var wtr = XmlWriter.Create(builder))
			{
				wtr.WriteStartDocument();
				wtr.WriteStartElement("root");

				foreach (Tester t in staff)
				{

					wtr.WriteStartElement("staff");
					wtr.WriteAttributeString("last", t.lastname);
					wtr.WriteAttributeString("first", t.firstname);
					wtr.WriteAttributeString("email", t.username);
					wtr.WriteEndElement();
				}
				wtr.WriteEndElement();
			}

			xmlStaff = builder.ToString();
		}
	}

	public class MongoData
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







