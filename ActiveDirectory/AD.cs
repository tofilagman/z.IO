using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace z.IO.ActiveDirectory
{
    public class AD
    {

        DirectoryEntry de;


        public AD(string server)
        {
            de = new DirectoryEntry(server);
        }

        public void Auth(string Username, string Password)
        {
            de.Username = Username;
            de.Password = Password;
            de.AuthenticationType = AuthenticationTypes.None;

            DirectorySearcher ds = new DirectorySearcher(de, "(uid=" + Username + ")", new string[] { "uid" });

            ds.SearchScope = SearchScope.Subtree;
            SearchResult sr = ds.FindOne();

        }

    }
}
