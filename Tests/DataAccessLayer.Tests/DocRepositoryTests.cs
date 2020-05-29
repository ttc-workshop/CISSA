using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.DataAccessLayerTests
{
    /// <summary>
    /// Тестирует DorRepository
    /// </summary>
    [TestClass]
    public class DocRepositoryTests
    {
        private readonly Guid _userId = Guid.Parse("180B1E71-6CDA-4887-9F83-941A12D7C979");

        [TestMethod]
        public void Check()
        {
            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, _userId);
                var doc = new Doc();

                rep.Check(doc);
            }
        }

        [TestMethod]
        public void SaveAndDelete()
        {
            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, /*_userId*/new Guid("{6909C6DF-3627-41E7-9346-C5CF2536AF43}"));

                var doc = new Doc
                              {
                                  Id = Guid.NewGuid(),
                                  DocDef = new DocDef { Id = /*Guid.Parse("2194385f-befc-452f-ab7c-19dd5f033225")*/ new Guid("{011B6A87-170D-412E-BB8F-B24FD8E19145}") },
                                  CreationTime = DateTime.Now,
                                  UserId = new Guid("{6909C6DF-3627-41E7-9346-C5CF2536AF43}"),
                                  Attributes = new List<AttributeBase>()
                              };

                var itemcur = new CurrencyAttribute(
                    new AttrDef
                        {
                            Id = new Guid("{0FDD3BD5-9FB5-4BE0-8C24-205529155AF0}")/*Guid.Parse("4B28F0D1-7DED-4828-87C6-23FC88576F9C")*/, 
                            Type = new TypeDef { Id = (short) CissaDataType.Currency }
                        }
                    ) { Value = 777.77m };


                doc.Attributes.Add(itemcur);

                rep.Save(doc);

                rep.DeleteById(doc.Id);
            }
        }

        [TestMethod]
        public void SaveAndSaveAgain()
        {
            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, _userId);

                var doc = new Doc
                              {
                                  Id = Guid.NewGuid(),
                                  DocDef = new DocDef {Id = Guid.Parse("{4455B9CB-2564-4A92-A295-E3C0BEDB7AC2}")},
                                  Attributes = new List<AttributeBase>()
                              };

                var itemcur = new TextAttribute(
                    new AttrDef {Id = Guid.Parse("{33CC96EE-C65E-4899-B0E1-AF089D4185DC}")}
                    ) {Value = "Ivanov"};


                doc.Attributes.Add(itemcur);

                rep.Save(doc);

                var savedDoc = rep.Save(doc);

                rep.Save(savedDoc);

                rep.DeleteById(doc.Id);
            }
        }

        [TestMethod]
        public void LoadById()
        {
            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, _userId);
                var docId = Guid.Parse("00001b55-f110-452f-b08f-8ceb0a112be0");

                Doc doc = rep.LoadById(docId);

                Assert.IsNotNull(doc);
                Assert.IsNotNull(doc.AttrCurrency);
                Assert.IsNotNull(doc.AttrInt);
                Assert.IsNotNull(doc.AttrText);
                Assert.IsNotNull(doc.AttrFloat);
                Assert.IsNotNull(doc.AttrEnum);
                Assert.IsNotNull(doc.AttrDoc);
            }
        }


        [TestMethod]
        public void LoadByIdAndSave()
        {
            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, _userId);
                var docId = Guid.Parse("00001b55-f110-452f-b08f-8ceb0a112be0");

                Doc doc = rep.LoadById(docId);

                doc.Id = Guid.NewGuid();
                rep.Save(doc);

                Doc savedDoc = rep.LoadById(doc.Id);

                Assert.IsNotNull(savedDoc);
                Assert.IsNotNull(savedDoc.AttrCurrency);
                Assert.IsNotNull(savedDoc.AttrInt);
                Assert.IsNotNull(savedDoc.AttrText);
                Assert.IsNotNull(savedDoc.AttrFloat);
                Assert.IsNotNull(savedDoc.AttrEnum);
                Assert.IsNotNull(savedDoc.AttrDoc);
            }
        }

        [TestMethod]
        public void New()
        {
            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, _userId);
                var docDefId = Guid.Parse("2194385f-befc-452f-ab7c-19dd5f033225");

                Doc doc = rep.New(docDefId);

                Assert.IsNotNull(doc);
                Assert.IsNotNull(doc.AttrCurrency);
                Assert.IsNotNull(doc.AttrInt);
                Assert.IsNotNull(doc.AttrText);
                Assert.IsNotNull(doc.AttrFloat);
                Assert.IsNotNull(doc.AttrEnum);
                Assert.IsNotNull(doc.AttrDoc);
            }
        }

        [TestMethod]
        public void NewComplex()
        {
            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, _userId);
                var docDefId = Guid.Parse("{C59A57D2-86F7-440F-BCF0-8DCC252B8C1F}");

                Doc doc = rep.New(docDefId);

                Assert.IsNotNull(doc);
                Assert.IsNotNull(doc.AttrCurrency);
                Assert.IsNotNull(doc.AttrInt);
                Assert.IsNotNull(doc.AttrText);
                Assert.IsNotNull(doc.AttrFloat);
                Assert.IsNotNull(doc.AttrEnum);
                Assert.IsNotNull(doc.AttrDoc);
            }
        }

        [TestMethod]
        [ExpectedException(
            typeof(ApplicationException), 
            "Пользователь 180B1E71-6CDA-4887-9F83-941A12D7C979 не имеет разрешений для создания документа типа C59A57D2-86F7-440F-BCF0-8DCC252B8C1F")]
        public void ForUserWithoutPermissions()
        {
            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, _userId);
                var docDefId = Guid.Parse("{C59A57D2-86F7-440F-BCF0-8DCC252B8C1F}");

                Doc doc = rep.New(docDefId);

                Assert.IsNotNull(doc);
            }
        }

        [TestMethod]
        public void InheritedDocNew()
        {
            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, _userId);
                var docDefId = Guid.Parse("{AC06F1EA-0065-4736-B92B-0ED8E9B78D35}");

                Doc doc = rep.New(docDefId);

                Assert.IsNotNull(doc);
                Assert.IsNotNull(doc.AttrCurrency);
                Assert.IsNotNull(doc.AttrInt);
                Assert.IsNotNull(doc.AttrText);
                Assert.IsNotNull(doc.AttrFloat);
                Assert.IsNotNull(doc.AttrEnum);
                Assert.IsNotNull(doc.AttrDoc);
            }
        }

        [TestMethod]
        public void SaveComplex()
        {
            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, _userId);
                var docDefId = Guid.Parse("{C59A57D2-86F7-440F-BCF0-8DCC252B8C1F}");

                Doc doc = rep.New(docDefId);

                Doc savedDoc = rep.Save(doc);

                Assert.IsNotNull(savedDoc);
                Assert.IsNotNull(savedDoc.AttrCurrency);
                Assert.IsNotNull(savedDoc.AttrInt);
                Assert.IsNotNull(savedDoc.AttrText);
                Assert.IsNotNull(savedDoc.AttrFloat);
                Assert.IsNotNull(savedDoc.AttrEnum);
                Assert.IsNotNull(savedDoc.AttrDoc);
            }
        }

        [TestMethod]
        [ExpectedException(
            typeof(ApplicationException),
            "Пользователь 180B1E71-6CDA-4887-9F83-941A12D7C979 не имеет разрешений для добавления документа типа C59A57D2-86F7-440F-BCF0-8DCC252B8C1F")]
        public void SaveWithoutPermissions()
        {
            var userId = Guid.Parse("180B1E71-6CDA-4887-9F83-941A12D7C979");

            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, userId);
                var docDefId = Guid.Parse("{C59A57D2-86F7-440F-BCF0-8DCC252B8C1F}");

                var doc1 = new Doc
                               {
                                   Id = Guid.NewGuid(),
                                   DocDef = new DocDef {Id = docDefId},
                                   Attributes = new List<AttributeBase>()
                               };

                rep.Save(doc1);
            }
        }

        [TestMethod]
        [ExpectedException(
            typeof(ApplicationException),
            "Пользователь 180B1E71-6CDA-4887-9F83-941A12D7C979 не имеет разрешений для загрузки документа типа 9BE7F7E9-EFBF-4D8D-B2E5-AB1AC9990078")]
        public void LoadByIdWithoutPermissions()
        {
            var userId = Guid.Parse("180B1E71-6CDA-4887-9F83-941A12D7C979");

            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, userId);
                var docId = Guid.Parse("{9BE7F7E9-EFBF-4D8D-B2E5-AB1AC9990078}");

                rep.LoadById(docId);
            }
        }

        [TestMethod]
        [ExpectedException(
            typeof(ApplicationException),
            "Пользователь 180B1E71-6CDA-4887-9F83-941A12D7C979 не имеет разрешений для удаления документа типа 00001b55-f110-452f-b08f-8ceb0a112be0")]
        public void DeleteWithoutPemissions()
        {
            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, _userId);
                var docId = Guid.Parse("00001b55-f110-452f-b08f-8ceb0a112be0");

                rep.DeleteById(docId);
            }
        }


        [TestMethod]
        public void SaveInheritedComplex()
        {
            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, _userId);
                var docDefId = Guid.Parse("{AC06F1EA-0065-4736-B92B-0ED8E9B78D35}");

                Doc doc = rep.New(docDefId);

                Doc savedDoc = rep.Save(doc);

                Assert.IsNotNull(savedDoc);
                Assert.IsNotNull(savedDoc.AttrCurrency);
                Assert.IsNotNull(savedDoc.AttrInt);
                Assert.IsNotNull(savedDoc.AttrText);
                Assert.IsNotNull(savedDoc.AttrFloat);
                Assert.IsNotNull(savedDoc.AttrEnum);
                Assert.IsNotNull(savedDoc.AttrDoc);
            }
        }

        [TestMethod]
        public void LoadComplex()
        {
            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, _userId);
                var docDefId = Guid.Parse("{C59A57D2-86F7-440F-BCF0-8DCC252B8C1F}");

                Doc doc = rep.New(docDefId);

                Doc savedDoc = rep.Save(doc);

                Doc loaded = rep.LoadById(savedDoc.Id);

                Assert.IsNotNull(loaded);
                Assert.IsNotNull(loaded.AttrCurrency);
                Assert.IsNotNull(loaded.AttrInt);
                Assert.IsNotNull(loaded.AttrText);
                Assert.IsNotNull(loaded.AttrFloat);
                Assert.IsNotNull(loaded.AttrEnum);
                Assert.IsNotNull(loaded.AttrDoc);
            }
        }

        [TestMethod]
        public void LoadInheritedComplex()
        {
            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, _userId);
                var docDefId = Guid.Parse("{AC06F1EA-0065-4736-B92B-0ED8E9B78D35}");

                Doc doc = rep.New(docDefId);

                Doc savedDoc = rep.Save(doc);

                Doc loaded = rep.LoadById(savedDoc.Id);

                Assert.IsNotNull(loaded);
                Assert.IsNotNull(loaded.AttrCurrency);
                Assert.IsNotNull(loaded.AttrInt);
                Assert.IsNotNull(loaded.AttrText);
                Assert.IsNotNull(loaded.AttrFloat);
                Assert.IsNotNull(loaded.AttrEnum);
                Assert.IsNotNull(loaded.AttrDoc);
            }
        }
        [TestMethod]
        [ExpectedException(typeof(ApplicationException), "Не найден документ с идентификатором 9BE7F7E9-EFBF-4D8D-B2E5-AB1AC9990078")]
        public void LoadByIdNonExistenDoc()
        {
            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, _userId);
                var docId = Guid.Parse("{9BE7F7E9-EFBF-4D8D-B2E5-AB1AC9990078}");

                rep.LoadById(docId);
            }
        }

        [TestMethod]
        public void GetDocState()
        {
            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, _userId);

                var docId = Guid.Parse("73899f52-f0c9-4579-b8c9-239bcb5a7680");

                var state = rep.GetDocState(docId);

                Assert.IsNotNull(state);
            }
        }

        [TestMethod]
        public void SetDocState()
        {
            var workerId = Guid.Parse("180b1e71-6cda-4887-9f83-941a12d7c979");

            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, workerId);

                var docId = Guid.Parse("73899f52-f0c9-4579-b8c9-239bcb5a7680");
                //подставить реальный stateTypeId
                var stateTypeId = Guid.Parse("{E5989DCF-7044-46CE-AA30-000000000001}") /*На подпись*/;

                rep.SetDocState(docId, stateTypeId);
            }
        }

