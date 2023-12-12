using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace veremautomata_HorvathDavid
{
    public partial class Form1 : Form
    {
        string inputSzalag;
        int index = 0; // Indexelés
        Stack<string> stack = new Stack<string>(); // Verem
        List<string> alkalmazottSzabalyok = new List<string>(); // Alkalmazott szabályok listája
        string[,] rulesMatrix; // szabály táblázat a mátrixban
        bool hiba = false;

        public Form1()
        {
            InitializeComponent();
            InitializeRulesMatrix();
            InitializeDataGridView();
            stack.Push("#");
            stack.Push("E");
            richTextBox2.Text = "i+i; i+i*(i+i); (i+i)*i";

        }

        private void InitializeRulesMatrix()
        {
            // Mátrix inicializálása az adott táblázat adataival
            rulesMatrix = new string[,] {
            //  +       *       (       )       i       #
            { null,   null,   "TE',1", null,   "TE',1", null   },   // E
            { "+TE',2", null,   null,   "e,3",   null,   "e,3"   }, // E'
            { null,   null,   "FT',4", null,   "FT',4", null   },   // T
            { "e,6",  "*FT',5", null,   "e,6",   null,   "e,6"   }, // T'
            { null,   null,   "(E),7", null,   "i,8",   null   },   // F
            { "pop",  null,   null,   null,   null,   null   },     // +
            { null,   "pop",  null,   null,   null,   null   },     // *
            { null,   null,   "pop",  null,   null,   null   },     // (
            { null,   null,   null,   "pop",  null,   null   },     // )
            { null,   null,   null,   null,   "pop",  null   },     // i
            { null,   null,   null,   null,   null,   "elfogad" }   // #
        };
        }

        private void InitializeDataGridView()
        {
            // Oszlopok hozzáadása
            dataGridView1.Columns.Add("+", "+");
            dataGridView1.Columns.Add("*", "*");
            dataGridView1.Columns.Add("(", "(");
            dataGridView1.Columns.Add(")", ")");
            dataGridView1.Columns.Add("i", "i");
            dataGridView1.Columns.Add("#", "#");

            // Az oszlopok fejléceinek beállítása
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                col.HeaderCell.Value = col.Name;
            }

            // A sorfejlécek megadása
            string[] rowHeaders = { "E", "E'", "T", "T'", "F", "+", "*", "(", ")", "i", "#" };

            // A mátrix sorainak hozzáadása a DataGridView-hoz
            for (int row = 0; row < rulesMatrix.GetLength(0); row++)
            {
                // új sor létrehozása a mátrix adataival
                DataGridViewRow newRow = new DataGridViewRow();
                newRow.CreateCells(dataGridView1);

                // sorfejléc beállítása
                newRow.HeaderCell.Value = rowHeaders[row];

                for (int col = 0; col < rulesMatrix.GetLength(1); col++)
                {
                    // cellák kitöltése a mátrix adataival
                    newRow.Cells[col].Value = rulesMatrix[row, col];
                }

                // sor hozzáadása a DataGridView-hoz
                dataGridView1.Rows.Add(newRow);
            }

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView1.RowHeadersWidth = 50;
            dataGridView1.RowHeadersVisible = true;
        }



        private void btnStart_Click(object sender, EventArgs e)
        {
            // alaphelyzetbe állítás
            ResetApplicationState();
            // folyamatosan dolgozza fel az input szalagot, amíg nem ér véget, vagy a verem ki nem ürül
            while (index < inputSzalag.Length && stack.Count > 0 && !hiba)
            {
                
                ProcessInput(); // felddolgozza a jelenlegi input karaktert és a verem tetején lévő elemet

                // megáll, ha a verem kiürült vagy csak a # marad benne
                if (stack.Count == 0 || (stack.Count > 0 && stack.Peek() == "#"))
                {
                    break;
                }
            }

            // ellenőrzi, hogy a veremben csak a '#' maradt-e, ami a helyes feldolgozás jele
            if (stack.Count == 1 && stack.Peek() == "#")
            {
                //MessageBox.Show("A kifejezés helyes.");
            }
            else
            {
                //MessageBox.Show("index == inputTape.Length ??: " + index +"=?"+ inputTape.Length);
                MessageBox.Show("A kifejezés helytelen vagy nem teljes.");
            }
        }

        private void ProcessInput()
        {
            // a jelenlegi input karakter és a verem tetején lévő elem lekérdezése
            string currentInput = inputSzalag[index].ToString();
            string topStack = stack.Peek();

            ApplyRule(currentInput, topStack);
        }

        private void ApplyRule(string input, string stackTop)
        {
            // itt meghatározza a jelenlegi állapotnak megfelelő sor és oszlopindexeket a szabálymátrixban
            int rowIndex = GetRowIndex(stackTop);
            int colIndex = GetColumnIndex(input);

            //MessageBox.Show("Row: " + rowIndex + ", Col: " + colIndex);

            // van-e érvényes szabály vagy null-ra mutat a mátrixban
            if (rulesMatrix[rowIndex, colIndex] == null && !hiba)
            {
                MessageBox.Show("Hiba: nincs szabály. Row: " + rowIndex+1 + ", Col: " + colIndex+1);
                ResetApplicationState();
                hiba = true;
                return;
            }
            if (!hiba)
            { // szabály feldolgozása
                string rule = rulesMatrix[rowIndex, colIndex];
                if (rule.StartsWith("pop"))
                {
                    // ha a sor és oszlop megegyezik akkor törlés és index++
                    stack.Pop();
                    index++;
                }
                else
                {
                    // szabály elemeinek hozzáadása a veremhez
                    string[] ruleParts = rule.Split(',');
                    string ruleToApply = ruleParts[0];
                    string ruleNumber = ruleParts[1];
                    alkalmazottSzabalyok.Add(ruleNumber);

                    stack.Pop(); // eltávolítjuk a verem tetejéről az elemet

                    // add új elemeket a veremhez, figyelembe véve a speciális "E'" és "T'" eseteket
                    if (ruleToApply != "e")
                    {
                        for (int i = ruleToApply.Length - 1; i >= 0; i--)
                        {
                            if (i > 0 && (ruleToApply[i] == '\'' && (ruleToApply[i - 1] == 'E' || ruleToApply[i - 1] == 'T')))
                            {
                                stack.Push(ruleToApply.Substring(i - 1, 2)); // hozzáadom az 'E'' vagy 'T''-t
                                i--; // kihagyjuk a következő karaktert, mivel az már része a fenti stringnek
                            }
                            else
                            {
                                stack.Push(ruleToApply[i].ToString());
                            }
                        }
                    }
                }
                UpdateUI();
            }
            else
            {
                return;
            }
        }

        private int GetRowIndex(string stackTop)
        {
            // sorindexeket határozza meg a mátrixban
            string rowHeaders = "E,E',T,T',F,+,*,(,),i,#";
            string[] rows = rowHeaders.Split(',');
            for (int i = 0; i < rows.Length; i++)
            {
                //MessageBox.Show("rows i: "+rows[i]+"\n"+ "stackTop: " + stackTop);
                if (stackTop == rows[i])
                {
                    return i;
                }
            }
            return -1;
        }

        private int GetColumnIndex(string input)
        {
            // oszlopindexeket határozza meg a mátrixban
            string colHeaders = "+,*,(,),i,#";
            string[] cols = colHeaders.Split(',');
            for (int i = 0; i < cols.Length; i++)
            {
                if (input == cols[i])
                {
                    return i;
                }
            }
            return -1;
        }

        private void ResetApplicationState()
        {
            // alaphelyzetbe állítás
            richTextBox1.Text = "";
            alkalmazottSzabalyok.Clear();
            stack.Clear();
            stack.Push("#");
            stack.Push("E");
            index = 0;
            hiba = false;
            inputSzalag = textBox1.Text + "#";
        }

        private void UpdateUI()
        {
            // kiíratás
            richTextBox1.AppendText(inputSzalag.Substring(index) + ", ");
            richTextBox1.AppendText(string.Join("", stack) + ", ");
            richTextBox1.AppendText(String.Join("", alkalmazottSzabalyok) + "\n");
            if (stack.Count == 1 && stack.Peek() == "#")
            {
                richTextBox1.AppendText("Elfogadva\n");
            }
        }
    }
}
