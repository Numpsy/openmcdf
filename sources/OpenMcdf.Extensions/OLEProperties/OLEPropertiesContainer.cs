﻿using OpenMcdf.Extensions.OLEProperties.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenMcdf.Extensions.OLEProperties
{
    public class OLEPropertiesContainer
    {
        public Dictionary<uint, string> PropertyNames = null;

        public OLEPropertiesContainer UserDefinedProperties { get; private set; }

        public bool HasUserDefinedProperties { get; private set; }

        /// <summary>
        /// Gets the type of the container.
        /// </summary>
        public ContainerType ContainerType { get; }

        /// <summary>
        /// Gets the FMTID of the properties container.
        /// </summary>
        public Guid FMTID0 { get; }

        public PropertyContext Context { get; private set; }

        private readonly List<OLEProperty> properties = new List<OLEProperty>();
        internal CFStream cfStream;

        /*
         Property name	Property ID	PID	Type
        Codepage	PID_CODEPAGE	1	VT_I2
        Title	PID_TITLE	2	VT_LPSTR
        Subject	PID_SUBJECT	3	VT_LPSTR
        Author	PID_AUTHOR	4	VT_LPSTR
        Keywords	PID_KEYWORDS	5	VT_LPSTR
        Comments	PID_COMMENTS	6	VT_LPSTR
        Template	PID_TEMPLATE	7	VT_LPSTR
        Last Saved By	PID_LASTAUTHOR	8	VT_LPSTR
        Revision Number	PID_REVNUMBER	9	VT_LPSTR
        Last Printed	PID_LASTPRINTED	11	VT_FILETIME
        Create Time/Date	PID_CREATE_DTM	12	VT_FILETIME
        Last Save Time/Date	PID_LASTSAVE_DTM	13	VT_FILETIME
        Page Count	PID_PAGECOUNT	14	VT_I4
        Word Count	PID_WORDCOUNT	15	VT_I4
        Character Count	PID_CHARCOUNT	16	VT_I4
        Creating Application	PID_APPNAME	18	VT_LPSTR
        Security	PID_SECURITY	19	VT_I4
             */
        public class SummaryInfoProperties
        {
            public short CodePage { get; set; }
            public string Title { get; set; }
            public string Subject { get; set; }
            public string Author { get; set; }
            public string KeyWords { get; set; }
            public string Comments { get; set; }
            public string Template { get; set; }
            public string LastSavedBy { get; set; }
            public string RevisionNumber { get; set; }
            public DateTime LastPrinted { get; set; }
            public DateTime CreateTime { get; set; }
            public DateTime LastSavedTime { get; set; }
            public int PageCount { get; set; }
            public int WordCount { get; set; }
            public int CharacterCount { get; set; }
            public string CreatingApplication { get; set; }
            public int Security { get; set; }
        }

        public static OLEPropertiesContainer CreateNewSummaryInfo(SummaryInfoProperties sumInfoProps)
        {
            return null;
        }

        /// <summary>
        /// Create a new instance of <see cref="OLEPropertiesContainer"/> with the specified code page and container type.
        /// </summary>
        /// <param name="codePage">The code page to use for the new container.</param>
        /// <param name="containerType">The type of the new container.</param>
        public OLEPropertiesContainer(int codePage, ContainerType containerType)
        {
            Context = new PropertyContext
            {
                CodePage = codePage,
                Behavior = Behavior.CaseInsensitive
            };

            ContainerType = containerType;
            FMTID0 = FmtIdFromContainerType(containerType);
        }

        internal OLEPropertiesContainer(CFStream cfStream)
        {
            PropertySetStream pStream = new PropertySetStream();

            this.cfStream = cfStream;
            pStream.Read(new BinaryReader(new StreamDecorator(cfStream)));

            FMTID0 = pStream.FMTID0;
            ContainerType = ContainerTypeFromFmtId(pStream.FMTID0);

            PropertyNames = (Dictionary<uint, string>)pStream.PropertySet0.Properties
                .Where(p => p.PropertyType == PropertyType.DictionaryProperty).FirstOrDefault()?.Value;

            Context = new PropertyContext()
            {
                CodePage = pStream.PropertySet0.PropertyContext.CodePage
            };

            for (int i = 0; i < pStream.PropertySet0.Properties.Count; i++)
            {
                if (pStream.PropertySet0.PropertyIdentifierAndOffsets[i].PropertyIdentifier == 0) continue;
                //if (pStream.PropertySet0.PropertyIdentifierAndOffsets[i].PropertyIdentifier == 1) continue;
                //if (pStream.PropertySet0.PropertyIdentifierAndOffsets[i].PropertyIdentifier == 0x80000000) continue;

                var p = (ITypedPropertyValue)pStream.PropertySet0.Properties[i];
                var poi = pStream.PropertySet0.PropertyIdentifierAndOffsets[i];

                var op = new OLEProperty(this)
                {
                    VTType = p.VTType,
                    PropertyIdentifier = pStream.PropertySet0.PropertyIdentifierAndOffsets[i].PropertyIdentifier,
                    Value = p.Value
                };

                properties.Add(op);
            }

            if (pStream.NumPropertySets == 2)
            {
                UserDefinedProperties = new OLEPropertiesContainer(pStream.PropertySet1.PropertyContext.CodePage, ContainerType.UserDefinedProperties);
                HasUserDefinedProperties = true;

                for (int i = 0; i < pStream.PropertySet1.Properties.Count; i++)
                {
                    if (pStream.PropertySet1.PropertyIdentifierAndOffsets[i].PropertyIdentifier == 0) continue;
                    //if (pStream.PropertySet1.PropertyIdentifierAndOffsets[i].PropertyIdentifier == 1) continue;
                    if (pStream.PropertySet1.PropertyIdentifierAndOffsets[i].PropertyIdentifier == 0x80000000) continue;

                    var p = (ITypedPropertyValue)pStream.PropertySet1.Properties[i];
                    var poi = pStream.PropertySet1.PropertyIdentifierAndOffsets[i];

                    var op = new OLEProperty(UserDefinedProperties)
                    {
                        VTType = p.VTType,
                        PropertyIdentifier = pStream.PropertySet1.PropertyIdentifierAndOffsets[i].PropertyIdentifier,
                        Value = p.Value
                    };

                    UserDefinedProperties.properties.Add(op);
                }

                var existingPropertyNames = (Dictionary<uint, string>)pStream.PropertySet1.Properties
                    .Where(p => p.PropertyType == PropertyType.DictionaryProperty).FirstOrDefault()?.Value;

                UserDefinedProperties.PropertyNames = existingPropertyNames ?? new Dictionary<uint, string>();
            }
        }

        public IEnumerable<OLEProperty> Properties => properties;

        public OLEProperty NewProperty(VTPropertyType vtPropertyType, uint propertyIdentifier, string propertyName = null)
        {
            //throw new NotImplementedException("API Unstable - Work in progress - Milestone 2.3.0.0");
            var op = new OLEProperty(this)
            {
                VTType = vtPropertyType,
                PropertyIdentifier = propertyIdentifier
            };

            return op;
        }

        public void AddProperty(OLEProperty property)
        {
            //throw new NotImplementedException("API Unstable - Work in progress - Milestone 2.3.0.0");
            properties.Add(property);
        }

        public void RemoveProperty(uint propertyIdentifier)
        {
            //throw new NotImplementedException("API Unstable - Work in progress - Milestone 2.3.0.0");
            var toRemove = properties.Where(o => o.PropertyIdentifier == propertyIdentifier).FirstOrDefault();

            if (toRemove != null)
                properties.Remove(toRemove);
        }

        /// <summary>
        /// Create a new UserDefinedProperties container within this container.
        /// </summary>
        /// <remarks>
        /// Only containers of type DocumentSummaryInfo can contain user defined properties.
        /// </remarks>
        /// <param name="codePage">The code page to use for the user defined properties.</param>
        /// <returns>The UserDefinedProperties container.</returns>
        /// <exception cref="CFInvalidOperation">If this container is a type that doesn't suppose user defined properties.</exception>
        public OLEPropertiesContainer CreateUserDefinedProperties(int codePage)
        {
            // Only the DocumentSummaryInfo stream can contain a UserDefinedProperties
            if (ContainerType != ContainerType.DocumentSummaryInfo)
            {
                throw new CFInvalidOperation($"Only a DocumentSummaryInfo can contain user defined properties. Current container type is {ContainerType}");
            }

            // Create the container, and add the codepage to the initial set of properties
            UserDefinedProperties = new OLEPropertiesContainer(codePage, ContainerType.UserDefinedProperties)
            {
                PropertyNames = new Dictionary<uint, string>()
            };

            var op = new OLEProperty(UserDefinedProperties)
            {
                VTType = VTPropertyType.VT_I2,
                PropertyIdentifier = 1,
                Value = (short)codePage
            };

            UserDefinedProperties.properties.Add(op);
            HasUserDefinedProperties = true;

            return UserDefinedProperties;
        }

        public void Save(CFStream cfStream)
        {
            //throw new NotImplementedException("API Unstable - Work in progress - Milestone 2.3.0.0");
            //properties.Sort((a, b) => a.PropertyIdentifier.CompareTo(b.PropertyIdentifier));

            Stream s = new StreamDecorator(cfStream);
            BinaryWriter bw = new BinaryWriter(s);

            PropertySetStream ps = new PropertySetStream
            {
                ByteOrder = 0xFFFE,
                Version = 0,
                SystemIdentifier = 0x00020006,
                CLSID = Guid.Empty,

                NumPropertySets = 1,

                FMTID0 = this.FMTID0,
                Offset0 = 0,

                FMTID1 = Guid.Empty,
                Offset1 = 0,

                PropertySet0 = new PropertySet
                {
                    NumProperties = (uint)Properties.Count(),
                    PropertyIdentifierAndOffsets = new List<PropertyIdentifierAndOffset>(),
                    Properties = new List<IProperty>(),
                    PropertyContext = Context
                }
            };

            // If we're writing an AppSpecific property set and have property names, then add a dictionary property
            if (ContainerType == ContainerType.AppSpecific && PropertyNames != null && PropertyNames.Count > 0)
            {
                AddDictionaryPropertyToPropertySet(PropertyNames, ps.PropertySet0);
                ps.PropertySet0.NumProperties += 1;
            }

            PropertyFactory factory =
                ContainerType == ContainerType.DocumentSummaryInfo ? DocumentSummaryInfoPropertyFactory.Instance : DefaultPropertyFactory.Instance;

            foreach (var op in Properties)
            {
                ITypedPropertyValue p = factory.NewProperty(op.VTType, Context.CodePage, op.PropertyIdentifier);
                p.Value = op.Value;
                ps.PropertySet0.Properties.Add(p);
                ps.PropertySet0.PropertyIdentifierAndOffsets.Add(new PropertyIdentifierAndOffset() { PropertyIdentifier = op.PropertyIdentifier, Offset = 0 });
            }

            if (HasUserDefinedProperties)
            {
                ps.NumPropertySets = 2;

                ps.PropertySet1 = new PropertySet
                {
                    // Number of user defined properties, plus 1 for the name dictionary
                    NumProperties = (uint)UserDefinedProperties.Properties.Count() + 1,
                    PropertyIdentifierAndOffsets = new List<PropertyIdentifierAndOffset>(),
                    Properties = new List<IProperty>(),
                    PropertyContext = UserDefinedProperties.Context
                };

                ps.FMTID1 = new Guid(WellKnownFMTID.FMTID_UserDefinedProperties);
                ps.Offset1 = 0;

                // Add the dictionary containing the property names
                AddDictionaryPropertyToPropertySet(UserDefinedProperties.PropertyNames, ps.PropertySet1);

                // Add the properties themselves
                foreach (var op in UserDefinedProperties.Properties)
                {
                    ITypedPropertyValue p = DefaultPropertyFactory.Instance.NewProperty(op.VTType, ps.PropertySet1.PropertyContext.CodePage, op.PropertyIdentifier);
                    p.Value = op.Value;
                    ps.PropertySet1.Properties.Add(p);
                    ps.PropertySet1.PropertyIdentifierAndOffsets.Add(new PropertyIdentifierAndOffset() { PropertyIdentifier = op.PropertyIdentifier, Offset = 0 });
                }
            }

            ps.Write(bw);
        }

        private void AddDictionaryPropertyToPropertySet(Dictionary<uint, string> propertyNames, PropertySet propertySet)
        {
            IDictionaryProperty dictionaryProperty = new DictionaryProperty(propertySet.PropertyContext.CodePage)
            {
                Value = propertyNames
            };
            propertySet.Properties.Add(dictionaryProperty);
            propertySet.PropertyIdentifierAndOffsets.Add(new PropertyIdentifierAndOffset() { PropertyIdentifier = 0, Offset = 0 });
        }

        // Determine the type of the container from the FMTID0 property.
        private static ContainerType ContainerTypeFromFmtId(Guid fmtId0)
        {
            if (fmtId0 == Guid.Parse(WellKnownFMTID.FMTID_SummaryInformation))
                return ContainerType.SummaryInfo;
            else if (fmtId0 == Guid.Parse(WellKnownFMTID.FMTID_DocSummaryInformation))
                return ContainerType.DocumentSummaryInfo;
            else if (fmtId0 == Guid.Parse(WellKnownFMTID.FMTID_GlobalInfo))
                return ContainerType.GlobalInfo;
            else if (fmtId0 == Guid.Parse(WellKnownFMTID.FMTID_ImageInfo))
                return ContainerType.ImageInfo;
            else if (fmtId0 == Guid.Parse(WellKnownFMTID.FMTID_ImageContents))
                return ContainerType.ImageContents;

            return ContainerType.AppSpecific;
        }

        // Determine the FMTID property from the container type.
        // Note: Uses FMTID_DocSummaryInformation by default to match the previous behavior.
        private static Guid FmtIdFromContainerType(ContainerType containerType)
        {
            switch (containerType)
            {
                case ContainerType.SummaryInfo:
                    return Guid.Parse(WellKnownFMTID.FMTID_SummaryInformation);
                case ContainerType.GlobalInfo:
                    return Guid.Parse(WellKnownFMTID.FMTID_GlobalInfo);
                case ContainerType.ImageContents:
                    return Guid.Parse(WellKnownFMTID.FMTID_ImageContents);
                case ContainerType.ImageInfo:
                    return Guid.Parse(WellKnownFMTID.FMTID_ImageInfo);
                default:
                    return Guid.Parse(WellKnownFMTID.FMTID_DocSummaryInformation);
            }
        }
    }
}
