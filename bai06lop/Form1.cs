using bai06lop;
using BUS;
using DAL.Entities;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace bai06lop
{
    public partial class Form1 : Form
    {
        private readonly StudentService studentService = new StudentService();
        private readonly FacultyService facultyService = new FacultyService();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                setGridViewStyle(dataGridView1);
                var listFacultys = facultyService.GetAll();
                var listStudents = studentService.GetAll();
                FillFalcultyCombobox(listFacultys);
                BindGrid(listStudents);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void FillFalcultyCombobox(List<Faculty> listFacutys)
        {
            listFacutys.Insert(0, new Faculty());
            this.cmbKhoa.DataSource = listFacutys;
            this.cmbKhoa.DisplayMember = "FacultyName";
            this.cmbKhoa.ValueMember = "FacultyID";
        }
        private void BindGrid(List<Student> listStudent)
        {
            dataGridView1.Rows.Clear();
            foreach (var item in listStudent)
            {
                int index = dataGridView1.Rows.Add();
                dataGridView1.Rows[index].Cells[0].Value = item.StudentID;
                dataGridView1.Rows[index].Cells[1].Value = item.FullName;
                if (item.Faculty != null)
                    dataGridView1.Rows[index].Cells[2].Value = item.Faculty.FacultyName;
                dataGridView1.Rows[index].Cells[3].Value = item.AverageScore.ToString("F2");

                if (item.MajorID != null)
                    dataGridView1.Rows[index].Cells[4].Value = item.Major.Name;
                ShowAvatar(item.Avatar);
            }
        }
        private void ShowAvatar(string ImageName)
        {
            if (string.IsNullOrEmpty(ImageName))
                picAvatar.Image = null;
            else
            {
                string parentDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
                string imagePath = Path.Combine(parentDirectory, "Images", ImageName);
                if (File.Exists(imagePath))
                {
                    picAvatar.Image = Image.FromFile(imagePath);
                    picAvatar.Refresh();
                }
                else
                {
                    picAvatar.Image = null; // Nếu không tìm thấy hình ảnh
                }
            }
        }

        public void setGridViewStyle(DataGridView dgv)
        {
            dgv.BorderStyle = BorderStyle.None;
            dgv.DefaultCellStyle.SelectionBackColor = Color.DarkTurquoise;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.BackgroundColor = Color.White;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }
        private void ResetForm()
        {
            txtMSSV.Clear();
            txtHoten.Clear();
            txtDiemTB.Clear();
            cmbKhoa.SelectedIndex = 0; // Reset về "Chọn khoa"
            picAvatar.Image = null;
        }

        private void LoadData()
        {
            try
            {
                var listFaculties = facultyService.GetAll();
                var listStudents = studentService.GetAll();
                FillFalcultyCombobox(listFaculties);
                BindGrid(listStudents);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Đảm bảo người dùng không click vào tiêu đề cột
            {
                int selectrow = e.RowIndex;
                txtMSSV.Text = dataGridView1.Rows[selectrow].Cells["Column1"].Value.ToString();
                txtHoten.Text = dataGridView1.Rows[selectrow].Cells["Column2"].Value.ToString();
                cmbKhoa.Text = dataGridView1.Rows[selectrow].Cells["Column3"].Value.ToString();
                txtDiemTB.Text = dataGridView1.Rows[selectrow].Cells["Column4"].Value.ToString();

                string studentId = txtMSSV.Text;
                string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

                foreach (string extension in imageExtensions)
                {
                    string imagePath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, "Images", studentId + extension);
                    if (File.Exists(imagePath))
                    {
                        ShowAvatar(studentId + extension);
                        return;
                    }
                }
                ShowAvatar(null);
            }
        }

        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {
            var listStudents = new List<Student>();

            if (this.checkBox1.Checked)
                listStudents = studentService.GetAllHasNoMajor();
            else
                listStudents = studentService.GetAll();

            BindGrid(listStudents);

        }

        private void btnDelete_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMSSV.Text))
            {
                MessageBox.Show("Vui lòng nhập mã số sinh viên cần xóa!");
                return;
            }

            using (var context = new Model1())
            {
                var studentToDelete = context.Students.FirstOrDefault(s => s.StudentID == txtMSSV.Text);
                if (studentToDelete == null)
                {
                    MessageBox.Show("Không tìm thấy MSSV cần xóa!");
                    return;
                }

                var confirmResult = MessageBox.Show("Bạn có chắc chắn muốn xóa sinh viên này không?", "Xác nhận xóa!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirmResult == DialogResult.Yes)
                {
                    try
                    {
                        context.Students.Remove(studentToDelete);
                        context.SaveChanges();
                        LoadData();
                        ResetForm();
                        MessageBox.Show("Xóa sinh viên thành công!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi xóa dữ liệu: " + ex.Message);
                    }
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMSSV.Text) || string.IsNullOrWhiteSpace(txtHoten.Text) || string.IsNullOrWhiteSpace(txtDiemTB.Text) || cmbKhoa.SelectedIndex == 0)
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            if (txtMSSV.Text.Length != 10)
            {
                MessageBox.Show("Mã số sinh viên phải có 10 kí tự!");
                return;
            }

            if (!float.TryParse(txtDiemTB.Text, out float averageScore) || averageScore < 0 || averageScore > 10)
            {
                MessageBox.Show("Điểm trung bình phải là số từ 0 đến 10!");
                return;
            }

            using (var context = new Model1())
            {
                var existingStudent = context.Students.FirstOrDefault(s => s.StudentID == txtMSSV.Text);
                if (existingStudent != null)
                {
                    MessageBox.Show("Mã số sinh viên đã tồn tại!");
                    return;
                }

                Student newStudent = new Student
                {
                    StudentID = txtMSSV.Text,
                    FullName = txtHoten.Text,
                    AverageScore = averageScore,
                    FacultyID = (int)cmbKhoa.SelectedValue
                };

                try
                {
                    context.Students.Add(newStudent);
                    context.SaveChanges();
                    LoadData();
                    ResetForm();
                    MessageBox.Show("Thêm mới sinh viên thành công!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi thêm dữ liệu: " + ex.Message);
                }
            }
        }

        private void btnPic_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*"
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                picAvatar.Image = Image.FromFile(dlg.FileName);
                string studentID = txtMSSV.Text;
                string filePath = dlg.FileName;
                string parentDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
                string imagesDirectory = Path.Combine(parentDirectory, "Images");
                string extension = Path.GetExtension(filePath);
                string imageName = $"{studentID}{extension}";
                string destinationPath = Path.Combine(imagesDirectory, imageName);

                File.Copy(filePath, destinationPath, true);
            }
        }
    }

}


