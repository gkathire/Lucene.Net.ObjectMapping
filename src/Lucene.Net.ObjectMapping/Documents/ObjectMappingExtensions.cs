﻿using Lucene.Net.Mapping;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization.Formatters;

namespace Lucene.Net.Documents
{
    /// <summary>
    /// Extension class to help mapping objects to Documents and vice versa.
    /// </summary>
    public static class ObjectMappingExtensions
    {
        #region Consts

        /// <summary>
        /// The name of the field which holds the object's actual type.
        /// </summary>
        public static readonly string FieldActualType = "$actualType";

        /// <summary>
        /// The name of the field which holds the object's static type.
        /// </summary>
        public static readonly string FieldStaticType = "$staticType";

        /// <summary>
        /// The name of the field which holds the JSON-serialized source of the object.
        /// </summary>
        public static readonly string FieldSource = "$source";

        /// <summary>
        /// The name of the field which holds the timestamp when the document was created.
        /// </summary>
        public static readonly string FieldTimestamp = "$timestamp";

        #endregion

        #region Fields

        /// <summary>
        /// The JsonSerializerSettings for serialization and deserialization of objects to/from JSON.
        /// </summary>
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
            TypeNameHandling = TypeNameHandling.Auto,
        };

        #endregion

        #region Public Methods

        /// <summary>
        /// Maps the source object to a Lucene.Net Document.
        /// </summary>
        /// <typeparam name="TSource">
        /// The Type of the source object to map.
        /// </typeparam>
        /// <param name="source">
        /// The source object to map.
        /// </param>
        /// <returns>
        /// An instance of Document that represents the mapped object.
        /// </returns>
        public static Document ToDocument<TSource>(this TSource source)
        {
            return ToDocument<TSource>(source, MappingSettings.Default);
        }

        /// <summary>
        /// Maps the source object to a Lucene.Net Document.
        /// </summary>
        /// <typeparam name="TSource">
        /// The Type of the source object to map.
        /// </typeparam>
        /// <param name="source">
        /// The source object to map.
        /// </param>
        /// <param name="mappingSettings">
        /// The MappingSettings to use.
        /// </param>
        /// <returns>
        /// An instance of Document that represents the mapped object.
        /// </returns>
        public static Document ToDocument<TSource>(this TSource source, MappingSettings mappingSettings)
        {
            if (null == mappingSettings)
            {
                throw new ArgumentNullException("mappingSettings");
            }

            Document doc = new Document();
            string json = JsonConvert.SerializeObject(source, typeof(TSource), settings);

            doc.Add(new Field(FieldActualType, Utils.GetTypeName(source.GetType()), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field(FieldStaticType, Utils.GetTypeName(typeof(TSource)), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field(FieldSource, json, Field.Store.YES, Field.Index.NO));
            doc.Add(new NumericField(FieldTimestamp, Field.Store.YES, true).SetLongValue(DateTime.UtcNow.Ticks));

            mappingSettings.ObjectMapper.AddToDocument<TSource>(source, doc);

            return doc;
        }

        /// <summary>
        /// Maps the data from the given Document to an object of type TObject.
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of object to map to.
        /// </typeparam>
        /// <param name="doc">
        /// The Document to map to an object.
        /// </param>
        /// <returns>
        /// An instance of TObject.
        /// </returns>
        public static TObject ToObject<TObject>(this Document doc)
        {
            string actualTypeName = doc.Get(FieldActualType);
            string staticTypeName = doc.Get(FieldStaticType);
            string source = doc.Get(FieldSource);
            string rawTimestamp = doc.Get(FieldTimestamp);

            // TODO: Additional checks on object types etc.
            TObject obj = JsonConvert.DeserializeObject<TObject>(source, settings);

            return obj;
        }

        /// <summary>
        /// Maps the data from the given Document to an object of type TObject.
        /// </summary>
        /// <param name="doc">
        /// The Document to map to an object.
        /// </param>
        /// <returns>
        /// An instance of object.
        /// </returns>
        public static object ToObject(this Document doc)
        {
            return ToObject<object>(doc);
        }

        #endregion
    }
}
