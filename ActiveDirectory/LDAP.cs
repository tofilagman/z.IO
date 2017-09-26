using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;

namespace z.IO.ActiveDirectory
{
    public class LDAP : IDisposable
    {

        private DirectoryEntry entry;

        public string Domain { get; private set; }
        //public bool SSL { get; private set; }
        public string User { get; set; }
        public string Password { get; set; }

        public LDAP(string Domain) //, bool SSL = true
        {
            this.Domain = Domain;
            //this.SSL = SSL;
        }

        #region Constants

        public const int SCRIPT = 0x0001;
        public const int ACCOUNTDISABLE = 0x0002;
        public const int HOMEDIR_REQUIRED = 0x0008;
        public const int LOCKOUT = 0x0010;
        public const int PASSWD_NOTREQD = 0x0020;
        public const int PASSWD_CANT_CHANGE = 0x0040;
        public const int ENCRYPTED_TEXT_PWD_ALLOWED = 0x0080;
        public const int TEMP_DUPLICATE_ACCOUNT = 0x0100;
        public const int NORMAL_ACCOUNT = 0x0200;
        public const int INTERDOMAIN_TRUST_ACCOUNT = 0x0800;
        public const int WORKSTATION_TRUST_ACCOUNT = 0x1000;
        public const int SERVER_TRUST_ACCOUNT = 0x2000;
        public const int DONT_EXPIRE_PASSWORD = 0x10000;
        public const int MNS_LOGON_ACCOUNT = 0x20000;
        public const int SMARTCARD_REQUIRED = 0x40000;
        public const int TRUSTED_FOR_DELEGATION = 0x80000;
        public const int NOT_DELEGATED = 0x100000;
        public const int USE_DES_KEY_ONLY = 0x200000;
        public const int DONT_REQ_PREAUTH = 0x400000;
        public const int PASSWORD_EXPIRED = 0x800000;
        public const int TRUSTED_TO_AUTH_FOR_DELEGATION = 0x1000000;

        #endregion

        #region Validate

        public bool Authenticate(string userName, string password)
        {
            bool authentic = false;
            try
            {
                entry = new DirectoryEntry("LDAP://" + this.Domain, userName, password);
                object nativeObject = entry.NativeObject;
                User = userName;
                Password = password;
                authentic = true;
            }
            catch (DirectoryServicesCOMException) { }
            return authentic;
        }

        #endregion

        #region Searcher

        public Dictionary<string, object> GetInfo(SearchKey key, object value)
        {
            DirectorySearcher search = new DirectorySearcher(entry);
            search.PropertiesToLoad.Add("cn");
            search.PropertiesToLoad.Add("mail");

            search.Filter = string.Format("(&(anr={0})(objectCategory=person))", value);
            search.PageSize = 1000;

            Dictionary<string, object> obj = new Dictionary<string, object>();

            SearchResult sr = search.FindOne();

            foreach (string s in sr.GetDirectoryEntry().Properties.PropertyNames)
            {
                obj.Add(s, sr.GetDirectoryEntry().Properties[s].Value);
            }

            return obj;
        }

        #endregion

        #region User

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ldapPath"></param>
        /// <param name="userName"></param>
        /// <param name="userPassword"></param>
        /// <param name="passwordproperty">
        /// See Constants
        /// *TRUSTED_FOR_DELEGATION - default
        /// </param>
        /// <returns></returns>
        public string CreateUserAccount(string ldapPath, string userName, string userPassword, int passwordproperty = 0x80000)
        {
            string oGUID = string.Empty;
            try
            {
                DirectoryEntry newUser = entry.Children.Add
                    ("CN=" + userName, "user");
                newUser.Properties["samAccountName"].Value = userName;
                newUser.CommitChanges();
                oGUID = newUser.Guid.ToString();

                newUser.Invoke("SetPassword", new object[] { userPassword });


                int val = (int)newUser.Properties["userAccountControl"].Value;

                newUser.Properties["userAccountControl"].Value = val | passwordproperty | DONT_EXPIRE_PASSWORD;

                newUser.CommitChanges();
                entry.Close();
                newUser.Close();
            }
            catch (System.DirectoryServices.DirectoryServicesCOMException E)
            {
                //DoSomethingwith --> E.Message.ToString();
                throw E;

            }
            return oGUID;
        }

        public void EnableUser(string userDn)
        {
            try
            {
                DirectoryEntry user = new DirectoryEntry(userDn);
                int val = (int)user.Properties["userAccountControl"].Value;
                user.Properties["userAccountControl"].Value = val & NORMAL_ACCOUNT;

                user.CommitChanges();
                user.Close();
            }
            catch (System.DirectoryServices.DirectoryServicesCOMException E)
            {
                //DoSomethingWith --> E.Message.ToString();
                throw E;

            }
        }

        public void Disable(string userDn)
        {
            try
            {
                DirectoryEntry user = new DirectoryEntry(userDn);
                int val = (int)user.Properties["userAccountControl"].Value;
                user.Properties["userAccountControl"].Value = val | ACCOUNTDISABLE;
                //ADS_UF_ACCOUNTDISABLE;

                user.CommitChanges();
                user.Close();
            }
            catch (System.DirectoryServices.DirectoryServicesCOMException E)
            {
                //DoSomethingWith --> E.Message.ToString();
                throw E;

            }
        }

