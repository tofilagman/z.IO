using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.Protocols;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace z.IO.ActiveDirectory
{
    public static class Common
    {

        public static bool PerformLdapAuth(string server, int portNumber, string baseDn, string username, string password)
        {
            try
            {
                // try to connect to LDAP server anonymously
                LdapConnection ldap = new LdapConnection(new LdapDirectoryIdentifier(server, portNumber));
                ldap.AuthType = AuthType.Anonymous;
                ldap.Bind();

                // try to connect to LDAP server using credentials
                ldap.AuthType = AuthType.Basic;
                SearchRequest request = new SearchRequest(baseDn, string.Format(CultureInfo.InvariantCulture, "uid={0}", username), SearchScope.Subtree);
                SearchResponse response = (SearchResponse)ldap.SendRequest(request);
                if (1 == response.Entries.Count)
                    ldap.Bind(new NetworkCredential(response.Entries[0].DistinguishedName, password));
                else
                    throw new Exception("User not found.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }


        public static bool Auth2(string server, string baseDn, string username, string password)
        {
            using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, server, baseDn, ContextOptions.SimpleBind))
            {
                
                // validate the credentials
                bool isValid = pc.ValidateCredentials(username, password);
                return isValid;
            }
        }

    }
}
