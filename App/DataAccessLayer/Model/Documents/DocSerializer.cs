using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    public class DocSerializer : IDocSerializer
    {
        private readonly IAttributeRepository _attributeRepository;
        private readonly IDocDefRepository _docDefRepository;
        private readonly IEnumRepository _enumRepository;

        public DocSerializer(IDataContext dataContext)
        {
            _attributeRepository = new AttributeRepository(/*dataContext*/); 
            _docDefRepository = new DocDefRepository(dataContext);
            _enumRepository = new EnumRepository(dataContext);
        }

        /// <summary>
        /// Сериализует Документ
        /// </summary>
        /// <param name="docs">Список Документов для сериализации</param>
        /// <returns>Строка содержащая xml сериализованного документа</returns>
        public string Serialize(IList<Doc> docs)
        {
            var xDoc = new XDocument();
            var xRoot = new XElement("Docs");

            foreach (Doc doc in docs)
            {
                var xElement = new XElement("Doc",
                                            new XAttribute("Id", doc.DocDef.Id.ToString()),
                                            new XAttribute("Name", doc.DocDef.Name)
                    );

                foreach (var attribute in doc.Attributes)
                {
                    var xAtr = new XElement("Attribute",
                                            new XAttribute("Id", attribute.AttrDef.Id.ToString()),
                                            new XAttribute("Name", attribute.AttrDef.Name)
                        ) {Value = attribute.ObjectValue.ToString()};

                    xElement.Add(xAtr);
                }
                xRoot.Add(xElement);
            }
            
            xDoc.Add(xRoot);

            using (var strWriter = new StringWriter())
            {
                xDoc.Save(strWriter);
                return strWriter.ToString();
            }
        }

        /// <summary>
        /// Десериализует документ
        /// </summary>
        /// <param name="xmlDoc">Строка содержащая xml сериализованного документа</param>
        /// <returns>Список Документов восстановленных из xml</returns>
        
        public IList<Doc> DeSerialize(string xmlDoc)
        {
            var xDocument = XDocument.Parse(xmlDoc);

            var docList = new List<Doc>();
            if (xDocument.Root == null) return docList;

            foreach (XElement xElementDoc in xDocument.Root.Elements())
            {
                XAttribute xDocId = xElementDoc.Attribute("Id");
                XAttribute xDocName = xElementDoc.Attribute("Name");

                if (xDocId == null && xDocName == null)
                {
                    throw new ApplicationException("Тип документа не указан");
                }

                DocDef docDef = xDocId == null ? 
                    _docDefRepository.DocDefByName(xDocName.Value) :
                    _docDefRepository.DocDefById(Guid.Parse(xDocId.Value)) ;

                var doc = new Doc
                              {
                                  Id = Guid.NewGuid(),
                                  DocDef = docDef,
                                  Attributes = new List<AttributeBase>()
                              };

                
                foreach (XElement xElementAttr in xElementDoc.Elements())
                {
                    XAttribute xElementId = xElementAttr.Attribute("Id");
                    XAttribute xElementName = xElementAttr.Attribute("Name");

                    if (xElementId == null && xElementName == null)
                    {
                        throw new ApplicationException("Невозможно определить атрибут");
                    }

                    var atr = xElementId == null
                                  ? _attributeRepository.CreateAttribute(docDef.GetByName(xElementName.Value))
                                  : _attributeRepository.CreateAttribute(docDef.GetById(Guid.Parse(xElementId.Value)));
                    //                        _attributeRepository.GetAttributeByName(xElementName.Value, doc.DocDef.Id) :
                    //                    _attributeRepository.GetAttributeById(Guid.Parse(xElementId.Value), Guid.Empty);


                    int typeId = atr.AttrDef.Type.Id;

                    switch (typeId)
                    {
                        case (short)CissaDataType.Enum:
                            var enumId = atr.AttrDef.EnumDefType.Id;
                            atr.ObjectValue = _enumRepository.GetEnumValueId(enumId, xElementAttr.Value);
                            break;

                        case (short)CissaDataType.Doc:
                            break;

                        case (short)CissaDataType.DocList:
                            break;

                        default:
                            atr.ObjectValue = xElementAttr.Value;
                            break;
                    }

                    //string attributeDefId = xElementAttr.Attribute("Id").Value;
                    //string attributeDefName = xElementAttr.Attribute("AttributeDefName").Value;
                    
                    doc.Attributes.Add(atr);
                }

                docList.Add(doc);
            }

            return docList;
        }
    }

    public interface IDocSerializer
    {
        /// <summary>
        /// Сериализует Документ
        /// </summary>
        /// <param name="docs">Список Документов для сериализации</param>
        /// <returns>Строка содержащая xml сериализованного документа</returns>
        string Serialize(IList<Doc> docs);

        /// <summary>
        /// Десериализует документ
        /// </summary>
        /// <param name="xmlDoc">Строка содержащая xml сериализованного документа</param>
        /// <returns>Список Документов восстановленных из xml</returns>
        IList<Doc> DeSerialize(string xmlDoc);
    }
}
