using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace EmbeddedDebugger.Connectors.Settings
{
    public class ConnectionSetting
    {
        public string Name { get; set; }
        public Type Type => this.Value.GetType();
        public object Value { get; set; }
        [XmlIgnore]
        public IEnumerable<object> Possibilities { get; set; }
        [XmlIgnore]
        public Action PossibilitiesRefresher { get; set; }
        public bool EnforcePossibilities { get; set; }
        public object MinimalValue { get; set; }
        public object MaximumValue { get; set; }
    }
}
