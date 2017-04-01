using System;
using System.IO;
using System.Collections.Generic;

namespace OWASP.Email
{
	public class MergeControl
	{
		private static string GetBody()
		{
			var assembly = System.Reflection.Assembly.GetExecutingAssembly();
			var resourceName = "OWASP.Email.Message.Body.txt";

			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			using (StreamReader reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}
		private static string GetRow()
		{
			var assembly = System.Reflection.Assembly.GetExecutingAssembly();
			var resourceName = "OWASP.Email.Message.Row.txt";

			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			using (StreamReader reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}

		public static string MergeBody(string rows)
		{
			string body = GetBody();
			return body.Replace("$$rows$$", rows);
		}

		public static string MergeRow(string first, string last, string wdEmail)
		{
			string row = GetRow();

			row = row.Replace("$$first$$", first);
			row = row.Replace("$$last$$", last);
			row = row.Replace("$$email$$", wdEmail);

			return row;
		}
	}
}
