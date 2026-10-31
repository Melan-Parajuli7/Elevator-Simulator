namespace Lift.Models
{
    /// <summary>
    /// The two floors of the building. The numbers are the FloorId values stored in the
    /// Floors table of the database, so a Floor can be saved without any conversion.
    /// </summary>
    public enum Floor
    {
        Ground = 1,
        First = 2
    }

    public static class FloorExtensions
    {
        public const Floor Lowest = Floor.Ground;
        public const Floor Highest = Floor.First;

        /// <summary>Short text for displays and buttons: "G" or "1".</summary>
        public static string Label(this Floor floor)
        {
            return floor == Floor.Ground ? "G" : "1";
        }

        /// <summary>Long text for messages: "Ground Floor" or "First Floor".</summary>
        public static string DisplayName(this Floor floor)
        {
            return floor == Floor.Ground ? "Ground Floor" : "First Floor";
        }

        public static Floor Other(this Floor floor)
        {
            return floor == Floor.Ground ? Floor.First : Floor.Ground;
        }

        public static bool IsValid(int floorNumber)
        {
            return floorNumber >= (int)Lowest && floorNumber <= (int)Highest;
        }
    }
}
