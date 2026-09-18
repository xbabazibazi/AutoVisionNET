using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SettingsManager;

namespace UI2.ScreenCapture;

public class TemplateManagerForm : Form
{
	private class TemplateSlot
	{
		public string Category = string.Empty;
		public string TaskId = string.Empty;
		public string DefaultPath = string.Empty;
	}

	private class TemplateGroup
	{
		public string DisplayName = string.Empty;
		public string DefaultPath = string.Empty;
		public List<TemplateSlot> Slots = new List<TemplateSlot>();
	}

	private static readonly Dictionary<string, string> FriendlyNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		["Images/IceResistance.jpg"] = "Buz Direnci",
		["Images/Undy.jpg"] = "Undy",
		["Images/300Ac.jpg"] = "300 AC",
		["Images/Sw.jpg"] = "SW",
		["Images/Wolf.jpg"] = "Wolf",
		["Images/GenieStart.jpg"] = "Cin (Genie) Başlat",
		["Images/BrokenTomahawk.jpg"] = "Kırık Tomahawk",
		["Images/RepairedTomahawk.jpg"] = "Onarılmış Tomahawk",
		["Images/EmptyInventorySlot.jpg"] = "Boş Envanter Slotu",
		["Images/Event.jpg"] = "Etkinlik",
		["Images/OpenMagicBag.jpg"] = "Sihirli Çanta Aç",
		["Images/CloseMagicBag.jpg"] = "Sihirli Çanta Kapat",
		["Images/SecondMagicBag.jpg"] = "İkinci Sihirli Çanta",
		["Images/Dead.jpg"] = "Ölüm",
		["Images/PartyHeader.jpg"] = "Parti Başlığı",
		["Images/BreakParty.jpg"] = "Partiden Ayrıl",
		["Images/DB.jpg"] = "DB (Cure)",
		["Images/Town.jpg"] = "Şehir",
		["Images/PartyCount.jpg"] = "Parti Sayacı",
		["Images/RequestParty.jpg"] = "Parti İsteği",
		["Images/katadora.jpg"] = "Katadora",
		["Images/RequestPartyMenuItem.jpg"] = "Parti Daveti (Sağ Tık Menüsü)",
		["Images/WhellOfFunButton.jpg"] = "Çark Butonu",
		["Images/WhellOfFunPushButton.jpg"] = "Çark Çevir Butonu",
		["Images/WhellOfFunYesButton.jpg"] = "Çark Onay Butonu",
		["Images/BrokenFullPlateArmorPauldron.jpg"] = "Kırık Zırh",
		["Images/EmptyRightHand.jpg"] = "Boş Sağ El",
		["Images/EmptyLeftHand.jpg"] = "Boş Sol El"
	};

	private static readonly TemplateSlot[] Slots = new (string Category, string TaskId, string DefaultPath)[]
	{
		("BuffLine", "StartGenieAfterTp", "Images/IceResistance.jpg"),
		("BuffLine", "DeleteResistance", "Images/IceResistance.jpg"),
		("BuffLine", "Undy", "Images/Undy.jpg"),
		("BuffLine", "300Ac", "Images/300Ac.jpg"),
		("BuffLine", "Sw", "Images/Sw.jpg"),
		("BuffLine", "Wolf", "Images/Wolf.jpg"),
		("Genie", "GenieStatus", "Images/GenieStart.jpg"),
		("Genie", "StartGenie", "Images/GenieStart.jpg"),
		("Inventory", "FindBrokenTomahawkOnInventory", "Images/BrokenTomahawk.jpg"),
		("Inventory", "CheckRepairedTomahawkOnInventory", "Images/RepairedTomahawk.jpg"),
		("Inventory", "EquipTomahawk", "Images/RepairedTomahawk.jpg"),
		("Inventory", "InventorySlotAlert", "Images/EmptyInventorySlot.jpg"),
		("LeftBotMenu", "Event", "Images/Event.jpg"),
		("MagicBag", "OpenMagicBag", "Images/OpenMagicBag.jpg"),
		("MagicBag", "OpenMagicBag2", "Images/OpenMagicBag.jpg"),
		("MagicBag", "CheckRepairedTomahawkOnFirstMagicBag", "Images/RepairedTomahawk.jpg"),
		("MagicBag", "CheckRepairedTomahawkOnFirstMagicBag2", "Images/RepairedTomahawk.jpg"),
		("MagicBag", "CheckRepairedTomahawkOnSecondMagicBag2", "Images/RepairedTomahawk.jpg"),
		("MagicBag", "CheckRepairedTomahawkOnSecondMagicBag", "Images/RepairedTomahawk.jpg"),
		("MagicBag", "FindRepairedTomahawkOnMagicBag", "Images/RepairedTomahawk.jpg"),
		("MagicBag", "CloseMagicBag", "Images/CloseMagicBag.jpg"),
		("MagicBag", "OpenSecondMagicBag", "Images/SecondMagicBag.jpg"),
		("Party", "HandlePartyMemberDeath", "Images/Dead.jpg"),
		("Party", "PartyHeader", "Images/PartyHeader.jpg"),
		("Party", "BreakParty", "Images/BreakParty.jpg"),
		("Party", "CureDB", "Images/DB.jpg"),
		("Party", "Town", "Images/Town.jpg"),
		("Party", "PartyMemberCount", "Images/PartyCount.jpg"),
		("Request", "RequestParty", "Images/RequestParty.jpg"),
		("Request", "katadora", "Images/katadora.jpg"),
		("Request", "SendPartyInviteMenuItem", "Images/RequestPartyMenuItem.jpg"),
		("Request", "CheckParty", "Images/RequestParty.jpg"),
		("Request", "WhellOfFunButton", "Images/WhellOfFunButton.jpg"),
		("Request", "WhellOfFunPushButton", "Images/WhellOfFunPushButton.jpg"),
		("Request", "WhellOfFunYesButton", "Images/WhellOfFunYesButton.jpg"),
		("SnapNet", "SnapNetReReRe", "Images/GenieStart.jpg"),
		("SnapNet", "SnapNetStartGenie", "Images/GenieStart.jpg"),
		("SnapNet", "SnapNetWhellOfFun", "Images/GenieStart.jpg"),
		("Weapons", "RepairArmors", "Images/BrokenFullPlateArmorPauldron.jpg"),
		("Weapons", "RepairWeapons", "Images/BrokenTomahawk.jpg"),
		("Weapons", "SwapTomahawk", "Images/BrokenTomahawk.jpg"),
		("Weapons", "CheckRightHandIsEmpty", "Images/EmptyRightHand.jpg"),
		("Weapons", "CheckLeftHandIsEmpty", "Images/EmptyLeftHand.jpg")
	}.Select(t => new TemplateSlot { Category = t.Category, TaskId = t.TaskId, DefaultPath = t.DefaultPath }).ToArray();

	private static IEnumerable<TemplateGroup> BuildGroups()
	{
		Dictionary<string, TemplateGroup> byPath = new Dictionary<string, TemplateGroup>(StringComparer.OrdinalIgnoreCase);
		List<TemplateGroup> ordered = new List<TemplateGroup>();
		foreach (TemplateSlot slot in Slots)
		{
			if (!byPath.TryGetValue(slot.DefaultPath, out TemplateGroup group))
			{
				group = new TemplateGroup
				{
					DefaultPath = slot.DefaultPath,
					DisplayName = FriendlyNames.TryGetValue(slot.DefaultPath, out string friendly) ? friendly : Path.GetFileNameWithoutExtension(slot.DefaultPath)
				};
				byPath[slot.DefaultPath] = group;
				ordered.Add(group);
			}
			group.Slots.Add(slot);
		}
		return ordered;
	}

	private readonly DataGridView _grid = new DataGridView();

	private readonly string _templatesFolder;

	public TemplateManagerForm()
	{
		_templatesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
		Directory.CreateDirectory(_templatesFolder);

		Text = "Şablon Yöneticisi";
		Size = new Size(820, 480);
		StartPosition = FormStartPosition.Manual;
		ShowInTaskbar = false;
		FormBorderStyle = FormBorderStyle.None;
		BackColor = Color.FromArgb(28, 28, 33);
		ForeColor = Color.FromArgb(235, 235, 240);
		Font = new Font("Segoe UI", 8f);

		Panel headerPanel = new Panel
		{
			Dock = DockStyle.Top,
			Height = 30,
			BackColor = Color.FromArgb(38, 38, 45)
		};
		Label lblTitle = new Label
		{
			Text = "Şablon Yöneticisi",
			ForeColor = Color.FromArgb(235, 235, 240),
			Font = AppFonts.Header(13f),
			AutoSize = true,
			Location = new Point(10, 7)
		};
		Button btnClose = new Button
		{
			Text = "✕",
			FlatStyle = FlatStyle.Flat,
			ForeColor = Color.FromArgb(235, 235, 240),
			BackColor = Color.Transparent,
			Font = new Font("Segoe UI", 9f, FontStyle.Bold),
			Size = new Size(20, 20),
			Anchor = AnchorStyles.Top | AnchorStyles.Right,
			Location = new Point(headerPanel.Width - 30, 5)
		};
		btnClose.FlatAppearance.BorderSize = 0;
		btnClose.Click += delegate { Hide(); };
		btnClose.MouseEnter += delegate { btnClose.BackColor = Color.FromArgb(90, 50, 50); };
		btnClose.MouseLeave += delegate { btnClose.BackColor = Color.Transparent; };
		headerPanel.Controls.Add(lblTitle);
		headerPanel.Controls.Add(btnClose);

		Panel bodyPanel = new Panel
		{
			Dock = DockStyle.Fill,
			BackColor = Color.FromArgb(28, 28, 33)
		};

		Button btnUpload = new Button
		{
			Text = "Toplu Görsel Yükle...",
			Size = new Size(160, 22),
			Location = new Point(headerPanel.Width - 200, 4),
			Anchor = AnchorStyles.Top | AnchorStyles.Right,
			Font = new Font("Segoe UI", 8f, FontStyle.Bold),
			FlatStyle = FlatStyle.Flat,
			BackColor = Color.FromArgb(55, 78, 92),
			ForeColor = Color.White,
			UseVisualStyleBackColor = false
		};
		btnUpload.FlatAppearance.BorderSize = 0;
		btnUpload.Click += BtnUpload_Click;
		headerPanel.Controls.Add(btnUpload);

		Button btnCaptureFromScreen = new Button
		{
			Text = "Ekrandan Kes...",
			Size = new Size(130, 22),
			Location = new Point(headerPanel.Width - 340, 4),
			Anchor = AnchorStyles.Top | AnchorStyles.Right,
			Font = new Font("Segoe UI", 8f, FontStyle.Bold),
			FlatStyle = FlatStyle.Flat,
			BackColor = Color.FromArgb(60, 65, 80),
			ForeColor = Color.White,
			UseVisualStyleBackColor = false
		};
		btnCaptureFromScreen.FlatAppearance.BorderSize = 0;
		btnCaptureFromScreen.Click += BtnCaptureFromScreen_Click;
		headerPanel.Controls.Add(btnCaptureFromScreen);

		_grid.Dock = DockStyle.Fill;
		_grid.BackgroundColor = Color.FromArgb(28, 28, 33);
		_grid.BorderStyle = BorderStyle.None;
		_grid.GridColor = Color.FromArgb(50, 50, 58);
		_grid.EnableHeadersVisualStyles = false;
		_grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(38, 38, 45);
		_grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(235, 235, 240);
		_grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
		_grid.DefaultCellStyle.BackColor = Color.FromArgb(38, 38, 45);
		_grid.DefaultCellStyle.ForeColor = Color.FromArgb(235, 235, 240);
		_grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(55, 78, 92);
		_grid.DefaultCellStyle.SelectionForeColor = Color.White;
		_grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(33, 33, 40);
		_grid.AlternatingRowsDefaultCellStyle.ForeColor = Color.FromArgb(235, 235, 240);
		_grid.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(38, 38, 45);
		_grid.AutoGenerateColumns = false;
		_grid.AllowUserToAddRows = false;
		_grid.AllowUserToDeleteRows = false;
		_grid.RowHeadersVisible = false;
		_grid.RowTemplate.Height = 46;

		_grid.Columns.Add(new DataGridViewTextBoxColumn
		{
			Name = "Name",
			HeaderText = "Ad",
			ReadOnly = true,
			Width = 220
		});
		DataGridViewComboBoxColumn dataGridViewComboBoxColumn = new DataGridViewComboBoxColumn
		{
			Name = "File",
			HeaderText = "Görsel Dosyası",
			Width = 340,
			FlatStyle = FlatStyle.Flat
		};
		_grid.Columns.Add(dataGridViewComboBoxColumn);
		_grid.Columns.Add(new DataGridViewImageColumn
		{
			Name = "Preview",
			HeaderText = "Önizleme",
			Width = 70,
			ImageLayout = DataGridViewImageCellLayout.Zoom
		});
		_grid.Columns.Add(new DataGridViewButtonColumn
		{
			Name = "Reset",
			HeaderText = "Sıfırla",
			Text = "Varsayılana Dön",
			UseColumnTextForButtonValue = true,
			Width = 140
		});

		bodyPanel.Controls.Add(_grid);
		Controls.Add(bodyPanel);
		Controls.Add(headerPanel);

		LoadRows();

		_grid.CellValueChanged += Grid_CellValueChanged;
		_grid.CurrentCellDirtyStateChanged += delegate
		{
			if (_grid.IsCurrentCellDirty && _grid.CurrentCell is DataGridViewComboBoxCell)
			{
				_grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
			}
		};
		_grid.CellClick += Grid_CellClick;
	}

	private IEnumerable<string> GetAvailableFiles()
	{
		string imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
		List<string> list = new List<string>();
		if (Directory.Exists(_templatesFolder))
		{
			list.AddRange(Directory.GetFiles(_templatesFolder).Select((string f) => "Templates/" + Path.GetFileName(f)));
		}
		if (Directory.Exists(imagesFolder))
		{
			list.AddRange(Directory.GetFiles(imagesFolder).Select((string f) => "Images/" + Path.GetFileName(f)));
		}
		return list.Distinct().OrderBy((string f) => f);
	}

	private void LoadRows()
	{
		_grid.Rows.Clear();
		List<string> availableFiles = GetAvailableFiles().ToList();
		DataGridViewComboBoxColumn dataGridViewComboBoxColumn = (DataGridViewComboBoxColumn)_grid.Columns["File"];
		dataGridViewComboBoxColumn.Items.Clear();
		dataGridViewComboBoxColumn.Items.AddRange(availableFiles.Cast<object>().ToArray());

		foreach (TemplateGroup group in BuildGroups())
		{
			string current = TemplateResolver.Resolve(group.Slots[0].TaskId, group.DefaultPath);
			if (!availableFiles.Contains(current))
			{
				availableFiles.Add(current);
				dataGridViewComboBoxColumn.Items.Add(current);
			}
			int rowIndex = _grid.Rows.Add(group.DisplayName, current, LoadThumbnail(current), "Varsayılana Dön");
			_grid.Rows[rowIndex].Tag = group;
			if (group.Slots.Count > 1)
			{
				_grid.Rows[rowIndex].Cells["Name"].ToolTipText = "Etkilenen görevler: " + string.Join(", ", group.Slots.Select((TemplateSlot s) => s.TaskId));
			}
		}
	}

	private static Image LoadThumbnail(string relativePath)
	{
		try
		{
			string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
			if (File.Exists(fullPath))
			{
				using Image image = Image.FromFile(fullPath);
				return new Bitmap(image, new Size(40, 40));
			}
		}
		catch
		{
		}
		return CreateMissingThumbnailPlaceholder();
	}

	private static Image CreateMissingThumbnailPlaceholder()
	{
		Bitmap bitmap = new Bitmap(40, 40);
		using (Graphics graphics = Graphics.FromImage(bitmap))
		{
			graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			using (SolidBrush brush = new SolidBrush(Color.FromArgb(48, 48, 55)))
			{
				graphics.FillRectangle(brush, 0, 0, 40, 40);
			}
			using (Pen pen = new Pen(Color.FromArgb(90, 95, 110), 1.5f))
			{
				graphics.DrawLine(pen, 12, 12, 28, 28);
				graphics.DrawLine(pen, 28, 12, 12, 28);
			}
		}
		return bitmap;
	}

	private void Grid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
	{
		if (e.RowIndex < 0 || _grid.Columns[e.ColumnIndex].Name != "File")
		{
			return;
		}
		if (_grid.Rows[e.RowIndex].Tag is not TemplateGroup group)
		{
			return;
		}
		string newPath = _grid.Rows[e.RowIndex].Cells["File"].Value as string;
		if (string.IsNullOrWhiteSpace(newPath))
		{
			return;
		}
		foreach (TemplateSlot slot in group.Slots)
		{
			TemplateResolver.SetOverride(slot.TaskId, newPath);
		}
		_grid.Rows[e.RowIndex].Cells["Preview"].Value = LoadThumbnail(newPath);
	}

	private void Grid_CellClick(object sender, DataGridViewCellEventArgs e)
	{
		if (e.RowIndex < 0 || _grid.Columns[e.ColumnIndex].Name != "Reset")
		{
			return;
		}
		if (_grid.Rows[e.RowIndex].Tag is not TemplateGroup group)
		{
			return;
		}
		foreach (TemplateSlot slot in group.Slots)
		{
			TemplateResolver.ClearOverride(slot.TaskId, group.DefaultPath);
		}
		_grid.Rows[e.RowIndex].Cells["File"].Value = group.DefaultPath;
		_grid.Rows[e.RowIndex].Cells["Preview"].Value = LoadThumbnail(group.DefaultPath);
	}

	private void BtnUpload_Click(object sender, EventArgs e)
	{
		using OpenFileDialog openFileDialog = new OpenFileDialog
		{
			Multiselect = true,
			Filter = "Görsel Dosyaları|*.jpg;*.jpeg;*.png;*.bmp",
			Title = "Şablon görsellerini seçin"
		};
		if (openFileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		int copied = 0;
		foreach (string sourceFile in openFileDialog.FileNames)
		{
			try
			{
				string destFileName = Path.GetFileName(sourceFile);
				string destPath = Path.Combine(_templatesFolder, destFileName);
				int suffix = 1;
				while (File.Exists(destPath))
				{
					destFileName = Path.GetFileNameWithoutExtension(sourceFile) + "_" + suffix + Path.GetExtension(sourceFile);
					destPath = Path.Combine(_templatesFolder, destFileName);
					suffix++;
				}
				File.Copy(sourceFile, destPath);
				copied++;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Kopyalanamadı: " + Path.GetFileName(sourceFile) + "\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}
		LoadRows();
		if (copied > 0)
		{
			MessageBox.Show($"{copied} görsel yüklendi. Şimdi listeden ilgili göreve atayabilirsiniz.", "Yükleme tamamlandı", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
	}

	private void BtnCaptureFromScreen_Click(object sender, EventArgs e)
	{
		List<Form> hiddenForms = new List<Form>();
		try
		{
			foreach (Form openForm in Application.OpenForms.Cast<Form>().ToList())
			{
				if (openForm.Visible)
				{
					hiddenForms.Add(openForm);
					openForm.Hide();
				}
			}
			Rectangle selectedArea;
			using (FullScreenSnipForm snipForm = new FullScreenSnipForm())
			{
				if (snipForm.ShowDialog() != DialogResult.OK || snipForm.SelectedArea.IsEmpty)
				{
					return;
				}
				selectedArea = snipForm.SelectedArea;
			}
			using Bitmap capturedBitmap = new Bitmap(selectedArea.Width, selectedArea.Height);
			using (Graphics graphics = Graphics.FromImage(capturedBitmap))
			{
				graphics.CopyFromScreen(selectedArea.X, selectedArea.Y, 0, 0, selectedArea.Size);
			}
			string fileName = PromptForFileName();
			if (string.IsNullOrWhiteSpace(fileName))
			{
				return;
			}
			foreach (char invalidChar in Path.GetInvalidFileNameChars())
			{
				fileName = fileName.Replace(invalidChar, '_');
			}
			string destPath = Path.Combine(_templatesFolder, fileName + ".jpg");
			int suffix = 1;
			while (File.Exists(destPath))
			{
				destPath = Path.Combine(_templatesFolder, fileName + "_" + suffix + ".jpg");
				suffix++;
			}
			capturedBitmap.Save(destPath, System.Drawing.Imaging.ImageFormat.Jpeg);
			LoadRows();
			MessageBox.Show("Kesit kaydedildi: " + Path.GetFileName(destPath) + "\nŞimdi listeden ilgili göreve atayabilirsiniz.", "Kesit alındı", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
		catch (Exception ex)
		{
			MessageBox.Show("Ekrandan kesit alınırken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}
		finally
		{
			foreach (Form hiddenForm in hiddenForms)
			{
				try
				{
					hiddenForm.Show();
					hiddenForm.BringToFront();
				}
				catch
				{
				}
			}
		}
	}

	private string PromptForFileName()
	{
		using Form prompt = new Form
		{
			FormBorderStyle = FormBorderStyle.FixedDialog,
			StartPosition = FormStartPosition.CenterScreen,
			ClientSize = new Size(360, 140),
			BackColor = Color.FromArgb(28, 28, 33),
			ForeColor = Color.FromArgb(235, 235, 240),
			Font = new Font("Segoe UI", 9f),
			Text = "Kesit Adı",
			MinimizeBox = false,
			MaximizeBox = false,
			ShowIcon = false
		};
		Label label = new Label
		{
			Text = "Bu kesit için bir isim girin:",
			Location = new Point(16, 16),
			AutoSize = true
		};
		TextBox textBox = new TextBox
		{
			Location = new Point(16, 44),
			Size = new Size(328, 24),
			BackColor = Color.FromArgb(48, 48, 55),
			ForeColor = Color.White,
			BorderStyle = BorderStyle.FixedSingle,
			Text = "Kesit_" + DateTime.Now.ToString("yyyyMMdd_HHmmss")
		};
		Button okButton = new Button
		{
			Text = "Kaydet",
			DialogResult = DialogResult.OK,
			Location = new Point(188, 84),
			Size = new Size(75, 30),
			BackColor = Color.FromArgb(55, 78, 92),
			ForeColor = Color.White,
			FlatStyle = FlatStyle.Flat
		};
		okButton.FlatAppearance.BorderSize = 0;
		Button cancelButton = new Button
		{
			Text = "İptal",
			DialogResult = DialogResult.Cancel,
			Location = new Point(269, 84),
			Size = new Size(75, 30),
			BackColor = Color.FromArgb(60, 60, 65),
			ForeColor = Color.White,
			FlatStyle = FlatStyle.Flat
		};
		cancelButton.FlatAppearance.BorderSize = 0;
		prompt.Controls.Add(label);
		prompt.Controls.Add(textBox);
		prompt.Controls.Add(okButton);
		prompt.Controls.Add(cancelButton);
		prompt.AcceptButton = okButton;
		prompt.CancelButton = cancelButton;
		textBox.SelectAll();
		return prompt.ShowDialog(this) == DialogResult.OK ? textBox.Text.Trim() : null;
	}
}
