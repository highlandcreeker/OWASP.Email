using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Xml;

namespace OWASP.Email
{
	public class QuizData
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
			staff = MongoDb.AsyncFind().Result;
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
}
