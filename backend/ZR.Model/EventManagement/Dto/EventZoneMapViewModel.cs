using System.Collections.Generic;

namespace ZR.Model.EventManagement.Dto
{
    /// <summary>
    /// View model for Event Zone Map PDF
    /// </summary>
    public class EventZoneMapViewModel
    {
        /// <summary>
        /// Event information
        /// </summary>
        public Event EventData { get; set; }

        /// <summary>
        /// List of zones for the event
        /// </summary>
        public List<EventZone> Zones { get; set; }
    }
}

