using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SettingsManager;
using SimpleLogger;

namespace UI2.Macro.UserControls;

public class Telegram : UserControl
{
	private readonly SettingsManager.GeneralSettingss.Telegram _settings = Settings.Instance.GeneralSettings.Telegram;

	private readonly Logger _logger = Logger.Instance;

	private IContainer components = null;

	private Label labelHeader;

	private Label labelInfo;

	private GroupBox groupBoxSettings;

	private CheckBox checkBoxIsActive;

	private Label labelBotToken;

	private TextBox textBoxBotToken;

	private Label labelChatId;

	private TextBox textBoxChatId;

	private ToolTip toolTip;

	public Telegram()
	{
		InitializeComponent();
		LoadSettings();
	}

	public void SaveSettings()
	{
		try
		{
			_settings.IsActive = checkBoxIsActive.Checked;
			_settings.BotToken = textBoxBotToken.Text.Trim();
			_settings.ChatId = textBoxChatId.Text.Trim();
			_logger.LogDebug("Telegram settings saved");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error saving Telegram settings: " + ex.Message);
		}
	}

	private void LoadSettings()
	{
		try
		{
			checkBoxIsActive.Checked = _settings.IsActive;
			textBoxBotToken.Text = _settings.BotToken;
			textBoxChatId.Text = _settings.ChatId;
			_logger.LogDebug("Telegram settings loaded");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error loading Telegram settings: " + ex.Message);
		}
	}

