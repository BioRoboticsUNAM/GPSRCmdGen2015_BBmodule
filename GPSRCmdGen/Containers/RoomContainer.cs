﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace GPSRCmdGen.Containers
{
	/// <summary>
	/// Helper class. Implements a container for (de)serlaizing locations
	/// </summary>
	[XmlRoot(ElementName = "rooms", Namespace = "")]
	public class RoomContainer
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="GPSRCmdGen.LocationContainer"/> class.
		/// </summary>
		public RoomContainer() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="GPSRCmdGen.LocationContainer"/> class.
		/// </summary>
		/// <param name="locations">List of locations.</param>
		public RoomContainer(List<Room> rooms) { this.Rooms = rooms; }

		/// <summary>
		/// Gets or sets the list of rooms.
		/// </summary>
		[XmlElement("room")]
		public List<Room> Rooms { get; set; }
	}
}
