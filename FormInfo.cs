using System.Collections.Generic;
using System.Xml.Serialization;

namespace SettingsSaver
{
    public class FormInfo
    {
        [XmlAttribute]
        public string Key { get; set; }
        [XmlAttribute]
        public  bool Maximized { get; set; }

        [XmlAttribute]
        public int FormWidth { get; set; }
        [XmlAttribute]
        public int FormHeight { get; set; }
        [XmlAttribute]
        public int LocationX { get; set; }
        [XmlAttribute]
        public int LocationY { get; set; }

        public List<GridsInfo> GridsInfos { get; set; }
    }

    public class GridsInfo
    {
        [XmlAttribute]
        public string GridName { get; set; }

        [XmlElement("width")]
        public List<SomeInfo<int>> GridWidths { get; set; }
    }

    public class SomeInfo<T>
    {
        [XmlAttribute]
        public T Value { get; set; }
    }
}

