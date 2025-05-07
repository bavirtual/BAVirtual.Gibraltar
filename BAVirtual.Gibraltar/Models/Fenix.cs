using SQLite;

namespace BAVirtual.Gibraltar.Models
{
    [Table("Airports")]
    public class Airport
    {
        [PrimaryKey]
        [Column("ID")]
        public int Id { get; set; }

        [Column("Name")]
        public string? Name { get; set; }

        [Column("ICAO")]
        public string? Icao { get; set; }

        [Column("PrimaryID")]
        public int? PrimaryId { get; set; }

        [Column("Latitude")]
        public double? Latitude { get; set; }

        [Column("Longtitude")]
        public double? Longtitude { get; set; }

        [Column("Elevation")]
        public int? Elevation { get; set; }

        [Column("TransitionAltitude")]
        public int? TransitionAltitude { get; set; }

        [Column("TransitionLevel")]
        public int? TransitionLevel { get; set; }

        [Column("SpeedLimit")]
        public int? SpeedLimit { get; set; }

        [Column("SpeedLimitAltitude")]
        public int? SpeedLimitAltitude { get; set; }
    }

    [Table("Waypoints")]
    public class Waypoint
    {
        [PrimaryKey]
        [Column("ID")]
        public int Id { get; set; }

        [NotNull]
        [Column("Ident")]
        public string? Ident { get; set; }

        [NotNull]
        [Column("Collocated")]
        public bool? Collocated { get; set; }

        [Column("Name")]
        public string? Name { get; set; }

        [Column("Latitude")]
        public double? Latitude { get; set; }

        [Column("Longtitude")]
        public double? Longtitude { get; set; }

        [Column("NavaidID")]
        public int? NavaidId { get; set; }
    }

    [Table("Navaids")]
    public class Navaid
    {
        [PrimaryKey]
        [Column("ID")]
        public int Id { get; set; }

        [Column("Ident")]
        public string? Ident { get; set; }

        [Column("Type")]
        public int? Type { get; set; }

        [Column("Name")]
        public string? Name { get; set; }

        [Column("Freq")]
        public int? Freq { get; set; }

        [Column("Channel")]
        public string? Channel { get; set; }

        [Column("Usage")]
        public string? Usage { get; set; }

        [Column("Latitude")]
        public double? Latitude { get; set; }

        [Column("Longtitude")]
        public double? Longtitude { get; set; }

        [Column("Elevation")]
        public int? Elevation { get; set; }

        [Column("SlavedVar")]
        public double? SlavedVar { get; set; }

        [Column("MagneticVariation")]
        public double? MagneticVariation { get; set; }

        [Column("Range")]
        public int? Range { get; set; }
    }



    [Table("WaypointLookup")]
    public class WaypointLookup
    {
        [NotNull]
        [Column("Ident")]
        public string? Ident { get; set; }

        [NotNull]
        [Column("Country")]
        public string? Country { get; set; }

        [NotNull]
        [Column("ID")]
        public int? Id { get; set; }
    }

    [Table("Runways")]
    public class Runway
    {
        [PrimaryKey]
        [Column("ID")]
        public int Id { get; set; }

        [Column("AirportID")]
        public int? AirportId { get; set; }

        [Column("Ident")]
        public string? Ident { get; set; }

        [Column("TrueHeading")]
        public double? TrueHeading { get; set; }

        [Column("Length")]
        public int? Length { get; set; }

        [Column("Width")]
        public int? Width { get; set; }

        [Column("Surface")]
        public string? Surface { get; set; }

        [Column("Latitude")]
        public double? Latitude { get; set; }

        [Column("Longtitude")]
        public double? Longtitude { get; set; }

        [Column("Elevation")]
        public int? Elevation { get; set; }
    }

    [Table("Terminals")]
    public class Terminal
    {
        [PrimaryKey]
        [Column("ID")]
        public int Id { get; set; }

        [Column("AirportID")]
        public int? AirportId { get; set; }

        [Column("Proc")]
        public int? Proc { get; set; }

        [NotNull]
        [Column("ICAO")]
        public string? Icao { get; set; }

        [NotNull]
        [Column("FullName")]
        public string? FullName { get; set; }

        [NotNull]
        [Column("Name")]
        public string? Name { get; set; }

        [Column("Rwy")]
        public string? Rwy { get; set; }

        [Column("RwyID")]
        public int? RwyId { get; set; }

        [Column("IlsID")]
        public int? IlsId { get; set; }
    }


    [Table("TerminalLegs")]
    public class TerminalLeg
    {
        [PrimaryKey]
        [Column("ID")]
        public int Id { get; set; }

        [Column("TerminalID")]
        public int? TerminalId { get; set; }

        [Column("Type")]
        public string? Type { get; set; }

        [Column("Transition")]
        public string? Transition { get; set; }

        [Column("TrackCode")]
        public string? TrackCode { get; set; }

        [Column("WptID")]
        public int? WptId { get; set; }

        [Column("WptLat")]
        public double? WptLat { get; set; }

        [Column("WptLon")]
        public double? WptLon { get; set; }

        [Column("TurnDir")]
        public string? TurnDir { get; set; }

        [Column("NavID")]
        public int? NavId { get; set; }

        [Column("NavLat")]
        public double? NavLat { get; set; }

        [Column("NavLon")]
        public double? NavLon { get; set; }

        [Column("NavBear")]
        public double? NavBear { get; set; }

        [Column("NavDist")]
        public double? NavDist { get; set; }

        [Column("Course")]
        public double? Course { get; set; }

        [Column("Distance")]
        public double? Distance { get; set; }

        [Column("Alt")]
        public string? Alt { get; set; }

        [Column("Vnav")]
        public double? Vnav { get; set; }

        [Column("CenterID")]
        public int? CenterId { get; set; }

        [Column("CenterLat")]
        public double? CenterLat { get; set; }

        [Column("CenterLon")]
        public double? CenterLon { get; set; }

        [Column("WptDescCode")]
        public string? WptDescCode { get; set; }
    }


    [Table("TerminalLegsEx")]
    public class TerminalLegEx
    {
        [PrimaryKey]
        [Column("ID")]
        public int Id { get; set; }

        [NotNull]
        [Column("IsFlyOver")]
        public bool? IsFlyOver { get; set; }

        [Column("SpeedLimit")]
        public double? SpeedLimit { get; set; }

        [Column("SpeedLimitDescription")]
        public string? SpeedLimitDescription { get; set; }
    }
}