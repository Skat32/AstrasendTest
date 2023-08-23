using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Astrasend.Models;
/// <summary>
/// Base on https://weblogs.asp.net/pwelter34/444961
/// </summary>
/// <remarks>https://gist.github.com/steelywing/c08ac7563ad1a918db84ffe406c397a9</remarks>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
[XmlRoot("dictionary")]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
{
    // XmlSerializer.Deserialize() will create a new Object, and then call ReadXml()
    // So cannot use instance field, use class field.

    public static string itemTag = "item";
    public static string keyTag = "key";
    public static string valueTag = "value";

    public SerializableDictionary(IDictionary<TKey, TValue>? values)
    {
        if (values is null)
            return;
        
        foreach (var keyValuePair in values) 
            Add(keyValuePair.Key, keyValuePair.Value);
    }

    public SerializableDictionary(){}
    
    public XmlSchema GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader)
    {
        if (reader.IsEmptyElement)
            return;

        var keySerializer = new XmlSerializer(typeof(TKey));
        var valueSerializer = new XmlSerializer(typeof(TValue));

        reader.ReadStartElement();

        while (reader.IsStartElement(itemTag))
        {
            reader.ReadStartElement(itemTag);

            reader.ReadStartElement(keyTag);
            var key = (TKey)keySerializer.Deserialize(reader);
            reader.ReadEndElement();

            reader.ReadStartElement(valueTag);
            var value = (TValue)valueSerializer.Deserialize(reader);
            reader.ReadEndElement();

            reader.ReadEndElement();
            Add(key, value);

        }
        reader.ReadEndElement();
    }

    public void WriteXml(XmlWriter writer)
    {
        var keySerializer = new XmlSerializer(typeof(TKey));
        var valueSerializer = new XmlSerializer(typeof(TValue));

        foreach (var kvp in this)
        {
            writer.WriteStartElement(itemTag);

            writer.WriteStartElement(keyTag);
            keySerializer.Serialize(writer, kvp.Key);
            writer.WriteEndElement();

            writer.WriteStartElement(valueTag);
            valueSerializer.Serialize(writer, kvp.Value);
            writer.WriteEndElement();

            writer.WriteEndElement();
        }
    }
}