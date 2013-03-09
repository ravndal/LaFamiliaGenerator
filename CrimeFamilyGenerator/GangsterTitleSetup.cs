using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace CrimeFamilyGenerator
{
    public class GangsterTitleDefinitions
    {
        public string TopBossTitle { get; set; }
        public List<GangsterTitleDefinition> GangsterTitles { get; set; }

    }
}