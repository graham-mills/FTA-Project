using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FaultTreeAnalysis
{
    /// <summary>
    /// Basic Failure Event
    /// </summary>
    public class BasicEvent : Event
    {
        public BasicEvent(XmlElement eventElement)
        {
            ParseXML(eventElement);
            base.SetKeyBit();
            Cutsets = CutsetGroup.Create(new Cutset(this));
        }

        public override CutsetGroup GetCutSets()
        {
            return Cutsets;
        }

        /// <summary>
        /// Parse XML event into memory
        /// </summary>
        /// <param name="eventElement">XML Event</param>
        private void ParseXML(XmlElement eventElement)
        {
            ID = int.Parse(eventElement.GetAttribute("ID"));
            Name = eventElement["Name"].InnerText;
            ShortName = eventElement["ShortName"].InnerText;
            Description = eventElement["Description"].InnerText;
        }

        /// <summary>
        /// Output Event as XML
        /// </summary>
        /// <param name="document">XML Document</param>
        /// <param name="parent">XML Parent to append to</param>
        public override void OutputXML(XmlDocument document, XmlNode parent)
        {
            XmlElement eventElement = document.CreateElement("Event");
            eventElement.SetAttribute("ID", ID.ToString());
            parent.AppendChild(eventElement);
        }
    }
}
