using System;
using System.IO;

namespace OWASP.Email
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				string rows = null;
				QuizData x = new QuizData();
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
}







