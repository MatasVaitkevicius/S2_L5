using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;

namespace L4_WebPage
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        public const string ResidentsDirectory = "/App_Data/Residents";
        public const string TerritoryCleaningData = "/App_Data/TerritoryCleaningData.txt";

        protected void Page_Load(object sender, EventArgs e)
        {
            Label5.Visible = false;
            Label6.Visible = false;
            Button2.Visible = false;
            Button3.Visible = false;
            Label4.Visible = false;
            TextBox1.Visible = false;
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                var residentsList = ReadResidentsData(Server.MapPath(ResidentsDirectory));
                var territoryCleaningList = ReadTerritoryCleaningData(Server.MapPath(TerritoryCleaningData));

                Session["StartingResidents"] = residentsList;
                Session["StartingTerritoryCleaning"] = territoryCleaningList;
                ResidentsTable(residentsList);
                TerritoryCleaningtable(territoryCleaningList);

                var chosenAmountOfMoney = 0;
                var filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                filePath = filePath + @"\Results.txt";
                WriteResidents(filePath, residentsList, 0, chosenAmountOfMoney, territoryCleaningList);
                WriteTerritoryCleaning(filePath, territoryCleaningList);
                Button2.Visible = true;
                Label4.Visible = true;
                TextBox1.Visible = true;
            }
            catch (Exception exception)
            {
                var message = $"Message: {exception.Message}\\n\\n";
                message += $"StackTrace: {exception.StackTrace.Replace(Environment.NewLine, string.Empty)}\\n\\n";
                message += $"Source: {exception.Source.Replace(Environment.NewLine, string.Empty)}\\n\\n";
                message += $"TargetSite: {exception.TargetSite.ToString().Replace(Environment.NewLine, string.Empty)}";
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert(\"" + message + "\");", true);
            }
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            var residentsList = (List<ResidentsList>)Session["StartingResidents"];
            var territoryCleaningList = (List<TerritoryCleaning>)Session["StartingTerritoryCleaning"];
            ResidentsTable(residentsList);
            TerritoryCleaningtable(territoryCleaningList);
            Button2.Visible = true;
            Label4.Visible = true;
            TextBox1.Visible = true;

            try
            {
                var chosenAmountOfMoney = double.Parse(TextBox1.Text);

                var filteredResidents =
                    FilterResidentsByMoney(chosenAmountOfMoney, territoryCleaningList, residentsList);

                Session["FilteredResidents"] = residentsList;
                Session["ChosenMoney"] = chosenAmountOfMoney;

                Label5.Visible = true;

                var filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                filePath = filePath + @"\Results.txt";

                ResidentsTableResult(filteredResidents, chosenAmountOfMoney, 0, territoryCleaningList);

                WriteResidents(filePath, filteredResidents, 1, chosenAmountOfMoney, territoryCleaningList);

                Button3.Visible = true;
            }
            catch (Exception exception)
            {
                var message = $"Message: {exception.Message}\\n\\n";
                message += $"StackTrace: {exception.StackTrace.Replace(Environment.NewLine, string.Empty)}\\n\\n";
                message += $"Source: {exception.Source.Replace(Environment.NewLine, string.Empty)}\\n\\n";
                message += $"TargetSite: {exception.TargetSite.ToString().Replace(Environment.NewLine, string.Empty)}";
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert(\"" + message + "\");", true);
            }

        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            var residentsList = (List<ResidentsList>)Session["StartingResidents"];
            var territoryCleaningList = (List<TerritoryCleaning>)Session["StartingTerritoryCleaning"];
            var filteredResidents = (List<ResidentsList>)Session["FilteredResidents"];
            var chosenAmountOfMoney = (double)Session["ChosenMoney"];

            ResidentsTable(residentsList);
            TerritoryCleaningtable(territoryCleaningList);
            ResidentsTableResult(filteredResidents, chosenAmountOfMoney, 0, territoryCleaningList);
            VisibleObjects();

            try
            {
                var filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                filePath = filePath + @"\Results.txt";

                Label6.Visible = true;

                RemoveResidents(territoryCleaningList, filteredResidents);

                ResidentsTableResult(filteredResidents, chosenAmountOfMoney, 1, territoryCleaningList);

                WriteResidents(filePath, filteredResidents, 2, chosenAmountOfMoney, territoryCleaningList);
            }
            catch (Exception exception)
            {
                var message = $"Message: {exception.Message}\\n\\n";
                message += $"StackTrace: {exception.StackTrace.Replace(Environment.NewLine, string.Empty)}\\n\\n";
                message += $"Source: {exception.Source.Replace(Environment.NewLine, string.Empty)}\\n\\n";
                message += $"TargetSite: {exception.TargetSite.ToString().Replace(Environment.NewLine, string.Empty)}";
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert(\"" + message + "\");", true);
            }

        }

        public bool CheckForResidents(List<Residents> residentsLists)
        {
            return residentsLists.Any();
        }

        public List<Residents> SortResidents(List<Residents> residentsLists)
        {
            return residentsLists.OrderBy(r => r.FlatOwner).ToList();
        }
    
        public List<TerritoryCleaning> SortTerritoryCleaning(List<TerritoryCleaning> territoryCleaning)
        {
           return territoryCleaning.OrderBy(t => t.PriceForSquare).ToList();
        }

        public List<Residents> GetDistinctResidents(List<Residents> residentsLists)
        {
            return residentsLists.Distinct().ToList();
        }

        public void VisibleObjects()
        {
            Label5.Visible = true;
            Label6.Visible = true;
            Button2.Visible = true;
            Button3.Visible = true;
            Label4.Visible = true;
            TextBox1.Visible = true;
        }

        public List<ResidentsList> ReadResidentsData(string directoryName)
        {
            var residentsList = new List<ResidentsList>();

            foreach (var thisFile in Directory.GetFiles(directoryName))
            {
                var temporaryList = new List<Residents>();
                string[] lines;
                try
                {
                    lines = File.ReadAllLines(thisFile);
                }
                catch (Exception e)
                {
                    continue;
                }
                var data = lines[0];

                for (var i = 1; i < lines.Length; i++)
                {
                    try
                    {
                        var values = lines[i].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        temporaryList.Add(new Residents
                        {
                            FlatOwner = values[0],
                            AmountOfAdults = int.Parse(values[1]),
                            AmountOfKids = int.Parse(values[2]),
                            FlatArea = double.Parse(values[3])
                        });
                    }
                    catch (Exception e)
                    {
                        var message = $"Message: {e.Message}\\n\\n";
                        message += $"StackTrace: {e.StackTrace.Replace(Environment.NewLine, string.Empty)}\\n\\n";
                        message += $"Source: {e.Source.Replace(Environment.NewLine, string.Empty)}\\n\\n";
                        message += $"TargetSite: {e.TargetSite.ToString().Replace(Environment.NewLine, string.Empty)}";
                        ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert(\"" + message + "\");",
                            true);
                    }
                }

                var smallerList = new ResidentsList(data, temporaryList);
                residentsList.Add(smallerList);
            }

            return residentsList;
        }

        public List<TerritoryCleaning> ReadTerritoryCleaningData(string fileName)
        {
            var territoryCleaningList = new List<TerritoryCleaning>();
            var lines = File.ReadAllLines(fileName);

            foreach (var territoryCleaning in lines)
            {
                try
                {
                    var values = territoryCleaning.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    territoryCleaningList.Add(new TerritoryCleaning
                    {
                        CleaningAmountOfAdults = int.Parse(values[0]),
                        CleaningAmountOfKids = int.Parse(values[1]),
                        PriceForSquare = double.Parse(values[2])
                    });
                }
                catch (Exception e)
                {
                    var message = $"Message: {e.Message}\\n\\n";
                    message += $"StackTrace: {e.StackTrace.Replace(Environment.NewLine, string.Empty)}\\n\\n";
                    message += $"Source: {e.Source.Replace(Environment.NewLine, string.Empty)}\\n\\n";
                    message += $"TargetSite: {e.TargetSite.ToString().Replace(Environment.NewLine, string.Empty)}";
                    ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert(\"" + message + "\");", true);
                }
            }

            return territoryCleaningList;
        }

        static bool CheckForAdultsAndKids(Residents resident, TerritoryCleaning territoryCleaning)
        {
            return resident.AmountOfAdults == territoryCleaning.CleaningAmountOfAdults &&
                   resident.AmountOfKids == territoryCleaning.CleaningAmountOfKids;
        }

        double CalculateAverage(List<TerritoryCleaning> territoryCleaningList,
            List<ResidentsList> residentsLists)
        {
            var sum = 0.0;
            var count = 0;
            try
            {
                if (residentsLists.Any())
                {
                    residentsLists.ForEach(lr => lr.ListOfResidents.ForEach(r => territoryCleaningList.ForEach(t =>
                    {
                        sum += CalculateMoneySpent(r, t);
                        count += r.AmountOfAdults + r.AmountOfKids;
                    })));
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception e)
            {
                var message = $"Message: {e.Message}\\n\\n";
                message += $"StackTrace: {e.StackTrace.Replace(Environment.NewLine, string.Empty)}\\n\\n";
                message += $"Source: {e.Source.Replace(Environment.NewLine, string.Empty)}\\n\\n";
                message += $"TargetSite: {e.TargetSite.ToString().Replace(Environment.NewLine, string.Empty)}";
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert(\"" + message + "\");", true);
            }

            return Math.Round(sum / count);
        }

        void RemoveResidents(List<TerritoryCleaning> territoryCleaningList,
            List<ResidentsList> residentsLists)
        {
            try
            {
                var average = CalculateAverage(territoryCleaningList, residentsLists);

                residentsLists.ForEach(lr => lr.ListOfResidents.ToList().ForEach(r =>
                {
                    territoryCleaningList.ForEach(t =>
                    {
                        if (CalculateMoneySpent(r, t) < average)
                        {
                            lr.ListOfResidents.Remove(r);
                        }
                    });
                }));
            }

            catch (Exception e)
            {
                var message = $"Message: {e.Message}\\n\\n";
                message += $"StackTrace: {e.StackTrace.Replace(Environment.NewLine, string.Empty)}\\n\\n";
                message += $"Source: {e.Source.Replace(Environment.NewLine, string.Empty)}\\n\\n";
                message += $"TargetSite: {e.TargetSite.ToString().Replace(Environment.NewLine, string.Empty)}";
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert(\"" + message + "\");", true);
            }
        }

        double CalculateMoneySpent(Residents resident, TerritoryCleaning territoryCleaning)
        {
            return resident.FlatArea * territoryCleaning.PriceForSquare;
        }

        List<ResidentsList> FilterResidentsByMoney(double chosenMoneyFloor,
            List<TerritoryCleaning> territoryCleaningList, List<ResidentsList> residentsLists)
        {
            var filteredList = new List<ResidentsList>();

            try
            {
                return (from street in residentsLists
                        let temporaryList = street.ListOfResidents
                            .Where(resident => territoryCleaningList
                                .Where(territoryCleaning => CheckForAdultsAndKids(resident, territoryCleaning))
                                .Any(territoryCleaning =>
                                    chosenMoneyFloor < CalculateMoneySpent(resident, territoryCleaning)))
                            .ToList()
                        select new ResidentsList(street.ListStreetName, temporaryList)).ToList();
            }
            catch (Exception e)
            {
                var message = $"Message: {e.Message}\\n\\n";
                message += $"StackTrace: {e.StackTrace.Replace(Environment.NewLine, string.Empty)}\\n\\n";
                message += $"Source: {e.Source.Replace(Environment.NewLine, string.Empty)}\\n\\n";
                message += $"TargetSite: {e.TargetSite.ToString().Replace(Environment.NewLine, string.Empty)}";
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert(\"" + message + "\");", true);
            }

            return filteredList;
        }

        public void WriteResidents(string file, List<ResidentsList> residentsList, int index,
            double chosenAmountOfMoney, List<TerritoryCleaning> territoryCleaningList)
        {
            var linesSeparator = new string('-', 92);
            var linesSeparatorResults = new string('-', 47);

            if (!File.Exists(file))
            {
                File.Create(file);
            }

            using (var writer = new StreamWriter(file, true))
            {
                switch (index)
                {
                    case 0:
                        writer.WriteLine("Pradiniai Duomenys");
                        writer.WriteLine();
                        writer.WriteLine(@"Gyventojai");
                        writer.WriteLine();

                        residentsList.ForEach(lr =>
                        {
                            writer.WriteLine("{0, 15}", "Gatves Pavadinimas");
                            writer.WriteLine(lr.ListStreetName);
                            writer.WriteLine();
                            writer.WriteLine(linesSeparator);
                            writer.WriteLine("| {0, 20} | {1,20} | {2,20} | {3, 20} |", "Savininko pavarde",
                                "Suagusiu skaicius", "Vaiku skaicius", "Buto plotas");
                            writer.WriteLine(linesSeparator);

                            lr.ListOfResidents.ForEach(r =>
                            {
                                writer.WriteLine(r.ToString());
                                writer.WriteLine(linesSeparator);
                            });
                        });
                        break;
                    case 1:
                        writer.WriteLine("Sudarytas naujas sąrašas");
                        writer.WriteLine();
                        if (residentsList.Any())
                        {
                            residentsList.ForEach(lr =>
                            {
                                writer.WriteLine("{0, 15}", "Gatves Pavadinimas");
                                writer.WriteLine(lr.ListStreetName);
                                writer.WriteLine();
                                if (lr.ListOfResidents.Any())
                                {
                                    writer.WriteLine(linesSeparatorResults);
                                    writer.WriteLine("| {0, 20} | {1,20} |", "Savininko Pavarde",
                                        "Gyvenanciu skaicius");
                                    writer.WriteLine(linesSeparatorResults);
                                    lr.ListOfResidents.ForEach(r =>
                                    {
                                        writer.WriteLine(r.Results());
                                        writer.WriteLine(linesSeparatorResults);
                                    });
                                }
                                else
                                {
                                    writer.WriteLine($"{lr.ListStreetName} - yra tuščia");
                                }
                            });
                        }
                        else
                        {
                            writer.WriteLine("Sąrašas yra tuščias");
                        }

                        writer.WriteLine($"Pasirinkta pinigų suma: {chosenAmountOfMoney}");
                        writer.WriteLine();
                        break;
                    case 2:
                        writer.WriteLine("Pašalintas naujas sąrašas");
                        writer.WriteLine();
                        if (residentsList.Any())
                        {
                            residentsList.ForEach(lr =>
                            {
                                writer.WriteLine("{0, 15}", "Gatves Pavadinimas");
                                writer.WriteLine(lr.ListStreetName);
                                writer.WriteLine();
                                if (lr.ListOfResidents.Any())
                                {
                                    writer.WriteLine(linesSeparatorResults);
                                    writer.WriteLine("| {0, 20} | {1,20} |", "Savininko Pavarde",
                                        "Gyvenanciu skaicius");
                                    writer.WriteLine(linesSeparatorResults);
                                    lr.ListOfResidents.ForEach(r =>
                                    {
                                        writer.WriteLine(r.Results());
                                        writer.WriteLine(linesSeparatorResults);
                                    });
                                }
                                else
                                {
                                    writer.WriteLine($"{lr.ListStreetName} - yra tuščia");
                                }
                            });

                            writer.WriteLine(
                                $"Vidurkis vienam žmogui: {CalculateAverage(territoryCleaningList, residentsList)}");
                            writer.WriteLine();
                        }
                        else
                        {
                            writer.WriteLine("Sąrašas yra tuščias");
                        }
                        break;
                }
            }
        }

        public void WriteTerritoryCleaning(string file, List<TerritoryCleaning> territoryCleanings)
        {
            var linesSeparator = new string('-', 70);
            if (!File.Exists(file))
            {
                File.Create(file);
            }

            using (var writer = new StreamWriter(file, true))
            {
                writer.WriteLine("Pradiniai Duomenys");
                writer.WriteLine();
                writer.WriteLine("Teritorijos valymo ikainiai");
                writer.WriteLine();
                writer.WriteLine(linesSeparator);
                writer.WriteLine("| {0, 20} | {1,20} | {2,20} |", "Suaugusiuju skaicius", "Vaiku skaicius",
                    "Ikainis 1m2");
                writer.WriteLine(linesSeparator);
                territoryCleanings.ForEach(t =>
                {
                    writer.WriteLine(t.ToString());
                    writer.WriteLine(linesSeparator);
                });
                writer.WriteLine();
            }
        }

        void ResidentsTable(List<ResidentsList> residentsLists)
        {
            residentsLists.ForEach(lr =>
            {
                var rowTop = new TableRow();
                var rowStreet = new TableRow();
                var cellStreet = new TableCell { Text = "Gatve" };
                FormatTableCellsStreet(cellStreet);

                rowStreet.Cells.Add(cellStreet);

                var cellStreetData = new TableCell { Text = lr.ListStreetName };
                FormatTableCellsStreet(cellStreetData);

                rowStreet.Cells.Add(cellStreetData);
                Table1.Rows.Add(rowStreet);


                for (var i = 0; i < 4; i++)
                {
                    var cell = new TableCell();
                    FormatTableCellsStreet(cell);

                    if (i == 0) cell.Text = "Savininko pavarde";

                    if (i == 1) cell.Text = "Suagusiu skaicius";

                    if (i == 2) cell.Text = "Vaiku skaicius";

                    if (i == 3) cell.Text = "Buto plotas";

                    rowTop.Cells.Add(cell);
                }

                Table1.Rows.Add(rowTop);

                lr.ListOfResidents.ForEach(r =>
                {
                    var row = new TableRow();

                    for (var i = 0; i < 4; i++)
                    {
                        var cell = new TableCell();
                        FormatTableCells(cell);

                        if (i == 0) cell.Text = r.FlatOwner;

                        if (i == 1) cell.Text = r.AmountOfAdults.ToString();

                        if (i == 2) cell.Text = r.AmountOfKids.ToString();

                        if (i == 3) cell.Text = r.FlatArea.ToString(CultureInfo.CurrentCulture);

                        row.Cells.Add(cell);
                    }
                    Table1.Rows.Add(row);
                });
            });
        }

        static void FormatTableCells(TableCell cell)
        {
            cell.BorderStyle = BorderStyle.Solid;
            cell.BorderWidth = 1;
            cell.Font.Size = 15;
            cell.Height = 30;
            cell.Width = 30;
        }

        static void FormatTableCellsStreet(TableCell cell)
        {
            cell.BorderStyle = BorderStyle.Solid;
            cell.BorderWidth = 2;
            cell.Font.Size = 20;
            cell.Height = 30;
            cell.Width = 30;
        }

        void ResidentsTableResult(List<ResidentsList> residentsLists, double chosenAmountOfMoney, int index,
            List<TerritoryCleaning> territoryCleaningList)
        {
            residentsLists.ForEach(lr =>
            {
                if (index == 0)
                {
                    var rowTop = new TableRow();
                    var rowStreet = new TableRow();

                    var cellStreet = new TableCell { Text = "Gatve" };
                    FormatTableCellsStreet(cellStreet);

                    rowStreet.Cells.Add(cellStreet);
                    Table3.Rows.Add(rowStreet);

                    var cellStreetData = new TableCell { Text = lr.ListStreetName };
                    FormatTableCellsStreet(cellStreetData);

                    rowStreet.Cells.Add(cellStreetData);
                    Table3.Rows.Add(rowStreet);

                    for (var i = 0; i < 2; i++)
                    {
                        var cell = new TableCell();
                        FormatTableCellsStreet(cell);

                        if (i == 0) cell.Text = "Savininko pavarde";

                        if (i == 1) cell.Text = "Zmoniu skaicius";

                        rowTop.Cells.Add(cell);
                    }

                    Table3.Rows.Add(rowTop);

                    lr.ListOfResidents.ForEach(r =>
                    {
                        var row = new TableRow();

                        for (var i = 0; i < 2; i++)
                        {
                            var cell = new TableCell();
                            FormatTableCells(cell);

                            if (i == 0) cell.Text = r.FlatOwner;

                            if (i == 1) cell.Text = (r.AmountOfAdults + r.AmountOfKids).ToString();

                            row.Cells.Add(cell);
                        }

                        Table3.Rows.Add(row);
                    });
                }

                if (index == 1)
                {
                    var rowTop = new TableRow();
                    var rowStreet = new TableRow();
                    var cellStreet = new TableCell { Text = "Gatve" };
                    FormatTableCellsStreet(cellStreet);

                    rowStreet.Cells.Add(cellStreet);

                    var cellStreetData = new TableCell { Text = lr.ListStreetName };
                    FormatTableCellsStreet(cellStreetData);

                    rowStreet.Cells.Add(cellStreetData);
                    Table4.Rows.Add(rowStreet);


                    for (var i = 0; i < 2; i++)
                    {
                        var cell = new TableCell();
                        FormatTableCellsStreet(cell);

                        if (i == 0) cell.Text = "Savininko pavarde";

                        if (i == 1) cell.Text = "Zmoniu skaicius";

                        rowTop.Cells.Add(cell);
                    }

                    Table4.Rows.Add(rowTop);

                    lr.ListOfResidents.ForEach(r =>
                    {

                        var row = new TableRow();

                        for (var i = 0; i < 2; i++)
                        {
                            var cell = new TableCell();
                            FormatTableCells(cell);

                            if (i == 0) cell.Text = r.FlatOwner;

                            if (i == 1) cell.Text = (r.AmountOfAdults + r.AmountOfKids).ToString();

                            row.Cells.Add(cell);
                        }

                        Table4.Rows.Add(row);
                    });
                }
            });

            if (index == 0)
            {

                var rowBottom = new TableRow();
                var cellMoney = new TableCell { Text = "Įvesta pinigų suma" };
                var cellEnteredMoney =
                new TableCell { Text = chosenAmountOfMoney.ToString(CultureInfo.CurrentCulture) };
                FormatTableCellsStreet(cellMoney);
                FormatTableCellsStreet(cellEnteredMoney);
                rowBottom.Cells.Add(cellMoney);
                rowBottom.Cells.Add(cellEnteredMoney);
                Table3.Rows.Add(rowBottom);
            }

            if (index == 1)
            {
                var rowBottom = new TableRow();
                var cellMoney = new TableCell { Text = "Pinigų vidurkis vienam zmogui" };
                if ((CalculateAverage(territoryCleaningList, residentsLists)) == 0)
                {
                    var cellAverage = new TableCell { Text = "Vidurkis neegzistuoja" };
                    FormatTableCellsStreet(cellMoney);
                    FormatTableCellsStreet(cellAverage);
                    rowBottom.Cells.Add(cellMoney);
                    rowBottom.Cells.Add(cellAverage);
                    Table4.Rows.Add(rowBottom);
                }
                else
                {
                    var cellAverage = new TableCell
                    {
                        Text = CalculateAverage(territoryCleaningList, residentsLists).ToString()
                    };
                    FormatTableCellsStreet(cellMoney);
                    FormatTableCellsStreet(cellAverage);
                    rowBottom.Cells.Add(cellMoney);
                    rowBottom.Cells.Add(cellAverage);
                    Table4.Rows.Add(rowBottom);
                }
            }
        }

        void TerritoryCleaningtable(List<TerritoryCleaning> territoryCleaningList)
        {
            var rowTop = new TableRow();

            for (var i = 0; i < 3; i++)
            {
                var cell = new TableCell();
                FormatTableCellsStreet(cell);

                if (i == 0) cell.Text = "Suaugusiu skaicius";

                if (i == 1) cell.Text = "Vaiku skaicius";

                if (i == 2) cell.Text = "Ikainis 1m2";

                rowTop.Cells.Add(cell);
            }

            Table2.Rows.Add(rowTop);

            territoryCleaningList.ForEach(t =>
            {
                var row = new TableRow();

                for (var i = 0; i < 3; i++)
                {
                    var cell = new TableCell();
                    FormatTableCells(cell);

                    if (i == 0) cell.Text = t.CleaningAmountOfAdults.ToString();

                    if (i == 1) cell.Text = t.CleaningAmountOfKids.ToString();

                    if (i == 2) cell.Text = t.PriceForSquare.ToString(CultureInfo.CurrentCulture);

                    row.Cells.Add(cell);
                }

                Table2.Rows.Add(row);
            });
        }
    }
}