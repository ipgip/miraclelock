using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace miraclelock
{
    static class CountriesPrepare
    {
        public static void FillDbCountries()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
            //create a new Generic list to hold the country names returned
            List<string> cultureList = new List<string>();

            //create an array of CultureInfo to hold all the cultures found, these include the users local cluture, and all the
            //cultures installed with the .Net Framework
            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures & ~CultureTypes.NeutralCultures);

            //loop through all the cultures found
            foreach (CultureInfo culture in cultures)
            {
                try
                {
                    //pass the current culture's Locale ID (http://msdn.microsoft.com/en-us/library/0h88fahh.aspx)
                    //to the RegionInfo contructor to gain access to the information for that culture
                    RegionInfo region = new RegionInfo(culture.LCID);

                    //make sure out generic list doesnt already
                    //contain this country
                    if (!(cultureList.Contains(region.DisplayName)))
                    {
                        //not there so add the EnglishName (http://msdn.microsoft.com/en-us/library/system.globalization.regioninfo.englishname.aspx)
                        //value to our generic list
                        cultureList.Add(region.EnglishName);
                    }
                }
                catch (ArgumentException ex)
                {
                    // just ignor this
                    continue;
                }
            }
            cultureList.Sort();

            using (LockDbDataContext db = new LockDbDataContext(Convert.ToString(Program.Config["ConnectionString"])))
            {
                foreach (var country in cultureList.Distinct())
                {
                    db.Countries.InsertOnSubmit(new Countries
                    {
                        Title = country
                    });
                }
                db.SubmitChanges();
            }
        }
    }
}