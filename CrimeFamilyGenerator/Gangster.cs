using System.Collections.Generic;

namespace CrimeFamilyGenerator
{
    public class MafiaFamily : HashSet<Gangster>
    {
        public string FamilyName { get; set; }
    }

    public class Gangster
    {
        private string _accountName;
        private string _fullName;
        public int ID { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public bool IsFemale { get; set; }
        public string Department { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Title { get; set; }
        public string Gangname { get; set; }
        public string Manager { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }

        public string Fullname
        {
            get
            {
                _fullName = string.Format("{0} {1}", Firstname, Lastname);
                return _fullName;
            }
            set { _fullName = value; }
        }
        public string Accountname
        {
            get
            {
                _accountName = string.Format("{0}.{1}", Firstname, Lastname).Replace("'", ".").Replace(" ", ".").ToLower();
                return _accountName;
            }
            set { _accountName = value; }
        }

        public override string ToString()
        {
            return string.Format("[{0}/{1}] {2} ({3}) is in the {4} department.",
                                 City,Title,Firstname, Lastname, Department);
        }
    }
}