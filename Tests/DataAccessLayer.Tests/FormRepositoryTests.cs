using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Intersoft.CISSA.DataAccessLayerTests
{
    /// <summary>
    /// Сводное описание для FormRepositoryTests
    /// </summary>
    [TestClass]
    public class FormRepositoryTests
    {
        private readonly Guid _userId = new Guid("180B1E71-6CDA-4887-9F83-941A12D7C979");

        private static BizForm GetNewFormForSave()
        {
            var formForSave =
                new BizDetailForm
                    {
                        Id = Guid.Parse("b88fe6ca-d327-44c5-bc84-37f47d85fb4a"),
                        DocumentDefId = Guid.Parse("{2194385F-BEFC-452F-AB7C-19DD5F033225}")
                        //DocumentId = Guid.NewGuid(), // сохранение нового документа
                    };

            formForSave.Children =
                new List<BizControl>
                    {
                        new BizEditInt
                            {
                                Id = Guid.Parse("0c42fb7d-4a3f-4d53-aa57-f0ad922a17a7"),
                                AttributeDefId = Guid.Parse("c21ec336-19f4-428f-bb5d-6adfcc95f61c")
//                                Attribute = new IntAttribute( new AttrDef
//                                                                  {
//                                                                      Id =
//                                                                          Guid.Parse(
//                                                                              "c21ec336-19f4-428f-bb5d-6adfcc95f61c")
//                                                                  }) {Value = 765}
                            },

                        new BizEditFloat
                            {
                                Id = Guid.Parse("D2592B70-24E3-4A78-A99F-CADF83B7649C"),
                                AttributeDefId = Guid.Parse("bc934574-b5f4-4851-844a-4166ac5a15f9")
//                                Attribute = new FloatAttribute (new AttrDef
//                                                                  {
//                                                                      Id =
//                                                                          Guid.Parse(
//                                                                              "bc934574-b5f4-4851-844a-4166ac5a15f9")
//                                                                  }){
//                                                    Value = 765.45f
//                                                }
                            },

                        new BizEditCurrency
                            {
                                Id = Guid.Parse("89fd428d-ca2a-4399-a9fa-326880666736"),
                                AttributeDefId = Guid.Parse("4b28f0d1-7ded-4828-87c6-23fc88576f9c")
//                                Attribute = new CurrencyAttribute(new AttrDef{Id =Guid.Parse("4b28f0d1-7ded-4828-87c6-23fc88576f9c")}) {Value = 555.445m}
                            }
                        ,
                        	
                        new BizEditText
                        {
                            Id = Guid.Parse("9eb77c06-ad3d-4cc7-b3cf-900dcad18005"), 
                            AttributeDefId = Guid.Parse("3e04ebae-f4d8-4fec-a394-e79878326ed2")
//                            Attribute = new TextAttribute(new AttrDef
//                                {
//                                    Id = Guid.Parse("3e04ebae-f4d8-4fec-a394-e79878326ed2")
//                                }){
//                                Value = "Привет!!! Данные атрибута text!"}
                        },
                        	
                        new BizComboBox()
                        {
                            Id = Guid.Parse("6a997b4e-602c-40ab-9b1b-dbe460688f4f"),
                            AttributeDefId = Guid.Parse("b839c58b-3d66-4cd3-953b-c84aaa217a55")
//                            Attribute = new EnumAttribute()
//                            {
//                                AttrDef = new AttrDef
//                                {
//                                    Id = Guid.Parse("b839c58b-3d66-4cd3-953b-c84aaa217a55"),
//                                    EnumDefType = new EnumDef
//                                                      {
//                                                          Id = Guid.Parse("151f3ec2-c7ab-4037-a3ce-b25688e9be0b")
//                                                      }
//                                },
//                                Value = Guid.Parse("C7A80C2C-F3D8-436C-BFE2-34A7B3F025E2")
//                            }
                        }
                    };

            return formForSave;
        }

        //[TestMethod]
        public void GetForm()
        {
            using (var dataContext = new DataContext())
            {
                var repo = new FormRepository(dataContext, _userId);

                BizForm formForSave = GetNewFormForSave();

                /*BizForm form = repo.SaveForm(formForSave); // Method removed!!

                BizForm loadedForm = repo.GetForm(form.Id/*, (Guid) form.DocumentId, false#1#);

                Assert.IsNotNull(loadedForm);*/
            }
        }

        [TestMethod]
        public void GetForm2()
        {
            using (var dataContext = new DataContext())
            {
                var repo = new FormRepository(dataContext, _userId);

                var formId = Guid.Parse("b88fe6ca-d327-44c5-bc84-37f47d85fb4a");
                //            var docId = Guid.Parse("97e19dca-7319-4742-a83e-b40d71460946");

                BizForm loadedForm = repo.GetForm(formId /*, docId, false*/);

                Assert.IsNotNull(loadedForm);
            }
        }
        
        //[TestMethod]
        //public void GetForm3()
        //{
        //    var repo = new FormRepository(_userId);

        //    var formId = Guid.Parse("df071987-7ec6-44c3-9b46-80283b27e6a0");
        //    var docId = Guid.NewGuid();

        //    BizForm loadedForm = repo.GetForm(formId, docId, false);

        //    Assert.IsNotNull(loadedForm);
        //}
        
        [TestMethod]
        public void GetTableForm()
        {
            using (var dataContext = new DataContext())
            {
                var repo = new FormRepository(dataContext, _userId);

                BizTableForm loadedForm = repo.GetTableForm(
                    Guid.Parse("{A76180D8-D3C1-48DC-87A5-66C15FBC6045}") /*, null*/);

                Assert.IsNotNull(loadedForm);
            }
        }

        [TestMethod]
        public void SaveForm()
        {
            using (var dataContext = new DataContext())
            {
                var repo = new FormRepository(dataContext, _userId);
                var docRepo = new DocRepository(dataContext, _userId);

                var formForSave = GetNewFormForSave();

                var doc = docRepo.New(formForSave.DocumentDefId ?? Guid.Empty);

                doc = repo.GetFormDoc(formForSave, doc);

                //BizForm form = repo.SaveForm(formForSave);
                docRepo.Save(doc);

                Assert.IsNotNull(doc /*form*/);
            }
        }

        [TestMethod]
        public void GetDoc()
        {
            using (var dataContext = new DataContext())
            {
                var repo = new FormRepository(dataContext, _userId);
                var docRepo = new DocRepository(dataContext, _userId);

                var formForSave = GetNewFormForSave();
                var doc = docRepo.New(Guid.Parse("{2194385F-BEFC-452F-AB7C-19DD5F033225}"));

                doc = repo.GetFormDoc(formForSave, doc);

                Assert.IsNotNull(doc);
                Assert.IsNotNull(doc.Attributes);
                Assert.AreEqual(5, doc.Attributes.Count);
            }
        }


        [TestMethod]
        public void GetFormNew()
        {
            using (var dataContext = new DataContext())
            {
                var repo = new FormRepository(dataContext, _userId);

                BizForm loadedForm = repo.GetForm(
                    Guid.Parse("B88FE6CA-D327-44C5-BC84-37F47D85FB4A") /*,
                Guid.Empty, false*/);

                Assert.IsNotNull(loadedForm);
            }
        }

        /* [TestMethod]
        public void GetControlById() // Method removed!
        {
            using (var dataContext = new DataContext())
            {
                var formRepo = new FormRepository(dataContext);
                var control = formRepo.GetControlById( 
                    Guid.Parse("B80D2A55-D83F-4E3E-A3E5-5EBB665EBCC5" /*"860a1cc8-66b5-44a6-8a01-fa622553b682"#1#)
                    /*,
                Guid.Empty, false#1#);

                Assert.IsNotNull(control);
            }
        }*/


        [TestMethod]
         public void GetFormWithLoadedDoc()
         {
             using (var dataContext = new DataContext())
             {
                 var formRepo = new FormRepository(dataContext, _userId);
                 var docRepo = new DocRepository(dataContext, _userId);

                 var formId = Guid.Parse("b88fe6ca-d327-44c5-bc84-37f47d85fb4a");
                 var docId = Guid.Parse("97e19dca-7319-4742-a83e-b40d71460946");

                 Doc docForShow = docRepo.LoadById(docId);

                 BizForm loadedForm = formRepo.GetForm(formId/*, docForShow*/);
                 loadedForm = (BizForm) formRepo.SetFormDoc(loadedForm, docForShow);

                 Assert.IsNotNull(loadedForm);
                 Assert.IsNotNull(loadedForm.Children);
                 Assert.AreNotSame(0, loadedForm.Children.Count);
             }
         }


         [TestMethod]
         public void GetFormWithNewDoc()
         {
             using (var dataContext = new DataContext())
             {
                 var formRepo = new FormRepository(dataContext, _userId);
                 var docRepo = new DocRepository(dataContext, _userId);

                 var formId = Guid.Parse("b88fe6ca-d327-44c5-bc84-37f47d85fb4a");
                 var docDefId = Guid.Parse("2194385f-befc-452f-ab7c-19dd5f033225");

                 Doc docForShow = docRepo.New(docDefId);

                 BizForm loadedForm = formRepo.GetForm(formId /*, docForShow*/);
                 loadedForm = (BizForm) formRepo.SetFormDoc(loadedForm, docForShow);

                 Assert.IsNotNull(loadedForm);
                 Assert.IsNotNull(loadedForm);
                 Assert.IsNotNull(loadedForm.Children);
                 Assert.AreNotSame(0, loadedForm.Children.Count);
             }
         }
    }
}
