﻿using System;
using System.Collections.Generic;
using System.Reflection;
using FluentNHibernate.MappingModel;
using FluentNHibernate.MappingModel.Collections;

namespace FluentNHibernate.Mapping
{
    public class OneToManyPart<TChild> : ToManyBase<OneToManyPart<TChild>, TChild, OneToManyMapping>
    {
        private readonly Type entity;
        private readonly ColumnMappingCollection<OneToManyPart<TChild>> columns;
        private readonly CollectionCascadeExpression<OneToManyPart<TChild>> cascade;
        private readonly NotFoundExpression<OneToManyPart<TChild>> notFound;
        private IndexManyToManyPart manyToManyIndex;
        private readonly Type childType;
        private Type valueType;
        private bool isTernary;

        public OneToManyPart(Type entity, PropertyInfo property)
            : this(entity, property, property.PropertyType)
        {
        }

        public OneToManyPart(Type entity, MethodInfo method)
            : this(entity, method, method.ReturnType)
        {
        }

        protected OneToManyPart(Type entity, MemberInfo member, Type collectionType)
            : base(entity, member, collectionType)
        {
            this.entity = entity;
            childType = collectionType;

            columns = new ColumnMappingCollection<OneToManyPart<TChild>>(this);
            cascade = new CollectionCascadeExpression<OneToManyPart<TChild>>(this, value => collectionAttributes.Set(x => x.Cascade, value));
            notFound = new NotFoundExpression<OneToManyPart<TChild>>(this, value => relationshipAttributes.Set(x => x.NotFound, value));

            collectionAttributes.SetDefault(x => x.Name, member.Name);
        }

        public NotFoundExpression<OneToManyPart<TChild>> NotFound
        {
            get { return notFound; }
        }

        public new CollectionCascadeExpression<OneToManyPart<TChild>> Cascade
        {
            get { return cascade; }
        }

        public override ICollectionMapping GetCollectionMapping()
        {
            var collection = base.GetCollectionMapping();

            if (columns.List().Count == 0)
                collection.Key.AddDefaultColumn(new ColumnMapping { Name = entity.Name + "_id" });

            foreach (var column in columns.List())
            {
                collection.Key.AddColumn(column);
            }

            // HACK: shouldn't have to do this!
            if (manyToManyIndex != null && collection is MapMapping)
                ((MapMapping)collection).Index = manyToManyIndex.GetIndexMapping();

            return collection;
        }

        private void EnsureGenericDictionary()
        {
            if (!(childType.IsGenericType && childType.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
                throw new ArgumentException(member.Name + " must be of type IDictionary<> to be used in a ternary assocation. Type was: " + childType);
        }

        public OneToManyPart<TChild> AsTernaryAssociation()
        {
            var keyType = childType.GetGenericArguments()[0];
            return AsTernaryAssociation(keyType.Name + "_id");
        }

        public OneToManyPart<TChild> AsTernaryAssociation(string indexColumnName)
        {
            EnsureGenericDictionary();

            var keyType = childType.GetGenericArguments()[0];
            var valType = childType.GetGenericArguments()[1];

            manyToManyIndex = new IndexManyToManyPart(typeof(ManyToManyPart<TChild>));
            manyToManyIndex.Column(indexColumnName);
            manyToManyIndex.Type(keyType);

            valueType = valType;
            isTernary = true;

            return this;
        }

        public OneToManyPart<TChild> AsEntityMap()
        {
            // The argument to AsMap will be ignored as the ternary association will overwrite the index mapping for the map.
            // Therefore just pass null.
            return AsMap(null).AsTernaryAssociation();
        }

        public OneToManyPart<TChild> AsEntityMap(string indexColumnName)
        {
            return AsMap(null).AsTernaryAssociation(indexColumnName);
        }

        protected override ICollectionRelationshipMapping GetRelationship()
        {
            var mapping = new OneToManyMapping(relationshipAttributes.CloneInner())
            {
                ContainingEntityType = entity
            };

            if (isTernary && valueType != null)
                mapping.Class = new TypeReference(valueType);

            return mapping;
        }

        public OneToManyPart<TChild> KeyColumn(Action<ColumnMapping> configurator)
        {
            var mapping = new ColumnMapping {Name = entity.Name + "_id"};

            if(configurator != null)
                configurator(mapping);

            KeyColumns.Clear();
            KeyColumns.Add(mapping);
            return this;
        }

        public OneToManyPart<TChild> KeyColumn(string columnName)
        {
            KeyColumns.Clear();
            KeyColumns.Add(new ColumnMapping { Name = columnName });
            return this;
        }

        public OneToManyPart<TChild> KeyColumn(string columnName, Action<ColumnMapping> configurator)
        {
            var mapping = new ColumnMapping {Name = columnName};

            if(configurator != null)
                configurator(mapping);

            KeyColumns.Clear();
            KeyColumns.Add(mapping);
            return this;
        }

        public ColumnMappingCollection<OneToManyPart<TChild>> KeyColumns
        {
            get { return columns; }
        }

        public OneToManyPart<TChild> ForeignKeyConstraintName(string foreignKeyName)
        {
            keyAttributes.Set(x => x.ForeignKey, foreignKeyName);
            return this;
        }

        /// <summary>
        /// Sets the order-by clause for this one-to-many relationship.
        /// </summary>
        public OneToManyPart<TChild> OrderBy(string orderBy)
        {
            collectionAttributes.Set(x => x.OrderBy, orderBy);
            return this;
        }

        public OneToManyPart<TChild> ReadOnly()
        {
            collectionAttributes.Set(x => x.Mutable, !nextBool);
            nextBool = true;
            return this;
        }

        public OneToManyPart<TChild> Subselect(string subselect)
        {
            collectionAttributes.Set(x => x.Subselect, subselect);
            return this;
        }
    }
}
