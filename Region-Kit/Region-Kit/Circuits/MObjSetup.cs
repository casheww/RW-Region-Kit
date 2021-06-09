using ManagedPlacedObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RegionKit.Circuits
{
    public struct MObjSetup
    {
        public MObjSetup(string name, Type abstractType, Type realisedType, PlacedObjectsManager.ManagedField[] fields)
        {
            Name = name;
            AbstractType = abstractType;
            RealisedType = realisedType;
            Fields = fields;
            FieldsByKey = fields.ToDictionary(f => f.key, f => f);
            ValuesByKey = fields.ToDictionary(f => f.key, f => f.DefaultValue);
        }

        public bool TryGetFieldAndValue(string key, out PlacedObjectsManager.ManagedField field, out object value)
        {
            if (FieldsByKey.ContainsKey(key))
            {
                field = FieldsByKey[key];
                value = ValuesByKey[key];
                return true;
            }
            else
            {
                field = null;
                value = null;
                return false;
            }
        }

        public T GetValue<T>(string key)
        {
            return (T)ValuesByKey[key];
        }

        public void SetValue(string key, object value)
        {
            if (ValuesByKey.ContainsKey(key))
            {
                ValuesByKey[key] = value;
            }
        }

        public string Name { get; private set; }
        public Type AbstractType { get; private set; }
        public Type RealisedType { get; private set; }
        public PlacedObjectsManager.ManagedField[] Fields { get; private set; }
        public Dictionary<string, PlacedObjectsManager.ManagedField> FieldsByKey { get; private set; }
        public Dictionary<string, object> ValuesByKey { get; private set; }

    }
}
