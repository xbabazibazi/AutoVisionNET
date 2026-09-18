using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using InputManager;
using SettingsManager;
using SettingsManager.GeneralSettingss;
using SimpleLogger;
using UI2.Interfaces;
using UI2.Macro.UserControls;

namespace UI2.Macro;

public class Macro2 : Form, IMacroForm
{
	private readonly Dictionary<string, UserControl> _macroControls;

	private readonly Settings _settings = Settings.Instance;

	private readonly Logger _logger = Logger.Instance;

	private readonly Attack _attack;

	private readonly Login _login;

	private readonly TpParty _tpParty;

	private readonly UndyAc _undyAc;

	private readonly Escape _escape;

	private readonly General _general;

	private readonly Color _backgroundColor = Color.FromArgb(38, 38, 45);

	private readonly Color _surfaceColor = Color.FromArgb(48, 48, 55);

	private readonly Color _textPrimaryColor = Color.FromArgb(235, 235, 240);

	private readonly Color _successColor = Color.FromArgb(0, 153, 102);

	private readonly Color _errorColor = Color.FromArgb(200, 60, 60);

	private IContainer components = null;

	private SplitContainer splitContainer1;

	private ListBox listBoxMacros;

	private Panel panelMacros;

	private Button btnHide;

	public Macro2(InputUtils inputUtils)
	{
		InitializeComponent();
		base.StartPosition = FormStartPosition.Manual;
		_attack = new Attack();
		_login = new Login();
		_tpParty = new TpParty();
		_undyAc = new UndyAc();
		_escape = new Escape(inputUtils);
		_general = new General();
		_macroControls = InitializeSettingControls();
		LoadSettings();
		SetupEventHandlers();
		OptimizeUserControlSizes();
	}

	private void OptimizeUserControlSizes()
	{
		foreach (UserControl value in _macroControls.Values)
		{
			value.MinimumSize = new Size(636, 365);
			value.MaximumSize = new Size(646, 375);
			value.Size = new Size(636, 365);
		}
	}

	public bool GetIsDisposed()
	{
		return base.IsDisposed;
	}

	public void GetClose()
	{
		Close();
	}

	public Form GetForm()
	{
		return this;
	}

	public void GetHide()
	{
		Hide();
	}

	public void SetEnabled(bool enabled)
	{
		base.Enabled = enabled;
	}

	public void ToggleControls(bool enabled)
	{
		base.Enabled = enabled;
	}

	private Dictionary<string, UserControl> InitializeSettingControls()
	{
		Dictionary<string, UserControl> dictionary = new Dictionary<string, UserControl>
		{
			{ "Atak", _attack },
			{ "Login", _login },
			{ "Tp Party", _tpParty },
			{ "Undy AC", _undyAc },
			{ "Escape", _escape },
			{ "Genel", _general }
		};
		try
		{
			ListBox.ObjectCollection items = listBoxMacros.Items;
			object[] items2 = dictionary.Keys.ToArray();
			items.AddRange(items2);
			_logger.LogDebug("Macro2 controls initialized");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error initializing controls: " + ex.Message);
		}
		return dictionary;
	}

	private void LoadSettings()
	{
		try
		{
			FormSettings formSettings = _settings.GeneralSettings.FormSettings;
			base.Location = new Point(formSettings.FormLocationX, formSettings.FormLocationY);
			_logger.LogDebug("Macro2 settings loaded");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error loading settings: " + ex.Message);
		}
	}

	private void SaveSettings()
	{
		try
		{
			_attack.SaveSettings();
			_login.SaveSettings();
			_tpParty.SaveSettings();
			_undyAc.SaveSettings();
			_escape.SaveSettings();
			_general.SaveSettings();
			FormSettings formSettings = _settings.GeneralSettings.FormSettings;
			formSettings.FormLocationX = base.Location.X;
			formSettings.FormLocationY = base.Location.Y;
			_logger.LogDebug("Macro2 settings saved");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error saving settings: " + ex.Message);
		}
	}

	private void SetupEventHandlers()
	{
		btnHide.MouseEnter += delegate
		{
			btnHide.BackColor = Color.FromArgb(70, 70, 75);
		};
		btnHide.MouseLeave += delegate
		{
			btnHide.BackColor = Color.FromArgb(60, 60, 65);
		};
	}

	private void Macro2_Load(object sender, EventArgs e)
	{
		try
		{
			if (listBoxMacros.Items.Count > 0)
			{
				listBoxMacros.SelectedIndex = 0;
			}
			_logger.LogDebug("Macro2 form loaded");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error during form load: " + ex.Message);
		}
	}

