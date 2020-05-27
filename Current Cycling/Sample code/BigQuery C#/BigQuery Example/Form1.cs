using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Cloud.BigQuery.V2;

namespace BigQuery_Example
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnAddData_Click(object sender, EventArgs e)
        {
            //Creates a connection to the database
            BigQueryClient client = BigQueryClient.Create("astute-engine-277918");


            // Create the dataset if it doesn't exist.
            BigQueryDataset dataset = client.GetOrCreateDataset("TestBQ");

            // Create the table if it doesn't exist.
            BigQueryTable table = dataset.GetOrCreateTable("Test_Table", new TableSchemaBuilder
                        {
                            { "Data", BigQueryDbType.String },
                            { "Time", BigQueryDbType.Timestamp }
                        }.Build());

            // Insert a single row. There are many other ways of inserting
            // data into a table.
            table.InsertRow(new BigQueryInsertRow                      
            {
                            { "Data", txtData.Text },
                            { "Time",  DateTime.UtcNow }
                        });

        }
    }
}
