using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CrimeFamilyGenerator
{
    public enum GangsterType
    {
        Admin, InADepertment, Associate
    }

    public class GansterNameGenerator
    {
        private readonly HashSet<int> _usedIndexes = new HashSet<int>();

        private const string CityFile = @"Metadata/cities.txt";
        private const string DepratmentFile = @"Metadata/departments.txt";
        private const string FemaleNamesFile = @"Metadata/femalenames.txt";
        private const string LastNamesFile = @"Metadata/lastnames.txt";
        private const string MaleNamesFile = @"Metadata/malenames.txt";
        private const string ResultsFileADImport = "Results/AD_Import.txt";
        private const string ResultsFileXml = "Results/results.xml";
        private const string SettingsFile = "Metadata/settings.xml";
        private const string TitlesFile = "Metadata/titles.xml";

        private Dictionary<int, Gangster> _potentialGangsters;
        private MafiaFamily _laFamiliaNetworkOfGangster;
        private GangseterNameSettings _settings;
        private GangsterTitleDefinitions _titles;

        public void Generate()
        {
            ReadSettingsFiles();
            if (_settings == null)
                return;

            // Read files;
            var cities = File.ReadAllLines(CityFile).ToList();
            var departments = File.ReadAllLines(DepratmentFile).ToList();
            _laFamiliaNetworkOfGangster = new MafiaFamily();

            CreatePoolOfPotentialGangster();

            // Start the legendary family
            InitiateGodFather();
            CreateMobsInCity(cities, departments);
            SetManagers();

            WriteFiles();
            Console.WriteLine("Your family now has a network of {0} mobs, thugs and lowlifes! ", _laFamiliaNetworkOfGangster.Count);
        }

        #region - Settings -
        private void ReadSettingsFiles()
        {
            if (!File.Exists(SettingsFile))
            {
                Console.WriteLine("The required file \"settings.xml\" was not found!");
                return;
            }

            _settings = File.ReadAllText(SettingsFile, Encoding.UTF8).DeserializeTo<GangseterNameSettings>();
            _titles = File.ReadAllText(TitlesFile, Encoding.UTF8).DeserializeTo<GangsterTitleDefinitions>();
        }

        #endregion

        #region - Map Titles -
        private void SetManagers()
        {
            var cities = _laFamiliaNetworkOfGangster.Select(p => p.City).Distinct().ToList();

            foreach (var city in cities)
            {
                var gangstersInCity = _laFamiliaNetworkOfGangster.Where(p => p.City == city).ToList();
                if (gangstersInCity.Count < 1)
                    continue;

                foreach (var title in _titles.GangsterTitles)
                {
                    if (title.TileOfManager == _titles.TopBossTitle)
                    {
                        var godfather = _laFamiliaNetworkOfGangster.First(p => p.Title == _titles.TopBossTitle);
                        var subTitle = title;
                        foreach (var gangster in gangstersInCity.Where(p => p.Title == subTitle.Title))
                        {
                            gangster.Manager = godfather.Accountname;
                        }
                    }
                    else
                    {
                        MapManagers(gangstersInCity, title.TileOfManager, title.Title);
                    }
                }

            }
        }

        private void MapManagers(List<Gangster> gangsters, string managerTitle, string subTitle)
        {
            var managers = gangsters.Where(p => p.Title == managerTitle).ToList();
            if (managers.Count == 0)
                return;
            var subordinates = gangsters.Where(p => p.Title == subTitle).ToList();

            foreach (var sub in subordinates)
            {
                var manager = managers.PickRandom();
                sub.Manager = manager.Accountname;
            }
        }
        #endregion

        #region - Mixup -
        private void CreatePoolOfPotentialGangster()
        {
            _potentialGangsters = new Dictionary<int, Gangster>();
            var lastnames = File.ReadAllLines(LastNamesFile, Encoding.UTF8).ToList();
            var maleNames = Mix(File.ReadAllLines(MaleNamesFile, Encoding.UTF8), lastnames, false).ToList();
            var femaleNames = Mix(File.ReadAllLines(FemaleNamesFile, Encoding.UTF8), lastnames, true).ToList();
            Console.WriteLine("Picking out names... Male: {0} | Female {1}", maleNames.Count, femaleNames.Count);
            var rnd = new Random(Guid.NewGuid().GetHashCode());
            var id = 1000000;
            var relocate = 1;
            var max = 600000;
            for (var i = 0; i < max; i++)
            {
                Gangster gangster;
                while (true)
                {
                    gangster = rnd.Next(0, 6) < 2 ? femaleNames.PickRandom() : maleNames.PickRandom();
                    if (gangster.ID == 0)
                        break;
                    Console.Title = string.Format("Generateing: {0} / {1} | {2:N2}%", _potentialGangsters.Count,max,(100*(double)_potentialGangsters.Count/max));
                }
                gangster.ID = id++;
                _potentialGangsters.Add(i, gangster);
            }
        }
        private IEnumerable<Gangster> Mix(IEnumerable<string> firstnames, IEnumerable<string> lastnames, bool isFemale)
        {
            return (from firstname in firstnames
                    from lastname in lastnames
                    select new Gangster
                    {
                        Firstname = firstname,
                        Lastname = lastname,
                        IsFemale = isFemale
                    }).ToList();
        }
        #endregion

        #region - Create la Familia -
        private void CreateMobsInCity(IEnumerable<string> cities, List<string> departments)
        {
            foreach (var cityAndStateName in cities)
            {
                if (cityAndStateName.StartsWith("#")) continue;

                var citySplit = cityAndStateName.Split(';');
                var cityName = citySplit[0];
                var state = citySplit[1];

                Console.WriteLine("City: {0}", cityAndStateName);

                foreach (var gangsterTitle in _titles.GangsterTitles)
                {
                    var addToDepartments = new List<string>();
                    switch (gangsterTitle.Type)
                    {
                        case GangsterType.Admin:
                            addToDepartments.Add("Admin");
                            break;
                        case GangsterType.InADepertment:
                            addToDepartments.AddRange(departments);
                            break;
                        default:
                            addToDepartments.Add("Associates");
                            break;
                    }
                    foreach (var dep in addToDepartments)
                    {
                        AddGangstersToLaFamilia(gangsterTitle, cityName, state, dep);
                    }
                }
                Console.WriteLine("\tGangsters in city: {0}", _laFamiliaNetworkOfGangster.Count(p => p.City == cityName));
            }
        }

        private void InitiateGodFather()
        {
            var rnd = new Random(Guid.NewGuid().GetHashCode());
            var godFather = new Gangster
            {
                Title = _titles.TopBossTitle,
                Firstname = _settings.GodfatherFirstname,
                Lastname = _settings.GodfatherLastname,
                State = _settings.GodfatherState,
                City = _settings.GodfatherCity,
                Department = "Admin",
                IsFemale = _settings.GodfatherIsActuallyAWoman,
                ID = 100000,
                Gangname = _settings.FamilyName,
                PhoneNumber = string.Format("{0}-{1}-{2}", rnd.Next(200, 900),
                                            rnd.Next(0, 999).ToString().PadLeft(3, '0'),
                                            rnd.Next(0, 9999).ToString().PadLeft(4, '0')),
            };
            _laFamiliaNetworkOfGangster.Add(godFather);
        }

        private Gangster RecruitAGangster()
        {
            var rnd = new Random(Guid.NewGuid().GetHashCode());
            var index = rnd.Next(0, _potentialGangsters.Count - 1);
            if (_usedIndexes.Contains(index))
                return RecruitAGangster();
            _usedIndexes.Add(index);
            return _potentialGangsters[index];
        }

        private void AddGangstersToLaFamilia(GangsterTitleDefinition titleDefinition, string cityName, string state, string department)
        {
            var rnd = new Random(Guid.NewGuid().GetHashCode());
            var numberOfGangsters = rnd.Next(titleDefinition.Minimum, titleDefinition.Maximum + 1);

            Console.WriteLine("\t\tAdding {0} {2} in the {1} department", numberOfGangsters, department, titleDefinition.Title);

            for (var i = 0; i < numberOfGangsters; i++)
            {
                var gangster = RecruitAGangster();
                if (!string.IsNullOrEmpty(gangster.Title))
                {
                    throw new Exception(gangster.ToString());
                }
                gangster.City = cityName;
                gangster.State = state;
                gangster.Department = department;
                gangster.Title = titleDefinition.Title;
                gangster.Gangname = _settings.FamilyName;
                gangster.Password = _settings.ADGenerateUniquePassword
                                        ? Guid.NewGuid().ToString().Substring(0, 6)
                                        : _settings.ADPassword;

                // This will create some form of US phone number (at least the format ;)
                gangster.PhoneNumber = string.Format("{0}-{1}-{2}", rnd.Next(200, 900),
                                                     rnd.Next(0, 999).ToString().PadLeft(3, '0'),
                                                     rnd.Next(0, 9999).ToString().PadLeft(4, '0'));
                _laFamiliaNetworkOfGangster.Add(gangster);
            }
        }

        #endregion

        #region - Write files -

        private void WriteFiles()
        {
            if (!Directory.Exists("Results"))
            {
                Directory.CreateDirectory("Results");
            }
            File.WriteAllText(ResultsFileXml, _laFamiliaNetworkOfGangster.Serialize(true), Encoding.UTF8);
            WriteADFile();
        }

        private void WriteADFile()
        {
            // Needs spesific order when importing, so that the manager already exists when being references
            var titleOrder = new List<string>
                           {
                               "Godfather",
                               "Boss",
                               "Underboss",
                               "Consigliere",
                               "Caporegime",
                               "Soldier",
                               "Associate/Informer",
                               "Associate/Thug",
                               "Associate/Lowlife"
                           };

            var lines = new List<string>();
            foreach (var title in titleOrder)
            {
                CreateADLinesForGangsters(_laFamiliaNetworkOfGangster.Where(p => p.Title == title).ToList(), lines);
            }
            File.WriteAllLines(ResultsFileADImport, lines, Encoding.UTF8);
        }

        private void CreateADLinesForGangsters(IEnumerable<Gangster> gangsters, List<string> lines)
        {
            var propertyInfo = typeof(Gangster).GetProperties();
            foreach (var g in gangsters)
            {
                var gangsterAdLine = "" + _settings.ADFormatString;
                foreach (var info in propertyInfo)
                {
                    var replaceName = "{" + info.Name + "}";
                    var value = (info.GetValue(g, null) ?? string.Empty).ToString().Replace("?", "_");
                    gangsterAdLine = gangsterAdLine.Replace(replaceName, value);
                }
                lines.Add(gangsterAdLine);
            }
        }

        #endregion

    }
}