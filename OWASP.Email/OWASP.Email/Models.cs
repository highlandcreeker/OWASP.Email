using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
