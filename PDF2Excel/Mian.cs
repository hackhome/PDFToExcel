using iText.Kernel.Pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TableExtractionFromPDF;

namespace PDF2Excel
{
    public partial class Form1 : Form
    {
        string[] filenames = null;
        public Form1()
        {
            InitializeComponent();
        }

        private void btn_Change_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
        
            openFileDialog1.FileName = "";
            openFileDialog1.Multiselect = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.lb_Change.Items.Clear();
                this.lb_Change.Items.AddRange(openFileDialog1.FileNames);
                filenames = openFileDialog1.FileNames;
            }
            btn_Check.Enabled = true;
        }

        private  async void btn_Check_Click(object sender, EventArgs e)
        {
            try
            {
                await PdfToExcel();
                MessageBox.Show("转换完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private Task PdfToExcel()
        {

            return Task.Run(() =>
            {
                List<Task> tasks = new List<Task>();

                foreach (var filename in filenames)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        try
                        {
                            string pdffilename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename));
                            List<PdfTable> pdfTables = new List<PdfTable>();
                            if (File.Exists(pdffilename + ".xlsx"))
                            {
                                for (int i = 1; i < 100; i++)
                                {
                                    if (!File.Exists(pdffilename + "_" + i + ".xlsx"))
                                    {
                                        pdffilename = pdffilename + "_" + i + ".xlsx";
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                pdffilename = pdffilename + ".xlsx";
                            }
                            PdfReader reader = new PdfReader(filename);
                            PdfDocument document = new PdfDocument(reader);
                            int tableNum = 0;
                            for (int i = 0; i < document.GetNumberOfPages(); i++)
                            {
                                PdfPage page = document.GetPage(i + 1);

                                FilterTableEventListener renderListener = new FilterTableEventListener(page, true);
                                var tables = renderListener.GetTables();
                                for (int j = 0; j < tables.Count; j++)
                                {
                                    var table = tables[j];
                                    table.TableName = "table_" + tableNum++;
                                    if (pdfTables.Count == 0 || j > 0)
                                    {
                                        pdfTables.Add(table);
                                    }
                                    else
                                    {
                                        PdfTable lastTable = pdfTables[pdfTables.Count - 1];
                                        if (!lastTable.MergeTable(table))
                                        {
                                            pdfTables.Add(table);
                                        }
                                    }
                                }
                                PrintMsg(filename + $":第{i+1}页装换...OK");

                            }
                            ExcelHelper.PdfTableToExcel(pdfTables, pdffilename);
                            document.Close();
                            reader.Close();
                            PrintMsg(filename + $":转换完成");
                        }
                        catch (Exception ex)
                        {
                            PrintMsg(filename + "转换失败！");
                            PrintMsg(ex.Message);
                        }

                    }));
                }
                Task.WaitAll(tasks.ToArray());
            });

        }

        public virtual void PrintMsg(string txt)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(() => PrintMsg(txt)));
            else
            {
                txt_ShowError.AppendText(txt);
                txt_ShowError.AppendText(System.Environment.NewLine);
            }

        }
    }
}
