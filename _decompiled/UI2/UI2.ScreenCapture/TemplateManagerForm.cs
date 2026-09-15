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

		Button btnUpload = new Button
		{
			Text = "Toplu Görsel Yükle...",
			Dock = DockStyle.Top,
			Height = 34
		};
		btnUpload.Click += BtnUpload_Click;

		_grid.Dock = DockStyle.Fill;
		_grid.AutoGenerateColumns = false;
		_grid.AllowUserToAddRows = false;
		_grid.AllowUserToDeleteRows = false;
		_grid.RowHeadersVisible = false;
		_grid.RowTemplate.Height = 46;

		_grid.Columns.Add(new DataGridViewTextBoxColumn
		{
			Name = "Category",
			HeaderText = "Kategori",
			ReadOnly = true,
			Width = 100
		});
		_grid.Columns.Add(new DataGridViewTextBoxColumn
		{
			Name = "TaskId",
			HeaderText = "Görev",
			ReadOnly = true,
			Width = 230
		});
		DataGridViewComboBoxColumn dataGridViewComboBoxColumn = new DataGridViewComboBoxColumn
		{
			Name = "File",
			HeaderText = "Görsel Dosyası",
			Width = 260,
			FlatStyle = FlatStyle.Flat
		};
		_grid.Columns.Add(dataGridViewComboBoxColumn);
		_grid.Columns.Add(new DataGridViewImageColumn
		{
			Name = "Preview",
			HeaderText = "Önizleme",
			Width = 60,
			ImageLayout = DataGridViewImageCellLayout.Zoom
		});
		_grid.Columns.Add(new DataGridViewButtonColumn
		{
			Name = "Reset",
			HeaderText = "",
			Text = "Varsayılana Dön",
			UseColumnTextForButtonValue = true,
			Width = 120
		});

		Controls.Add(_grid);
		Controls.Add(btnUpload);

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

		foreach (TemplateSlot slot in Slots)
		{
			string current = TemplateResolver.Resolve(slot.TaskId, slot.DefaultPath);
			if (!availableFiles.Contains(current))
			{
				availableFiles.Add(current);
				dataGridViewComboBoxColumn.Items.Add(current);
			}
			int rowIndex = _grid.Rows.Add(slot.Category, slot.TaskId, current, LoadThumbnail(current), "Varsayılana Dön");
			_grid.Rows[rowIndex].Tag = slot;
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
		return null;
	}

	private void Grid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
	{
		if (e.RowIndex < 0 || _grid.Columns[e.ColumnIndex].Name != "File")
		{
			return;
		}
		if (_grid.Rows[e.RowIndex].Tag is not TemplateSlot slot)
		{
			return;
		}
		string newPath = _grid.Rows[e.RowIndex].Cells["File"].Value as string;
		if (string.IsNullOrWhiteSpace(newPath))
		{
			return;
		}
		TemplateResolver.SetOverride(slot.TaskId, newPath);
		_grid.Rows[e.RowIndex].Cells["Preview"].Value = LoadThumbnail(newPath);
	}

	private void Grid_CellClick(object sender, DataGridViewCellEventArgs e)
	{
		if (e.RowIndex < 0 || _grid.Columns[e.ColumnIndex].Name != "Reset")
		{
			return;
		}
		if (_grid.Rows[e.RowIndex].Tag is not TemplateSlot slot)
		{
			return;
		}
		TemplateResolver.ClearOverride(slot.TaskId, slot.DefaultPath);
		_grid.Rows[e.RowIndex].Cells["File"].Value = slot.DefaultPath;
		_grid.Rows[e.RowIndex].Cells["Preview"].Value = LoadThumbnail(slot.DefaultPath);
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
}