	private void listBoxMacros_SelectedIndexChanged(object sender, EventArgs e)
	{
		try
		{
			if (listBoxMacros.SelectedItem is string text && _macroControls.TryGetValue(text, out UserControl value))
			{
				panelMacros.Controls.Clear();
				value.Dock = DockStyle.None;
				value.Size = new Size(636, 365);
				value.Location = new Point(5, 5);
				panelMacros.Controls.Add(value);
				_logger.LogDebug("Selected macro control: " + text);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing selected macro control: " + ex.Message);
		}
	}

	private void Macro2_FormClosing(object sender, FormClosingEventArgs e)
	{
		try
		{
			SaveSettings();
			foreach (UserControl value in _macroControls.Values)
			{
				try
				{
					value.GetType().GetMethod("SaveSettings")?.Invoke(value, null);
					_logger.LogDebug("Saved settings for control: " + value.GetType().Name);
				}
				catch (Exception ex)
				{
					_logger.LogError("Error saving settings for control " + value.GetType().Name + ": " + ex.Message);
				}
			}
		}
		catch (Exception ex2)
		{
			_logger.LogError("Error during form closing: " + ex2.Message);
		}
	}

	private void btnHide_Click(object sender, EventArgs e)
	{
		try
		{
			Hide();
			_logger.LogDebug("Macro2 form hidden");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error hiding form: " + ex.Message);
		}
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
		this.splitContainer1 = new System.Windows.Forms.SplitContainer();
		this.listBoxMacros = new System.Windows.Forms.ListBox();
		this.panelMacros = new System.Windows.Forms.Panel();
		this.btnHide = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).BeginInit();
		this.splitContainer1.Panel1.SuspendLayout();
		this.splitContainer1.Panel2.SuspendLayout();
		this.splitContainer1.SuspendLayout();
		base.SuspendLayout();
		this.splitContainer1.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer1.Location = new System.Drawing.Point(0, 0);
		this.splitContainer1.Name = "splitContainer1";
		this.splitContainer1.IsSplitterFixed = true;
		this.splitContainer1.Panel1.Controls.Add(this.listBoxMacros);
		this.splitContainer1.Panel1MinSize = 150;
		this.splitContainer1.Panel2.Controls.Add(this.panelMacros);
		this.splitContainer1.Panel2MinSize = 646;
		this.splitContainer1.Size = new System.Drawing.Size(800, 375);
		this.splitContainer1.SplitterDistance = 150;
		this.splitContainer1.SplitterWidth = 1;
		this.splitContainer1.TabIndex = 0;
		this.listBoxMacros.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.listBoxMacros.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.listBoxMacros.Dock = System.Windows.Forms.DockStyle.Fill;
		this.listBoxMacros.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		this.listBoxMacros.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.listBoxMacros.FormattingEnabled = true;
		this.listBoxMacros.ItemHeight = 16;
		this.listBoxMacros.Location = new System.Drawing.Point(0, 0);
		this.listBoxMacros.Name = "listBoxMacros";
		this.listBoxMacros.Size = new System.Drawing.Size(150, 375);
		this.listBoxMacros.TabIndex = 0;
		this.listBoxMacros.SelectedIndexChanged += new System.EventHandler(listBoxMacros_SelectedIndexChanged);
		this.panelMacros.AutoScroll = false;
		this.panelMacros.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.panelMacros.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panelMacros.Location = new System.Drawing.Point(0, 0);
		this.panelMacros.Name = "panelMacros";
		this.panelMacros.Padding = new System.Windows.Forms.Padding(0);
		this.panelMacros.Size = new System.Drawing.Size(646, 375);
		this.panelMacros.TabIndex = 0;
		this.btnHide.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnHide.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.btnHide.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 85);
		this.btnHide.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(70, 70, 75);
		this.btnHide.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnHide.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.btnHide.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.btnHide.Location = new System.Drawing.Point(765, 5);
		this.btnHide.Name = "btnHide";
		this.btnHide.Size = new System.Drawing.Size(30, 28);
		this.btnHide.TabIndex = 1;
		this.btnHide.Text = "X";
		this.btnHide.UseVisualStyleBackColor = false;
		this.btnHide.Click += new System.EventHandler(btnHide_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		base.ClientSize = new System.Drawing.Size(800, 375);
		base.Controls.Add(this.btnHide);
		base.Controls.Add(this.splitContainer1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		this.MinimumSize = new System.Drawing.Size(800, 375);
		base.Name = "Macro2";
		this.Text = "Macro2";
		base.TopMost = true;
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(Macro2_FormClosing);
		base.Load += new System.EventHandler(Macro2_Load);
		this.splitContainer1.Panel1.ResumeLayout(false);
		this.splitContainer1.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).EndInit();
		this.splitContainer1.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
