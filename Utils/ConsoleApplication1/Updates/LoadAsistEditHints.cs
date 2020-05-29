using System;
using System.IO;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Utils;

namespace ConsoleApplication1.Updates
{
    public class LoadAsistEditHints
    {
        public static void LoadAsistFormHints(IDataContext dataContext)
        {
            var en = dataContext.GetEntityDataContext();

            using (var file = new FileStream(@"C:\Users\Администратор\Desktop\Asist.Hints.csv", FileMode.Open))
            {
                using (var reader = new CsvReader(file))
                {
                    reader.Delimiters = new[] {((char) 9).ToString()};
                    var i = 0;
                    var count = 0;
                    while (reader.Read())
                    {
                        var s = reader.Fields[0];
                        Guid id;
                        if (Guid.TryParse(s, out id))
                        {
                            s = reader.Fields[1];
                            if (!String.IsNullOrEmpty(s))
                            {
                                var hint = new Text{Id = Guid.NewGuid(), Parent_Id = id, Created = DateTime.Now, Full_Name = s};
                                en.Entities.AddToObject_Defs(hint);
                                count++;
                                i++;
                                Console.Write(".");
                                if (count > 10)
                                {
                                    en.SaveChanges();
                                    count = 0;
                                    Console.WriteLine(i);
                                }
                            }
                        }
                    }
                    if (count > 0)
                        en.SaveChanges();
                }
            }
        }
    }
}
