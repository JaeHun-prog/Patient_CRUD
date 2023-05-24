using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        void Clearing()
        {
            name_box.Clear();
            name_box2.Clear();
            adr_box.Clear();
            phone_box.Clear();
            phone_box2.Clear();
            p_id_box.Clear();
            email_box.Clear();
            pATIENT_DBTableAdapter.Fill(this.patient_DB.PATIENT_DB);
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: 이 코드는 데이터를 'patient_DB.PATIENT_DB' 테이블에 로드합니다. 필요 시 이 코드를 이동하거나 제거할 수 있습니다.
            this.pATIENT_DBTableAdapter.Fill(this.patient_DB.PATIENT_DB);

        }

        private void insert_btn_Click(object sender, EventArgs e)
        {
            string p_name = name_box.Text;
            string p_address = adr_box.Text;
            string p_phone_number = phone_box.Text.Replace("-", ""); // - 삭제
            DateTime birthDate = DateTime.Parse(dateTimePicker1.Value.ToString("yyyy-MM-dd"));
            string email = email_box.Text;
            string sex = "";
            if (radioButton1.Checked == true) { sex = "남성"; }
            else { sex = "여성"; }

            pATIENT_DBTableAdapter.Insert(Convert.ToDecimal(pATIENT_DBTableAdapter.getP_id_SeqVal()), p_name, sex, Convert.ToDecimal(p_phone_number), birthDate, p_address, email);
            Clearing();
        }

        private void update_btn_Click(object sender, EventArgs e)
        {
            string p_name = name_box.Text;
            string p_address = adr_box.Text;
            string p_phone_number = phone_box.Text.Replace("-", ""); // - 삭제
            string email = email_box.Text;
            string sex = "";
            if (radioButton1.Checked == true) { sex = "남성"; } else { sex = "여성"; }
            string currentPId = p_id_box.Text;
            DateTime currentBirthDate = DateTime.Parse(dateTimePicker1.Value.ToString("yyyy-MM-dd"));
            if (MessageBox.Show("환자의 번호와 생년월일은 변동이 없습니다.", "환자 정보 수정", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                pATIENT_DBTableAdapter.UpdateQuery(p_name, p_address, Convert.ToDecimal(p_phone_number), email, sex, Convert.ToDecimal(currentPId), currentBirthDate);
                Clearing();
            }
        }

        private void srh_btn_Click(object sender, EventArgs e)
        {
            // 환자의 번호, 성명, 휴대폰 번호의 text box가 비어있으면 오류 발생
            if (string.IsNullOrEmpty(p_id_box.Text) && string.IsNullOrEmpty(name_box2.Text) && string.IsNullOrEmpty(phone_box2.Text))
            {
                MessageBox.Show("환자의 번호 또는 성명, 전화번호를 기입하세요.", "잘못된 검색", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // select가 아닌 필터링을 사용해서 사용(환자의 전화번호, 성명에 변경사항이 있을 때를 가정하여 사용 가능)
            if (pATIENTDBBindingSource.Filter != null)
            {
                pATIENTDBBindingSource.RemoveFilter();
                srh_btn.Text = "검색";
                Clearing();
            }
            else
            {
                string filterExpression = "";
                if (!string.IsNullOrEmpty(p_id_box.Text))
                {
                    filterExpression = "p_id = '" + p_id_box.Text + "'";
                }
                if (!string.IsNullOrEmpty(name_box2.Text))
                {
                    if (!string.IsNullOrEmpty(filterExpression)) { filterExpression += " OR "; }    // AND를 할 시 정확하지만 환자가 개명을 하고 병원을 방문할 시 검색하기 힘든 조건이 발생함.
                    filterExpression += "p_name LIKE '%" + name_box2.Text + "%'";   // LIKE를 사용해서 환자의 성명을 검색 시 중복되는 성명 출력 ex) 이00, 0재0, 00훈
                }

                if (!string.IsNullOrEmpty(phone_box2.Text))
                {
                    if (!string.IsNullOrEmpty(filterExpression)) { filterExpression += " OR "; }    // 99번째 줄과 동일
                    filterExpression += "phone_number = '" + phone_box2.Text + "'";
                }
                pATIENTDBBindingSource.Filter = filterExpression;
                srh_btn.Text = "검색 해제";     // 검색 해제라는 text가 보이게 해서 사용자가 datagridview가 검색 기능이 종료되지 않았다는 것을 인식시킴
            }
        }

        private void delete_btn_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("삭제하시겠습니까?", "환자 정보 삭제", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                int p_id = Int32.Parse(p_id_box.Text);

                List<int> rowsToDelete = new List<int>();
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (!row.IsNewRow)  
                    {
                        rowsToDelete.Add(row.Index);

                        pATIENT_DBTableAdapter.DeleteQuery(p_id);
                        for (int i = rowsToDelete.Count - 1; i >= 0; i--)
                        {
                            dataGridView1.Rows.RemoveAt(rowsToDelete[i]);
                        }
                        Clearing();
                    }
                }
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)    //모든 셀 클릭 시 이벤트 발생  
        {
            // 체크박스의 사용 가능성도 있지만 환자의 정보를 자세히 알고 있어야 가능할 수 있기 때문에 모든 셀을 클릭 할 때 이벤트가 발생하도록 설정 (환자의 성명, 생년월일이 같을 때 헷갈릴 가능성을 방지)
            DataGridViewRow selectedRow = dataGridView1.Rows[e.RowIndex];

            p_id_box.Text = selectedRow.Cells[0].Value.ToString();
            name_box.Text = selectedRow.Cells[1].Value.ToString();
            name_box2.Text = selectedRow.Cells[1].Value.ToString();
            adr_box.Text = selectedRow.Cells[4].Value.ToString();
            phone_box.Text = selectedRow.Cells[3].Value.ToString();
            phone_box2.Text = selectedRow.Cells[3].Value.ToString();
            email_box.Text = selectedRow.Cells[6].Value.ToString();

            string sex = selectedRow.Cells[2].FormattedValue.ToString();
            if (sex == "남성")
            {
                radioButton1.Checked = true;
                radioButton2.Checked = false;
            }
            else if (sex == "여성")
            {
                radioButton2.Checked = true;
                radioButton1.Checked = false;
            }
            DateTime birthDate = (DateTime)selectedRow.Cells["bIRTHDATEDataGridViewTextBoxColumn"].Value;
            dateTimePicker1.Value = birthDate;
        }
    }
}