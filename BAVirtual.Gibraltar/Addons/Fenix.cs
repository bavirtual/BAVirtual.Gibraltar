using BAVirtual.Gibraltar.Interfaces;
using BAVirtual.Gibraltar.Models;
using Spectre.Console;
using SQLite;

namespace BAVirtual.Gibraltar.Addons
{
    public class Fenix : IAddon
    {
        private readonly string databasePath = "C:\\ProgramData\\Fenix\\Navdata\\nd.db3";
        private SQLiteConnection? _db;

        private int airportId;
        private int runway09Id;
        private int runway27Id;
        private int terminal09Id;
        private int terminal27Id;
        private List<Waypoint> waypoints = [];
        private List<Navaid> navaids = [];
        private List<Terminal> terminals = [];
        private List<TerminalLeg> terminalLegs09 = [];
        private List<TerminalLeg> terminalLegs27 = [];
        private List<TerminalLegEx> terminalLegExs = [];
        private List<Runway> runways = [];

        public ProcessResponse ProcessGibraltarData()
        {
            ProcessResponse response = new();

            if (!DatabaseInit())
            {
                response.Success = false;
                response.Info = "Database File could not be Found";
                return response;
            }

            if (ExistingDataCheck())
            {
                response.Success = false;
                response.Info = "Database already contains this data";
                return response;
            }

            #region Backup
            // GET EXISTING DATA
            BackupDB();

            AnsiConsole.WriteLine("Backup has been saved");
            Thread.Sleep(1000);
            #endregion Backup

            try
            {
                #region Airport Data
                // GET EXISTING DATA
                airportId = GetAirportID();
                runway09Id = GetRunwayID("09");
                runway27Id = GetRunwayID("27");
                GetRunways();

                AnsiConsole.WriteLine("Airport Data has been collated");
                Thread.Sleep(1000);
                #endregion Airport Data

                #region Terminals
                // CREATE THE ARRIVALS
                CreateTerminals();

                AnsiConsole.WriteLine("Terminal Data has been inserted");
                Thread.Sleep(1000);
                #endregion Terminals

                #region Waypoints
                // CREATE THE WAYPOINTS
                CreateWaypoints();

                AnsiConsole.WriteLine("Waypoint Data has been inserted");
                Thread.Sleep(1000);
                #endregion Terminals

                #region Load Additional Waypoints
                // CREATE THE WAYPOINTS
                LoadAdditionalWaypoints();

                AnsiConsole.WriteLine("Additional Waypoint Data has been collated");
                Thread.Sleep(1000);
                #endregion Terminals

                #region Terminal Legs
                // CREATE THE ARRIVALS
                CreateTerminalLegs();

                AnsiConsole.WriteLine("Terminal Leg Data has been inserted");
                Thread.Sleep(1000);
                #endregion Terminal Legs
            }
            catch (Exception)
            {
                RollbackDB();

                response.Success = false;
                response.Info = "An exception occured adding the data. We have restored the backup";
                return response;
            }

            response.Success = true;

            _db?.Close();
            return response;
        }

        private void BackupDB()
        {
            File.Copy(databasePath, databasePath + ".bak", true);
        }

        private void RollbackDB()
        {
            File.Copy(databasePath + ".bak", databasePath, true);
        }