/*
        [TestMethod]
        public void Search() // Method removed!!
        {
            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, _userId);

                var parameters = new List<SearchParameter>
                                     {
                                         new SearchParameter
                                             {
                                                 Attribute = new TextAttribute(new AttrDef { Id = Guid.Parse("3E04EBAE-F4D8-4FEC-A394-E79878326ED2") })
                                                                 {
                                                                     Value = "ффф"
                                                                 },
                                                 CompareOperator = CompareOperator.Like
                                             },
                                         new SearchParameter
                                             {
                                                 Attribute = new IntAttribute(new AttrDef
                                                                                  {
                                                                                      Id =
                                                                                          Guid.Parse(
                                                                                              "c21ec336-19f4-428f-bb5d-6adfcc95f61c")
                                                                                  })
                                                                 {
                                                                     Value = 333
                                                                 },
                                                 CompareOperator = CompareOperator.Equal
                                             }
                                     };

                List<Guid> findedDocs = rep.Search(parameters, LogicOperation.Or);

                Assert.IsNotNull(findedDocs);
                Assert.AreNotEqual(0, findedDocs.Count);
            }
        }*/

        [TestMethod]
        public void SaveWithHistoryDateTIme()
        {
            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, _userId);

                Guid docDefId = Guid.Parse("{ED9D1436-B4C4-4AC9-97CD-F142BA48F43A}");

                Doc newDoc = rep.New(docDefId);

                newDoc["DateTimeAttr"] = DateTime.Now.AddYears(1);

                rep.Save(newDoc);
            }
        }

        [TestMethod]
        public void SaveWithHistory()
        {
            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, _userId);

                for (int i = 0; i < 1; i++)
                {
                    var doc = new Doc
                                  {
                                      Id = Guid.Parse("{D9713AEE-6857-4A01-B3BA-E160B8844B91}"),
                                      //всегда пишем в один и тот же документ
                                      DocDef = new DocDef {Id = Guid.Parse("ED9D1436-B4C4-4AC9-97CD-F142BA48F43A")},
                                      Attributes = new List<AttributeBase>()
                                  };

                    var floatAttribute = new FloatAttribute(
                        new AttrDef {Id = Guid.Parse("{9B1C8BA9-EC8A-4D7E-8542-178302452EE8}")})
                                             {
                                                 Value = (new Random()).Next()/3.333
                                             };

                    var intAttribute =
                        new IntAttribute(new AttrDef {Id = Guid.Parse("{7F7C106F-A7C4-4802-BC4C-EA69DCF23A9A}")})
                            {
                                Value = (new Random()).Next()
                            };

                    var textAttribute = new TextAttribute(
                        new AttrDef {Id = Guid.Parse("{6DA77C5F-3727-4DDF-86A7-BE8248C4AC23}")})
                                            {
                                                Value = "Som random text and ranodm value is " + (new Random()).Next()
                                            };


                    var dateTimeAttribute = new DateTimeAttribute(
                        new AttrDef {Id = Guid.Parse("2C43F420-3875-43CE-A7C6-9C8811B7C824")})
                                                {
                                                    Value = DateTime.Now.AddYears(5)
                                                };

                    var blobAttribute = new BlobAttribute(
                        new AttrDef {Id = Guid.Parse("{67BAE7F6-E159-471F-932A-72F10209F646}")})
                                            {
                                                Value = new byte[] {1, 2, 3}
                                            };

                    doc.Attributes.Add(floatAttribute);
                    doc.Attributes.Add(intAttribute);
                    doc.Attributes.Add(textAttribute);
                    doc.Attributes.Add(dateTimeAttribute);
                    doc.Attributes.Add(blobAttribute);

                    rep.Save(doc);

                    Doc loadedDoc = rep.LoadById(doc.Id);

                    Assert.AreEqual(6, loadedDoc.Attributes.Count);
                    Assert.AreEqual(loadedDoc.AttrText.First().Value, textAttribute.Value);
                    Assert.AreEqual(loadedDoc.AttrInt.First().Value, intAttribute.Value);
                    Assert.AreEqual(loadedDoc.AttrFloat.First().Value, floatAttribute.Value);
                    Assert.AreEqual(null, loadedDoc.AttrDoc.First().Value);
                    Assert.AreEqual(blobAttribute.Value.Length, loadedDoc.AttrBlob.First().Value.Length);

                    var loadedDateTime = (DateTime) loadedDoc.AttrDateTime.First().Value;
                    var currentDateTime = (DateTime) dateTimeAttribute.Value;

                    Assert.AreEqual(loadedDateTime.Year, currentDateTime.Year);
                    Assert.AreEqual(loadedDateTime.Month, currentDateTime.Month);
                    Assert.AreEqual(loadedDateTime.Day, currentDateTime.Day);
                    Assert.AreEqual(loadedDateTime.Hour, currentDateTime.Hour);
                    Assert.AreEqual(loadedDateTime.Minute, currentDateTime.Minute);
                    Assert.AreEqual(loadedDateTime.Second, currentDateTime.Second);
                    // не всегда совпадает из-за менее точного типа DateTime в базе данных
                    //Assert.AreEqual(loadedDateTime.Millisecond, currentDateTime.Millisecond);
                }
            }
        }

        [TestMethod]
        public void SaveDocList()
        {
            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, _userId);

                var doc = new Doc
                              {
                                  Id = Guid.NewGuid(),
                                  //всегда пишем в один и тот же документ
                                  DocDef = new DocDef {Id = Guid.Parse("{2194385F-BEFC-452F-AB7C-19DD5F033225}")},
                                  Attributes = new List<AttributeBase>()
                              };


                int itemsCount = 0;
                var docListAttribute =
                    new DocListAttribute(new AttrDef {Id = Guid.Parse("{78DDDBA1-4223-4E74-B43F-1B8FD5E687F2}")})
                        {
                            // закидываем 3 произволных идентификатора документа "TestDoc"
                            ItemsDocId =
                                rep.List(out itemsCount, Guid.Parse("{846B1B55-F110-452F-B08F-8CEB0A112BE0}"), 0, 3)
                        };

                doc.Attributes.Add(docListAttribute);

                rep.Save(doc);

                Doc loadedDoc = rep.LoadById(doc.Id);

                Assert.IsNotNull(loadedDoc);
                Assert.AreEqual(7, loadedDoc.Attributes.Count);
                Assert.IsNotNull(loadedDoc.AttrDocList);
                Assert.AreNotEqual(0, loadedDoc.AttrDocList.Count());
                Assert.AreEqual(3, loadedDoc.AttrDocList.First().ItemsDocId.Count());

                foreach (Guid id in docListAttribute.ItemsDocId)
                {
                    Guid id1 = id;
                    Assert.IsTrue(loadedDoc.AttrDocList.First().ItemsDocId.Contains(id1));
                }
            }
        }

        [TestMethod]
        public void LoadAndSaveWithHistory()
        {
            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, _userId);

                Guid docId = Guid.Parse("{D9713AEE-6857-4A01-B3BA-E160B8844B91}");

                // LOAD OLD DOC, CHANGING ATTRIBUTES AND SAVE
                Doc doc = rep.LoadById(docId, DateTime.Now);

                var floatAttribute = doc.AttrFloat.First();
                floatAttribute.Value = (new Random()).Next()/3.333;

                var intAttribute = doc.AttrInt.First();
                intAttribute.Value = (new Random()).Next();

                var textAttribute = doc.AttrText.First();
                textAttribute.Value = "Som random text and ranodm value is " + (new Random()).Next();

                var dateTimeAttribute = doc.AttrDateTime.First();
                dateTimeAttribute.Value = DateTime.Now.AddYears(5);

                rep.Save(doc);


                // LOADING SAVED DOC AND TEST
                Doc loadedDoc = rep.LoadById(doc.Id);

                Assert.AreEqual(6, loadedDoc.Attributes.Count);
                Assert.AreEqual(loadedDoc.AttrText.First().Value, textAttribute.Value);
                Assert.AreEqual(loadedDoc.AttrInt.First().Value, intAttribute.Value);
                Assert.AreEqual(loadedDoc.AttrFloat.First().Value, floatAttribute.Value);
                Assert.AreEqual(null, loadedDoc.AttrDoc.First().Value);

                var loadedDateTime = (DateTime) loadedDoc.AttrDateTime.First().Value;
                var currentDateTime = (DateTime) dateTimeAttribute.Value;

                Assert.AreEqual(loadedDateTime.Year, currentDateTime.Year);
                Assert.AreEqual(loadedDateTime.Month, currentDateTime.Month);
                Assert.AreEqual(loadedDateTime.Day, currentDateTime.Day);
                Assert.AreEqual(loadedDateTime.Hour, currentDateTime.Hour);
                Assert.AreEqual(loadedDateTime.Minute, currentDateTime.Minute);
                Assert.AreEqual(loadedDateTime.Second, currentDateTime.Second);
                // не всегда совпадает из-за менее точного типа DateTime в базе данных
                //Assert.AreEqual(loadedDateTime.Millisecond, currentDateTime.Millisecond);
            }
        }


        [TestMethod]
        public void LoadAndSaveWithHistoryWithTime()
        {
            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, _userId);

                Guid docId = Guid.Parse("{D9713AEE-6857-4A01-B3BA-E160B8844B91}");

                // LOAD OLD DOC, CHANGING ATTRIBUTES AND SAVE
                Doc doc = rep.LoadById(docId, DateTime.Now);

                var floatAttribute = doc.AttrFloat.First();
                floatAttribute.Value = (new Random()).Next()/3.333;

                var intAttribute = doc.AttrInt.First();
                intAttribute.Value = (new Random()).Next();

                var textAttribute = doc.AttrText.First();
                textAttribute.Value = "Som random text and ranodm value is " + (new Random()).Next();

                var dateTimeAttribute = doc.AttrDateTime.First();
                dateTimeAttribute.Value = DateTime.Now.AddYears(5);

                rep.Save(doc);


                // LOADING SAVED DOC AND TEST
                Doc loadedDoc = rep.LoadById(doc.Id, DateTime.Now);

                Assert.AreEqual(6, loadedDoc.Attributes.Count);
                Assert.AreEqual(loadedDoc.AttrText.First().Value, textAttribute.Value);
                Assert.AreEqual(loadedDoc.AttrInt.First().Value, intAttribute.Value);
                Assert.AreEqual(loadedDoc.AttrFloat.First().Value, floatAttribute.Value);
                Assert.AreEqual(null, loadedDoc.AttrDoc.First().Value);

                var loadedDateTime = (DateTime) loadedDoc.AttrDateTime.First().Value;
                var currentDateTime = (DateTime) dateTimeAttribute.Value;

                Assert.AreEqual(loadedDateTime.Year, currentDateTime.Year);
                Assert.AreEqual(loadedDateTime.Month, currentDateTime.Month);
                Assert.AreEqual(loadedDateTime.Day, currentDateTime.Day);
                Assert.AreEqual(loadedDateTime.Hour, currentDateTime.Hour);
                Assert.AreEqual(loadedDateTime.Minute, currentDateTime.Minute);
                Assert.AreEqual(loadedDateTime.Second, currentDateTime.Second);
                // не всегда совпадает из-за менее точного типа DateTime в базе данных
                //Assert.AreEqual(loadedDateTime.Millisecond, currentDateTime.Millisecond);
            }
        }


        [TestMethod]
        public void LoadAndSaveWithHistoryWithTimeForDocAttribute()
        {
            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, _userId);

                Guid docRefId1 = Guid.Parse("6F6DA058-2299-4BDD-A868-21BE90DE8F3F");
                Guid docRefId2 = Guid.Parse("73899F52-F0C9-4579-B8C9-239BCB5A7680");
                Guid docRefId3 = Guid.Parse("BF5D41E5-27C1-4FEC-9838-35A7FC184397");


                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //    DOC 1
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                var doc1 = new Doc
                               {
                                   Id = Guid.Parse("{7DF5F4D9-C7C4-4090-AD5A-C2DC4720341D}"),
                                   DocDef = new DocDef {Id = Guid.Parse("ED9D1436-B4C4-4AC9-97CD-F142BA48F43A")},
                                   Attributes = new List<AttributeBase>()
                               };

                var docAttr1 = new DocAttribute(new AttrDef {Id = Guid.Parse("{5EDA3DC6-D27A-4C73-A02A-3CF03FC508E3}")})
                                   {
                                       Value = docRefId1
                                   };

                doc1.Attributes.Add(docAttr1);

                rep.Save(doc1);

                Thread.Sleep(500);
                DateTime dateTime1 = DateTime.Now;
                Thread.Sleep(500);


                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //    DOC 2
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                var doc2 = new Doc
                               {
                                   Id = Guid.Parse("{7DF5F4D9-C7C4-4090-AD5A-C2DC4720341D}"),
                                   DocDef = new DocDef {Id = Guid.Parse("ED9D1436-B4C4-4AC9-97CD-F142BA48F43A")},
                                   Attributes = new List<AttributeBase>()
                               };

                var docAttr2 = new DocAttribute(new AttrDef {Id = Guid.Parse("{5EDA3DC6-D27A-4C73-A02A-3CF03FC508E3}")})
                                   {
                                       Value = docRefId2
                                   };

                doc2.Attributes.Add(docAttr2);

                rep.Save(doc2);

                Thread.Sleep(500);
                DateTime dateTime2 = DateTime.Now;
                Thread.Sleep(500);


                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //    DOC 3
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                var doc3 = new Doc
                               {
                                   Id = Guid.Parse("{7DF5F4D9-C7C4-4090-AD5A-C2DC4720341D}"),
                                   DocDef = new DocDef {Id = Guid.Parse("ED9D1436-B4C4-4AC9-97CD-F142BA48F43A")},
                                   Attributes = new List<AttributeBase>()
                               };

                var docAttr3 = new DocAttribute(new AttrDef {Id = Guid.Parse("{5EDA3DC6-D27A-4C73-A02A-3CF03FC508E3}")})
                                   {
                                       Value = docRefId3
                                   };

                doc3.Attributes.Add(docAttr3);

                rep.Save(doc3);

                Thread.Sleep(500);
                DateTime dateTime3 = DateTime.Now;
                Thread.Sleep(500);

                Doc loadedDoc1 = rep.LoadById(doc1.Id, dateTime1);
                Doc loadedDoc2 = rep.LoadById(doc2.Id, dateTime2);
                Doc loadedDoc3 = rep.LoadById(doc3.Id, dateTime3);


                Assert.AreEqual(loadedDoc1.AttrDoc.First().Value, docRefId1);
                Assert.AreEqual(loadedDoc2.AttrDoc.First().Value, docRefId2);
                Assert.AreEqual(loadedDoc3.AttrDoc.First().Value, docRefId3);


                //Assert.AreEqual(4, loadedDoc.Attributes.Count);
                //Assert.AreEqual(loadedDoc.AttrText.First().Value, textAttribute.Value);
                //Assert.AreEqual(loadedDoc.AttrInt.First().Value, intAttribute.Value);
                //Assert.AreEqual(loadedDoc.AttrFloat.First().Value, floatAttribute.Value);

                //var loadedDateTime = (DateTime)loadedDoc.AttrDateTime.First().Value;
                //var currentDateTime = (DateTime)dateTimeAttribute.Value;

                //Assert.AreEqual(loadedDateTime.Year, currentDateTime.Year);
                //Assert.AreEqual(loadedDateTime.Month, currentDateTime.Month);
                //Assert.AreEqual(loadedDateTime.Day, currentDateTime.Day);
                //Assert.AreEqual(loadedDateTime.Hour, currentDateTime.Hour);
                //Assert.AreEqual(loadedDateTime.Minute, currentDateTime.Minute);
                //Assert.AreEqual(loadedDateTime.Second, currentDateTime.Second);
                // не всегда совпадает из-за менее точного типа DateTime в базе данных
                //Assert.AreEqual(loadedDateTime.Millisecond, currentDateTime.Millisecond);
            }
        }

        [TestMethod]
        public void LoadDocAttributeValue()
        {
            Guid docDefId = Guid.Parse("1623596C-F4E8-4AAC-8203-98E3ECE4863D");

            using (var dataContext = new DataContext())
            {
                var docsWithDocAttrs = dataContext.Documents.Where(d => d.Def_Id == docDefId).Select(d => d.Id);

                var rep = new DocRepository(dataContext, _userId);

                foreach (var docsWithDocAttr in docsWithDocAttrs)
                {
                    var doc = rep.LoadById(docsWithDocAttr);

                    var docAttr = doc.AttrDoc.First();

                    if (docAttr.Value != null)
                    {
                        var doc2 = rep.LoadById(docAttr.Value ?? Guid.Empty);
                    }
                }
            }
        }
    }

}
