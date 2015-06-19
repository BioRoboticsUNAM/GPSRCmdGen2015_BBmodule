﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace GPSRCmdGen.Containers
{
	/// <summary>
	/// Helper class. Implements a container for (de)serlaizing gestures  
	/// </summary>
	[XmlRoot(ElementName = "gestures", Namespace = "")]
	public class GestureContainer
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GPSRCmdGen.GestureContainer"/> class.
		/// </summary>
		public GestureContainer() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="GPSRCmdGen.GestureContainer"/> class.
		/// </summary>
		/// <param name="gestures">List of gestures</param>
		public GestureContainer(List<Gesture> gestures) { this.Gestures = gestures; }

		/// <summary>
		/// Gets or sets the list of gestures.
		/// </summary>
		[XmlElement("gesture")]
		public List<Gesture> Gestures { get; set; }
	}
}
