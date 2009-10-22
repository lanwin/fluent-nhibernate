using System.Collections.Generic;
using FluentNHibernate.MappingModel;

namespace FluentNHibernate.Mapping
{
    public class ColumnMappingCollection<TParent>
    {
        private readonly IList<ColumnMapping> columnMappings = new List<ColumnMapping>();
        private readonly TParent parent;

        public ColumnMappingCollection(TParent parent)
        {
            this.parent = parent;
        }

        public TParent Add(string name)
        {
            columnMappings.Add(new ColumnMapping{Name = name});
            return parent;
        }

        public TParent Add(ColumnMapping mapping)
        {
            columnMappings.Add(mapping);
            return parent;
        }

        public TParent Add(params ColumnMapping[] mappings)
        {
            foreach (var mapping in mappings)
            {
                Add(mapping);
            }
            return parent;
        }

        public TParent Add(params string[] names)
        {
            foreach(var name in names)
            {
                Add(name);
            }
            return parent;
        }

        public TParent Clear()
        {
            columnMappings.Clear();
            return parent;
        }

        public IList<ColumnMapping> List()
        {
            return new List<ColumnMapping>(columnMappings).AsReadOnly();
        }
    }
}