using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FluxDB.Models;
using SettingsManager;
using SettingsManager.ScreenCapture;
using UI2.Interfaces;

namespace UI2.ScreenCapture;

public class SettingsForm : Form, ISettingsForm
{
	private readonly RectanglesSettings _settings = Settings.Instance.ScreenCapture.RectanglesSettings;

	private readonly List<AreaDefinition> _areaDefinitions = new List<AreaDefinition>();

	private IContainer components = null;

	private Label lblTitle;

	private Panel pnlContainer;

	private Label labelParty;

	private Label labelTown;

	private Label labelMagicBag;

	private Button buttonParty;

	private Button buttonTown;

	private Label labelAcceptPartyCoordinates;

	private Label labelInventory;

	private Button buttonMagicBag;

	private Label labelBuffLine;

	private Label labelStopMacrosWhenGenieStoppedCoordinates;

	private Label labelChatWindowCoordinates;

	private Button buttonBuffLine;

	private Label labelWeapons;

	private Button buttonStopMacrosWhenGenieStopped;

	private Button buttonInventory;

	private Button buttonChatWindowReSaveCoordinates;

	private Button buttonAcceptPartySaveCoordinates;

	private Button buttonWeapons;

	private Button buttonInfo;

	private Label labelInfo;

	private Button btnHide;

	private Label labelLeftBotMenu;

	private Button buttonLeftBotMenu;

	private ComboBox cmbResolutionProfile;

	public SettingsForm()
	{
		InitializeComponent();
		InitializeAreaDefinitions();
		InitializeSettings();
		WireUpAllEvents();
		LoadAllSettings();
		InitializeResolutionProfileBar();
	}