        public void Unlock(string userDn)
        {
            try
            {
                DirectoryEntry uEntry = new DirectoryEntry(userDn);
                uEntry.Properties["LockOutTime"].Value = 0; //unlock account

                uEntry.CommitChanges(); //may not be needed but adding it anyways

                uEntry.Close();
            }
            catch (System.DirectoryServices.DirectoryServicesCOMException E)
            {
                //DoSomethingWith --> E.Message.ToString();
                throw E;

            }
        }

        public bool IsLocked
        {
            get { return Convert.ToBoolean(entry.InvokeGet("IsAccountLocked")); }
            set { entry.InvokeSet("IsAccountLocked", value); }
        }

        public void ResetPassword(string userDn, string password)
        {
            DirectoryEntry uEntry = new DirectoryEntry(userDn);
            uEntry.Invoke("SetPassword", new object[] { password });
            uEntry.Properties["LockOutTime"].Value = 0; //unlock account

            uEntry.Close();
        }

        public static void Rename(string objectDn, string newName)
        {
            DirectoryEntry child = new DirectoryEntry("LDAP://" + objectDn);
            child.Rename("CN=" + newName);
        }

        #endregion

        #region Group

        public void CreateGroup(string Name)
        {
            if (!DirectoryEntry.Exists("LDAP://CN=" + Name + "," + Domain))
            {
                try
                {
                    DirectoryEntry group = entry.Children.Add("CN=" + Name, "group");
                    group.Properties["sAmAccountName"].Value = Name;
                    group.CommitChanges();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            else { Console.WriteLine(Name + " already exists"); }
        }

        public void DeleteGroup(string Name)
        {
            if (DirectoryEntry.Exists("LDAP://" + Name))
            {
                try
                {
                    DirectoryEntry group = entry.Children.Find(Name, "group");
                    entry.Children.Remove(group);
                    group.CommitChanges();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            else
            {
                throw new Exception(Name + " doesn't exist");
            }
        }

        public void AddUserToGroup(string Name)
        {
            try
            {
                DirectoryEntry group = entry.Children.Find(Name, "group");
                group.Properties["member"].Add(User);
                group.CommitChanges();
                group.Close();
            }
            catch (System.DirectoryServices.DirectoryServicesCOMException E)
            {
                //doSomething with E.Message.ToString();
                throw E;
            }
        }

        public ArrayList Groups(bool recursive = true)
        {
            ArrayList groupMemberships = new ArrayList();
            return AttributeValuesMultiString("memberOf", User, groupMemberships, recursive);
        }

        #endregion

        #region Common

        public void Move(string objectLocation, string newLocation)
        {
            //For brevity, removed existence checks

            DirectoryEntry eLocation = new DirectoryEntry("LDAP://" + objectLocation);
            DirectoryEntry nLocation = new DirectoryEntry("LDAP://" + newLocation);
            string newName = eLocation.Name;
            eLocation.MoveTo(nLocation, newName);
            nLocation.Close();
            eLocation.Close();
        }

        public ArrayList AttributeValuesMultiString(string attributeName, string objectDn, ArrayList valuesCollection, bool recursive)
        {
            DirectoryEntry ent = new DirectoryEntry(objectDn);
            PropertyValueCollection ValueCollection = ent.Properties[attributeName];
            IEnumerator en = ValueCollection.GetEnumerator();

            while (en.MoveNext())
            {
                if (en.Current != null)
                {
                    if (!valuesCollection.Contains(en.Current.ToString()))
                    {
                        valuesCollection.Add(en.Current.ToString());
                        if (recursive)
                        {
                            AttributeValuesMultiString(attributeName, "LDAP://" + en.Current.ToString(), valuesCollection, true);
                        }
                    }
                }
            }
            ent.Close();
            ent.Dispose();
            return valuesCollection;
        }

        public string AttributeValuesSingleString(string attributeName, string objectDn)
        {
            string strValue;
            DirectoryEntry ent = new DirectoryEntry(objectDn);
            strValue = ent.Properties[attributeName].Value.ToString();
            ent.Close();
            ent.Dispose();
            return strValue;
        }

        public static ArrayList GetUsedAttributes(string objectDn)
        {
            DirectoryEntry objRootDSE = new DirectoryEntry("LDAP://" + objectDn);
            ArrayList props = new ArrayList();

            foreach (string strAttrName in objRootDSE.Properties.PropertyNames)
            {
                props.Add(strAttrName);
            }
            return props;
        }

        #endregion

        #region Enumeration

        public enum SearchKey
        {
            homemdb,
            countrycode,
            cn,
            msexchuseraccountcontrol,
            mailnickname,
            msexchhomeservername,
            msexchhidefromaddresslists,
            msexchalobjectversion,
            usncreated,
            objectguid,
            msexchrequireauthtosendto,
            whenchanged,
            memberof,
            accountexpires,
            displayname,
            primarygroupid,
            badpwdcount,
            objectclass,
            instancetype,
            msmqdigests,
            objectcategory,
            samaccounttype,
            whencreated,
            lastlogon,
            useraccountcontrol,
            msmqsigncertificates,
            samaccountname,
            userparameters,
            mail,
            msexchmailboxsecuritydescriptor,
            adspath,
            lockouttime,
            homemta,
            description,
            msexchmailboxguid,
            pwdlastset,
            logoncount,
            codepage,
            name,
            usnchanged,
            legacyexchangedn,
            proxyaddresses,
            userprincipalname,
            admincount,
            badpasswordtime,
            objectsid,
            msexchpoliciesincluded,
            mdbusedefaults,
            distinguishedname,
            showinaddressbook,
            givenname,
            textencodedoraddress,
            lastlogontimestamp
        }

        #endregion

        public void Dispose()
        {
            entry.Dispose();
            entry = null;
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}