	private void checkBoxIsActive_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.IsActive = checkBoxIsActive.Checked;
			_logger.LogDebug("Telegram IsActive setting changed");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing Telegram IsActive: " + ex.Message);
		}
	}

	private void textBoxBotToken_TextChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.BotToken = textBoxBotToken.Text.Trim();
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing Telegram BotToken: " + ex.Message);
		}
	}

	private void textBoxChatId_TextChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.ChatId = textBoxChatId.Text.Trim();
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing Telegram ChatId: " + ex.Message);
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
		this.components = new System.ComponentModel.Container();
		this.labelHeader = new System.Windows.Forms.Label();
		this.labelInfo = new System.Windows.Forms.Label();
		this.groupBoxSettings = new System.Windows.Forms.GroupBox();
		this.checkBoxIsActive = new System.Windows.Forms.CheckBox();
		this.labelBotToken = new System.Windows.Forms.Label();
		this.textBoxBotToken = new System.Windows.Forms.TextBox();
		this.labelChatId = new System.Windows.Forms.Label();
		this.textBoxChatId = new System.Windows.Forms.TextBox();
		this.toolTip = new System.Windows.Forms.ToolTip(this.components);
		this.groupBoxSettings.SuspendLayout();
		base.SuspendLayout();
		this.labelHeader.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.labelHeader.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelHeader.Font = UI2.AppFonts.Header(13f);
		this.labelHeader.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelHeader.Location = new System.Drawing.Point(0, 0);
		this.labelHeader.Name = "labelHeader";
		this.labelHeader.Size = new System.Drawing.Size(570, 25);
		this.labelHeader.TabIndex = 0;
		this.labelHeader.Text = "Telegram Bildirimleri";
		this.labelHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelInfo.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.labelInfo.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelInfo.Font = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Italic);
		this.labelInfo.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelInfo.Location = new System.Drawing.Point(0, 25);
		this.labelInfo.Name = "labelInfo";
		this.labelInfo.Size = new System.Drawing.Size(570, 15);
		this.labelInfo.TabIndex = 1;
		this.labelInfo.Text = "Envanter dolduğunda veya bağlantı koptuğunda Telegram'a bildirim gönderilir";
		this.labelInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.groupBoxSettings.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.groupBoxSettings.Controls.Add(this.checkBoxIsActive);
		this.groupBoxSettings.Controls.Add(this.labelBotToken);
		this.groupBoxSettings.Controls.Add(this.textBoxBotToken);
		this.groupBoxSettings.Controls.Add(this.labelChatId);
		this.groupBoxSettings.Controls.Add(this.textBoxChatId);
		this.groupBoxSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.groupBoxSettings.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.groupBoxSettings.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.groupBoxSettings.Location = new System.Drawing.Point(15, 50);
		this.groupBoxSettings.Name = "groupBoxSettings";
		this.groupBoxSettings.Size = new System.Drawing.Size(540, 160);
		this.groupBoxSettings.TabIndex = 2;
		this.groupBoxSettings.TabStop = false;
		this.groupBoxSettings.Text = "Telegram Ayarları";
		this.checkBoxIsActive.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkBoxIsActive.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.checkBoxIsActive.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 85);
		this.checkBoxIsActive.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(55, 78, 92);
		this.checkBoxIsActive.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.checkBoxIsActive.Font = new System.Drawing.Font("Segoe UI", 8f);
		this.checkBoxIsActive.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.checkBoxIsActive.Location = new System.Drawing.Point(20, 120);
		this.checkBoxIsActive.Name = "checkBoxIsActive";
		this.checkBoxIsActive.Size = new System.Drawing.Size(200, 30);
		this.checkBoxIsActive.TabIndex = 4;
		this.checkBoxIsActive.Text = "Bildirimler Aktif";
		this.checkBoxIsActive.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip.SetToolTip(this.checkBoxIsActive, "Telegram bildirimlerini aç/kapat");
		this.checkBoxIsActive.UseVisualStyleBackColor = false;
		this.checkBoxIsActive.CheckedChanged += new System.EventHandler(checkBoxIsActive_CheckedChanged);
		this.labelBotToken.AutoSize = true;
		this.labelBotToken.Font = new System.Drawing.Font("Segoe UI", 8f);
		this.labelBotToken.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelBotToken.Location = new System.Drawing.Point(20, 14);
		this.labelBotToken.Name = "labelBotToken";
		this.labelBotToken.Size = new System.Drawing.Size(64, 15);
		this.labelBotToken.TabIndex = 0;
		this.labelBotToken.Text = "Bot Token:";
		this.textBoxBotToken.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.textBoxBotToken.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.textBoxBotToken.Location = new System.Drawing.Point(20, 34);
		this.textBoxBotToken.Name = "textBoxBotToken";
		this.textBoxBotToken.Size = new System.Drawing.Size(500, 22);
		this.textBoxBotToken.TabIndex = 1;
		this.toolTip.SetToolTip(this.textBoxBotToken, "@BotFather üzerinden alınan bot token'ı");
		this.textBoxBotToken.TextChanged += new System.EventHandler(textBoxBotToken_TextChanged);
		this.labelChatId.AutoSize = true;
		this.labelChatId.Font = new System.Drawing.Font("Segoe UI", 8f);
		this.labelChatId.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelChatId.Location = new System.Drawing.Point(20, 66);
		this.labelChatId.Name = "labelChatId";
		this.labelChatId.Size = new System.Drawing.Size(52, 15);
		this.labelChatId.TabIndex = 2;
		this.labelChatId.Text = "Chat Id:";
		this.textBoxChatId.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.textBoxChatId.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.textBoxChatId.Location = new System.Drawing.Point(20, 86);
		this.textBoxChatId.Name = "textBoxChatId";
		this.textBoxChatId.Size = new System.Drawing.Size(220, 22);
		this.textBoxChatId.TabIndex = 3;
		this.toolTip.SetToolTip(this.textBoxChatId, "https://api.telegram.org/bot{token}/getUpdates adresinden alınan chat id");
		this.textBoxChatId.TextChanged += new System.EventHandler(textBoxChatId_TextChanged);
		this.toolTip.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.toolTip.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		base.Controls.Add(this.groupBoxSettings);
		base.Controls.Add(this.labelInfo);
		base.Controls.Add(this.labelHeader);
		this.Dock = System.Windows.Forms.DockStyle.Fill;
		this.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		base.Name = "Telegram";
		base.Size = new System.Drawing.Size(570, 300);
		this.groupBoxSettings.ResumeLayout(false);
		this.groupBoxSettings.PerformLayout();
		base.ResumeLayout(false);
	}
}
