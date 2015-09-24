using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FaultTreeAnalysis
{
    /// <summary>
    /// Non-failure Event
    /// </summary>
    public class NormalEvent : Event
    {
        public NormalEvent(XmlElement eventElement)
        {
            ParseXML(eventElement);
            base.SetKeyBit();
            Cutsets = CutsetGroup.Create(new Cutset(this));
        }

        public override CutsetGroup GetCutSets()
        {
            return Cutsets;
        }

        private void ParseXML(XmlElement eventElement)
        {
            ID = int.Parse(eventElement.GetAttribute("ID"));
            Name = eventElement["Name"].InnerText;
            ShortName = eventElement["ShortName"].InnerText;
            Description = eventElement["Description"].InnerText;
        }

        public override void OutputXML(XmlDocument document, XmlNode parent)
        {
            XmlElement eventElement = document.CreateElement("NormalEvent");
            eventElement.SetAttribute("ID", ID.ToString());
            eventElement.SetAttribute("FirstVisit", FirstVisit.ToString());
            eventElement.SetAttribute("LastVisit", LastVisit.ToString());
            parent.AppendChild(eventElement);
        }
    }
}
