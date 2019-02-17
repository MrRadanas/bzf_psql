using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net.NetworkInformation;

namespace graph_to_psql
{
    class graph
    {
        [DllImport("ODBCCP32.dll")]
        private static extern bool SQLConfigDataSource(IntPtr parent, int request, string
        driver, string attributes);

        static void Main(string[] args)
        {
            string pathconf = "";
            string modes = "";

            if (args.Length != 0)
            {
                if (args.Length != 2) { Console.WriteLine("Два аргумента(режим,полный путь к config файлу!)"); Console.ReadKey(); return; }
                modes = args[0];
                pathconf = args[1] + pathconf;

                if (modes == "auto_rtp4")
                {
                    rtp4graph_exe(pathconf, true, 0, 0, 0);
                    return;
                }
                else if (modes == "auto_gou")
                {
                    voda_graph_exe(1, pathconf, -1);
                    voda_graph_exe(2, pathconf, -1);
                    doz_exe(pathconf);
                    dozdata_exe(pathconf);
                    kts_exe(pathconf);
                    Gou_exe(pathconf, 0, 125, -1);
                    TehDBF(pathconf); return;
                }
                else if (modes == "auto_rtp1_sm1")
                {
                    rtp1_exe(pathconf, "1sm", 0, 0, 0);
                    return;
                }
                else if (modes == "auto_rtp1_sm2")
                {
                    rtp1_exe(pathconf, "2sm", 0, 0, 0);
                    return;
                }
                else if (modes == "auto_rtp1_sm3")
                {
                    rtp1_exe(pathconf, "3sm_auto", 0, 0, 0);
                    return;
                }
                else if (modes == "auto_rtp1_1h")
                {
                    rtp1_exe(pathconf, "1h", 0, 0, 0);
                    return;
                }
                else if (modes == "auto_rtp2")
                {
                    rtp2_graph_msql_exe(pathconf, "a_hour", "", "");
                    return;
                }
                else if (modes == "auto_rtp2_date")
                {
                    rtp2_graph_msql_exe(pathconf, "a_sm2", "", "");
                    return;
                }
                else if (modes == "auto2")
                {
                    gou3_exe(pathconf);
                    return;
                }
                else
                {
                    Console.WriteLine("Неверно составлен config файл!");
                    Console.ReadKey();
                    return;
                }

            }
            else
            {
                Console.WriteLine("Используется Config.txt в каталоге программы;\nРучной режим:\n");
                try
                {
                    Menuwrite();
                    Menu(Directory.GetCurrentDirectory() + "\\config.txt");
                    return;
                }
                catch { Console.WriteLine("Неверно составлен config файл"); return; }
            }
        }

        static void Menuwrite()
        {
            Console.WriteLine("Программа конвертации исходных файлов и импорта данных для графиков в БД Технолог.");
            Console.WriteLine("");
            Console.WriteLine("МЕНЮ:");
            Console.WriteLine("[1] - настроить DSN");
            Console.WriteLine("[2] - запустить программу конвертации для РТП-4");
            Console.WriteLine("[3] - запустить программу конвертации для УОВ-1");
            Console.WriteLine("[4] - запустить программу конвертации для УОВ-2");
            Console.WriteLine("[5] - запустить программу конвертации для РТП-2 MySQL");
            Console.WriteLine("[6] - запустить программу конвертации для РТП-1");
            Console.WriteLine("[7] - запустить программу конвертации для Дозировки");
            Console.WriteLine("[8] - запустить программу конвертации для ГОУ1");
            Console.WriteLine("[9] - запустить программу конвертации для ГОУ2");
            Console.WriteLine("[10] - запустить программу конвертации для ГОУ3");
            Console.WriteLine("[11] - запустить программу конвертации для ГОУ4");
            Console.WriteLine("[12] - запустить программу конвертации для Котельной");
            Console.WriteLine("[13] - запустить программу конвертации для ГОУ3sybase");
            Console.WriteLine("[14] - запустить программу импорта данных с ДО");
            Console.WriteLine("[15] - запустить программу импорта данных с КТС");
            Console.WriteLine("[16] - запустить программу импорта данных с РТП2 Access");
            Console.WriteLine("[menu] - Меню");
            Console.WriteLine("[exit] - выход");
            Console.WriteLine("\nСправка автоматического режима запуск из командной строки с аргументами:\n\nПервый аргумент:\nauto_rtp4-автозагрузка данных РТП4;\nauto_gou-автозагрузка данных УОВ,ДО,ГОУ,КТС,Кател.;\nauto_rtp1-автозагрузка данных РТП1,\nauto_rtp2-автозагрузка данных РТП2;\nauto2-автозагрузка данных ГОУ3\nВторой аргумент:Полный путь config файла в каталоге приложения\n");
        }

        static void Menu(string pathconf)
        {
            starts:
            try
            {
                int yy = 0;
                int mm = 0;
                int dd = 0;
                string mode = "1d";
                int next = 100;
                int chor = 0; int starth = 0;
                int dated = 0;
                string date_b = "";
                string date_p = "";
                Console.WriteLine("Введите команду:");
                string lkey = Console.ReadLine();
                Console.Beep();
                if (lkey == "1") { next = 1; }
                else
                if (lkey == "2")
                {
                    next = 2;
                    Console.WriteLine("Введите год полностью:");
                    yy = Convert.ToInt16(Console.ReadLine());
                    Console.WriteLine("Введите месяц:");
                    mm = Convert.ToInt16(Console.ReadLine());
                    Console.WriteLine("Введите день:");
                    dd = Convert.ToInt16(Console.ReadLine());
                }
                else
                if (lkey == "6")
                {
                    next = 6;
                    Console.WriteLine("Введите день:1d; смена:1sm,2sm,3sm; час:1h; часы:hc ");
                    mode = Convert.ToString(Console.ReadLine());
                    Console.WriteLine("Введите дату текущая 0, дней назад: -x (например -2) :");
                    dated = Convert.ToInt16(Console.ReadLine());
                    if (mode == "hc")
                    {
                        Console.WriteLine("Введите количество часов за одни сутки:");
                        chor = Convert.ToInt16(Console.ReadLine());
                        Console.WriteLine("Введите стартовый час:");
                        starth = Convert.ToInt16(Console.ReadLine());
                    }
                    else
                        if (mode != "1sm" && mode != "2sm" && mode != "3sm" && mode != "1d" && mode != "1h")
                    {
                        Console.WriteLine("Неверная команда:" + mode + "\r\nПродолжение Enter!");
                        Console.ReadLine();
                        Menuwrite();
                        goto starts;
                    }
                }
                else
                    if (lkey == "3")
                {
                    next = 3; Console.WriteLine("Введите дату текущая 0, дней назад: -x (например -2) :");
                    dated = Convert.ToInt16(Console.ReadLine());
                }
                else
                        if (lkey == "4")
                {
                    next = 4; Console.WriteLine("Введите дату текущая 0, дней назад: -x (например -2) :");
                    dated = Convert.ToInt16(Console.ReadLine());
                }
                else
                        if (lkey == "5")
                {
                    next = 5;

                    Console.WriteLine("Введите начальную дату в формате d.m.Y T");
                    date_b = Console.ReadLine();
                    Console.WriteLine("Введите конечную дату в формате d.m.Y T");
                    date_p = Console.ReadLine();
                }
                else
                                if (lkey == "7") { next = 7; }
                else
                                    if (lkey == "8")
                {
                    Console.WriteLine("Введите дату текущая 0, дней назад: -x (например -2) :");
                    dated = Convert.ToInt16(Console.ReadLine());
                    next = 8;
                }
                else
                                        if (lkey == "9")
                {
                    Console.WriteLine("Введите дату текущая 0, дней назад: -x (например -2) :");
                    dated = Convert.ToInt16(Console.ReadLine()); next = 9;
                }
                else
                                            if (lkey == "10")
                {
                    Console.WriteLine("Введите дату текущая 0, дней назад: -x (например -2) :");
                    dated = Convert.ToInt16(Console.ReadLine()); next = 10;
                }
                else
                                                if (lkey == "11")
                {
                    Console.WriteLine("Введите дату текущая 0, дней назад: -x (например -2) :");
                    dated = Convert.ToInt16(Console.ReadLine()); next = 11;
                }
                else
                                                    if (lkey == "12") { next = 12; }
                else
                                                        if (lkey == "13") { next = 13; }
                else
                                                            if (lkey == "14") { next = 14; }
                else
                                                                if (lkey == "15") { next = 16; }
                else
                                                                    if (lkey == "16") { next = 17; }
                else
                                                                    if (lkey == "menu") { next = 15; }
                else
                                                                        if (lkey == "exit") { return; }
                else
                {
                    Console.WriteLine("Неверная команда:");
                    Menuwrite();
                    goto starts;
                }
                switch (next)
                {
                    case 1:
                        addDSN(pathconf);
                        break;
                    case 2:

                        rtp4graph_exe(pathconf, false, yy, mm, dd);
                        break;
                    case 3:
                        voda_graph_exe(1, pathconf, dated);
                        break;
                    case 4:
                        voda_graph_exe(2, pathconf, dated);
                        break;
                    case 5:
                        rtp2_graph_msql_exe(pathconf, "man", date_b, date_p);
                        //  rtp2_graph_exe(pathconf);
                        break;
                    case 6:
                        rtp1_exe(pathconf, mode, chor, starth, dated);
                        break;
                    case 7:
                        doz_exe(pathconf);
                        break;
                    case 8:
                        Gou_exe(pathconf, 0, 36, dated);
                        break;
                    case 9:
                        Gou_exe(pathconf, 36, 72, dated);
                        break;
                    case 10:
                        Gou_exe(pathconf, 109, 125, dated);
                        break;
                    case 11:
                        Gou_exe(pathconf, 72, 109, dated);
                        break;
                    case 12:
                        TehDBF(pathconf);
                        break;
                    case 13:
                        gou3_exe(pathconf);
                        break;
                    case 14:
                        dozdata_exe(pathconf);
                        break;
                    case 15:
                        Menuwrite();
                        break;
                    case 16:
                        kts_exe(pathconf);
                        break;
                    case 17:
                        rtp2_graph_exe(pathconf);
                        break;
                    default:
                        break;
                }
                goto starts;
            }
            catch
            {
                Console.WriteLine("Ошибка выполнения проверить config файл!\n");
                Console.ReadKey();
                Menuwrite();
                goto starts;
            }
        }