	private void InitializeResolutionProfileBar()
	{
		Panel pnlProfile = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = 40,
			BorderStyle = BorderStyle.FixedSingle
		};
		Label lbl = new Label
		{
			Text = "Çözünürlük Profili:",
			AutoSize = true,
			Location = new Point(8, 12)
		};
		cmbResolutionProfile = new ComboBox
		{
			Location = new Point(140, 8),
			Width = 160,
			DropDownStyle = ComboBoxStyle.DropDownList
		};
		Button btnLoad = new Button
		{
			Text = "Yükle",
			Location = new Point(310, 6),
			Width = 70
		};
		btnLoad.Click += BtnLoadProfile_Click;
		Button btnSave = new Button
		{
			Text = "Farklı Kaydet...",
			Location = new Point(385, 6),
			Width = 110
		};
		btnSave.Click += BtnSaveProfile_Click;
		Button btnDelete = new Button
		{
			Text = "Sil",
			Location = new Point(500, 6),
			Width = 60
		};
		btnDelete.Click += BtnDeleteProfile_Click;
		pnlProfile.Controls.Add(lbl);
		pnlProfile.Controls.Add(cmbResolutionProfile);
		pnlProfile.Controls.Add(btnLoad);
		pnlProfile.Controls.Add(btnSave);
		pnlProfile.Controls.Add(btnDelete);
		Controls.Add(pnlProfile);
		base.ClientSize = new Size(base.ClientSize.Width, base.ClientSize.Height + 40);
		RefreshProfileList();
	}

	private void RefreshProfileList()
	{
		string active = _settings.ActiveProfileName;
		cmbResolutionProfile.Items.Clear();
		foreach (string profile in _settings.ListProfiles())
		{
			cmbResolutionProfile.Items.Add(profile);
		}
		if (!string.IsNullOrEmpty(active) && cmbResolutionProfile.Items.Contains(active))
		{
			cmbResolutionProfile.SelectedItem = active;
		}
	}

	private void BtnLoadProfile_Click(object sender, EventArgs e)
	{
		if (cmbResolutionProfile.SelectedItem is not string profileName)
		{
			MessageBox.Show("Önce bir profil seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			return;
		}
		if (_settings.LoadProfile(profileName))
		{
			LoadAllSettings();
			MessageBox.Show("'" + profileName + "' profili uygulandı.", "Tamam", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
		else
		{
			MessageBox.Show("Bu profilde kayıtlı ayar bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}
	}

	private void BtnSaveProfile_Click(object sender, EventArgs e)
	{
		using SimpleTextPromptForm prompt = new SimpleTextPromptForm("Profil Kaydet", "Profil adı (örn. 1920x1080):", cmbResolutionProfile.Text);
		if (prompt.ShowDialog() != DialogResult.OK || string.IsNullOrWhiteSpace(prompt.ResultText))
		{
			return;
		}
		string profileName = prompt.ResultText.Trim();
		_settings.SaveAsProfile(profileName);
		RefreshProfileList();
		cmbResolutionProfile.SelectedItem = profileName;
		MessageBox.Show("Mevcut ayarlar '" + profileName + "' profili olarak kaydedildi.", "Tamam", MessageBoxButtons.OK, MessageBoxIcon.Information);
	}

	private void BtnDeleteProfile_Click(object sender, EventArgs e)
	{
		if (cmbResolutionProfile.SelectedItem is not string profileName)
		{
			return;
		}
		if (MessageBox.Show("'" + profileName + "' profili silinsin mi?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
		{
			_settings.DeleteProfile(profileName);
			RefreshProfileList();
		}
	}

	private void InitializeAreaDefinitions()
	{
		_areaDefinitions.Add(new AreaDefinition("AcceptParty", buttonAcceptPartySaveCoordinates, labelAcceptPartyCoordinates, "Parti Kabul Onayı"));
		_areaDefinitions.Add(new AreaDefinition("Genie", buttonStopMacrosWhenGenieStopped, labelStopMacrosWhenGenieStoppedCoordinates, "Genie Durdurma Kontrolü"));
		_areaDefinitions.Add(new AreaDefinition("ChatWindow", buttonChatWindowReSaveCoordinates, labelChatWindowCoordinates, "Chat Window"));
		_areaDefinitions.Add(new AreaDefinition("BuffLine", buttonBuffLine, labelBuffLine, "Buff Line"));
		_areaDefinitions.Add(new AreaDefinition("Weapons", buttonWeapons, labelWeapons, "Silahlar ve Zırhlar"));
		_areaDefinitions.Add(new AreaDefinition("Inventory", buttonInventory, labelInventory, "Envanter"));
		_areaDefinitions.Add(new AreaDefinition("MagicBag", buttonMagicBag, labelMagicBag, "Magic Bag"));
		_areaDefinitions.Add(new AreaDefinition("Town", buttonTown, labelTown, "Town"));
		_areaDefinitions.Add(new AreaDefinition("Party", buttonParty, labelParty, "Parti Üyeleri"));
		_areaDefinitions.Add(new AreaDefinition("Info", buttonInfo, labelInfo, "Info"));
		_areaDefinitions.Add(new AreaDefinition("LeftBotMenu", buttonLeftBotMenu, labelLeftBotMenu, "Left Bot Menu"));
	}

	private void WireUpAllEvents()
	{
		foreach (AreaDefinition areaDefinition in _areaDefinitions)
		{
			areaDefinition.Button.Tag = areaDefinition;
			areaDefinition.Button.Click += SaveCoordinates_Click;
			areaDefinition.Label.Click += ShowArea_Click;
		}
	}

	private void LoadAllSettings()
	{
		foreach (AreaDefinition areaDefinition in _areaDefinitions)
		{
			LoadSetting(areaDefinition);
		}
	}

	private AreaDefinition GetAreaDefinitionForLabel(Label label)
	{
		return _areaDefinitions.Find((AreaDefinition a) => a.Label == label);
	}

	private AreaDefinition GetAreaDefinitionForSettingName(string settingName)
	{
		return _areaDefinitions.Find((AreaDefinition a) => a.SettingName == settingName);
	}

	private RectangleSettings GetSettingsForArea(AreaDefinition area)
	{
		return _settings.GetType().GetProperty(area.SettingName)?.GetValue(_settings) as RectangleSettings;
	}

	private void UpdateSettingsForArea(AreaDefinition area, RectangleSettings newSettings)
	{
		_settings.GetType().GetProperty(area.SettingName)?.SetValue(_settings, newSettings);
	}

	private void InitializeSettings()
	{
		base.StartPosition = FormStartPosition.Manual;
	}

	private void ShowArea_Click(object sender, EventArgs e)
	{
		if (!(sender is Label label))
		{
			return;
		}
		AreaDefinition areaDefinitionForLabel = GetAreaDefinitionForLabel(label);
		if (areaDefinitionForLabel != null)
		{
			RectangleSettings settingsForArea = GetSettingsForArea(areaDefinitionForLabel);
			if (settingsForArea != null)
			{
				Rectangle targetRect = new Rectangle(settingsForArea.CoordinateX, settingsForArea.CoordinateY, settingsForArea.Width, settingsForArea.Height);
				new RectangleOverlayForm(targetRect, areaDefinitionForLabel.DisplayName).Show();
			}
		}
	}

	private void BtnHide_Click(object sender, EventArgs e)
	{
		Hide();
	}

	private void SaveCoordinates_Click(object sender, EventArgs e)
	{
		if (sender is Button { Tag: AreaDefinition tag })
		{
			OpenSnipForm(tag);
		}
	}

	private void OpenSnipForm(AreaDefinition area)
	{
		List<Form> list = new List<Form>();
		try
		{
			foreach (Form openForm in Application.OpenForms)
			{
				if (openForm.Visible && openForm != this)
				{
					list.Add(openForm);
					openForm.Hide();
				}
			}
			Hide();
			using FullScreenSnipForm fullScreenSnipForm = new FullScreenSnipForm();
			if (fullScreenSnipForm.ShowDialog() == DialogResult.OK && !fullScreenSnipForm.SelectedArea.IsEmpty)
			{
				RectangleSettings settingsForArea = GetSettingsForArea(area);
				if (settingsForArea != null)
				{
					UpdateSetting(settingsForArea, fullScreenSnipForm.SelectedArea);
					UpdateLabel(area.Label, settingsForArea);
					UpdateSettingsForArea(area, settingsForArea);
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("Bölge seçilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		finally
		{
			Show();
			foreach (Form item in list)
			{
				try
				{
					item.Show();
					item.BringToFront();
				}
				catch
				{
				}
			}
		}
	}

	private void UpdateSetting(RectangleSettings setting, Rectangle area)
	{
		setting.CoordinateX = area.X;
		setting.CoordinateY = area.Y;
		setting.Width = area.Width;
		setting.Height = area.Height;
	}

	private void UpdateLabel(Label label, RectangleSettings setting)
	{
		label.InvokeIfRequired(delegate(Label l)
		{
			l.Text = $"({setting.CoordinateX},{setting.CoordinateY})-({setting.Width}x{setting.Height})";
		});
	}

	private void LoadSetting(AreaDefinition area)
	{
		RectangleSettings settingsForArea = GetSettingsForArea(area);
		if (settingsForArea != null)
		{
			area.Label.Text = $"({settingsForArea.CoordinateX},{settingsForArea.CoordinateY})-({settingsForArea.Width}x{settingsForArea.Height})";
		}
	}

	public Form GetForm()
	{
		return this;
	}

	public void GetHide()
	{
		Hide();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.lblTitle = new System.Windows.Forms.Label();
		this.pnlContainer = new System.Windows.Forms.Panel();
		this.labelLeftBotMenu = new System.Windows.Forms.Label();
		this.buttonLeftBotMenu = new System.Windows.Forms.Button();
		this.labelParty = new System.Windows.Forms.Label();
		this.labelTown = new System.Windows.Forms.Label();
		this.labelMagicBag = new System.Windows.Forms.Label();
		this.buttonParty = new System.Windows.Forms.Button();
		this.buttonTown = new System.Windows.Forms.Button();
		this.labelAcceptPartyCoordinates = new System.Windows.Forms.Label();
		this.labelInventory = new System.Windows.Forms.Label();
		this.buttonMagicBag = new System.Windows.Forms.Button();
		this.labelBuffLine = new System.Windows.Forms.Label();
		this.labelStopMacrosWhenGenieStoppedCoordinates = new System.Windows.Forms.Label();
		this.labelChatWindowCoordinates = new System.Windows.Forms.Label();
		this.buttonBuffLine = new System.Windows.Forms.Button();
		this.labelWeapons = new System.Windows.Forms.Label();
		this.buttonStopMacrosWhenGenieStopped = new System.Windows.Forms.Button();
		this.buttonInventory = new System.Windows.Forms.Button();
		this.buttonChatWindowReSaveCoordinates = new System.Windows.Forms.Button();
		this.buttonAcceptPartySaveCoordinates = new System.Windows.Forms.Button();
		this.buttonWeapons = new System.Windows.Forms.Button();
		this.buttonInfo = new System.Windows.Forms.Button();
		this.labelInfo = new System.Windows.Forms.Label();
		this.btnHide = new System.Windows.Forms.Button();
		this.pnlContainer.SuspendLayout();
		base.SuspendLayout();
		this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
		this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold);
		this.lblTitle.ForeColor = System.Drawing.Color.White;
		this.lblTitle.Location = new System.Drawing.Point(0, 0);
		this.lblTitle.Name = "lblTitle";
		this.lblTitle.Size = new System.Drawing.Size(650, 40);
		this.lblTitle.TabIndex = 0;
		this.lblTitle.Text = "EKRAN AYARLARI";
		this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.pnlContainer.Controls.Add(this.labelLeftBotMenu);
		this.pnlContainer.Controls.Add(this.buttonLeftBotMenu);
		this.pnlContainer.Controls.Add(this.labelParty);
		this.pnlContainer.Controls.Add(this.labelTown);
		this.pnlContainer.Controls.Add(this.labelMagicBag);
		this.pnlContainer.Controls.Add(this.buttonParty);
		this.pnlContainer.Controls.Add(this.buttonTown);
		this.pnlContainer.Controls.Add(this.labelAcceptPartyCoordinates);
		this.pnlContainer.Controls.Add(this.labelInventory);
		this.pnlContainer.Controls.Add(this.buttonMagicBag);
		this.pnlContainer.Controls.Add(this.labelBuffLine);
		this.pnlContainer.Controls.Add(this.labelStopMacrosWhenGenieStoppedCoordinates);
		this.pnlContainer.Controls.Add(this.labelChatWindowCoordinates);
		this.pnlContainer.Controls.Add(this.buttonBuffLine);
		this.pnlContainer.Controls.Add(this.labelWeapons);
		this.pnlContainer.Controls.Add(this.buttonStopMacrosWhenGenieStopped);
		this.pnlContainer.Controls.Add(this.buttonInventory);
		this.pnlContainer.Controls.Add(this.buttonChatWindowReSaveCoordinates);
		this.pnlContainer.Controls.Add(this.buttonAcceptPartySaveCoordinates);
		this.pnlContainer.Controls.Add(this.buttonWeapons);
		this.pnlContainer.Controls.Add(this.buttonInfo);
		this.pnlContainer.Controls.Add(this.labelInfo);
		this.pnlContainer.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pnlContainer.Location = new System.Drawing.Point(0, 40);
		this.pnlContainer.Name = "pnlContainer";
		this.pnlContainer.Padding = new System.Windows.Forms.Padding(20);
		this.pnlContainer.Size = new System.Drawing.Size(650, 390);
		this.pnlContainer.TabIndex = 1;
		this.labelLeftBotMenu.AutoSize = true;
		this.labelLeftBotMenu.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		this.labelLeftBotMenu.ForeColor = System.Drawing.Color.White;
		this.labelLeftBotMenu.Location = new System.Drawing.Point(220, 307);
		this.labelLeftBotMenu.Name = "labelLeftBotMenu";
		this.labelLeftBotMenu.Size = new System.Drawing.Size(147, 17);
		this.labelLeftBotMenu.TabIndex = 25;
		this.labelLeftBotMenu.Text = "(0000,0000)-(0000,0000)";
		this.buttonLeftBotMenu.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		this.buttonLeftBotMenu.Location = new System.Drawing.Point(20, 303);
		this.buttonLeftBotMenu.Name = "buttonLeftBotMenu";
		this.buttonLeftBotMenu.Size = new System.Drawing.Size(180, 28);
		this.buttonLeftBotMenu.TabIndex = 24;
		this.buttonLeftBotMenu.Text = "Left Bot Menu";
		this.buttonLeftBotMenu.UseVisualStyleBackColor = true;
		this.buttonLeftBotMenu.Click += new System.EventHandler(SaveCoordinates_Click);
		this.labelParty.AutoSize = true;
		this.labelParty.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		this.labelParty.ForeColor = System.Drawing.Color.White;
		this.labelParty.Location = new System.Drawing.Point(220, 248);
		this.labelParty.Name = "labelParty";
		this.labelParty.Size = new System.Drawing.Size(147, 17);
		this.labelParty.TabIndex = 23;
		this.labelParty.Text = "(0000,0000)-(0000,0000)";
		this.labelTown.AutoSize = true;
		this.labelTown.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		this.labelTown.ForeColor = System.Drawing.Color.White;
		this.labelTown.Location = new System.Drawing.Point(220, 219);
		this.labelTown.Name = "labelTown";
		this.labelTown.Size = new System.Drawing.Size(147, 17);
		this.labelTown.TabIndex = 21;
		this.labelTown.Text = "(0000,0000)-(0000,0000)";
		this.labelMagicBag.AutoSize = true;
		this.labelMagicBag.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		this.labelMagicBag.ForeColor = System.Drawing.Color.White;
		this.labelMagicBag.Location = new System.Drawing.Point(220, 132);
		this.labelMagicBag.Name = "labelMagicBag";
		this.labelMagicBag.Size = new System.Drawing.Size(147, 17);
		this.labelMagicBag.TabIndex = 20;
		this.labelMagicBag.Text = "(0000,0000)-(0000,0000)";
		this.buttonParty.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		this.buttonParty.Location = new System.Drawing.Point(20, 244);
		this.buttonParty.Name = "buttonParty";
		this.buttonParty.Size = new System.Drawing.Size(180, 28);
		this.buttonParty.TabIndex = 9;
		this.buttonParty.Text = "Party";
		this.buttonParty.UseVisualStyleBackColor = true;
		this.buttonParty.Click += new System.EventHandler(SaveCoordinates_Click);
		this.buttonTown.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		this.buttonTown.Location = new System.Drawing.Point(20, 215);
		this.buttonTown.Name = "buttonTown";
		this.buttonTown.Size = new System.Drawing.Size(180, 28);
		this.buttonTown.TabIndex = 10;
		this.buttonTown.Text = "Town";
		this.buttonTown.UseVisualStyleBackColor = true;
		this.buttonTown.Click += new System.EventHandler(SaveCoordinates_Click);
		this.labelAcceptPartyCoordinates.AutoSize = true;
		this.labelAcceptPartyCoordinates.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		this.labelAcceptPartyCoordinates.ForeColor = System.Drawing.Color.White;
		this.labelAcceptPartyCoordinates.Location = new System.Drawing.Point(220, 45);
		this.labelAcceptPartyCoordinates.Name = "labelAcceptPartyCoordinates";
		this.labelAcceptPartyCoordinates.Size = new System.Drawing.Size(147, 17);
		this.labelAcceptPartyCoordinates.TabIndex = 19;
		this.labelAcceptPartyCoordinates.Text = "(0000,0000)-(0000,0000)";
		this.labelInventory.AutoSize = true;
		this.labelInventory.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		this.labelInventory.ForeColor = System.Drawing.Color.White;
		this.labelInventory.Location = new System.Drawing.Point(220, 103);
		this.labelInventory.Name = "labelInventory";
		this.labelInventory.Size = new System.Drawing.Size(147, 17);
		this.labelInventory.TabIndex = 18;
		this.labelInventory.Text = "(0000,0000)-(0000,0000)";
		this.buttonMagicBag.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		this.buttonMagicBag.Location = new System.Drawing.Point(20, 128);
		this.buttonMagicBag.Name = "buttonMagicBag";
		this.buttonMagicBag.Size = new System.Drawing.Size(180, 28);
		this.buttonMagicBag.TabIndex = 12;
		this.buttonMagicBag.Text = "Magic Bag";
		this.buttonMagicBag.UseVisualStyleBackColor = true;
		this.buttonMagicBag.Click += new System.EventHandler(SaveCoordinates_Click);
		this.labelBuffLine.AutoSize = true;
		this.labelBuffLine.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		this.labelBuffLine.ForeColor = System.Drawing.Color.White;
		this.labelBuffLine.Location = new System.Drawing.Point(220, 190);
		this.labelBuffLine.Name = "labelBuffLine";
		this.labelBuffLine.Size = new System.Drawing.Size(147, 17);
		this.labelBuffLine.TabIndex = 16;
		this.labelBuffLine.Text = "(0000,0000)-(0000,0000)";
		this.labelStopMacrosWhenGenieStoppedCoordinates.AutoSize = true;
		this.labelStopMacrosWhenGenieStoppedCoordinates.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		this.labelStopMacrosWhenGenieStoppedCoordinates.ForeColor = System.Drawing.Color.White;
		this.labelStopMacrosWhenGenieStoppedCoordinates.Location = new System.Drawing.Point(220, 16);
		this.labelStopMacrosWhenGenieStoppedCoordinates.Name = "labelStopMacrosWhenGenieStoppedCoordinates";
		this.labelStopMacrosWhenGenieStoppedCoordinates.Size = new System.Drawing.Size(147, 17);
		this.labelStopMacrosWhenGenieStoppedCoordinates.TabIndex = 17;
		this.labelStopMacrosWhenGenieStoppedCoordinates.Text = "(0000,0000)-(0000,0000)";
		this.labelChatWindowCoordinates.AutoSize = true;
		this.labelChatWindowCoordinates.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		this.labelChatWindowCoordinates.ForeColor = System.Drawing.Color.White;
		this.labelChatWindowCoordinates.Location = new System.Drawing.Point(220, 161);
		this.labelChatWindowCoordinates.Name = "labelChatWindowCoordinates";
		this.labelChatWindowCoordinates.Size = new System.Drawing.Size(147, 17);
		this.labelChatWindowCoordinates.TabIndex = 22;
		this.labelChatWindowCoordinates.Text = "(0000,0000)-(0000,0000)";
		this.buttonBuffLine.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		this.buttonBuffLine.Location = new System.Drawing.Point(20, 186);
		this.buttonBuffLine.Name = "buttonBuffLine";
		this.buttonBuffLine.Size = new System.Drawing.Size(180, 28);
		this.buttonBuffLine.TabIndex = 14;
		this.buttonBuffLine.Text = "Buff Line";
		this.buttonBuffLine.UseVisualStyleBackColor = true;
		this.buttonBuffLine.Click += new System.EventHandler(SaveCoordinates_Click);
		this.labelWeapons.AutoSize = true;
		this.labelWeapons.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		this.labelWeapons.ForeColor = System.Drawing.Color.White;
		this.labelWeapons.Location = new System.Drawing.Point(220, 74);
		this.labelWeapons.Name = "labelWeapons";
		this.labelWeapons.Size = new System.Drawing.Size(147, 17);
		this.labelWeapons.TabIndex = 15;
		this.labelWeapons.Text = "(0000,0000)-(0000,0000)";
		this.buttonStopMacrosWhenGenieStopped.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		this.buttonStopMacrosWhenGenieStopped.Location = new System.Drawing.Point(20, 12);
		this.buttonStopMacrosWhenGenieStopped.Name = "buttonStopMacrosWhenGenieStopped";
		this.buttonStopMacrosWhenGenieStopped.Size = new System.Drawing.Size(180, 28);
		this.buttonStopMacrosWhenGenieStopped.TabIndex = 11;
		this.buttonStopMacrosWhenGenieStopped.Text = "Genie";
		this.buttonStopMacrosWhenGenieStopped.UseVisualStyleBackColor = true;
		this.buttonStopMacrosWhenGenieStopped.Click += new System.EventHandler(SaveCoordinates_Click);
		this.buttonInventory.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		this.buttonInventory.Location = new System.Drawing.Point(20, 99);
		this.buttonInventory.Name = "buttonInventory";
		this.buttonInventory.Size = new System.Drawing.Size(180, 28);
		this.buttonInventory.TabIndex = 8;
		this.buttonInventory.Text = "Inventory";
		this.buttonInventory.UseVisualStyleBackColor = true;
		this.buttonInventory.Click += new System.EventHandler(SaveCoordinates_Click);
		this.buttonChatWindowReSaveCoordinates.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		this.buttonChatWindowReSaveCoordinates.Location = new System.Drawing.Point(20, 157);
		this.buttonChatWindowReSaveCoordinates.Name = "buttonChatWindowReSaveCoordinates";
		this.buttonChatWindowReSaveCoordinates.Size = new System.Drawing.Size(180, 28);
		this.buttonChatWindowReSaveCoordinates.TabIndex = 7;
		this.buttonChatWindowReSaveCoordinates.Text = "Chat Penceresi";
		this.buttonChatWindowReSaveCoordinates.UseVisualStyleBackColor = true;
		this.buttonChatWindowReSaveCoordinates.Click += new System.EventHandler(SaveCoordinates_Click);
		this.buttonAcceptPartySaveCoordinates.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		this.buttonAcceptPartySaveCoordinates.Location = new System.Drawing.Point(20, 41);
		this.buttonAcceptPartySaveCoordinates.Name = "buttonAcceptPartySaveCoordinates";
		this.buttonAcceptPartySaveCoordinates.Size = new System.Drawing.Size(180, 28);
		this.buttonAcceptPartySaveCoordinates.TabIndex = 13;
		this.buttonAcceptPartySaveCoordinates.Text = "Party Confirm";
		this.buttonAcceptPartySaveCoordinates.UseVisualStyleBackColor = true;
		this.buttonAcceptPartySaveCoordinates.Click += new System.EventHandler(SaveCoordinates_Click);
		this.buttonWeapons.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		this.buttonWeapons.Location = new System.Drawing.Point(20, 70);
		this.buttonWeapons.Name = "buttonWeapons";
		this.buttonWeapons.Size = new System.Drawing.Size(180, 28);
		this.buttonWeapons.TabIndex = 6;
		this.buttonWeapons.Text = "Silah ve Armor";
		this.buttonWeapons.UseVisualStyleBackColor = true;
		this.buttonWeapons.Click += new System.EventHandler(SaveCoordinates_Click);
		this.buttonInfo.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		this.buttonInfo.Location = new System.Drawing.Point(20, 273);
		this.buttonInfo.Name = "buttonInfo";
		this.buttonInfo.Size = new System.Drawing.Size(180, 28);
		this.buttonInfo.TabIndex = 9;
		this.buttonInfo.Text = "Info";
		this.buttonInfo.UseVisualStyleBackColor = true;
		this.buttonInfo.Click += new System.EventHandler(SaveCoordinates_Click);
		this.labelInfo.AutoSize = true;
		this.labelInfo.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		this.labelInfo.ForeColor = System.Drawing.Color.White;
		this.labelInfo.Location = new System.Drawing.Point(220, 277);
		this.labelInfo.Name = "labelInfo";
		this.labelInfo.Size = new System.Drawing.Size(147, 17);
		this.labelInfo.TabIndex = 23;
		this.labelInfo.Text = "(0000,0000)-(0000,0000)";
		this.btnHide.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnHide.BackColor = System.Drawing.Color.FromArgb(60, 60, 80);
		this.btnHide.FlatAppearance.BorderSize = 0;
		this.btnHide.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnHide.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.btnHide.ForeColor = System.Drawing.Color.White;
		this.btnHide.Location = new System.Drawing.Point(608, 4);
		this.btnHide.Name = "btnHide";
		this.btnHide.Size = new System.Drawing.Size(30, 30);
		this.btnHide.TabIndex = 0;
		this.btnHide.Text = "X";
		this.btnHide.UseVisualStyleBackColor = false;
		this.btnHide.Click += new System.EventHandler(BtnHide_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.FromArgb(28, 32, 43);
		base.ClientSize = new System.Drawing.Size(650, 430);
		base.Controls.Add(this.btnHide);
		base.Controls.Add(this.pnlContainer);
		base.Controls.Add(this.lblTitle);
		this.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "SettingsForm";
		this.Text = "SettingsForm";
		base.TopMost = true;
		this.pnlContainer.ResumeLayout(false);
		this.pnlContainer.PerformLayout();
		base.ResumeLayout(false);
	}
}