        #region Database Functions
        private bool DatabaseInit()
        {
            try
            {
                _db = new SQLiteConnection("C:\\ProgramData\\Fenix\\Navdata\\nd.db3");
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private bool ExistingDataCheck()
        {
            if (_db != null)
            {
                var result = _db.Table<Waypoint>()
                    .Where(r => r.Ident == "BUGOV")
                    .FirstOrDefault();

                if (result?.Ident == "BUGOV")
                {
                    // it already exists... let's kill this process
                    return true;
                }
            }

            return false;
        }

        private int GetAirportID()
        {
            if (_db != null)
            {
                var result = _db.Table<Airport>()
                    .Where(r => r.Icao == "LXGB")
                    .FirstOrDefault();

                return result?.Id ?? 0;
            }

            return 0;
        }

        private void GetRunways()
        {
            if (_db != null)
            {
                runways =
                [
                    .. _db.Table<Runway>()
                                        .Where(r => r.AirportId == airportId)
,
                ];
            }
        }

        private int GetRunwayID(string rwy)
        {
            if (_db != null)
            {
                var result = _db.Table<Runway>()
                    .Where(r => r.AirportId == airportId)
                    .Where(r => r.Ident == rwy)
                    .FirstOrDefault();

                return result?.Id ?? 0;
            }

            return 0;
        }

        private void LoadAdditionalWaypoints()
        {
            if (_db != null)
            {
                var pimos = _db.Table<Waypoint>()
                    .Where(r => r.Ident == "PIMOS")
                    .First();
                var upmup = _db.Table<Waypoint>()
                    .Where(r => r.Ident == "UPMUP")
                    .First();

                waypoints.Add(pimos);
                waypoints.Add(upmup);
            }
        }

        private void CreateTerminals()
        {
            if (_db != null)
            {
                var oldestIdRow = _db.Table<Terminal>()
                    .OrderByDescending(r => r.Id)
                    .FirstOrDefault();

                int oldestId = oldestIdRow?.Id ?? 0;

                terminals = [
                    new Terminal { Id = ++oldestId, AirportId = airportId, Proc = 3, Icao = "LXGB", FullName = "R09-V RNAV 09", Name = "R09-V", Rwy = "09", RwyId = runway09Id },
                    new Terminal { Id = ++oldestId, AirportId = airportId, Proc = 3, Icao = "LXGB", FullName = "R27-V RNAV 27", Name = "R27-V", Rwy = "27", RwyId = runway27Id }
                ];

                _db.InsertAll(terminals);

                terminal09Id = terminals.Single(t => t.RwyId == runway09Id).Id;
                terminal27Id = terminals.Single(t => t.RwyId == runway27Id).Id;
            }
        }

        private void CreateWaypoints()
        {
            if (_db != null)
            {
                var oldestWaypointIdRow = _db.Table<Waypoint>()
                    .OrderByDescending(r => r.Id)
                    .FirstOrDefault();

                int oldestWaypointId = oldestWaypointIdRow?.Id ?? 0;

                waypoints = [
                    new Waypoint { Id = ++oldestWaypointId, Ident = "BUGOV", Collocated = false, Name = "BUGOV", Latitude = 36.080958, Longtitude = -5.239742 },
                    new Waypoint { Id = ++oldestWaypointId, Ident = "GB092", Collocated = false, Name = "GB092", Latitude = 36.06885, Longtitude = -5.299592 },
                    new Waypoint { Id = ++oldestWaypointId, Ident = "GB094", Collocated = false, Name = "GB094", Latitude = 36.093792, Longtitude = -5.390228 },
                    new Waypoint { Id = ++oldestWaypointId, Ident = "GB09F", Collocated = false, Name = "GB09F", Latitude = 36.106847, Longtitude = -5.403039 },
                    new Waypoint { Id = ++oldestWaypointId, Ident = "GB096", Collocated = false, Name = "GB096", Latitude = 36.150164, Longtitude = -5.378714 },
                    new Waypoint { Id = ++oldestWaypointId, Ident = "GB270", Collocated = false, Name = "GB270", Latitude = 36.100419, Longtitude = -4.928906 },
                    new Waypoint { Id = ++oldestWaypointId, Ident = "GB272", Collocated = false, Name = "GB272", Latitude = 36.087944, Longtitude = -5.092822 },
                    new Waypoint { Id = ++oldestWaypointId, Ident = "KUXOX", Collocated = false, Name = "KUXOX", Latitude = 36.157414, Longtitude = -5.166344 },
                    new Waypoint { Id = ++oldestWaypointId, Ident = "GB276", Collocated = false, Name = "GB276", Latitude = 36.155358, Longtitude = -5.227647 },
                    new Waypoint { Id = ++oldestWaypointId, Ident = "GB27F", Collocated = false, Name = "GB27F", Latitude = 36.154664, Longtitude = -5.248211 },
                    new Waypoint { Id = ++oldestWaypointId, Ident = "GBM02", Collocated = false, Name = "GBM02", Latitude = 36.151669, Longtitude = -5.335536 },
                    new Waypoint { Id = ++oldestWaypointId, Ident = "GBM03", Collocated = false, Name = "GBM03", Latitude = 36.150828, Longtitude = -5.359675 },
                    new Waypoint { Id = ++oldestWaypointId, Ident = "GBM04", Collocated = false, Name = "GBM04", Latitude = 36.086842, Longtitude = -5.400922 },
                    new Waypoint { Id = ++oldestWaypointId, Ident = "GBM05", Collocated = false, Name = "GBM05", Latitude = 36.072736, Longtitude = -5.389931 },
                    new Waypoint { Id = ++oldestWaypointId, Ident = "GBM06", Collocated = false, Name = "GBM06", Latitude = 36.040867, Longtitude = -5.3298 },
                    new Waypoint { Id = ++oldestWaypointId, Ident = "GB501", Collocated = false, Name = "GB501", Latitude = 36.141175, Longtitude = -5.0163 },
                    new Waypoint { Id = ++oldestWaypointId, Ident = "GB509", Collocated = false, Name = "GB509", Latitude = 36.109142, Longtitude = -5.357464 },
                    new Waypoint { Id = ++oldestWaypointId, Ident = "GB513", Collocated = false, Name = "GB513", Latitude = 36.110183, Longtitude = -5.316917 },
                    new Waypoint { Id = ++oldestWaypointId, Ident = "GB409", Collocated = false, Name = "GB409", Latitude = 36.123483, Longtitude = -5.377294 },
                    new Waypoint { Id = ++oldestWaypointId, Ident = "GB403", Collocated = false, Name = "GB403", Latitude = 36.138483, Longtitude = -5.321014 },
                ];

                _db.InsertAll(waypoints);

                waypoints.ForEach(waypoint =>
                {
                    _db.Insert(new WaypointLookup { Ident = waypoint.Ident, Country = "LE", Id = waypoint.Id });
                });

                var oldestNavaidIdRow = _db.Table<Navaid>()
                    .OrderByDescending(r => r.Id)
                    .FirstOrDefault();

                int oldestNavaidId = oldestNavaidIdRow?.Id ?? 0;

                navaids = [
                    new Navaid { Id = ++oldestNavaidId, Ident = "GBR", Type = 3, Name = "GIBRALTAR", Latitude = 36.142828, Longtitude = -5.342906, Elevation = 1397, SlavedVar = -1, MagneticVariation = -1, Range = 25, Usage = "B", Channel = "", Freq = 18046976 }
                ];

                _db.InsertAll(navaids);
            }
        }

        private void CreateTerminalLegs()
        {
            if (_db != null)
            {
                var oldestIdRow = _db.Table<TerminalLeg>()
                    .OrderByDescending(r => r.Id)
                    .FirstOrDefault();

                int oldestId = oldestIdRow?.Id ?? 0;

                // RUNWAY 27
                var pimos = waypoints.Single(w => w.Ident == "PIMOS");
                int pimos27Id = ++oldestId;
                terminalLegs27.Add(new TerminalLeg { Id = pimos27Id, TerminalId = terminal27Id, Transition = "PIMOS", Type = "A", TrackCode = "IF", WptId = pimos.Id, WptLat = pimos.Latitude, WptLon = pimos.Longtitude, Alt = "9000A" });
                terminalLegExs.Add(new TerminalLegEx { Id = pimos27Id, IsFlyOver = false, SpeedLimit = 240, SpeedLimitDescription = "B" });

                var gb270 = waypoints.Single(w => w.Ident == "GB270");
                int gb270Id = ++oldestId;
                terminalLegs27.Add(new TerminalLeg { Id = gb270Id, TerminalId = terminal27Id, Transition = "PIMOS", Type = "A", TrackCode = "TF", WptId = gb270.Id, WptLat = gb270.Latitude, WptLon = gb270.Longtitude });
                terminalLegExs.Add(new TerminalLegEx { Id = gb270Id, IsFlyOver = false });

                var gb272 = waypoints.Single(w => w.Ident == "GB272");
                int gb272Id = ++oldestId;
                var gb501 = waypoints.Single(w => w.Ident == "GB501");
                terminalLegs27.Add(new TerminalLeg { Id = gb272Id, TerminalId = terminal27Id, Transition = "PIMOS", Type = "A", TrackCode = "RF", WptId = gb272.Id, WptLat = gb272.Latitude, WptLon = gb272.Longtitude, TurnDir = "R", Course = 320.1, Distance = 9.4, CenterId = gb501.Id, CenterLat = gb501.Latitude, CenterLon = gb501.Longtitude });
                terminalLegExs.Add(new TerminalLegEx { Id = gb272Id, IsFlyOver = false });

                var kuxox = waypoints.Single(w => w.Ident == "KUXOX");
                int kuxoxId = ++oldestId;
                terminalLegs27.Add(new TerminalLeg { Id = kuxoxId, TerminalId = terminal27Id, Transition = "PIMOS", Type = "A", TrackCode = "TF", WptId = kuxox.Id, WptLat = kuxox.Latitude, WptLon = kuxox.Longtitude });
                terminalLegExs.Add(new TerminalLegEx { Id = kuxoxId, IsFlyOver = false, SpeedLimit = 210, SpeedLimitDescription = "B" });

                var gb276 = waypoints.Single(w => w.Ident == "GB276");
                int gb276Id = ++oldestId;
                terminalLegs27.Add(new TerminalLeg { Id = gb276Id, TerminalId = terminal27Id, Transition = "PIMOS", Type = "A", TrackCode = "TF", WptId = gb276.Id, WptLat = gb276.Latitude, WptLon = gb276.Longtitude });
                terminalLegExs.Add(new TerminalLegEx { Id = gb276Id, IsFlyOver = false });

                var gb27f = waypoints.Single(w => w.Ident == "GB27F");
                int gb27fTfId = ++oldestId;
                int gb27fIfId = ++oldestId;
                terminalLegs27.Add(new TerminalLeg { Id = gb27fTfId, TerminalId = terminal27Id, Transition = "PIMOS", Type = "A", TrackCode = "TF", WptId = gb27f.Id, WptLat = gb27f.Latitude, WptLon = gb27f.Longtitude, Alt = "1500" });
                terminalLegs27.Add(new TerminalLeg { Id = gb27fIfId, TerminalId = terminal27Id, Type = "R", TrackCode = "IF", WptId = gb27f.Id, WptLat = gb27f.Latitude, WptLon = gb27f.Longtitude, Alt = "1500" });
                terminalLegExs.Add(new TerminalLegEx { Id = gb27fTfId, IsFlyOver = false, SpeedLimit = 160, SpeedLimitDescription = "B" });
                terminalLegExs.Add(new TerminalLegEx { Id = gb27fIfId, IsFlyOver = false, SpeedLimit = 160, SpeedLimitDescription = "B" });

                var rw27 = runways.Single(r => r.Ident == "27");
                int rw27Id = ++oldestId;
                terminalLegs27.Add(new TerminalLeg { Id = rw27Id, TerminalId = terminal27Id, Type = "R", TrackCode = "TF", WptLat = rw27.Latitude, WptLon = rw27.Longtitude, Course = 269.0, Distance = 4.5, Alt = "MAP", Vnav = 3.0 });
                terminalLegExs.Add(new TerminalLegEx { Id = rw27Id, IsFlyOver = true });

                var gbm03 = waypoints.Single(w => w.Ident == "GBM03");
                int gbm03Id = ++oldestId;
                terminalLegs27.Add(new TerminalLeg { Id = gbm03Id, TerminalId = terminal27Id, Type = "R", TrackCode = "TF", WptId = gbm03.Id, WptLat = gbm03.Latitude, WptLon = gbm03.Longtitude });
                terminalLegExs.Add(new TerminalLegEx { Id = gbm03Id, IsFlyOver = false });

                var gbm04 = waypoints.Single(w => w.Ident == "GBM04");
                int gbm04Id = ++oldestId;
                var gb509 = waypoints.Single(w => w.Ident == "GB509");
                terminalLegs27.Add(new TerminalLeg { Id = gbm04Id, TerminalId = terminal27Id, Type = "R", TrackCode = "RF", WptId = gbm04.Id, WptLat = gbm04.Latitude, WptLon = gbm04.Longtitude, TurnDir = "L", Course = 148.5, Distance = 5.2, CenterId = gb509.Id, CenterLat = gb509.Latitude, CenterLon = gb509.Longtitude });
                terminalLegExs.Add(new TerminalLegEx { Id = gbm04Id, IsFlyOver = false, SpeedLimit = 185, SpeedLimitDescription = "B" });

                var gbm05 = waypoints.Single(w => w.Ident == "GBM05");
                int gbm05Id = ++oldestId;
                terminalLegs27.Add(new TerminalLeg { Id = gbm05Id, TerminalId = terminal27Id, Type = "R", TrackCode = "TF", WptId = gbm05.Id, WptLat = gbm05.Latitude, WptLon = gbm05.Longtitude });
                terminalLegExs.Add(new TerminalLegEx { Id = gbm05Id, IsFlyOver = false });

                var gbm06 = waypoints.Single(w => w.Ident == "GBM06");
                int gbm06Id = ++oldestId;
                var gb513 = waypoints.Single(w => w.Ident == "GB513");
                terminalLegs27.Add(new TerminalLeg { Id = gbm06Id, TerminalId = terminal27Id, Type = "R", TrackCode = "RF", WptId = gbm06.Id, WptLat = gbm06.Latitude, WptLon = gbm06.Longtitude, TurnDir = "L", Course = 99.4, Distance = 3.6, CenterId = gb513.Id, CenterLat = gb513.Latitude, CenterLon = gb513.Longtitude });
                terminalLegExs.Add(new TerminalLegEx { Id = gbm06Id, IsFlyOver = false });

                var upmup = waypoints.Single(w => w.Ident == "UPMUP");
                int upmupTfId = ++oldestId;
                int upmupHmId = ++oldestId;
                terminalLegs27.Add(new TerminalLeg { Id = upmupTfId, TerminalId = terminal27Id, Type = "R", TrackCode = "TF", WptId = upmup.Id, WptLat = upmup.Latitude, WptLon = upmup.Longtitude, Alt = "4000" });
                terminalLegs27.Add(new TerminalLeg { Id = upmupHmId, TerminalId = terminal27Id, Type = "R", TrackCode = "HM", WptId = upmup.Id, WptLat = upmup.Latitude, WptLon = upmup.Longtitude, TurnDir = "L", Course = 305.0, Distance = 4.0 });
                terminalLegExs.Add(new TerminalLegEx { Id = upmupTfId, IsFlyOver = false });
                terminalLegExs.Add(new TerminalLegEx { Id = upmupHmId, IsFlyOver = false });


                // RUNWAY 09
                int pimos09Id1 = ++oldestId;
                terminalLegs09.Add(new TerminalLeg { Id = pimos09Id1, TerminalId = terminal09Id, Transition = "PIMOS", Type = "A", TrackCode = "IF", WptId = pimos.Id, WptLat = pimos.Latitude, WptLon = pimos.Longtitude, Alt = "9000A" });
                terminalLegExs.Add(new TerminalLegEx { Id = pimos09Id1, IsFlyOver = false, SpeedLimit = 240, SpeedLimitDescription = "B" });

                var bugov = waypoints.Single(w => w.Ident == "BUGOV");
                int bugovId = ++oldestId;
                terminalLegs09.Add(new TerminalLeg { Id = bugovId, TerminalId = terminal09Id, Transition = "PIMOS", Type = "A", TrackCode = "TF", WptId = bugov.Id, WptLat = bugov.Latitude, WptLon = bugov.Longtitude });
                terminalLegExs.Add(new TerminalLegEx { Id = bugovId, IsFlyOver = false });

                var gb092 = waypoints.Single(w => w.Ident == "GB092");
                int gb092Id = ++oldestId;
                terminalLegs09.Add(new TerminalLeg { Id = gb092Id, TerminalId = terminal09Id, Transition = "PIMOS", Type = "A", TrackCode = "TF", WptId = gb092.Id, WptLat = gb092.Latitude, WptLon = gb092.Longtitude });
                terminalLegExs.Add(new TerminalLegEx { Id = gb092Id, IsFlyOver = false });

                var gb094 = waypoints.Single(w => w.Ident == "GB094");
                int gb094Id = ++oldestId;
                var gb403 = waypoints.Single(w => w.Ident == "GB403");
                terminalLegs09.Add(new TerminalLeg { Id = gb094Id, TerminalId = terminal09Id, Transition = "PIMOS", Type = "R", TrackCode = "RF", WptId = gb094.Id, WptLat = gb094.Latitude, WptLon = gb094.Longtitude, Course = 322.3, Distance = 4.9, CenterId = gb403.Id, CenterLat = gb403.Latitude, CenterLon = gb403.Longtitude, TurnDir = "R" });
                terminalLegExs.Add(new TerminalLegEx { Id = gb094Id, IsFlyOver = false });

                var gb09f = waypoints.Single(w => w.Ident == "GB09F");
                int gb09fTfId = ++oldestId;
                int gb09fIfId = ++oldestId;
                terminalLegs09.Add(new TerminalLeg { Id = gb09fTfId, TerminalId = terminal09Id, Transition = "PIMOS", Type = "A", TrackCode = "TF", WptId = gb09f.Id, WptLat = gb09f.Latitude, WptLon = gb09f.Longtitude, Alt = "1500" });
                terminalLegs09.Add(new TerminalLeg { Id = gb09fIfId, TerminalId = terminal09Id, Type = "R", TrackCode = "IF", WptId = gb09f.Id, WptLat = gb09f.Latitude, WptLon = gb09f.Longtitude, Alt = "1500" });
                terminalLegExs.Add(new TerminalLegEx { Id = gb09fTfId, IsFlyOver = false, SpeedLimit = 160, SpeedLimitDescription = "B" });
                terminalLegExs.Add(new TerminalLegEx { Id = gb09fIfId, IsFlyOver = false, SpeedLimit = 160, SpeedLimitDescription = "B" });

                var gb096 = waypoints.Single(w => w.Ident == "GB096");
                int gb096Id = ++oldestId;
                var gb409 = waypoints.Single(w => w.Ident == "GB409");
                terminalLegs09.Add(new TerminalLeg { Id = gb096Id, TerminalId = terminal09Id, Type = "R", TrackCode = "RF", WptId = gb096.Id, WptLat = gb096.Latitude, WptLon = gb096.Longtitude, Course = 88.4, Distance = 3.5, CenterId = gb409.Id, CenterLat = gb409.Latitude, CenterLon = gb409.Longtitude, Alt = "380A", Vnav = 3.0, TurnDir = "R" });
                terminalLegExs.Add(new TerminalLegEx { Id = gb096Id, IsFlyOver = false });

                var rw09 = runways.Single(r => r.Ident == "09");
                int rw09Id = ++oldestId;
                terminalLegs09.Add(new TerminalLeg { Id = rw09Id, TerminalId = terminal09Id, Type = "R", TrackCode = "TF", WptLat = rw09.Latitude, WptLon = rw09.Longtitude, Course = 89.0, Distance = 1.0, Alt = "MAP", Vnav = 3.0 });
                terminalLegExs.Add(new TerminalLegEx { Id = rw09Id, IsFlyOver = true });

                var gbm02 = waypoints.Single(w => w.Ident == "GBM02");
                int gbm02Id = ++oldestId;
                terminalLegs09.Add(new TerminalLeg { Id = gbm02Id, TerminalId = terminal09Id, Type = "R", TrackCode = "TF", WptId = gbm02.Id, WptLat = gbm02.Latitude, WptLon = gbm02.Longtitude });
                terminalLegExs.Add(new TerminalLegEx { Id = gbm02Id, IsFlyOver = false });

                int kuxoxId2 = ++oldestId;
                terminalLegs09.Add(new TerminalLeg { Id = kuxoxId2, TerminalId = terminal09Id, Type = "R", TrackCode = "TF", WptId = kuxox.Id, WptLat = kuxox.Latitude, WptLon = kuxox.Longtitude });
                terminalLegExs.Add(new TerminalLegEx { Id = kuxoxId2, IsFlyOver = false });


                int pimos09Id2 = ++oldestId;
                terminalLegs09.Add(new TerminalLeg { Id = pimos09Id2, TerminalId = terminal09Id, Type = "R", TrackCode = "TF", WptId = pimos.Id, WptLat = pimos.Latitude, WptLon = pimos.Longtitude });
                terminalLegExs.Add(new TerminalLegEx { Id = pimos09Id2, IsFlyOver = true });

                int upmupDfId = ++oldestId;
                int upmupHmId2 = ++oldestId;
                terminalLegs09.Add(new TerminalLeg { Id = upmupDfId, TerminalId = terminal09Id, Type = "R", TrackCode = "DF", WptId = upmup.Id, WptLat = upmup.Latitude, WptLon = upmup.Longtitude, Alt = "4000", TurnDir = "R" });
                terminalLegs09.Add(new TerminalLeg { Id = upmupHmId2, TerminalId = terminal09Id, Type = "R", TrackCode = "HM", WptId = upmup.Id, WptLat = upmup.Latitude, WptLon = upmup.Longtitude, TurnDir = "L", Course = 305.0, Distance = 4.0 });
                terminalLegExs.Add(new TerminalLegEx { Id = upmupDfId, IsFlyOver = false, SpeedLimit = 230, SpeedLimitDescription = "B" });
                terminalLegExs.Add(new TerminalLegEx { Id = upmupHmId2, IsFlyOver = false, SpeedLimit = 230, SpeedLimitDescription = "B" });

                _db.InsertAll(terminalLegExs);
                _db.InsertAll(terminalLegs09);
                _db.InsertAll(terminalLegs27);
            }
        }

        #endregion Database Functions
    }
}