        static void ExecuteTransaction(int queryz, string pathconf)
        {
            string pg_odbc = "Driver={PostgreSQL Unicode};Server=10.21.1.222;Database=technolog;uid=BorodinAE;Password=bae^Y30;Port=5432;";

            using (OdbcConnection connection = new OdbcConnection(pg_odbc))
            {
                string pg_query = System.IO.File.ReadAllLines(System.IO.File.ReadAllLines(pathconf)[9])[queryz];
                OdbcCommand command = new OdbcCommand();
                OdbcTransaction transaction = null;
                command.Connection = connection;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    command.Connection = connection;
                    command.Transaction = transaction;
                    command.CommandText = pg_query;
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    transaction.Rollback();
                }
            }

        }

        /// <summary>
        /// Вставка данных в PSQL
        /// </summary>
        /// <param name="pathconf">Запрос</param>
        static void ExecuteTransactionIndiv(string pg_query)
        {
            string pg_odbc = "Driver={PostgreSQL Unicode};Server=bzf-nas02;Database=technolog;uid=BorodinAE;Password=bae^Y30;Port=5432;";

            using (OdbcConnection connection = new OdbcConnection(pg_odbc))
            {
                OdbcCommand command = new OdbcCommand();
                OdbcTransaction transaction = null;
                command.Connection = connection;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    command.Connection = connection;
                    command.Transaction = transaction;
                    command.CommandText = pg_query;
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    try
                    {
                        transaction.Rollback();
                    }
                    catch
                    {
                    }
                    return;
                }
            }
        }

        /// <summary>
        /// Экспорт данных ГОУ: 1, 2, 4
        /// </summary>
        /// <param name="pathconf">Путь конфига</param>
        /// <param name="av">av</param>
        /// <param name="bv">bv</param>
        /// <param name="x">x</param>
        static void Gou_exe(string pathconf, int av, int bv, int x)
        {
            try
            {
                bool ip1 = true, ip2 = true, ip3 = true;

                Ping ping = new Ping();
                PingReply pingReply = ping.Send("10.21.1.218");
                if (pingReply.Status != IPStatus.Success)
                {
                    ip1 = false; Console.WriteLine("Нет связи с ГОУ1");
                }

                pingReply = ping.Send("10.21.1.215");
                if (pingReply.Status != IPStatus.Success)
                {
                    ip2 = false; Console.WriteLine("Нет связи с ГОУ2");
                }

                pingReply = ping.Send("10.21.1.209");
                if (pingReply.Status != IPStatus.Success)
                {
                    ip3 = false; Console.WriteLine("Нет связи с ГОУ4");
                }

                string pg_odbc;

                string[] tablenamepsql = new string[125] {
                "gou11_c401","gou11_c402","gou11_c404","gou11_c403",
                "gou11_cai101","gou11_cai102","gou11_cai103","gou11_cai104","gou11_cai105","gou11_cai107","gou11_cai108","gou11_cai109",
                "gou12_cai117","gou12_cai118","gou12_cai119","gou12_cai120","gou12_cai122","gou12_cai123","gou12_cai125","gou12_cai126",
                "gou13_c401","gou13_c402","gou13_c403","gou13_c404","gou13_cai113",
                "gou13_cai101","gou13_cai102","gou13_cai103","gou13_cai104","gou13_cai105","gou13_cai106","gou13_cai107","gou13_cai108","gou13_cai109",
                "gou1_cai115","gou1_cai116",

                "gou21_c401","gou21_c402","gou21_c404","gou21_c403","gou21_cai113",
                "gou21_G1_cai105","gou21_cai101","gou21_cai102","gou21_cai103","gou21_cai104","gou21_cai107","gou21_cai108","gou21_cai109",
                "gou22_cai117","gou22_cai118","gou22_cai119","gou22_cai120","gou22_cai122","gou22_cai123","gou22_cai125","gou22_cai126",
                "gou23_c401" ,"gou23_c402" ,"gou23_c403" ,"gou23_c404" ,"gou23_cai113",
                "gou23_cai101","gou23_cai102","gou23_cai103","gou23_cai104","gou23_cai105","gou23_cai107","gou23_cai108","gou23_cai109",
                "gou2_cai115","gou2_cai116",

                "gou41_c401","gou41_c402","gou41_c404","gou41_c403","gou41_cai113",
                "gou41_cai105","gou41_cai101","gou41_cai102","gou41_cai103","gou41_cai104","gou41_cai106","gou41_cai107","gou41_cai108","gou41_cai109",
                "gou42_cai117","gou42_cai118","gou42_cai119","gou42_cai120","gou42_cai122","gou42_cai123","gou42_cai125","gou42_cai126",
                "gou43_c401" ,"gou43_c402" ,"gou43_c403" ,"gou43_c404" ,"gou43_cai113",
                "gou43_cai101","gou43_cai102","gou43_cai103","gou43_cai104","gou43_cai105","gou43_cai107","gou43_cai108","gou43_cai109",
                "gou4_cai115","gou4_cai116",

                "gou31_cai129","gou31_cai130","gou31_cai131","gou31_cai132","gou31_cai137",
                "gou33_cai133","gou33_cai134","gou33_cai135","gou33_cai136","gou33_cai138",
                "gou32_cai117","gou32_cai118","gou32_cai119","gou32_cai120",
                "gou3_cai115","gou3_cai116" };

                string[] fildname = new string[125] {
                "c401","c402","c404","c403",
                "cai101","cai102","cai103","cai104","cai105","cai107","cai108","cai109",
                "cai117","cai118","cai119","cai120","cai122","cai123","cai125","cai126",
                "c401","c402","c403","c404","cai113",
                "cai101","cai102","cai103","cai104","cai105","cai106","cai107","cai108","cai109",
                "cai115","cai116",
                "c401","c402","c404","c403","cai113",
                "G1_cai105","cai101","cai102","cai103","cai104","cai107","cai108","cai109",
                "cai117","cai118","cai119","cai120","cai122","cai123","cai125","cai126",
                "c401" ,"c402" ,"c403" ,"c404" ,"cai113",
                "cai101","cai102","cai103","cai104","cai105","cai107","cai108","cai109",
                "cai115","cai116",
                "c401","c402","c404","c403","cai113",
                "cai105","cai101","cai102","cai103","cai104","cai106","cai107","cai108","cai109",
                "cai117","cai118","cai119","cai120","cai122","cai123","cai125","cai126",
                "c401" ,"c402" ,"c403" ,"c404" ,"cai113",
                "cai101","cai102","cai103","cai104","cai105","cai107","cai108","cai109",
                "cai115","cai116",
                "cai129","cai130","cai131","cai132","cai137",
                "cai133","cai134","cai135","cai136","cai138",
                "cai117","cai118","cai119","cai120",
                "cai115","cai116"
                };

                int step = 0;

                DateTime dated = DateTime.Now.AddDays(x);
                string dateday = dated.Date.ToShortDateString().Replace(".", "/");
                string datenow = DateTime.Now.Date.ToShortDateString().Replace(".", "/");
                string year = dated.Year.ToString();
                string md;
                if (dated.Month < 10)
                {
                    md = "0" + dated.Month.ToString();
                }
                else
                {
                    md = dated.Month.ToString();
                };
                if (dated.Day < 10)
                {
                    md = md + "_0" + dated.Day.ToString();
                }
                else
                {
                    md = md + "_" + dated.Day.ToString();
                };

                string[] Dbhost = new string[125];
                for (int i = 0; i < 20; i++)
                { Dbhost[i] = "GOT1_1_" + year + "_"; }
                for (int i = 20; i < 36; i++)
                { Dbhost[i] = "GOT3_1_" + year + "_"; }
                for (int i = 36; i < 57; i++)
                { Dbhost[i] = "GOT1_2_" + year + "_"; }
                for (int i = 57; i < 72; i++)
                { Dbhost[i] = "GOT3_2_" + year + "_"; }
                for (int i = 72; i < 94; i++)
                { Dbhost[i] = "GOT1_" + year + "_"; }
                for (int i = 94; i < 107; i++)
                { Dbhost[i] = "GOT3_" + year + "_"; }
                for (int i = 107; i < 109; i++)
                { Dbhost[i] = "GOT1_" + year + "_"; }
                for (int i = 109; i < 125; i++)
                { Dbhost[i] = "GOT3_" + year + "_"; }

                string[] iphost = new string[3] { "10.21.1.218", "10.21.1.215", "10.21.1.209" };


                string path = File.ReadAllLines(pathconf)[5];
                string pg_query;

                DataTable bindSource = new DataTable();
                OdbcConnection conn;
                OdbcCommand cmd;

                for (int i = av; i < bv; i++)
                {
                    try
                    {
                        if (i < 36 && !ip1) { continue; }
                        if (i >= 36) { if (!ip2) { continue; } step = 1; }
                        if (i >= 72) { if (!ip3) { continue; } step = 2; }

                        string Dbname = Dbhost[i] + md;
                        pg_odbc = "Driver={MySQL ODBC 3.51 Driver};Server=" + iphost[step] + ";Database=" + Dbname + ";User=pcguest;Password=pcguest;Option=0;Port=0;";
                        conn = new OdbcConnection(pg_odbc);
                        cmd = new OdbcCommand();
                        cmd.Connection = conn;


                        pg_query = "SELECT time as time_gp, value as value_gp FROM " + fildname[i];

                        cmd.CommandText = pg_query;
                        OdbcDataAdapter ourAdapter = new OdbcDataAdapter(cmd);
                        DataTable ourDataTable = new DataTable();
                        try
                        {
                            conn.Open();
                            ourAdapter.Fill(ourDataTable);
                            bindSource = ourDataTable;
                            conn.Close();
                        }
                        catch
                        {
                            Console.WriteLine("Ошибка доступа к базе ГОУ, проверть связь с ГОУ");
                            continue;
                        }

                        StringBuilder sb = new StringBuilder();
                        ourDataTable.Columns.Add("fild_gp", typeof(string));
                        ourDataTable.Columns.Add("date_gp", typeof(string));

                        for (int gi = 0; gi < ourDataTable.Rows.Count; gi++)
                        {
                            ourDataTable.Rows[gi][2] = fildname[i];
                            ourDataTable.Rows[gi][3] = dated.Date.ToString("MM/dd/yy");
                        }

                        IEnumerable<string> columnNames = ourDataTable.Columns.Cast<DataColumn>().
                        Select(column => column.ColumnName);
                        sb.AppendLine(string.Join(",", columnNames));

                        foreach (DataRow row in ourDataTable.Rows)
                        {
                            IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                            sb.AppendLine(string.Join(",", fields));
                        }

                        File.WriteAllText(path + "gou\\" + fildname[i] + ".csv", sb.ToString(), Encoding.UTF8);
                        Console.WriteLine("Иморт графиков для ГОУ завершен" + fildname[i]);
                        ExecuteTransactionIndiv("copy asutp." + tablenamepsql[i] + " (time_gp, value_gp, fild_gp, date_gp ) from '/share/import/gou/" + fildname[i] + ".csv' DELIMITER ',' CSV null as '' HEADER escape '\\'");

                    }
                    catch
                    {
                        Console.WriteLine("Ошибка записи данных в БД Технолог, проверьте связь с NAS");
                        continue;
                    }
                }
            }
            catch { return; }
        }

        static void gou3_exe(string pathconf)
        {
            try
            {
                string[] dateday = new string[12];
                string datenow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                string path = File.ReadAllLines(pathconf)[5];
                string pg_query;
                string[] gou3tab = new string[12] { "DBA.PDE#HD#Archive#Discharge_in", "DBA.PDE#HD#Archive#Discharge_in_1", "DBA.PDE#HD#Archive#Discharge_out", "DBA.PDE#HD#Archive#Discharge_out_1", "DBA.PDE#HD#Archive#Tempr_in", "DBA.PDE#HD#Archive#Tempr_in_1", "DBA.PDE#HD#Archive#Tempr_out", "DBA.PDE#HD#Archive#Tempr_out_1", "DBA.PDE#HD#Archive_plc#Delta_discharge", "DBA.PDE#HD#Archive_plc#Delta_discharge_1", "DBA.PDE#HD#Archive_plc#Tempr_in_filter", "DBA.PDE#HD#Archive_plc#Tempr_in_filter_1" };
                string[] gou3fild = new string[12] { "RAB_PDE_HD_Archive_Discharge_in_V", "RAB_PDE_HD_Archive_Discharge_in_1_V", "RAB_PDE_HD_Archive_Discharge_out_V", "RAB_PDE_HD_Archive_Discharge_out_1_V", "RAB_PDE_HD_Archive_Tempr_in_V", "RAB_PDE_HD_Archive_Tempr_in_1_V", "RAB_PDE_HD_Archive_Tempr_out_V", "RAB_PDE_HD_Archive_Tempr_out_1_V", "RAB_PDE_HD_Archive_plc_Delta_discharge_V", "RAB_PDE_HD_Archive_plc_Delta_discharge_1_V", "RAB_PDE_HD_Archive_plc_Tempr_in_filter_V", "RAB_PDE_HD_Archive_plc_Tempr_in_filter_1_V" };
                string[] tablesgou = new string[12] { "gou3Discharge_in_V", "gou3Discharge_in_1_V", "gou3Discharge_out_V", "gou3Discharge_out_1_V", "gou3Tempr_in_V", "gou3Tempr_in_1_V", "gou3Tempr_out_V", "gou3Tempr_out_1_V", "gou3Delta_discharge_V", "gou3Delta_discharge_1_V", "gou3Tempr_in_filter_V", "gou3Tempr_in_filter_1_V" };

                string sy_odbc = "dsn=GOU_3;db='CC_GAS_CLEA_07-04-13_15:18:21R';na=10.21.1.212;uid=dba;pwd=sql;";
                string pg_odbc = "Driver={PostgreSQL Unicode};Server=10.21.1.222;Database=technolog;uid=BorodinAE;Password=bae^Y30;Port=5432;";


                DataTable pg_table = new DataTable();

                OdbcConnection pg_conn;
                OdbcCommand pg_cmd;
                pg_conn = new OdbcConnection(pg_odbc);
                pg_cmd = new OdbcCommand();
                pg_cmd.Connection = pg_conn;
                pg_cmd.CommandText = "(SELECT \"D\"   FROM asutp.\"gou3Discharge_in_V\" order by \"D\" desc limit 1) union all (SELECT \"D\"   FROM asutp.\"gou3Discharge_in_1_V\" order by \"D\" desc limit 1) union all (SELECT \"D\"   FROM asutp.\"gou3Discharge_out_V\" order by \"D\" desc limit 1) union all (SELECT \"D\"   FROM asutp.\"gou3Discharge_out_1_V\" order by \"D\" desc limit 1) union all (SELECT \"D\"   FROM asutp.\"gou3Tempr_in_V\" order by \"D\" desc limit 1) union all (SELECT \"D\"   FROM asutp.\"gou3Tempr_in_1_V\" order by \"D\" desc limit 1) union all (SELECT \"D\"   FROM asutp.\"gou3Tempr_out_V\" order by \"D\" desc limit 1) union all (SELECT \"D\"   FROM asutp.\"gou3Tempr_out_1_V\" order by \"D\" desc limit 1) union all (SELECT \"D\"   FROM asutp.\"gou3Delta_discharge_V\" order by \"D\" desc limit 1) union all (SELECT \"D\"   FROM asutp.\"gou3Delta_discharge_1_V\" order by \"D\" desc limit 1) union all (SELECT \"D\"   FROM asutp.\"gou3Tempr_in_filter_1_V\" order by \"D\" desc limit 1) union all (SELECT \"D\"   FROM asutp.\"gou3Tempr_in_filter_V\" order by \"D\" desc limit 1) ";
                pg_conn.Open();
                OdbcDataAdapter pg_ourAdapter = new OdbcDataAdapter(pg_cmd);
                pg_ourAdapter.Fill(pg_table);
                pg_conn.Close();

                for (int y = 0; y < 12; y++)
                {
                    dateday[y] = DateTime.Parse(pg_table.Rows[y][0].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                }

                OdbcConnection conn;
                OdbcCommand cmd;
                conn = new OdbcConnection(sy_odbc);
                cmd = new OdbcCommand();

                cmd.Connection = conn;

                StreamWriter[] files = new StreamWriter[12];

                string sp;


                for (int y = 0; y < 12; y++)
                {
                    DataTable ourDataTable = new DataTable();

                    pg_query = "SELECT " + gou3tab[y] + ".T as datetimes, " + gou3tab[y] + ".V as " + gou3fild[y] + " FROM " + gou3tab[y] + " where T between '" + dateday[y] + "' and '" + datenow + "'";

                    conn.Open();
                    cmd.CommandText = pg_query;
                    OdbcDataAdapter ourAdapter = new OdbcDataAdapter(cmd);
                    ourAdapter.Fill(ourDataTable);
                    conn.Close();
                    if (ourDataTable.Rows.Count == 0) { continue; }
                    string[][] mod = new string[2][] { new string[ourDataTable.Rows.Count], new string[ourDataTable.Rows.Count] };

                    for (int i = 0; i < ourDataTable.Rows.Count; i++)
                    {
                        mod[0][i] = DateTime.Parse(ourDataTable.Rows[i][0].ToString()).ToString("MM/dd/yy HH:mm:ss");
                    }

                    for (int i = 0; i < ourDataTable.Rows.Count; i++)
                    {
                        mod[1][i] = ourDataTable.Rows[i][1].ToString();
                    }

                    files[y] = new StreamWriter(@"" + path + gou3fild[y] + ".csv");
                    for (int i = 0; i < ourDataTable.Rows.Count; i++)
                    {
                        sp = ",";
                        for (int t = 0; t < 2; t++)
                        {
                            if (t == 1) { sp = ""; }
                            files[y].Write(mod[t][i] + sp);
                        }
                        files[y].Write(Environment.NewLine);
                    }
                    files[y].Close();

                }

                for (int i = 0; i < 12; i++)
                {
                    ExecuteTransactionIndiv("copy asutp.\"" + tablesgou[i] + "\" (\"D\", \"V\") from '/share/import/" + gou3fild[i] + ".csv' DELIMITER ',' CSV null as '' escape '\\'");
                }

            }
            catch (Exception ex) { Console.WriteLine("Ошибка при иморте графиков для GOU3"); Console.WriteLine(ex.Message); return; }
        }

        static void TehDBF(string pathconf)
        {
            try
            {
                IEnumerable<string> columnNames;
                IEnumerable<string> fields;

                string path = System.IO.File.ReadAllLines(pathconf)[5];
                string datein = DateTime.Parse(DateTime.Now.AddDays(-1).ToShortDateString()).ToString("MM.dd.yy");

                DirectoryInfo dir = new DirectoryInfo(System.IO.File.ReadAllLines(pathconf)[15]);
                FileSystemInfo[] files = dir.GetFileSystemInfos("*.dbf");
                Array.Sort<FileSystemInfo>(files, delegate (FileSystemInfo a, FileSystemInfo b)
                {
                    return a.LastWriteTime.CompareTo(b.LastWriteTime);
                });

                string dateday;
                DataTable pg_table = new DataTable();
                string pg_odbc = "Driver={PostgreSQL Unicode};Server=10.21.1.222;Database=technolog;uid=BorodinAE;Password=bae^Y30;Port=5432;";
                string query;
                OdbcConnection pg_conn;
                OdbcCommand pg_cmd;
                pg_conn = new OdbcConnection(pg_odbc);
                pg_cmd = new OdbcCommand();
                pg_cmd.Connection = pg_conn;
                pg_cmd.CommandText = "SELECT datetimegraph FROM asutp.boilergraph order by datetimegraph desc limit 1";
                pg_conn.Open();
                OdbcDataAdapter pg_ourAdapter = new OdbcDataAdapter(pg_cmd);
                pg_ourAdapter.Fill(pg_table);
                pg_conn.Close();
                DateTime datedbf = DateTime.Parse(pg_table.Rows[0][0].ToString());
                dateday = datedbf.ToString("MM/dd/yy HH:mm:ss");




                OdbcConnection conn = new OdbcConnection();
                conn.ConnectionString = @"Driver={Microsoft dBase Driver (*.dbf)};DriverID=277; Dbq=" + System.IO.File.ReadAllLines(pathconf)[15] + ";";

                conn.Open();
                DataTable dt = new DataTable();
                DataTable dtemp = new DataTable();

                DataColumn[] colString = new DataColumn[13];
                colString[0] = new DataColumn("datetimegraph");
                colString[1] = new DataColumn("K1");
                colString[2] = new DataColumn("K2");
                colString[3] = new DataColumn("K3");
                colString[4] = new DataColumn("K4");
                colString[5] = new DataColumn("K5");
                colString[6] = new DataColumn("K6");
                colString[7] = new DataColumn("K7");
                colString[8] = new DataColumn("K8");
                colString[9] = new DataColumn("K9");
                colString[10] = new DataColumn("K10");
                colString[11] = new DataColumn("K11");
                colString[12] = new DataColumn("K12");
                colString[0].DataType = Type.GetType("System.DateTime");
                colString[1].DataType = Type.GetType("System.String");
                colString[2].DataType = Type.GetType("System.String");
                colString[3].DataType = Type.GetType("System.String");
                colString[4].DataType = Type.GetType("System.String");
                colString[5].DataType = Type.GetType("System.String");
                colString[6].DataType = Type.GetType("System.String");
                colString[7].DataType = Type.GetType("System.String");
                colString[8].DataType = Type.GetType("System.String");
                colString[9].DataType = Type.GetType("System.String");
                colString[10].DataType = Type.GetType("System.String");
                colString[11].DataType = Type.GetType("System.String");
                colString[12].DataType = Type.GetType("System.String");


                query = "SELECT DATA+' '+TIME AS datetimegraph,K1,K2,K3,K4,K5,K6,K7,K8,K9,K10,K11,K12 from ( SELECT DATA,TIME,K1,K2,K3,K4,K5,K6,K7,K8,K9,K10,K11,K12 FROM " + files[files.Length - 1];

                for (int i = 2; i < 5; i++)
                { query += " union all SELECT DATA,TIME,K1,K2,K3,K4,K5,K6,K7,K8,K9,K10,K11,K12 FROM " + files[files.Length - i]; }
                query += " )";

                System.Data.Odbc.OdbcCommand oCmd = conn.CreateCommand();

                oCmd.CommandText = query;

                dt.Load(oCmd.ExecuteReader());
                conn.Close();


                DataRow workRow;

                for (int i = 0; i < 13; i++)
                {
                    dtemp.Columns.Add(colString[i]);
                }

                for (int y = 0; y < dt.Rows.Count; y++)
                {
                    workRow = dtemp.NewRow();
                    for (int i = 0; i < 13; i++)
                    {

                        if (i == 0)
                        {
                            workRow[i] = Convert.ToDateTime(dt.Rows[y][i].ToString());
                        }
                        else
                        {
                            workRow[i] = dt.Rows[y][i].ToString();
                        }
                    }
                    dtemp.Rows.Add(workRow);
                }

                DataRow[] foundRows;

                string expression = "datetimegraph > #" + dateday + "#";
                foundRows = dtemp.Select(expression);


                StringBuilder sb = new StringBuilder();
                columnNames = dt.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
                sb.AppendLine(string.Join(",", columnNames));
                foreach (DataRow row in foundRows)
                {
                    fields = row.ItemArray.Select(field => field.ToString());
                    sb.AppendLine(string.Join(",", fields));
                }

                int daych = (DateTime.Now - datedbf).Days;
                for (int i = 0; i <= daych + 1; i++)
                {
                    sb.Replace(datedbf.AddDays(i).ToString("dd.MM.yy"), datedbf.AddDays(i).ToString("MM.dd.yy"));
                }

                File.WriteAllText(path + "TehDBF.csv", sb.ToString(), Encoding.UTF8);

                sb.Clear();

                dt.Clear();

                ExecuteTransaction(7, pathconf);

                Console.WriteLine("Иморт графиков для котельной завершен");

            }
            catch { Console.WriteLine("Ошибка при иморте графиков для котельной"); return; }
        }

        static void kts_exe(string pathconf)
        {
            try
            {
                int[] idVTI = new int[39] { 1124, 1125, 1126, 1127, 1141, 1142, 1143, 1144, 1158, 1159, 1160, 1161, 1175, 1176, 1177, 1178, 1000, 1001, 1002, 1003, 1004, 1005, 1006, 1007, 1008, 1009, 1010, 1011, 1228, 1233, 1235, 1237, 1140, 1157, 1174, 1191, 1035, 1195, 1199 };


                DateTime currentDT = DateTime.Now.AddDays(-1);


                string pg_odbc = "Dsn=KTS_energy;" + "Uid=eng6;" + "Pwd=eng6;";

                string md;
                if (currentDT.Month < 10) { md = "0" + currentDT.Month.ToString(); } else { md = currentDT.Month.ToString(); };

                string datedayon = currentDT.Date.ToString("dd-MM-yyyy");

                string path = System.IO.File.ReadAllLines(pathconf)[5];
                string pg_query;



                DataTable ourDataTable = new DataTable();
                OdbcConnection conn;
                OdbcCommand cmd;
                conn = new OdbcConnection(pg_odbc);
                cmd = new OdbcCommand();
                cmd.Connection = conn;

                string filepath = "";
                string odbccomand = "";





                for (int ix = 0; ix < idVTI.Length; ix++)
                {

                    Random Rand = new Random();
                    int idReq = Convert.ToInt32(Rand.Next(1000000, 2000000));
                    try
                    {
                        cmd.CommandText = "exec [dbo].[ep_AskVTIdata] @Cmd='List', @idVTI=" + idVTI[ix].ToString() + ", @idReq=" + idReq + ", @TimeStart='" + datedayon + " 00:00:00', @TimeEnd='" + datedayon + " 23:59:59', @ShiftBeg=3, @ShiftEnd=3";
                        conn.Open();
                        OdbcDataAdapter pg_ourAdapter = new OdbcDataAdapter(cmd);
                        DataTable pg_table = new DataTable();
                        pg_ourAdapter.Fill(pg_table);
                        conn.Close();
                        pg_table.Dispose();
                    }
                    catch { Console.WriteLine("Ошибка выполнения процедуры!"); return; }



                    pg_query = "SELECT [TimeSQL],[ValueFl] FROM [e6work].[dbo].[VTIdataList] where [idREQ]=" + idReq + " and [idVTI]=" + idVTI[ix].ToString();
                    conn.Open();
                    cmd.CommandText = pg_query;
                    OdbcDataAdapter ourAdapter = new OdbcDataAdapter(cmd);

                    ourAdapter.Fill(ourDataTable);
                    conn.Close();



                    filepath = @"kts\kts_energy_" + idVTI[ix] + ".csv"; odbccomand = "copy asutp.kts_energ_f" + idVTI[ix] + " (datetime_graph, value_fl) from '/share/import/kts/kts_energy_" + idVTI[ix] + ".csv' DELIMITER ',' CSV null as '' HEADER escape '\\'";

                    StringBuilder sb = new StringBuilder();
                    IEnumerable<string> columnNames = ourDataTable.Columns.Cast<DataColumn>().
                    Select(column => column.ColumnName);
                    sb.AppendLine(string.Join(",", columnNames));


                    foreach (DataRow row in ourDataTable.Rows)
                    {
                        IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                        sb.AppendLine(string.Join(",", fields));
                    }

                    sb.Replace(currentDT.AddDays(-1).ToString("dd.MM.yy"), currentDT.AddDays(-1).ToString("MM.dd.yy"));
                    sb.Replace(currentDT.ToString("dd.MM.yy"), currentDT.ToString("MM.dd.yy"));
                    sb.Replace(currentDT.AddDays(1).ToString("dd.MM.yy"), currentDT.AddDays(1).ToString("MM.dd.yy"));
                    File.WriteAllText(path + filepath, sb.ToString(), Encoding.UTF8);

                    ExecuteTransactionIndiv(odbccomand);
                    Console.WriteLine("Иморт даннных для КТС завершен " + idVTI[ix]);

                    ourDataTable.Clear();
                    ourDataTable.Columns.Clear();
                }






            }
            catch
            {
                Console.WriteLine("Ошибка при иморте графиков для КТС "); return;
            }

        }

        static void dozdata_exe(string pathconf)
        {
            try
            {
                CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
                culture.NumberFormat.NumberDecimalSeparator = ".";
                culture.NumberFormat.NumberGroupSeparator = ",";
                DateTimeFormatInfo info;

                info = culture.DateTimeFormat;
                info.ShortDatePattern = "MM.dd.yy";
                info.LongDatePattern = "MM.dd.yyyy";

                DateTime currentDT;
                string datedayon;
                DataTable pg_table = new DataTable();
                string pg_odbc = "Driver={PostgreSQL Unicode};Server=10.21.1.222;Database=technolog;uid=BorodinAE;Password=bae^Y30;Port=5432;";

                OdbcConnection pg_conn;
                OdbcCommand pg_cmd;
                pg_conn = new OdbcConnection(pg_odbc);
                pg_cmd = new OdbcCommand();
                pg_cmd.Connection = pg_conn;
                pg_cmd.CommandText = "SELECT \"Data\" FROM techbase.\"RAB_RabDoza\" order by \"Data\" desc limit 1";
                pg_conn.Open();
                OdbcDataAdapter pg_ourAdapter = new OdbcDataAdapter(pg_cmd);
                pg_ourAdapter.Fill(pg_table);
                pg_conn.Close();

                DateTime datedbf = DateTime.Parse(pg_table.Rows[0][0].ToString());
                datedayon = datedbf.ToString("dd/MM/yyyy");
                pg_table.Dispose();

                int plt = 0;
                int plt2 = 0;
                try
                {
                    currentDT = DateTime.Now;
                }
                catch { Console.WriteLine("Дата в бaзе больше чем текущая!"); return; }

                string Dbname = "";
                string md;
                string path = System.IO.File.ReadAllLines(pathconf)[5];
                string[] pg_querys = new string[4];
                int df = 0;
                int step = 0;
                do
                {
                    if (datedbf.Month < 10) { md = "0" + datedbf.Month.ToString(); } else { md = datedbf.Month.ToString(); };

                    if (plt > 0) { step = 1; }
                    if (datedbf == currentDT.AddDays(-1)) { step = 2; }
                    if (datedbf.Month != datedbf.AddDays(-1).Month && plt == plt2)
                    {
                        datedbf = datedbf.AddDays(-1);
                        datedayon = datedbf.ToString("dd/MM/yyyy");
                        step = 0;
                        plt2++;
                    }

                    Dbname = "Dozav2_" + datedbf.Year.ToString() + "_" + md;

                    DataTable bindSource = new DataTable();
                    OdbcConnection conn;
                    OdbcCommand cmd;
                    pg_odbc = "Driver={MySQL ODBC 3.51 Driver};Server=10.21.1.208;Database=" + Dbname + ";User=pcguest;Password=pcguest;Option=0;Port=0;table=technolog";
                    conn = new OdbcConnection(pg_odbc);
                    cmd = new OdbcCommand();
                    cmd.Connection = conn;

                    try
                    {

                        pg_querys[0] = "select Data,Shift,TStart,TStop,Nline,Nfurn,Side,NPock,Quarz,Oreh,OilCoke,Coal,Iron,WChip,Other,BurCoal,SetQz,SetOreh,SetOilCoke,SetCoal,SetIron,SetWChip,SetOther,SetBurCoal from TECHNOLOG where Data = \'" + datedayon + "\' and TStart>=\'22:30:00'";
                        pg_querys[1] = "select Data,Shift,TStart,TStop,Nline,Nfurn,Side,NPock,Quarz,Oreh,OilCoke,Coal,Iron,WChip,Other,BurCoal,SetQz,SetOreh,SetOilCoke,SetCoal,SetIron,SetWChip,SetOther,SetBurCoal from TECHNOLOG where Data = \'" + datedayon + "\'";
                        pg_querys[2] = "select Data,Shift,TStart,TStop,Nline,Nfurn,Side,NPock,Quarz,Oreh,OilCoke,Coal,Iron,WChip,Other,BurCoal,SetQz,SetOreh,SetOilCoke,SetCoal,SetIron,SetWChip,SetOther,SetBurCoal from TECHNOLOG where Data = \'" + datedayon + "\' and TStart<=\'22:30:00'";
                        conn.Open();
                        cmd.CommandText = pg_querys[step];
                        OdbcDataAdapter ourAdapter = new OdbcDataAdapter(cmd);
                        DataTable ourDataTable = new DataTable();
                        ourAdapter.Fill(ourDataTable);
                        bindSource = ourDataTable;
                        conn.Close();

                        StringBuilder sb = new StringBuilder();


                        System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
                        customCulture.NumberFormat.NumberDecimalSeparator = ".";
                        Thread.CurrentThread.CurrentCulture = customCulture;


                        if (plt == 0)
                        {
                            IEnumerable<string> columnNames = ourDataTable.Columns.Cast<DataColumn>().
                            Select(column => column.ColumnName);
                            sb.AppendLine(string.Join(",", columnNames));
                        }

                        foreach (DataRow row in ourDataTable.Rows)
                        {
                            IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                            sb.AppendLine(string.Join(",", fields));
                        }

                        sb.Replace(datedbf.ToString("dd.MM.yyyy"), datedbf.ToString("MM.dd.yyyy"));

                        if (plt == 0) { File.WriteAllText(path + "rabdoz.csv", sb.ToString(), Encoding.UTF8); }
                        else
                        {
                            File.AppendAllText(path + "rabdoz.csv", sb.ToString(), Encoding.UTF8);
                        }

                        datedbf = datedbf.AddDays(1);
                        datedayon = datedbf.ToString("dd/MM/yyyy"); plt++; plt2++;


                    }
                    catch
                    {
                        Console.WriteLine("Ошибка при иморте графиков для ДО ");
                        continue;
                    }
                    df = (datedbf - currentDT).Days;

                } while (df != 0);

                ExecuteTransactionIndiv("copy techbase.\"RAB_RabDoza\" (\"Data\",\"Shift\",\"TStart\",\"TStop\",\"Nline\",\"Nfurn\",\"Side\",\"NPock\",\"Quarz\",\"Oreh\",\"OilCoke\",\"Coal\",\"Iron\",\"WChip\",\"Other\",\"BurCoal\",\"SetQz\",\"SetOreh\",\"SetOilCoke\",\"SetCoal\",\"SetIron\",\"SetWChip\",\"SetOther\",\"SetBurCoal\") from '/share/import/rabdoz.csv' DELIMITER ',' CSV null as '' HEADER escape '\\'");
                Console.WriteLine("Иморт даннных для ДО завершен");

            }
            catch { return; }

        }

        static void doz_exe(string pathconf)
        {
            try
            {
                string[] fildname = new string[86] { "fqn2_638_2", "fqn1_637_2", "fqn3_639_2", "fqn4_640_2", "fqn5_641_2", "fqn6_642_2", "fqn7_643_2", "fqn2_638_3", "fqn1_637_3", "fqn3_639_3", "fqn4_640_3", "fqn5_641_3", "fqn6_642_3", "fqn7_643_3", "fdelta1_671_2", "fdelta2_672_2", "fdelta3_673_2", "fdelta4_674_2", "fdelta5_675_2", "fdelta6_676_2", "fdelta7_677_2", "fUt1_616_2", "fUt2_617_2", "fUt3_618_2", "fUt4_619_2", "fUt5_620_2", "fUt6_621_2", "fUt7_622_2", "fUt1_616_3", "fUt2_617_3", "fUt3_618_3", "fUt4_619_3", "fUt5_620_3", "fUt6_621_3", "fUt7_622_3", "I10_corr0", "I11_corr0", "I12_corr0", "I13_corr0", "I14_corr0", "I8_corr0", "I9_corr0", "I19_corr0", "I15_corr0", "I16_corr0", "I17_corr0", "I18_corr0", "I20_corr0", "I21_corr0", "fKadop3", "fKadop2", "Mcur_10", "Mcur_11", "Mcur_14", "Mcur_13", "Mcur_12", "Mcur_8", "Mcur_9", "Mcur_15", "Mcur_16", "Mcur_17", "Mcur_18", "Mcur_19", "Mcur_20", "Mcur_21", "fDcur_11", "fDcur_12", "fDcur_13", "fDcur_14", "fDcur_15", "fDcur_16", "fDcur_17", "coN2_810_1", "coN1_809_1", "coN3_811_1", "coN4_812_1", "coN5_813_1", "coN6_814_1", "coN7_815_1", "dDcur_11_1043", "dDcur_12_1044", "dDcur_13_1045", "dDcur_14_1046", "dDcur_15_1047", "dDcur_16_1048", "dDcur_17_1049" };



                DateTime dated = DateTime.Now.AddDays(-1);
                string dateday = dated.Date.ToShortDateString().Replace(".", "/");
                string datenow = DateTime.Now.Date.ToShortDateString().Replace(".", "/");
                string md;
                if (dated.Month < 10) { md = "0" + dated.Month.ToString(); } else { md = dated.Month.ToString(); };
                if (dated.Day < 10) { md = md + "_0" + dated.Day.ToString(); } else { md = md + "_" + dated.Day.ToString(); };
                string year = dated.Year.ToString();


                string path = System.IO.File.ReadAllLines(pathconf)[5];
                string pg_query;
                string Dbname = "Dozav2_" + year + "_" + md;
                DataTable bindSource = new DataTable();
                OdbcConnection conn;
                OdbcCommand cmd;
                string pg_odbc = "Driver={MySQL ODBC 3.51 Driver};Server=10.21.1.208;Database=" + Dbname + ";User=pcguest;Password=pcguest;Option=0;Port=0;";
                conn = new OdbcConnection(pg_odbc);
                cmd = new OdbcCommand();
                cmd.Connection = conn;

                for (int y = 0; y < 86; y++)
                {
                    try
                    {
                        pg_query = "SELECT time as time_gp, value as value_gp FROM " + fildname[y];
                        conn.Open();
                        cmd.CommandText = pg_query;
                        OdbcDataAdapter ourAdapter = new OdbcDataAdapter(cmd);
                        DataTable ourDataTable = new DataTable();
                        ourAdapter.Fill(ourDataTable);
                        bindSource = ourDataTable;
                        conn.Close();

                        StringBuilder sb = new StringBuilder();
                        ourDataTable.Columns.Add("fild_gp", typeof(string));
                        ourDataTable.Columns.Add("date_gp", typeof(string));

                        for (int gi = 0; gi < ourDataTable.Rows.Count; gi++)
                        {
                            ourDataTable.Rows[gi][2] = fildname[y];
                            ourDataTable.Rows[gi][3] = dated.Date.ToString("MM/dd/yy");
                        }

                        IEnumerable<string> columnNames = ourDataTable.Columns.Cast<DataColumn>().
                        Select(column => column.ColumnName);
                        sb.AppendLine(string.Join(",", columnNames));

                        foreach (DataRow row in ourDataTable.Rows)
                        {
                            IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                            sb.AppendLine(string.Join(",", fields));
                        }

                        File.WriteAllText(path + "dozir\\" + fildname[y] + ".csv", sb.ToString(), Encoding.UTF8);
                        Console.WriteLine("Иморт графиков для ДО завершен" + fildname[y]);
                        ExecuteTransactionIndiv("copy asutp.doz_" + fildname[y] + " (time_gp, value_gp, fild_gp, date_gp ) from '/share/import/dozir/" + fildname[y] + ".csv' DELIMITER ',' CSV null as '' HEADER escape '\\'");
                    }
                    catch
                    {
                        Console.WriteLine("Ошибка при иморте графиков для ДО ");
                        continue;
                    }
                }


            }
            catch { return; }

        }

        static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }

        static void rtp4graph_exe(string pathconf, bool dtb, int yy, int mm, int dd)
        {
            DateTime dated;

            if (dtb) { dated = DateTime.Now; if (dated.Hour == 0) { dated = DateTime.Now.AddDays(-1); } }
            else
            {
                dated = new DateTime(yy, mm, dd);
            }

            int pday = dated.Day;
            string day;
            string month;
            string year = dated.Year.ToString();
            if (pday < 10) { day = "0" + pday.ToString(); } else { day = pday.ToString(); }
            if (dated.Month < 10) { month = "0" + dated.Month.ToString(); } else { month = dated.Month.ToString(); }


            int xcol = 0;
            string sp;
            int acols = 463;


            string file = "";
            string[] path = System.IO.File.ReadAllLines(pathconf);
            int arows = System.IO.File.ReadAllLines(path[3] + day + month + year + "_11.csv").Length;
            string[,] insertarray = new string[arows, acols];

            int rows;



            for (int e = 11; e < 98; e++)
            {
                file = path[3] + day + month + year + "_" + e + ".csv";
                string[] csvRows = System.IO.File.ReadAllLines(file);
                string[] fields = null;
                int cmach = Regex.Matches(csvRows[0], ",").Count;

                rows = 0;

                foreach (string csvRow in csvRows)
                {
                    fields = csvRow.Split(',');
                    if (xcol == 0) { insertarray[rows, 0] = month + "." + day + "." + year + " " + fields[0]; }
                    for (int col = 1; col <= cmach; col++)
                    {
                        insertarray[rows, xcol + col] = fields[col];
                    }
                    rows++;
                }
                xcol = xcol + cmach;
            }

            for (int e = 11; e < 30; e++)
            {
                file = path[3] + day + month + year + "_e" + e + ".csv";
                if (!System.IO.File.Exists(file)) { file = path[3] + day + month + year + "_E" + e + ".csv"; }
                if (!System.IO.File.Exists(file)) { break; }
                string[] csvRows = System.IO.File.ReadAllLines(file);
                string[] fields = null;
                int cmach = Regex.Matches(csvRows[0], ",").Count;
                rows = 0;
                foreach (string csvRow in csvRows)
                {
                    fields = csvRow.Split(',');
                    for (int col = 1; col <= cmach; col++)
                    {
                        insertarray[rows, xcol + col] = fields[col];
                    }
                    rows++;
                }
                xcol = xcol + cmach;
            }

            StreamWriter files1 = new StreamWriter(path[5] + "rtp4graph.csv");
            for (int i = 1; i < arows - 1; i++)
            {
                sp = ",";
                for (int y = 0; y < 463; y++)
                {
                    if (y == 462) { sp = ""; }
                    files1.Write(insertarray[i, y] + sp);
                }
                files1.Write(Environment.NewLine);
            }
            files1.Flush();
            try
            {
                ExecuteTransaction(1, pathconf);

            }
            catch { Console.WriteLine("Ошибка импорта исходных файлов!"); Console.ReadKey(); return; }

        }

        static void voda_graph_exe(int systoq, string pathconf, int dt)
        {

            try
            {

                string path = System.IO.File.ReadAllLines(pathconf)[5];
                string pg_query;
                DataTable bindSource = new DataTable();
                OdbcConnection conn;
                OdbcCommand cmd;
                OdbcDataAdapter ourAdapter;
                DataTable ourDataTable = new DataTable();
                IEnumerable<string> columnNames;
                IEnumerable<string> fields;

                DateTime dated = DateTime.Now.AddDays(dt);
                int pday = dated.Day;
                string day;
                string month;
                string year = dated.Year.ToString();
                if (pday < 10) { day = "0" + pday.ToString(); } else { day = pday.ToString(); }
                if (dated.Month < 10) { month = "0" + dated.Month.ToString(); } else { month = dated.Month.ToString(); }

                DateTime fdate = DateTime.Now;
                int sday = fdate.Day;
                string fday;
                string fmonth;
                string fyear = fdate.Year.ToString();
                if (sday < 10) { fday = "0" + sday.ToString(); } else { fday = sday.ToString(); }
                if (fdate.Month < 10) { fmonth = "0" + fdate.Month.ToString(); } else { fmonth = fdate.Month.ToString(); }

                string pg_odbc = "DSN=ACCESS_UOV";
                conn = new OdbcConnection(pg_odbc);
                cmd = new OdbcCommand();
                cmd.Connection = conn;
                conn.Open();
                string coven = '"'.ToString(); ;

                pg_query = "";
                pg_query = System.IO.File.ReadAllLines(System.IO.File.ReadAllLines(pathconf)[11])[systoq];
                if (systoq == 1) { pg_query = pg_query + " FROM `E:\\! BZF\\TEHNOLOG_GRAPH_LOGS\\UOV.mdb`.T_RAB_ADAM T_RAB_ADAM where (T_RAB_ADAM.ДатаВремя>={ts '" + year + "-" + month + "-" + day + " 00:00:00'}) and (T_RAB_ADAM.ДатаВремя<{ts '" + fyear + "-" + fmonth + "-" + fday + " 00:00:00'})"; }
                if (systoq == 2) { pg_query = pg_query + " FROM `E:\\! BZF\\TEHNOLOG_GRAPH_LOGS\\UOV.mdb`.T_RAB_OWEN T_RAB_OWEN where (T_RAB_OWEN.ДатаВремя>={ts '" + year + "-" + month + "-" + day + " 00:00:00'}) and (T_RAB_OWEN.ДатаВремя<{ts '" + fyear + "-" + fmonth + "-" + fday + " 00:00:00'})"; }

                cmd.CommandText = pg_query;
                ourAdapter = new OdbcDataAdapter(cmd);
                ourAdapter.Fill(ourDataTable);
                bindSource = ourDataTable;

                StringBuilder sb = new StringBuilder();
                columnNames = ourDataTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
                sb.AppendLine(string.Join(",", columnNames));
                foreach (DataRow row in ourDataTable.Rows)
                {
                    fields = row.ItemArray.Select(field => field.ToString());
                    sb.AppendLine(string.Join(",", fields));
                }

                File.WriteAllText(path + systoq.ToString() + "VOgraph.csv", sb.ToString(), Encoding.UTF8);

                sb.Clear();

                ourDataTable.Clear();
                bindSource.Clear();
                conn.Close();

                ExecuteTransaction(systoq + 3, pathconf);

                Console.WriteLine("Иморт графиков УОВ завершен");
            }
            catch
            {
                Console.WriteLine("Ошибка при иморте графиков УОВ"); return;
            }
        }

        static void rtp2_graph_exe(string pathconf)
        {
            try
            {
                DateTime dated = DateTime.Now;
                int pday = dated.AddDays(-1).Day;
                string day;
                string month;
                string year = dated.Year.ToString();
                if (pday < 10) { day = "0" + pday.ToString(); } else { day = pday.ToString(); }
                if (dated.Month < 10) { month = "0" + dated.Month.ToString(); } else { month = dated.Month.ToString(); }

                DateTime fdate = DateTime.Now;
                int sday = fdate.Day;
                string fday;
                string fmonth;
                string fyear = fdate.Year.ToString();
                if (sday < 10) { fday = "0" + sday.ToString(); } else { fday = sday.ToString(); }
                if (fdate.Month < 10) { fmonth = "0" + fdate.Month.ToString(); } else { fmonth = fdate.Month.ToString(); }


                string path = System.IO.File.ReadAllLines(pathconf)[5];
                string pg_query = System.IO.File.ReadAllLines(System.IO.File.ReadAllLines(pathconf)[11])[3];
                pg_query = pg_query + "  FROM `D:\\Temp\\RTP2\\Access.mdb`.rtp2 rtp2 ";


                OdbcConnection conn;
                OdbcCommand cmd;
                string pg_odbc = "DSN=ACCESS_RTP2";
                conn = new OdbcConnection(pg_odbc);
                cmd = new OdbcCommand();
                cmd.Connection = conn;
                conn.Open();
                cmd.CommandText = pg_query;
                OdbcDataAdapter ourAdapter = new OdbcDataAdapter(cmd);
                DataTable ourDataTable = new DataTable();
                ourAdapter.Fill(ourDataTable);
                conn.Close();

                StringBuilder sb = new StringBuilder();

                IEnumerable<string> columnNames = ourDataTable.Columns.Cast<DataColumn>().
                Select(column => column.ColumnName);
                sb.AppendLine(string.Join(",", columnNames));

                foreach (DataRow row in ourDataTable.Rows)
                {
                    IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                    sb.AppendLine(string.Join(",", fields));
                }

                File.WriteAllText(path + "rtp2graph.csv", sb.ToString(), Encoding.UTF8);


                ExecuteTransaction(3, pathconf);
                Console.WriteLine("Иморт графиков РТП2 завершен");
            }
            catch
            {
                Console.WriteLine("Ошибка при иморте графиков РТП2"); return;
            }
        }

        static void rtp2_graph_msql_exe(string pathconf, string key, string date_b, string date_p)
        {
            try
            {
                DateTime dated = DateTime.Now;
                int pday = dated.Day;
                string day;
                string month;
                string year = dated.Year.ToString();
                if (pday < 10) { day = "0" + pday.ToString(); } else { day = pday.ToString(); }
                if (dated.Month < 10) { month = "0" + dated.Month.ToString(); } else { month = dated.Month.ToString(); }

                DateTime fdate = DateTime.Now;
                int sday = fdate.Day;
                string fday;
                string fmonth;
                string fyear = fdate.Year.ToString();
                if (sday < 10) { fday = "0" + sday.ToString(); } else { fday = sday.ToString(); }
                if (fdate.Month < 10) { fmonth = "0" + fdate.Month.ToString(); } else { fmonth = fdate.Month.ToString(); }


                string path = System.IO.File.ReadAllLines(pathconf)[5];
                string pg_query = System.IO.File.ReadAllLines(System.IO.File.ReadAllLines(pathconf)[11])[3];

                pg_query = "select DATE_FORMAT(datetime_graph,'%m.%d.%Y %T'),t_gh1,t_gh2,t_gh3,t_gh4,t_coj1,t_coj2,t_coj3,t_ch_1_1,t_ch_1_2,t_ch_1_3,t_ch_1_4,t_ch_1_5, t_ch_1_6,t_ch_1_7,t_ch_1_8,t_ch_2_1,t_ch_2_2,t_ch_2_3,t_ch_2_4, t_ch_2_5,t_ch_2_6,t_ch_2_7,t_ch_2_8,t_ch_3_1,t_ch_3_2,t_ch_3_3, t_ch_3_4,t_ch_3_5,t_ch_3_6,t_ch_3_7,t_ch_3_8,t_c_1_12,t_c_1_34, t_c_1_56,t_c_1_78,t_c_2_12,t_c_2_34,t_c_2_56,t_c_2_78,t_c_3_12, t_c_3_34,t_c_3_56,t_c_3_78,up_1,ul_1,knp_1,knl_1,up_2, ul_2,knp_2,knl_2,up_3,ul_3,knp_3,knl_3,ue_1,ue_2,ue_3, p_1,p_2,p_3,p,cos_1,cos_2,cos_3,cos,ut_1,ut_2,ut_3, tm_1,tm_2,tm_3,q,s,x,jf_1,jf_2,jf_3,je_1,je_2,je_3, ped_1,ped_2,ped_3,pzs_1,pzs_2,pzs_3,uab,ubc,uac,if1_test, if2_test,if3_test,st1,st2,st3,mode1,mode2,mode3,count_bypass1,count_bypass2,count_bypass3 from rtp2 ";
                if (key == "a_hour")
                {
                    pg_query += "where datetime_graph>STR_TO_DATE('" + dated.AddHours(-1).ToString("dd.MM.yyyy HH:00:00") + "', '%d.%m.%Y %T') and  datetime_graph<=STR_TO_DATE('" + dated.ToString("dd.MM.yyyy HH:00:00") + "', '%d.%m.%Y %T')";
                }
                else if (key == "man")
                {
                    pg_query += "where datetime_graph>STR_TO_DATE('" + date_b + "', '%d.%m.%Y %T') and  datetime_graph<=STR_TO_DATE('" + date_p + "', '%d.%m.%Y %T')";
                }
                else if (key == "a_sm2")
                {
                    pg_query += "where datetime_graph>STR_TO_DATE('" + dated.AddDays(-1).ToString("dd.MM.yyyy") + " 16:00:00', '%d.%m.%Y %T') and  datetime_graph<=STR_TO_DATE('" + dated.ToString("dd.MM.yyyy") + " 08:00:00', '%d.%m.%Y %T')";
                }

                OdbcConnection conn;
                OdbcCommand cmd;
                string pg_odbc = "Driver={MySQL ODBC 3.51 Driver};Server=10.21.1.206;Database=rtp2;User=root;Password=vector;Option=0;Port=0;"; ;
                conn = new OdbcConnection(pg_odbc);
                cmd = new OdbcCommand();
                cmd.Connection = conn;
                conn.Open();
                cmd.CommandText = pg_query;
                OdbcDataAdapter ourAdapter = new OdbcDataAdapter(cmd);
                DataTable ourDataTable = new DataTable();
                ourAdapter.Fill(ourDataTable);
                conn.Close();

                StringBuilder sb = new StringBuilder();

                IEnumerable<string> columnNames = ourDataTable.Columns.Cast<DataColumn>().
                Select(column => column.ColumnName);
                sb.AppendLine(string.Join(",", columnNames));

                foreach (DataRow row in ourDataTable.Rows)
                {
                    IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                    sb.AppendLine(string.Join(",", fields));
                }

                File.WriteAllText(path + "rtp2graph.csv", sb.ToString(), Encoding.UTF8);


                ExecuteTransaction(3, pathconf);
                Console.WriteLine("Иморт графиков РТП2 завершен");
            }
            catch
            {
                Console.WriteLine("Ошибка при иморте графиков РТП2"); return;
            }
        }

        static void addDSN(string pathconf)
        {
            try
            {
                string str;
                str = "DSN=ACCESS_UOV\0DefaultDir=E:\\! BZF\\TEHNOLOG_GRAPH_LOGS\0DBQ=E:\\! BZF\\TEHNOLOG_GRAPH_LOGS\\UOV.mdb\0";
                SQLConfigDataSource((IntPtr)0, 4, "Microsoft Access Driver (*.mdb)", str);
                Console.WriteLine("Добавление DSN UOV завершенно");
                str = "DSN=ACCESS_RTP2\0DefaultDir=D:\\Temp\\RTP2\0DBQ=D:\\Temp\\RTP2\\Access.mdb\0";
                SQLConfigDataSource((IntPtr)0, 4, "Microsoft Access Driver (*.mdb)", str);
                Console.WriteLine("Добавление DSN завершенно");
            }
            catch { Console.WriteLine("Ошибка при добавлении DSN"); }
        }

        static void rtp1_exe_v1(string pathconf)
        {
            try
            {
                string path = File.ReadAllLines(pathconf)[5];
                DateTime dated = DateTime.Now.AddDays(-1);
                string dateday = dated.Date.ToString("MM.dd.yy");
                int pday = dated.Day;
                string day;
                string month;
                string year = dated.Year.ToString();
                if (pday < 10) { day = "0" + pday.ToString(); } else { day = pday.ToString(); }
                if (dated.Month < 10) { month = "0" + dated.Month.ToString(); } else { month = dated.Month.ToString(); }
                float tempv = 0;

                float[][] insertarray = new float[122][] { new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441] };

                int[] NumberLine = { 15, 21, 31, 32, 33, 34, 35, 36, 50, 51, 52, 253, 254, 255, 78, 79, 80, 84, 85, 86, 74, 75, 76, 120, 192, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 220, 221, 222, 223, 224, 225, 226, 227, 228, 229, 230, 231, 232, 233, 234, 235, 236, 237, 238, 239, 240, 241, 242, 243, 244, 245, 246, 247, 248, 249, 250, 251, 252, 117, 118, 119, 325, 164, 344, 369, 377, 378, 379, 385, 386, 387, 125, 126, 128, 425, 426, 427, 87, 88, 89, 347, 348, 349, 49, 428, 429, 430, 431, 432, 433, 484, 485, 468, 469, 470 };
                string[] NameFilds = { "Акт", "Реакт", "AB", "BC", "CA", "AB", "BC", "CA", "Эл1", "Эл2", "Эл3", "Эл 1", "Эл 2", "Эл 3", "Эл1", "Эл2", "Эл3", "Эл 1", "Эл 2", "Эл 3", "Эл 1", "Эл 2", "Эл 3", "Ванна", "Тп 1", "Тп 2", "Тп 3", "Тк 1", "Тк 2", "Тк 3", "Т 1", "Т 2", "Т 3", "Т 4", "Т 1", "Т 2", "Т 3", "Т 1", "Т 2", "Т 3", "Т 4", "Т 1", "Т 2", "Т 3", "Т 4", "Т 1", "Т 2", "Т 3", "Т 4", "Т 1", "Т 2", "Т 3", "Т 4", "Т 5", "Т 6", "Т 7", "Т 8", "Т 1", "Т 2", "Т 3", "Т 4", "Т 5", "Т 6", "Т 7", "Т 8", "Т 1", "Т 2", "Т 3", "Т 4", "Т 5", "Т 6", "Т 7", "Т 8", "Т 1 Эл 1", "Т 2 Эл 1", "Т 1 Эл 2", "Т 2 Эл 2", "Т 1 Эл 3", "Т 2 Эл 3", "Т 1 Эл 1", "Т 2 Эл 1", "Т 1 Эл 2", "Т 2 Эл 2", "Т 1 Эл 3", "Т 2 Эл 3", "Эл 1", "Эл 2", "Эл 3", "VV", "T", "Q", "EP", "эл1", "эл2", "эл3", "Т1", "Т2", "Т3", "Q1", "Q2", "Q3", "Q1", "Q2", "Q3", "E1", "E2", "E3", "I1", "I2", "I3", "Х", "Q1c", "Q2c", "Q3c", "E1c", "E2c", "E3c", "M1", "M2", "Kiu1", "Kiu2", "Kiu3" };


                string fileName = System.IO.File.ReadAllLines(pathconf)[13] + day + "-" + month + "-" + year;

                byte[] bytearr = new byte[1441];
                try
                {
                    if (File.Exists(fileName))
                    {
                        using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
                        {

                            for (int y = 0; y < 118; y++)
                            {
                                int LineN = NumberLine[y];
                                long Position = 5760 * LineN;
                                reader.BaseStream.Seek(Position, SeekOrigin.Begin);
                                for (int i = 0; i <= 1440; i++)
                                {
                                    tempv = reader.ReadSingle();
                                    if (tempv > 1e+30) { continue; }
                                    insertarray[y][i] = tempv;
                                }
                            }

                        }

                    }
                    string time;
                    TimeSpan timeformat;
                    string sp;
                    StreamWriter files1 = new StreamWriter(path + "rtp1graph.csv");
                    for (int i = 0; i < 1440; i++)
                    {
                        timeformat = TimeSpan.FromMinutes(i);
                        time = timeformat.ToString("hh':'mm':'ss");
                        sp = ",";
                        for (int y = 0; y < 121; y++)
                        {
                            if (y == 121) { sp = ""; }
                            if (y == 0) { files1.Write(dateday + " " + time + "," + insertarray[y][i] + sp); }
                            else { files1.Write(insertarray[y][i] + sp); }
                        }
                        files1.Write(Environment.NewLine);
                    }
                    files1.Flush();
                }
                catch
                {
                    Console.WriteLine("Ошибка выполнения");
                }

                ExecuteTransaction(2, pathconf);
            }
            catch { return; }

        }

        static void rtp1_exe(string pathconf, string mode, int colh, int starth, int datef)
        {
            try
            {
                List<string> logh = new List<string>();

                string path = System.IO.File.ReadAllLines(pathconf)[5];
                DateTime dated = DateTime.Now.AddDays(datef);
                if (mode == "3sm_auto")
                {
                    dated = DateTime.Now.AddDays(-1);
                }
                string dateday = dated.Date.ToString("MM.dd.yy");
                int pday = dated.Day;
                string day;
                string month;
                string year = dated.Year.ToString();
                if (pday < 10) { day = "0" + pday.ToString(); } else { day = pday.ToString(); }
                if (dated.Month < 10) { month = "0" + dated.Month.ToString(); } else { month = dated.Month.ToString(); }
                float tempv = 0;
                int countflow = 0;

                int dpa = 0;
                int dpb = 0;
                if (mode == "1sm")
                {
                    dpa = 0; dpb = 479;
                }
                else if (mode == "2sm")
                {
                    dpa = 480; dpb = 959;
                }
                else if (mode == "3sm" || mode == "3sm_auto")
                {
                    dpa = 960; dpb = 1439;
                }
                else if (mode == "1h")
                {
                    if (dated.Hour == 0) { dpa = 1380; dpb = 1439; }
                    else
                    {
                        dpa = 60 * (dated.Hour - 1); dpb = (60 * dated.Hour) - 1;
                    }
                }
                else if (mode == "1d")
                {
                    dpa = 0; dpb = 1439;
                }
                else if (mode == "hc")
                {
                    dpa = 60 * (starth); dpb = dpa + (60 * colh) - 1;

                }
                else { return; }

                float[][] insertarray = new float[122][] { new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441], new float[1441] };

                int[] NumberLine = { 15, 21, 31, 32, 33, 34, 35, 36, 50, 51, 52, 253, 254, 255, 78, 79, 80, 84, 85, 86, 74, 75, 76, 120, 192, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 220, 221, 222, 223, 224, 225, 226, 227, 228, 229, 230, 231, 232, 233, 234, 235, 236, 237, 238, 239, 240, 241, 242, 243, 244, 245, 246, 247, 248, 249, 250, 251, 252, 117, 118, 119, 325, 164, 344, 369, 377, 378, 379, 385, 386, 387, 125, 126, 128, 425, 426, 427, 87, 88, 89, 347, 348, 349, 49, 428, 429, 430, 431, 432, 433, 484, 485, 468, 469, 470 };
                string[] NameFilds = { "Акт", "Реакт", "AB", "BC", "CA", "AB", "BC", "CA", "Эл1", "Эл2", "Эл3", "Эл 1", "Эл 2", "Эл 3", "Эл1", "Эл2", "Эл3", "Эл 1", "Эл 2", "Эл 3", "Эл 1", "Эл 2", "Эл 3", "Ванна", "Тп 1", "Тп 2", "Тп 3", "Тк 1", "Тк 2", "Тк 3", "Т 1", "Т 2", "Т 3", "Т 4", "Т 1", "Т 2", "Т 3", "Т 1", "Т 2", "Т 3", "Т 4", "Т 1", "Т 2", "Т 3", "Т 4", "Т 1", "Т 2", "Т 3", "Т 4", "Т 1", "Т 2", "Т 3", "Т 4", "Т 5", "Т 6", "Т 7", "Т 8", "Т 1", "Т 2", "Т 3", "Т 4", "Т 5", "Т 6", "Т 7", "Т 8", "Т 1", "Т 2", "Т 3", "Т 4", "Т 5", "Т 6", "Т 7", "Т 8", "Т 1 Эл 1", "Т 2 Эл 1", "Т 1 Эл 2", "Т 2 Эл 2", "Т 1 Эл 3", "Т 2 Эл 3", "Т 1 Эл 1", "Т 2 Эл 1", "Т 1 Эл 2", "Т 2 Эл 2", "Т 1 Эл 3", "Т 2 Эл 3", "Эл 1", "Эл 2", "Эл 3", "VV", "T", "Q", "EP", "эл1", "эл2", "эл3", "Т1", "Т2", "Т3", "Q1", "Q2", "Q3", "Q1", "Q2", "Q3", "E1", "E2", "E3", "I1", "I2", "I3", "Х", "Q1c", "Q2c", "Q3c", "E1c", "E2c", "E3c", "M1", "M2", "Kiu1", "Kiu2", "Kiu3" };


                string fileName = System.IO.File.ReadAllLines(pathconf)[13] + day + "-" + month + "-" + year;
                if (File.Exists(fileName))
                {
                    byte[] bytearr = new byte[1441];
                    try
                    {

                        using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
                        {

                            for (int y = 0; y < 118; y++)
                            {
                                int LineN = NumberLine[y];
                                long Position = 5760 * LineN;
                                reader.BaseStream.Seek(Position, SeekOrigin.Begin);
                                for (int i = 0; i <= 1440; i++)
                                {
                                    tempv = reader.ReadSingle();
                                    if (tempv > 1e+30) { continue; }
                                    insertarray[y][i] = tempv;
                                    if (insertarray[y][i] != 0) { countflow++; }
                                }
                            }

                        }

                        string time;
                        TimeSpan timeformat;
                        string sp;
                        StreamWriter files1 = new StreamWriter(path + "rtp1graph.csv");
                        for (int i = dpa; i <= dpb; i++)
                        {
                            timeformat = TimeSpan.FromMinutes(i);
                            time = timeformat.ToString("hh':'mm':'ss");
                            sp = ",";
                            for (int y = 0; y < 121; y++)
                            {
                                if (y == 121) { sp = ""; }
                                if (y == 0) { files1.Write(dateday + " " + time + "," + insertarray[y][i] + sp); }
                                else { files1.Write(insertarray[y][i] + sp); }
                            }
                            files1.Write(Environment.NewLine);
                        }
                        files1.Flush();
                    }
                    catch
                    {
                        logerror("РТП1 Ошибка выполнения блока чтения и заполнения массива! " + DateTime.Now.ToString());
                        Console.WriteLine("Ошибка выполнения"); Console.ReadKey();
                    }

                }
                else
                {
                    Console.WriteLine("Нет файла:" + fileName); return;
                }

                if (countflow > 0)
                {
                    ExecuteTransaction(2, pathconf);
                }
                else
                {
                    logerror("РТП1 Ошибка добавления записей! " + DateTime.Now.ToString());
                }
            }
            catch
            {
                logerror("Ошибка выполнения функции rtp1_exe! " + DateTime.Now.ToString());
                return;
            }

        }

        static void logerror(string mode)
        {
            if (File.Exists(@"%USERPROFILE%\Desktop\log_graph.txt"))
            {
                File.AppendAllText(@"%USERPROFILE%\Desktop\log_graph.txt", mode, Encoding.UTF8);
            }
            else
            {
                File.WriteAllText(@"%USERPROFILE%\Desktop\log_graph.txt", mode, Encoding.UTF8);
            }
        }

        static void logerror_l(List<string> mode)
        {
            if (File.Exists(@"%USERPROFILE%\Desktop\log_graph.txt"))
            {
                File.AppendAllLines(@"%USERPROFILE%\Desktop\log_graph.txt", mode, Encoding.UTF8);
            }
            else
            {
                File.WriteAllLines(@"%USERPROFILE%\Desktop\log_graph.txt", mode, Encoding.UTF8);
            }
        }
    }
}
